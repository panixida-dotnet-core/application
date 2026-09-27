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

    [Theory(DisplayName = "Schedulers accept their supported message types for both scheduling methods")]
    [InlineData("ICommandScheduler", "Command")]
    [InlineData("ICommandScheduler", "CommandWithResult")]
    [InlineData("IEventScheduler", "Event")]
    public void Schedule_WhenMessageMatchesContract_Compiles(string schedulerType, string messageType)
    {
        var source = CreateConsumerSource(schedulerType, messageType);

        var errors = GetCompilationErrors(source);

        errors.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Schedulers reject queries, unrelated messages, and the other message kind")]
    [InlineData("ICommandScheduler", "Query")]
    [InlineData("ICommandScheduler", "Event")]
    [InlineData("ICommandScheduler", "UnrelatedMessage")]
    [InlineData("IEventScheduler", "Query")]
    [InlineData("IEventScheduler", "Command")]
    [InlineData("IEventScheduler", "UnrelatedMessage")]
    public void Schedule_WhenMessageDoesNotMatchContract_FailsToCompile(string schedulerType, string messageType)
    {
        var source = CreateConsumerSource(schedulerType, messageType);

        var errors = GetCompilationErrors(source);

        errors.Length.ShouldBe(2);
        errors.ShouldAllBe(error => error.Id == "CS0311");
    }

    private static string CreateConsumerSource(string schedulerType, string messageType)
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
                    {{schedulerType}} scheduler,
                    {{messageType}} message,
                    CancellationToken cancellationToken)
                {
                    await scheduler.ScheduleAsync(message, TimeSpan.FromMinutes(15), cancellationToken);
                    await scheduler.ScheduleAtAsync(message, DateTimeOffset.UtcNow.AddDays(1), cancellationToken);
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

        return compilation.GetDiagnostics(TestContext.Current.CancellationToken)
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
    }
}
