using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttendanceController(AttendanceService attendanceService) : ControllerBase
{
    public record MarkAttendanceRequest(Guid AthleteId, AttendanceStatus Status, string? Notes);
    public record BulkAttendanceRequest(Dictionary<Guid, AttendanceStatus> Records);
    public record CancelSessionRequest(string? Reason);

    [HttpGet("class/{classId:guid}/sessions")]
    public async Task<IActionResult> GetSessions(Guid classId, CancellationToken ct)
        => Ok(await attendanceService.GetSessionsForClassAsync(classId, ct));

    [HttpPost("class/{classId:guid}/sessions")]
    public async Task<IActionResult> CreateSession(Guid classId, [FromBody] DateOnly date, CancellationToken ct)
    {
        var session = await attendanceService.CreateSessionAsync(classId, date, ct);
        return Ok(session);
    }

    [HttpPost("sessions/{sessionId:guid}/mark")]
    public async Task<IActionResult> MarkAttendance(Guid sessionId, [FromBody] MarkAttendanceRequest request, CancellationToken ct)
    {
        await attendanceService.MarkAttendanceAsync(sessionId, request.AthleteId, request.Status, request.Notes, ct);
        return NoContent();
    }

    [HttpPost("sessions/{sessionId:guid}/bulk-mark")]
    public async Task<IActionResult> BulkMarkAttendance(Guid sessionId, [FromBody] BulkAttendanceRequest request, CancellationToken ct)
    {
        await attendanceService.BulkMarkAttendanceAsync(sessionId, request.Records, ct);
        return NoContent();
    }

    [HttpPost("sessions/{sessionId:guid}/cancel")]
    public async Task<IActionResult> CancelSession(Guid sessionId, [FromBody] CancelSessionRequest request, CancellationToken ct)
    {
        await attendanceService.CancelSessionAsync(sessionId, request.Reason, ct);
        return NoContent();
    }
}
