using System.Text.Json;
using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.Core.Application.UnitTests.Storage.Models;

public sealed class FileCopyResultTests
{
    [Theory(DisplayName = "Copy results preserve opaque or unavailable destination version identifiers")]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("opaque/version+id==")]
    public void Serialize_WhenRoundTripped_PreservesVersionId(string? versionId)
    {
        var copied = new FileCopyResult(versionId);

        var json = JsonSerializer.Serialize(copied);
        var result = JsonSerializer.Deserialize<FileCopyResult>(json);

        result.ShouldNotBeNull();
        result.ShouldBe(copied);
    }
}
