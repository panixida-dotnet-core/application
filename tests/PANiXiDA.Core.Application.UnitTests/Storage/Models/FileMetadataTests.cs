using System.Collections.ObjectModel;
using System.Text.Json;
using PANiXiDA.Core.Application.Storage.Constants;
using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.Core.Application.UnitTests.Storage.Models;

public sealed class FileMetadataTests
{
    [Theory(DisplayName = "File metadata preserves content type and byte size through JSON serialization")]
    [InlineData("application/octet-stream", 0L)]
    [InlineData("image/png", 4_294_967_296L)]
    public void Serialize_WhenRoundTripped_PreservesStoredMetadata(string contentType, long size)
    {
        var metadata = new FileMetadata(contentType, size, ReadOnlyDictionary<string, string>.Empty);

        var json = JsonSerializer.Serialize(metadata);
        var result = JsonSerializer.Deserialize<FileMetadata>(json);

        result.ShouldNotBeNull();
        result.ContentType.ShouldBe(contentType);
        result.Size.ShouldBe(size);
        result.Metadata.ShouldBeEmpty();
    }

    [Fact(DisplayName = "File metadata defaults the optional version while retaining the supplied metadata")]
    public void Constructor_WhenVersionIsOmitted_RetainsMetadataAndDefaultsVersionToNull()
    {
        var values = ReadOnlyDictionary<string, string>.Empty;
        var metadata = new FileMetadata("text/plain", 4, values);

        var (contentType, size, callerMetadata, versionId) = metadata;

        contentType.ShouldBe("text/plain");
        size.ShouldBe(4);
        callerMetadata.ShouldBeSameAs(values);
        versionId.ShouldBeNull();
        metadata.VersionId.ShouldBeNull();
        metadata.Metadata.ShouldBeSameAs(values);
    }

    [Theory(DisplayName = "File metadata preserves the optional version identifier through JSON serialization")]
    [InlineData(null)]
    [InlineData("opaque/version+id==")]
    [InlineData("null")]
    public void Serialize_WhenRoundTripped_PreservesStoredVersion(string? versionId)
    {
        var metadata = new FileMetadata("text/plain", 4, ReadOnlyDictionary<string, string>.Empty, versionId);

        var json = JsonSerializer.Serialize(metadata);
        var result = JsonSerializer.Deserialize<FileMetadata>(json);

        result.ShouldNotBeNull();
        result.ContentType.ShouldBe(metadata.ContentType);
        result.Size.ShouldBe(metadata.Size);
        result.VersionId.ShouldBe(versionId);
        result.Metadata.ShouldBeEmpty();
    }

    [Fact(DisplayName = "File metadata accepts an explicit empty metadata dictionary when the version is absent")]
    public void Deserialize_WhenCallerMetadataIsEmpty_PreservesStoredMetadata()
    {
        const string json = """{"ContentType":"text/plain","Size":4,"Metadata":{}}""";

        var metadata = JsonSerializer.Deserialize<FileMetadata>(json);

        metadata.ShouldNotBeNull();
        metadata.ContentType.ShouldBe("text/plain");
        metadata.Size.ShouldBe(4);
        metadata.VersionId.ShouldBeNull();
        metadata.Metadata.ShouldBeEmpty();
    }

    [Fact(DisplayName = "File metadata preserves caller metadata through JSON serialization")]
    public void Serialize_WhenCallerMetadataExists_PreservesStoredMetadata()
    {
        var metadata = new FileMetadata(
            "text/plain", 4,
            new Dictionary<string, string> { [FileMetadataNames.OperationAttemptId] = "attempt-1" },
            "opaque/version+id==");

        var json = JsonSerializer.Serialize(metadata);
        var result = JsonSerializer.Deserialize<FileMetadata>(json);

        result.ShouldNotBeNull();
        result.ContentType.ShouldBe(metadata.ContentType);
        result.Size.ShouldBe(metadata.Size);
        result.VersionId.ShouldBe(metadata.VersionId);
        result.Metadata.Count.ShouldBe(1);
        result.Metadata["operation-attempt-id"].ShouldBe("attempt-1");
    }

    [Fact(DisplayName = "File metadata copies retain the supplied caller metadata dictionary")]
    public void With_WhenSizeChanges_RetainsCallerMetadata()
    {
        var values = new Dictionary<string, string> { ["update-id"] = "attempt-1" };
        var metadata = new FileMetadata("text/plain", 4, values, "version-1");

        var copy = metadata with { Size = 8 };

        metadata.Size.ShouldBe(4);
        metadata.Metadata.ShouldBeSameAs(values);
        copy.Size.ShouldBe(8);
        copy.Metadata.ShouldBeSameAs(values);
        copy.Metadata["update-id"].ShouldBe("attempt-1");
    }
}
