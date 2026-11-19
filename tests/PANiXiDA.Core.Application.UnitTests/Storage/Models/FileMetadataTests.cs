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

    [Fact(DisplayName = "File metadata defaults the optional constructor version to null")]
    public void Constructor_WhenVersionIsOmitted_DefaultsToNull()
    {
        var metadata = new FileMetadata("text/plain", 4);

        var (contentType, size, versionId) = metadata;

        contentType.ShouldBe("text/plain");
        size.ShouldBe(4);
        versionId.ShouldBeNull();
        metadata.VersionId.ShouldBeNull();
    }

    [Theory(DisplayName = "File metadata preserves the optional version identifier through JSON serialization")]
    [InlineData(null)]
    [InlineData("opaque/version+id==")]
    [InlineData("null")]
    public void Serialize_WhenRoundTripped_PreservesStoredVersion(string? versionId)
    {
        var metadata = new FileMetadata("text/plain", 4, versionId);

        var json = JsonSerializer.Serialize(metadata);
        var result = JsonSerializer.Deserialize<FileMetadata>(json);

        result.ShouldNotBeNull();
        result.ShouldBe(metadata);
        result.VersionId.ShouldBe(versionId);
    }

    [Fact(DisplayName = "File metadata accepts existing JSON when version identifier is absent")]
    public void Deserialize_WhenVersionIsAbsent_PreservesStoredMetadata()
    {
        const string json = """{"ContentType":"text/plain","Size":4}""";

        var metadata = JsonSerializer.Deserialize<FileMetadata>(json);

        metadata.ShouldNotBeNull();
        metadata.ContentType.ShouldBe("text/plain");
        metadata.Size.ShouldBe(4);
        metadata.VersionId.ShouldBeNull();
    }
}
