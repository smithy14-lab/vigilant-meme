namespace CheerDeck.Infrastructure.Stubs;

using CheerDeck.Domain.Integration;

public class StubMusicLicenceProvider : IMusicLicenceProvider
{
    public Task<MusicLicenceResult> VerifyLicenceAsync(string licenceProof, string fileName, CancellationToken ct = default)
    {
        var isValid = !string.IsNullOrWhiteSpace(licenceProof);

        return Task.FromResult(new MusicLicenceResult(
            IsValid: isValid,
            LicenceId: isValid ? $"STUB-LIC-{Guid.NewGuid():N}".Substring(0, 16) : null,
            Provider: "ClicknClear (Stub)",
            ExpiryDate: isValid ? DateTime.UtcNow.AddYears(1) : null,
            Reason: isValid ? null : "No licence proof provided"));
    }
}
