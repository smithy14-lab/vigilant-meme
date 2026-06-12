using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController(ChatService chatService) : ControllerBase
{
    public record AddMemberRequest(string UserId, string DisplayName, string Role);
    public record SendMessageRequest(string Content);

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms([FromQuery] string userId, CancellationToken ct)
        => Ok(await chatService.GetRoomsForUserAsync(userId, ct));

    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom([FromBody] ChatRoom room, CancellationToken ct)
    {
        var created = await chatService.CreateRoomAsync(room, ct);
        return Ok(created);
    }

    [HttpPost("rooms/{roomId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid roomId, [FromBody] AddMemberRequest request, CancellationToken ct)
    {
        var member = await chatService.AddMemberAsync(roomId, request.UserId, request.DisplayName, request.Role, ct);
        return Ok(member);
    }

    [HttpGet("rooms/{roomId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid roomId, [FromQuery] int take = 50, CancellationToken ct = default)
        => Ok(await chatService.GetMessagesAsync(roomId, take, ct));

    [HttpPost("rooms/{roomId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid roomId, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var message = await chatService.SendMessageAsync(roomId, request.Content, ct);
        return Ok(message);
    }

    [HttpPost("rooms/{roomId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid roomId, [FromQuery] string userId, CancellationToken ct)
    {
        await chatService.MarkReadAsync(roomId, userId, ct);
        return NoContent();
    }
}
