using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrivateLessonsController(PrivateLessonService privateLessonService) : ControllerBase
{
    public record CreatePrivateLessonRequest(PrivateLesson Lesson, List<Guid> AthleteIds);
    public record UpdateStatusRequest(PrivateLessonStatus Status);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await privateLessonService.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrivateLessonRequest request, CancellationToken ct)
    {
        var created = await privateLessonService.CreateAsync(request.Lesson, request.AthleteIds, ct);
        return Ok(created);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        await privateLessonService.UpdateStatusAsync(id, request.Status, ct);
        return NoContent();
    }
}
