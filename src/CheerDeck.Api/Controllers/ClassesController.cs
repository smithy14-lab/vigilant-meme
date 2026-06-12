using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClassesController(ClassService classService) : ControllerBase
{
    public record EnrolRequest(Guid AthleteId);
    public record CreateSessionRequest(DateOnly Date);
    public record MarkAttendanceRequest(Guid AthleteId, AttendanceStatus Status);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await classService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var cls = await classService.GetByIdAsync(id, ct);
        return cls is null ? NotFound() : Ok(cls);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Class cls, CancellationToken ct)
    {
        var created = await classService.CreateAsync(cls, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/enrol")]
    public async Task<IActionResult> Enrol(Guid id, [FromBody] EnrolRequest request, CancellationToken ct)
    {
        var enrolment = await classService.EnrolAthleteAsync(id, request.AthleteId, ct);
        return Ok(enrolment);
    }

    [HttpPost("{id:guid}/sessions")]
    public async Task<IActionResult> CreateSession(Guid id, [FromBody] CreateSessionRequest request, CancellationToken ct)
    {
        var session = await classService.CreateSessionAsync(id, request.Date, ct);
        return Ok(session);
    }

    [HttpPost("sessions/{sessionId:guid}/attendance")]
    public async Task<IActionResult> MarkAttendance(Guid sessionId, [FromBody] MarkAttendanceRequest request, CancellationToken ct)
    {
        await classService.MarkAttendanceAsync(sessionId, request.AthleteId, request.Status, ct);
        return NoContent();
    }
}
