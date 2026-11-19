using System.Collections.ObjectModel;

namespace PANiXiDA.Core.Application.Storage.Models;

/// <summary>
/// Describes metadata reported by storage for an existing file without reading its content.
/// </summary>
/// <param name="ContentType">The stored media type, without application-level validation.</param>
/// <param name="Size">The non-negative size of the stored content in bytes; zero-length files are allowed.</param>
/// <param name="VersionId">The opaque version identifier reported by storage, or null when unavailable.</param>
/// <param name="Metadata">Caller metadata without header prefixes, or null for an empty collection.</param>
public sealed record FileMetadata(
    string ContentType,
    long Size,
    string? VersionId = null,
    IReadOnlyDictionary<string, string>? Metadata = null)
{
    /// <summary>
    /// Gets an immutable snapshot of caller metadata with case-insensitive names and without header prefixes.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; } = Metadata is null || Metadata.Count == 0
        ? ReadOnlyDictionary<string, string>.Empty
        : new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Deconstructs the content type, size and version identifier.
    /// </summary>
    /// <param name="contentType">The stored media type.</param>
    /// <param name="size">The stored size in bytes.</param>
    /// <param name="versionId">The opaque version identifier, or null when unavailable.</param>
    public void Deconstruct(out string contentType, out long size, out string? versionId)
    {
        contentType = ContentType;
        size = Size;
        versionId = VersionId;
    }
}
