using System.Text.Json;
using PANiXiDA.Core.Application.Storage.Models;

namespace PANiXiDA.Core.Application.UnitTests.Storage.Models;

public sealed class FileUploadResultTests
{
    [Theory(DisplayName = "Upload results preserve opaque or unavailable version identifiers")]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("opaque/version+id==")]
    public void Serialize_WhenRoundTripped_PreservesVersionId(string? versionId)
    {
        var uploaded = new FileUploadResult(versionId);

        var json = JsonSerializer.Serialize(uploaded);
        var result = JsonSerializer.Deserialize<FileUploadResult>(json);

        result.ShouldNotBeNull();
        result.ShouldBe(uploaded);
    }
}
