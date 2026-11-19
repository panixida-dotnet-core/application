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
        metadata.Metadata.ShouldBeEmpty();
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

    [Fact(DisplayName = "File metadata snapshots caller metadata and supports case-insensitive lookup")]
    public void Constructor_WhenCallerMetadataChanges_PreservesSnapshot()
    {
        var values = new Dictionary<string, string> { ["update-id"] = "attempt-1" };

        var metadata = new FileMetadata("text/plain", 4, "version-1", values);
        values["update-id"] = "attempt-2";
        values.Clear();

        metadata.Metadata.Count.ShouldBe(1);
        metadata.Metadata["UPDATE-ID"].ShouldBe("attempt-1");
        Should.Throw<NotSupportedException>(() =>
            ((IDictionary<string, string>)metadata.Metadata)["update-id"] = "attempt-3");
    }

    [Fact(DisplayName = "File metadata preserves caller metadata through JSON serialization")]
    public void Serialize_WhenCallerMetadataExists_PreservesStoredMetadata()
    {
        var metadata = new FileMetadata(
            "text/plain", 4, "opaque/version+id==",
            new Dictionary<string, string> { ["update-id"] = "attempt-1" });

        var json = JsonSerializer.Serialize(metadata);
        var result = JsonSerializer.Deserialize<FileMetadata>(json);

        result.ShouldNotBeNull();
        result.ContentType.ShouldBe(metadata.ContentType);
        result.Size.ShouldBe(metadata.Size);
        result.VersionId.ShouldBe(metadata.VersionId);
        result.Metadata.Count.ShouldBe(1);
        result.Metadata["UPDATE-ID"].ShouldBe("attempt-1");
    }

    [Fact(DisplayName = "File metadata treats absent caller metadata as an empty collection")]
    public void Constructor_WhenCallerMetadataIsNull_ReturnsEmptyCollection()
    {
        var metadata = new FileMetadata("text/plain", 4, "version-1", metadata: null);

        metadata.Metadata.ShouldBeEmpty();
    }
}
