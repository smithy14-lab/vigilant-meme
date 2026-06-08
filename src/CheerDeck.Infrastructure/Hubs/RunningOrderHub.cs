namespace CheerDeck.Infrastructure.Hubs;

using Microsoft.AspNetCore.SignalR;

public class RunningOrderHub : Hub
{
    public async Task JoinEventGroup(string eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"event-{eventId}");
    }

    public async Task LeaveEventGroup(string eventId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event-{eventId}");
    }

    public async Task JoinBlockGroup(string blockId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"block-{blockId}");
    }
}

public class ScoreHub : Hub
{
    public async Task JoinScoringGroup(string eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"scoring-{eventId}");
    }

    public async Task JoinJudgeGroup(string panelId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"panel-{panelId}");
    }
}

public class LeaderboardHub : Hub
{
    public async Task JoinLeaderboard(string eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"leaderboard-{eventId}");
    }

    public async Task JoinDivisionLeaderboard(string divisionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"division-{divisionId}");
    }
}
