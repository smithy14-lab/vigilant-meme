namespace CheerDeck.Domain.Integration;

public record EligibilityCheckResult(
    bool IsEligible,
    string? Reason,
    bool MembershipActive,
    bool CoachCredentialsValid,
    List<string> Warnings);

public record AthleteEligibilityResult(
    Guid AthleteId,
    bool IsEligible,
    bool MembershipActive,
    string? MembershipId,
    string? Reason);

public record CoachCredentialResult(
    Guid CoachId,
    bool AllCredentialsValid,
    List<CredentialStatus> Credentials);

public record CredentialStatus(
    string Type,
    bool IsValid,
    DateOnly? ExpiryDate,
    string? Notes);

public interface IEligibilityProvider
{
    Task<AthleteEligibilityResult> CheckAthleteEligibilityAsync(
        Guid athleteId, string? externalMembershipId, DateOnly dateOfBirth, string division, CancellationToken ct = default);

    Task<CoachCredentialResult> CheckCoachCredentialsAsync(
        Guid coachId, string? externalCredentialId, CancellationToken ct = default);

    Task<EligibilityCheckResult> ValidateTeamEntryAsync(
        List<(Guid AthleteId, string? ExternalId, DateOnly Dob)> athletes,
        (Guid CoachId, string? ExternalId) coach,
        string division, string level, CancellationToken ct = default);
}
