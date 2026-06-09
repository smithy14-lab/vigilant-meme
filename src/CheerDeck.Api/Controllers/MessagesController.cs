using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController(MessageService messageService) : ControllerBase
{
    public record SendMessageRequest(Message Message, List<Guid>? GuardianIds);

    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox([FromQuery] Guid? recipientGuardianId, CancellationToken ct)
        => Ok(await messageService.GetInboxAsync(recipientGuardianId, ct));

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var sent = await messageService.SendAsync(request.Message, request.GuardianIds, ct);
        return Ok(sent);
    }
}
