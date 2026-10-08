namespace PANiXiDA.Core.Application.Storage;

/// <summary>
/// Describes a signed URL for direct file transfer.
/// </summary>
/// <param name="Url">The absolute URL that must be used without modification.</param>
/// <param name="ExpiresAt">The configured expiration time; access may end earlier if credentials expire or are revoked.</param>
/// <param name="RequiredHeaders">The required request headers, or an empty dictionary when none are required.</param>
public sealed record PresignedFileUrl(
    string Url,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> RequiredHeaders);
