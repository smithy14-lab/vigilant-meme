using CheerDeck.Infrastructure.Identity;
using System.Security.Claims;

namespace CheerDeck.Club.Web.Services;

public class ActiveRoleState
{
    private string? _activeRole;

    public string ActiveRole
    {
        get => _activeRole ?? DefaultRole;
        set
        {
            if (_activeRole != value)
            {
                _activeRole = value;
                OnChanged?.Invoke();
            }
        }
    }

    public string DefaultRole { get; private set; } = AppRoles.Guardian;
    public List<string> AvailableRoles { get; private set; } = new();

    public event Action? OnChanged;

    public void Initialize(ClaimsPrincipal user)
    {
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var clubRoles = new[] { AppRoles.ClubOwner, AppRoles.ClubAdmin, AppRoles.Coach, AppRoles.Guardian, AppRoles.Athlete };
        AvailableRoles = roles.Where(r => clubRoles.Contains(r)).ToList();

        if (AvailableRoles.Count == 0)
            AvailableRoles.Add(AppRoles.Guardian);

        DefaultRole = GetPriorityRole(AvailableRoles);
        _activeRole ??= DefaultRole;
    }

    public bool IsInRole(string role) => ActiveRole == role;

    public bool IsManagement => ActiveRole is AppRoles.ClubOwner or AppRoles.ClubAdmin or AppRoles.Coach;
    public bool IsParent => ActiveRole == AppRoles.Guardian;
    public bool IsAthlete => ActiveRole == AppRoles.Athlete;

    private static string GetPriorityRole(List<string> roles)
    {
        if (roles.Contains(AppRoles.ClubOwner)) return AppRoles.ClubOwner;
        if (roles.Contains(AppRoles.ClubAdmin)) return AppRoles.ClubAdmin;
        if (roles.Contains(AppRoles.Coach)) return AppRoles.Coach;
        if (roles.Contains(AppRoles.Guardian)) return AppRoles.Guardian;
        if (roles.Contains(AppRoles.Athlete)) return AppRoles.Athlete;
        return roles.First();
    }
}
