namespace CheerDeck.Infrastructure.Identity;

public static class AppRoles
{
    public const string ClubOwner = "ClubOwner";
    public const string ClubAdmin = "ClubAdmin";
    public const string Coach = "Coach";
    public const string Guardian = "Guardian";
    public const string Athlete = "Athlete";
    public const string EventProducer = "EventProducer";
    public const string Judge = "Judge";
    public const string Tabulator = "Tabulator";
    public const string Announcer = "Announcer";

    public static readonly string[] All =
    [
        ClubOwner, ClubAdmin, Coach, Guardian, Athlete,
        EventProducer, Judge, Tabulator, Announcer
    ];

    public static string DisplayName(string role) => role switch
    {
        ClubOwner => "Club Owner",
        ClubAdmin => "Admin",
        Coach => "Coach",
        Guardian => "Parent",
        Athlete => "Athlete",
        EventProducer => "Event Producer",
        Judge => "Judge",
        Tabulator => "Tabulator",
        Announcer => "Announcer",
        _ => role
    };
}
