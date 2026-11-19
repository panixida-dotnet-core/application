namespace PANiXiDA.Core.Application.Storage.Models;

/// <summary>
/// Describes a stored content version returned by version enumeration.
/// </summary>
public sealed record FileVersion
{
    /// <summary>
    /// Creates a content version descriptor.
    /// </summary>
    /// <param name="versionId">The non-empty, opaque version identifier.</param>
    /// <param name="lastModified">The modification time reported by storage.</param>
    public FileVersion(string versionId, DateTimeOffset lastModified)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionId);

        VersionId = versionId;
        LastModified = lastModified;
    }

    /// <summary>
    /// Gets the opaque identifier used to request this exact version.
    /// </summary>
    public string VersionId { get; }

    /// <summary>
    /// Gets the modification time reported by storage.
    /// </summary>
    public DateTimeOffset LastModified { get; }
}
