using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.Core.Application.Storage;

/// <summary>
/// Stores file content using caller-supplied keys.
/// </summary>
/// <remarks>
/// Keys are case-sensitive and must not be null, empty, or whitespace.
/// Key generation belongs to the consuming application or domain.
/// Infrastructure implementations configure the storage location and optional key prefix.
/// </remarks>
public interface IFileStorage
{
    /// <summary>
    /// Uploads content, replacing any existing file at the same key.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="content">A readable stream consumed from its current position and left open.</param>
    /// <param name="contentType">The non-empty media type of the content.</param>
    /// <param name="cancellationToken">The token used to cancel the upload.</param>
    /// <returns>A task that completes when the upload finishes.</returns>
    Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Uploads content with caller-defined metadata and reports the version created by this upload.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="content">A readable stream consumed from its current position and left open.</param>
    /// <param name="contentType">The non-empty media type of the content.</param>
    /// <param name="metadata">Metadata names and values without provider-specific header prefixes.</param>
    /// <param name="cancellationToken">The token used to cancel the upload.</param>
    /// <returns>The uploaded version identifier, or null when the provider does not report one.</returns>
    /// <remarks>
    /// Adapters validate metadata against provider constraints and must not silently discard it.
    /// The returned version identifies this upload, not a subsequent read of the current file.
    /// Cancellation or a lost response does not prove that storage rejected the upload.
    /// </remarks>
    Task<FileUploadResult> UploadAsync(
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken);

    /// <summary>
    /// Opens a readable stream for the file at the specified key.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="cancellationToken">The token used to cancel opening the stream.</param>
    /// <returns>A readable stream that the caller must dispose. Seeking is not guaranteed.</returns>
    /// <exception cref="FileNotFoundException">No file exists at the specified key.</exception>
    Task<Stream> DownloadAsync(
        string key,
        CancellationToken cancellationToken);

    /// <summary>
    /// Opens a readable stream for exactly the specified file version.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="versionId">The non-empty, opaque version identifier.</param>
    /// <param name="cancellationToken">The token used to cancel opening the stream.</param>
    /// <returns>A readable stream that the caller must dispose. Seeking is not guaranteed.</returns>
    /// <exception cref="FileNotFoundException">The specified key or version does not exist.</exception>
    /// <remarks>
    /// Adapters must not fall back to the current version when the requested version is absent.
    /// Access failures and other storage errors propagate to the caller.
    /// </remarks>
    Task<Stream> DownloadAsync(
        string key,
        string versionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads the stored file's metadata without downloading its content.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="cancellationToken">The token used to cancel the metadata request.</param>
    /// <returns>
    /// The stored content type, size, version and caller-defined metadata, or null if the file does not exist.
    /// </returns>
    /// <exception cref="ArgumentException">The key is null, empty, whitespace, or invalid for the provider.</exception>
    /// <exception cref="OperationCanceledException">The cancellation token is canceled.</exception>
    /// <remarks>
    /// Only an absent file produces a null result. Authorization failures, an unavailable storage location,
    /// and other storage errors must propagate to the caller.
    /// Metadata describes the stored object; it does not verify its content against application expectations.
    /// </remarks>
    Task<FileMetadata?> GetMetadataAsync(
        string key,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads metadata for exactly the specified file version without downloading content.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="versionId">The non-empty, opaque version identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the metadata request.</param>
    /// <returns>The version's metadata, or null if that key or version does not exist.</returns>
    /// <remarks>
    /// Adapters must not fall back to the current version when the requested version is absent.
    /// Authorization failures and other storage errors propagate to the caller.
    /// </remarks>
    Task<FileMetadata?> GetMetadataAsync(
        string key,
        string versionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Enumerates stored content versions of exactly the specified key.
    /// </summary>
    /// <param name="key">The exact file key, not a prefix.</param>
    /// <param name="cancellationToken">The token used to cancel enumeration and provider requests.</param>
    /// <returns>Content versions, excluding deletion markers; an absent key produces an empty sequence.</returns>
    /// <remarks>
    /// Adapters fetch provider pages as enumeration advances rather than buffering all versions.
    /// Historical content versions remain enumerable when the current file is deleted.
    /// No ordering is guaranteed. Access failures and other storage errors propagate.
    /// Version entries do not include caller-defined metadata; inspect a specific version when needed.
    /// </remarks>
    IAsyncEnumerable<FileVersion> GetVersionsAsync(
        string key,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the file at the specified key, succeeding if the file does not exist.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="cancellationToken">The token used to cancel deletion.</param>
    /// <returns>A task that completes when deletion finishes.</returns>
    Task DeleteAsync(
        string key,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a signed URL for uploading file content with an HTTP PUT request.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="contentType">The non-empty media type required for the upload.</param>
    /// <param name="size">The expected non-negative content length in bytes.</param>
    /// <param name="cancellationToken">The token used to cancel URL generation.</param>
    /// <returns>The signed URL, its expiration time, and required request headers.</returns>
    /// <remarks>
    /// The infrastructure implementation configures the URL lifetime.
    /// Generating a URL does not upload content or confirm a completed upload.
    /// </remarks>
    Task<PresignedUploadUrl> GetPresignedUploadUrlAsync(
        string key,
        string contentType,
        long size,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a signed URL for downloading file content with an HTTP GET request.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="fileName">The non-empty file name suggested to the client when saving the download.</param>
    /// <param name="contentType">The non-empty media type returned in the download response.</param>
    /// <param name="cancellationToken">The token used to cancel URL generation.</param>
    /// <returns>The signed URL and its expiration time.</returns>
    /// <remarks>
    /// The infrastructure implementation configures the URL lifetime.
    /// Generating a URL does not verify that the file exists or authorize the caller.
    /// </remarks>
    Task<PresignedDownloadUrl> GetPresignedDownloadUrlAsync(
        string key,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a signed GET URL for exactly the specified file version.
    /// </summary>
    /// <param name="key">The file key within the configured storage location.</param>
    /// <param name="versionId">The non-empty, opaque version identifier.</param>
    /// <param name="fileName">The non-empty file name suggested when saving the download.</param>
    /// <param name="contentType">The non-empty media type returned in the download response.</param>
    /// <param name="cancellationToken">The token used to cancel URL generation.</param>
    /// <returns>The signed URL and its expiration time.</returns>
    /// <remarks>
    /// The URL must remain bound to the requested version even if the current file changes.
    /// Generating a URL does not verify that the version exists or authorize the caller.
    /// </remarks>
    Task<PresignedDownloadUrl> GetPresignedDownloadUrlAsync(
        string key,
        string versionId,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);
}
