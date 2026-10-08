using System.Text.Json;
using PANiXiDA.Core.Application.Storage;

namespace PANiXiDA.Core.Application.UnitTests.Storage;

public sealed class PresignedDownloadUrlTests
{
    [Fact(DisplayName = "Presigned download URLs preserve signed request data without request headers")]
    public void Serialize_WhenRoundTripped_PreservesSignedRequestWithoutHeaders()
    {
        var url = new PresignedDownloadUrl(
            "https://storage.example.test/files/avatar?signature=a%2Bb%2Fc%3D&expires=123",
            new DateTimeOffset(2026, 10, 8, 12, 0, 0, TimeSpan.FromHours(4)));

        var json = JsonSerializer.Serialize(url);
        var result = JsonSerializer.Deserialize<PresignedDownloadUrl>(json);

        result.ShouldNotBeNull();
        result.Url.ShouldBe(url.Url);
        result.ExpiresAt.ShouldBe(url.ExpiresAt);
        json.ShouldNotContain("RequiredHeaders");
    }
}
