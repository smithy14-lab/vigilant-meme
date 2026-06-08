namespace CheerDeck.Infrastructure.Stubs;

using CheerDeck.Domain.Integration;

public class StubEligibilityProvider : IEligibilityProvider
{
    public Task<AthleteEligibilityResult> CheckAthleteEligibilityAsync(
        Guid athleteId, string? externalMembershipId, DateOnly dateOfBirth, string division, CancellationToken ct = default)
    {
        return Task.FromResult(new AthleteEligibilityResult(
            athleteId,
            IsEligible: true,
            MembershipActive: true,
            MembershipId: externalMembershipId ?? $"STUB-{athleteId:N}".Substring(0, 12),
            Reason: null));
    }

    public Task<CoachCredentialResult> CheckCoachCredentialsAsync(
        Guid coachId, string? externalCredentialId, CancellationToken ct = default)
    {
        return Task.FromResult(new CoachCredentialResult(
            coachId,
            AllCredentialsValid: true,
            Credentials:
            [
                new CredentialStatus("Level 2 Coaching", true, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), null),
                new CredentialStatus("DBS Check", true, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)), null),
                new CredentialStatus("First Aid", true, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)), null)
            ]));
    }

    public Task<EligibilityCheckResult> ValidateTeamEntryAsync(
        List<(Guid AthleteId, string? ExternalId, DateOnly Dob)> athletes,
        (Guid CoachId, string? ExternalId) coach,
        string division, string level, CancellationToken ct = default)
    {
        return Task.FromResult(new EligibilityCheckResult(
            IsEligible: true,
            Reason: null,
            MembershipActive: true,
            CoachCredentialsValid: true,
            Warnings: []));
    }
}
