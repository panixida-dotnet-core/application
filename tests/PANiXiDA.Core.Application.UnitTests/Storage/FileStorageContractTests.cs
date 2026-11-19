using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PANiXiDA.Core.Application.UnitTests.Storage;

public sealed class FileStorageContractTests
{
    private static readonly ImmutableArray<MetadataReference> References =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
    ];

    [Fact(DisplayName = "File storage consumers can inspect nullable metadata without downloading content")]
    public void GetMetadata_WhenAdapterImplementsMetadata_Compiles()
    {
        var source = CreateAdapterSource(includeMetadata: true);

        var errors = GetCompilationErrors(source);

        errors.ShouldBeEmpty();
    }

    [Fact(DisplayName = "File storage adapters must implement the metadata operation")]
    public void GetMetadata_WhenAdapterOmitsMetadata_FailsToCompile()
    {
        var source = CreateAdapterSource(includeMetadata: false);

        var errors = GetCompilationErrors(source);

        errors.ShouldHaveSingleItem();
        errors[0].Id.ShouldBe("CS0535");
        errors[0].GetMessage().ShouldContain("GetMetadataAsync");
    }

    [Theory(DisplayName = "File storage adapters must implement each version-aware operation")]
    [InlineData("upload", "UploadAsync")]
    [InlineData("download", "DownloadAsync")]
    [InlineData("metadata", "GetMetadataAsync")]
    [InlineData("versions", "GetVersionsAsync")]
    [InlineData("download-url", "GetPresignedDownloadUrlAsync")]
    public void VersionOperation_WhenAdapterOmitsIt_FailsToCompile(string operation, string methodName)
    {
        var source = CreateAdapterSource(includeMetadata: true, omittedVersionOperation: operation);

        var errors = GetCompilationErrors(source);

        errors.ShouldHaveSingleItem();
        errors[0].Id.ShouldBe("CS0535");
        errors[0].GetMessage().ShouldContain(methodName);
    }

    private static string CreateAdapterSource(bool includeMetadata, string? omittedVersionOperation = null)
    {
        var metadataMethod = includeMetadata
            ? """
                public Task<FileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken)
                    => Task.FromResult<FileMetadata?>(new FileMetadata("application/octet-stream", 4_294_967_296L));
                """
            : string.Empty;

        var versionMethods = new Dictionary<string, string>
        {
            ["upload"] = """
                public Task<FileUploadResult> UploadAsync(
                    string key, Stream content, string contentType,
                    IReadOnlyDictionary<string, string> metadata, CancellationToken cancellationToken)
                    => Task.FromResult(new FileUploadResult("version-1"));
                """,
            ["download"] = """
                public Task<Stream> DownloadAsync(
                    string key, string versionId, CancellationToken cancellationToken)
                    => throw new NotSupportedException();
                """,
            ["metadata"] = """
                public Task<FileMetadata?> GetMetadataAsync(
                    string key, string versionId, CancellationToken cancellationToken)
                    => Task.FromResult<FileMetadata?>(new FileMetadata(
                        "text/plain", 4, versionId,
                        new Dictionary<string, string> { ["operation-id"] = "attempt-1" }));
                """,
            ["versions"] = """
                public IAsyncEnumerable<FileVersion> GetVersionsAsync(
                    string key, CancellationToken cancellationToken)
                    => throw new NotSupportedException();
                """,
            ["download-url"] = """
                public Task<PresignedDownloadUrl> GetPresignedDownloadUrlAsync(
                    string key, string versionId, string fileName,
                    string contentType, CancellationToken cancellationToken)
                    => throw new NotSupportedException();
                """
        };
        var versionImplementations = string.Join(
            Environment.NewLine,
            versionMethods.Where(method => method.Key != omittedVersionOperation).Select(method => method.Value));

        return $"""
            #nullable enable
            using System;
            using System.IO;
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;
            using PANiXiDA.Core.Application.Storage;
            using PANiXiDA.Core.Application.Storage.Models;

            public sealed class Adapter : IFileStorage
            {
                {{metadataMethod}}
                {{versionImplementations}}

                public Task UploadAsync(
                    string key, Stream content, string contentType, CancellationToken cancellationToken)
                    => throw new NotSupportedException();
                public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken)
                    => throw new NotSupportedException();
                public Task DeleteAsync(string key, CancellationToken cancellationToken)
                    => throw new NotSupportedException();
                public Task<PresignedUploadUrl> GetPresignedUploadUrlAsync(
                    string key, string contentType, long size, CancellationToken cancellationToken)
                    => throw new NotSupportedException();
                public Task<PresignedDownloadUrl> GetPresignedDownloadUrlAsync(
                    string key, string fileName, string contentType, CancellationToken cancellationToken)
                    => throw new NotSupportedException();
            }

            public static class Consumer
            {
                public static async Task<string?> ReadUploadedVersionAsync(
                    IFileStorage storage, string key, Stream content, CancellationToken cancellationToken)
                {
                    var uploaded = await storage.UploadAsync(
                        key, content, "text/plain",
                        new Dictionary<string, string> { ["operation-id"] = "attempt-1" }, cancellationToken);
                    if (uploaded.VersionId is not { } versionId)
                    {
                        return null;
                    }

                    await using var downloaded = await storage.DownloadAsync(key, versionId, cancellationToken);
                    var metadata = await storage.GetMetadataAsync(key, versionId, cancellationToken);
                    await storage.GetPresignedDownloadUrlAsync(
                        key, versionId, "file.txt", "text/plain", cancellationToken);
                    await foreach (var version in storage.GetVersionsAsync(key, cancellationToken))
                    {
                        if (version.VersionId == versionId)
                        {
                            return metadata?.Metadata["operation-id"];
                        }
                    }

                    return null;
                }

                public static async Task<long?> ReadSizeAsync(
                    IFileStorage storage, string key, CancellationToken cancellationToken)
                {
                    FileMetadata? metadata = await storage.GetMetadataAsync(key, cancellationToken);
                    return metadata?.Size;
                }
            }
            """;
    }

    private static Diagnostic[] GetCompilationErrors(string source)
    {
        var compilation = CSharpCompilation.Create(
            "FileStorageConsumer",
            [CSharpSyntaxTree.ParseText(source)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return
        [
            .. compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
        ];
    }
}
