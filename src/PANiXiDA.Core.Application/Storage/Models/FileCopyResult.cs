namespace PANiXiDA.Core.Application.Storage.Models;

/// <summary>
/// Identifies the stored version produced by a completed copy.
/// </summary>
/// <param name="VersionId">
/// The opaque destination version identifier, or null when the provider does not report one.
/// </param>
public sealed record FileCopyResult(string? VersionId);
