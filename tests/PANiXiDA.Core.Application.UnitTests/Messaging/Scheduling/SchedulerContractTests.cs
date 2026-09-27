using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Scheduling;

public sealed class SchedulerContractTests
{
    private static readonly ImmutableArray<MetadataReference> References =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
    ];

    [Theory(DisplayName = "Scheduler accepts supported message types for delayed and timed delivery")]
    [InlineData("Command", "Command")]
    [InlineData("Command", "CommandWithResult")]
    [InlineData("Event", "Event")]
    public void Schedule_WhenMessageMatchesContract_Compiles(string messageKind, string messageType)
    {
        var source = CreateConsumerSource(messageKind, messageType);

        var errors = GetCompilationErrors(source);

        errors.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Scheduler rejects queries, unrelated messages, and the other message kind")]
    [InlineData("Command", "Query")]
    [InlineData("Command", "Event")]
    [InlineData("Command", "UnrelatedMessage")]
    [InlineData("Event", "Query")]
    [InlineData("Event", "Command")]
    [InlineData("Event", "UnrelatedMessage")]
    public void Schedule_WhenMessageDoesNotMatchContract_FailsToCompile(string messageKind, string messageType)
    {
        var source = CreateConsumerSource(messageKind, messageType);

        var errors = GetCompilationErrors(source);

        errors.Length.ShouldBe(2);
        errors.ShouldAllBe(error => error.Id == "CS0311");
    }

    private static string CreateConsumerSource(string messageKind, string messageType)
    {
        return $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
            using PANiXiDA.Core.Application.Messaging.Scheduling;
            using PANiXiDA.Core.Domain.DomainEvents;
            using PANiXiDA.Core.ResultPattern;

            public sealed record Command : ICommand<Result>;
            public sealed record CommandWithResult : ICommand<Result<Guid>>;
            public sealed record Query : IQuery<Result>;
            public sealed record Event : DomainEvent;
            public sealed record UnrelatedMessage;

            public static class Consumer
            {
                public static async Task ScheduleAsync(
                    IScheduler scheduler,
                    {{messageType}} message,
                    CancellationToken cancellationToken)
                {
                    await scheduler.Schedule{{messageKind}}Async(message, TimeSpan.FromMinutes(15), cancellationToken);
                    await scheduler.Schedule{{messageKind}}AtAsync(message, DateTimeOffset.UtcNow.AddDays(1), cancellationToken);
                }
            }
            """;
    }

    private static Diagnostic[] GetCompilationErrors(string source)
    {
        var compilation = CSharpCompilation.Create(
            "SchedulerConsumer",
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
