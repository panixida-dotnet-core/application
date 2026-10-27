using System.Text.Json;
using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.Core.Application.UnitTests.Storage.Models;

public sealed class FileMetadataTests
{
    [Theory(DisplayName = "File metadata preserves content type and byte size through JSON serialization")]
    [InlineData("application/octet-stream", 0L)]
    [InlineData("image/png", 4_294_967_296L)]
    public void Serialize_WhenRoundTripped_PreservesStoredMetadata(string contentType, long size)
    {
        var metadata = new FileMetadata(contentType, size);

        var json = JsonSerializer.Serialize(metadata);
        var result = JsonSerializer.Deserialize<FileMetadata>(json);

        result.ShouldNotBeNull();
        result.ContentType.ShouldBe(contentType);
        result.Size.ShouldBe(size);
    }
}
