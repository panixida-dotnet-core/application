using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.Core.Application.UnitTests.Storage;

public sealed class VersionedFileStorageContractTests
{
    [Theory(DisplayName = "Legacy adapters reject unsupported version operations without falling back")]
    [InlineData("upload")]
    [InlineData("metadata")]
    [InlineData("versions")]
    [InlineData("download-url")]
    public async Task VersionOperations_WhenAdapterDoesNotImplementThem_ThrowNotSupportedException(string operation)
    {
        IFileStorage storage = new LegacyFileStorage();
        var token = TestContext.Current.CancellationToken;
        await using var content = new MemoryStream([1]);

        await Should.ThrowAsync<NotSupportedException>(ExecuteAsync);

        async Task ExecuteAsync()
        {
            switch (operation)
            {
                case "upload":
                    await storage.UploadAsync(
                        "avatar/key", content, "text/plain",
                        new Dictionary<string, string> { ["update-id"] = "attempt-1" }, token);
                    break;
                case "metadata":
                    await storage.GetMetadataAsync("avatar/key", "version-1", token);
                    break;
                case "versions":
                    await foreach (var version in storage.GetVersionsAsync("avatar/key", token))
                    {
                        _ = version;
                    }
                    break;
                case "download-url":
                    await storage.GetPresignedDownloadUrlAsync(
                        "avatar/key", "version-1", "file.txt", "text/plain", token);
                    break;
            }
        }
    }

    private sealed class LegacyFileStorage : IFileStorage
    {
        private const string LegacyOperationMessage =
            "A legacy operation must not replace a version-aware operation.";

        public Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(LegacyOperationMessage);
        }

        public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(LegacyOperationMessage);
        }

        public Task<FileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(LegacyOperationMessage);
        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(LegacyOperationMessage);
        }

        public Task<PresignedUploadUrl> GetPresignedUploadUrlAsync(
            string key, string contentType, long size, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(LegacyOperationMessage);
        }

        public Task<PresignedDownloadUrl> GetPresignedDownloadUrlAsync(
            string key, string fileName, string contentType, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(LegacyOperationMessage);
        }
    }
}
