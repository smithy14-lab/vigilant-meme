using CheerDeck.Application.Interfaces;
using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EntriesController(EntryService entryService, IAppDbContext db) : ControllerBase
{
    public record SubmitEntryRequest(Guid EventId, Guid DivisionId, Guid TeamId, Guid? MusicFileId);
    public record ConfirmPaymentRequest(string PaymentId);

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav",
        "audio/aac", "audio/mp4", "audio/x-m4a"
    };

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitEntryRequest request, CancellationToken ct)
    {
        var entry = await entryService.SubmitEntryAsync(
            request.EventId, request.DivisionId, request.TeamId, request.MusicFileId, ct);
        return Ok(entry);
    }

    [HttpGet("by-event/{eventId:guid}")]
    public async Task<IActionResult> GetByEvent(Guid eventId, CancellationToken ct)
        => Ok(await entryService.GetEntriesForEventAsync(eventId, ct));

    [HttpPost("{id:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentRequest request, CancellationToken ct)
    {
        await entryService.ConfirmPaymentAsync(id, request.PaymentId, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/upload-music")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadMusic(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest("No file provided");

        if (file.Length > 50_000_000)
            return BadRequest("File must be under 50 MB");

        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest("Only MP3, WAV, AAC, and M4A files are allowed");

        var entry = await db.EventEntries.FindAsync(new object[] { id }, ct);
        if (entry is null)
            return NotFound("Entry not found");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "music");
        Directory.CreateDirectory(uploadsDir);

        var safeFileName = $"{id}_{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, safeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var music = new TeamMusic
        {
            TeamId = entry.TeamId ?? Guid.Empty,
            TenantId = entry.TenantId,
            FileName = file.FileName,
            StoragePath = filePath,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            IsCurrent = true,
            UploadedAt = DateTime.UtcNow
        };

        db.TeamMusic.Add(music);
        entry.MusicFileId = music.Id;
        entry.MusicFileName = file.FileName;
        await db.SaveChangesAsync(ct);

        return Ok(new { music.Id, music.FileName, music.FileSizeBytes });
    }
}
