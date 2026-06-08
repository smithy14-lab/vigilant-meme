namespace CheerDeck.Tests;

using CheerDeck.Infrastructure.Stubs;
using FluentAssertions;

public class EligibilityTests
{
    [Fact]
    public async Task Stub_Provider_Returns_Eligible_For_All_Athletes()
    {
        var provider = new StubEligibilityProvider();

        var result = await provider.CheckAthleteEligibilityAsync(
            Guid.NewGuid(), "SC-10001", new DateOnly(2012, 5, 15), "Junior");

        result.IsEligible.Should().BeTrue();
        result.MembershipActive.Should().BeTrue();
        result.MembershipId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Stub_Provider_Returns_Valid_Coach_Credentials()
    {
        var provider = new StubEligibilityProvider();

        var result = await provider.CheckCoachCredentialsAsync(Guid.NewGuid(), "UKCC-2-12345");

        result.AllCredentialsValid.Should().BeTrue();
        result.Credentials.Should().HaveCount(3);
        result.Credentials.Should().AllSatisfy(c => c.IsValid.Should().BeTrue());
    }

    [Fact]
    public async Task Stub_Provider_Validates_Team_Entry()
    {
        var provider = new StubEligibilityProvider();

        var athletes = new List<(Guid AthleteId, string? ExternalId, DateOnly Dob)>
        {
            (Guid.NewGuid(), "SC-10001", new DateOnly(2012, 5, 15)),
            (Guid.NewGuid(), "SC-10002", new DateOnly(2013, 3, 10)),
        };

        var result = await provider.ValidateTeamEntryAsync(
            athletes, (Guid.NewGuid(), "UKCC-2-12345"), "Junior", "Level2");

        result.IsEligible.Should().BeTrue();
        result.MembershipActive.Should().BeTrue();
        result.CoachCredentialsValid.Should().BeTrue();
        result.Warnings.Should().BeEmpty();
    }
}
