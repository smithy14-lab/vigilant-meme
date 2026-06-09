namespace CheerDeck.Infrastructure.Hubs;

using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat-{roomId}");
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat-{roomId}");
    }

    public async Task SendMessage(string roomId, string senderName, string content)
    {
        await Clients.Group($"chat-{roomId}").SendAsync("ReceiveMessage", senderName, content, DateTime.UtcNow);
    }

    public async Task UserTyping(string roomId, string userName)
    {
        await Clients.OthersInGroup($"chat-{roomId}").SendAsync("UserTyping", userName);
    }
}
