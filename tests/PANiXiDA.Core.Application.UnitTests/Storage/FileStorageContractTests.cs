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

    private static string CreateAdapterSource(bool includeMetadata)
    {
        var metadataMethod = includeMetadata
            ? """
                public Task<FileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken)
                    => Task.FromResult<FileMetadata?>(new FileMetadata("application/octet-stream", 4_294_967_296L));
                """
            : string.Empty;

        return $$"""
            #nullable enable
            using System;
            using System.IO;
            using System.Threading;
            using System.Threading.Tasks;
            using PANiXiDA.Core.Application.Storage;
            using PANiXiDA.Core.Application.Storage.Models;

            public sealed class Adapter : IFileStorage
            {
                {{metadataMethod}}

                public Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken)
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
