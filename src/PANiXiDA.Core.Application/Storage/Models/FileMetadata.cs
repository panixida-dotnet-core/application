using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace PANiXiDA.Core.Application.Storage.Models;

/// <summary>
/// Describes metadata reported by storage for an existing file without reading its content.
/// </summary>
/// <param name="ContentType">The stored media type, without application-level validation.</param>
/// <param name="Size">The non-negative size of the stored content in bytes; zero-length files are allowed.</param>
/// <param name="VersionId">The opaque version identifier reported by storage, or null when unavailable.</param>
public sealed record FileMetadata(
    string ContentType,
    long Size,
    string? VersionId = null)
{
    /// <summary>
    /// Creates stored file metadata with an immutable snapshot of caller-defined metadata.
    /// </summary>
    /// <param name="contentType">The stored media type.</param>
    /// <param name="size">The stored size in bytes.</param>
    /// <param name="versionId">The opaque version identifier, or null when unavailable.</param>
    /// <param name="metadata">Metadata without header prefixes, or null for an empty collection.</param>
    [JsonConstructor]
    public FileMetadata(
        string contentType,
        long size,
        string? versionId,
        IReadOnlyDictionary<string, string>? metadata)
        : this(contentType, size, versionId)
    {
        Metadata = metadata is null || metadata.Count == 0
            ? ReadOnlyDictionary<string, string>.Empty
            : new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets caller-defined metadata with case-insensitive names and without provider header prefixes.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; } = ReadOnlyDictionary<string, string>.Empty;
}
