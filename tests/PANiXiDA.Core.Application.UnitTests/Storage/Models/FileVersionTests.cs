using System.Text.Json;
using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.Core.Application.UnitTests.Storage.Models;

public sealed class FileVersionTests
{
    [Theory(DisplayName = "File versions preserve opaque identifiers and provider modification times")]
    [InlineData("null")]
    [InlineData("opaque/version+id==")]
    public void Serialize_WhenRoundTripped_PreservesVersion(string versionId)
    {
        var timestamp = new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.FromHours(4));
        var version = new FileVersion(versionId, timestamp);

        var json = JsonSerializer.Serialize(version);
        var result = JsonSerializer.Deserialize<FileVersion>(json);

        result.ShouldNotBeNull();
        result.ShouldBe(version);
        result.LastModified.Offset.ShouldBe(timestamp.Offset);
    }

    [Fact(DisplayName = "File version copies preserve the stored identifier and modification time")]
    public void With_WhenCopied_PreservesVersion()
    {
        var version = new FileVersion("opaque/version+id==", DateTimeOffset.UtcNow);

        var copy = version with { };
        var (versionId, lastModified) = copy;

        copy.ShouldNotBeSameAs(version);
        copy.ShouldBe(version);
        versionId.ShouldBe(version.VersionId);
        lastModified.ShouldBe(version.LastModified);
    }
}
