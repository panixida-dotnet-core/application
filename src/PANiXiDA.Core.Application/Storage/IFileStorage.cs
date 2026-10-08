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
}
