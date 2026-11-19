namespace PANiXiDA.Core.Application.Storage.Models;

/// <summary>
/// Identifies the stored version produced by a completed upload.
/// </summary>
/// <param name="VersionId">The opaque version identifier, or null when the provider does not report one.</param>
public sealed record FileUploadResult(string? VersionId);
