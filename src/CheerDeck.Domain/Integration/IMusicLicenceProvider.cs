namespace CheerDeck.Domain.Integration;

public record MusicLicenceResult(
    bool IsValid,
    string? LicenceId,
    string? Provider,
    DateTime? ExpiryDate,
    string? Reason);

public interface IMusicLicenceProvider
{
    Task<MusicLicenceResult> VerifyLicenceAsync(string licenceProof, string fileName, CancellationToken ct = default);
}
