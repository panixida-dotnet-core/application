namespace PANiXiDA.Core.Application.Storage.Models;

/// <summary>
/// Describes a stored content version returned by version enumeration.
/// </summary>
/// <param name="VersionId">The opaque identifier used to request this exact version.</param>
/// <param name="LastModified">The modification time reported by storage.</param>
public sealed record FileVersion(
    string VersionId,
    DateTimeOffset LastModified);
