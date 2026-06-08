namespace CheerDeck.Infrastructure.Identity;

public static class AppRoles
{
    public const string ClubOwner = "ClubOwner";
    public const string ClubAdmin = "ClubAdmin";
    public const string Coach = "Coach";
    public const string Guardian = "Guardian";
    public const string EventProducer = "EventProducer";
    public const string Judge = "Judge";
    public const string Tabulator = "Tabulator";
    public const string Announcer = "Announcer";

    public static readonly string[] All =
    [
        ClubOwner, ClubAdmin, Coach, Guardian,
        EventProducer, Judge, Tabulator, Announcer
    ];
}
