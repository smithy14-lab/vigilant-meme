namespace CheerDeck.Tests;

using CheerDeck.Infrastructure.Stubs;
using FluentAssertions;

public class MusicLicenceTests
{
    [Fact]
    public async Task Stub_Verifies_Valid_Licence()
    {
        var provider = new StubMusicLicenceProvider();

        var result = await provider.VerifyLicenceAsync("CC-LIC-2025-001", "routine.mp3");

        result.IsValid.Should().BeTrue();
        result.LicenceId.Should().NotBeNullOrEmpty();
        result.Provider.Should().Contain("ClicknClear");
        result.ExpiryDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Stub_Rejects_Empty_Licence()
    {
        var provider = new StubMusicLicenceProvider();

        var result = await provider.VerifyLicenceAsync("", "routine.mp3");

        result.IsValid.Should().BeFalse();
        result.Reason.Should().NotBeNullOrEmpty();
    }
}
