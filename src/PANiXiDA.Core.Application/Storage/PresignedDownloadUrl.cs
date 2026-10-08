namespace PANiXiDA.Core.Application.Storage;

/// <summary>
/// Describes a signed URL for direct file download.
/// </summary>
/// <param name="Url">The absolute URL that must be used without modification.</param>
/// <param name="ExpiresAt">The configured expiration time; access may end earlier if credentials expire or are revoked.</param>
public sealed record PresignedDownloadUrl(
    string Url,
    DateTimeOffset ExpiresAt);
