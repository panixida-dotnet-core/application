using System.Text.Json;
using PANiXiDA.Core.Application.Storage;

namespace PANiXiDA.Core.Application.UnitTests.Storage;

public sealed class PresignedFileUrlTests
{
    [Theory(DisplayName = "Presigned file URLs preserve signed request data through JSON serialization")]
    [InlineData(true)]
    [InlineData(false)]
    public void Serialize_WhenRoundTripped_PreservesSignedRequest(bool hasRequiredHeaders)
    {
        var headers = new Dictionary<string, string>();
        if (hasRequiredHeaders)
        {
            headers.Add("Content-Type", "image/png");
        }

        var url = new PresignedFileUrl(
            "https://storage.example.test/files/avatar?signature=a%2Bb%2Fc%3D&expires=123",
            new DateTimeOffset(2026, 10, 8, 12, 0, 0, TimeSpan.FromHours(4)),
            headers);

        var json = JsonSerializer.Serialize(url);
        var result = JsonSerializer.Deserialize<PresignedFileUrl>(json);

        result.ShouldNotBeNull();
        result.Url.ShouldBe(url.Url);
        result.ExpiresAt.ShouldBe(url.ExpiresAt);
        result.RequiredHeaders.Count.ShouldBe(headers.Count);
        foreach (var header in headers)
        {
            result.RequiredHeaders[header.Key].ShouldBe(header.Value);
        }
    }
}
