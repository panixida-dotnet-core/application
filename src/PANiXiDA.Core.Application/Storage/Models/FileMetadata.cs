namespace PANiXiDA.Core.Application.Storage.Models;

/// <summary>
/// Describes metadata reported by storage for an existing file without reading its content.
/// </summary>
/// <param name="ContentType">The stored media type, without application-level validation.</param>
/// <param name="Size">The non-negative size of the stored content in bytes; zero-length files are allowed.</param>
public sealed record FileMetadata(
    string ContentType,
    long Size);
