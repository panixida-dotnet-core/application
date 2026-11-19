using System.Text.Json;
using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.Core.Application.UnitTests.Storage.Models;

public sealed class FileVersionTests
{
    [Theory(DisplayName = "File versions reject missing identifiers")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenVersionIdIsMissing_ThrowsArgumentException(string? versionId)
    {
        Should.Throw<ArgumentException>(() => new FileVersion(versionId!, DateTimeOffset.UtcNow));
    }

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
}
