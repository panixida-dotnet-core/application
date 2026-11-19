namespace PANiXiDA.Core.Application.Storage.Models;

/// <summary>
/// Describes metadata reported by storage for an existing file without reading its content.
/// </summary>
/// <param name="ContentType">The stored media type, without application-level validation.</param>
/// <param name="Size">The non-negative size of the stored content in bytes; zero-length files are allowed.</param>
/// <param name="Metadata">Caller metadata without header prefixes; pass an empty dictionary when absent.</param>
/// <param name="VersionId">The opaque version identifier reported by storage, or null when unavailable.</param>
public sealed record FileMetadata(
    string ContentType,
    long Size,
    IReadOnlyDictionary<string, string> Metadata,
    string? VersionId = null);
