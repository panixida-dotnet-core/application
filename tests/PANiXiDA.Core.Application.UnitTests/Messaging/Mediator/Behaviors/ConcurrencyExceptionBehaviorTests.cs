using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors;

public sealed class ConcurrencyExceptionBehaviorTests
{
    [Theory(DisplayName = "ConcurrencyExceptionBehavior offers a conflict for requests returning Result")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleExceptionAsync_WhenResultIsMissingOrSuccessful_ReturnsConflict(bool hasResult)
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var exception = new ConcurrencyException("Private provider diagnostics.");
        var originalResult = hasResult ? Result.Success() : null;

        var result = await behavior.HandleExceptionAsync(
            new TestCommand(), originalResult, exception, TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        var error = result.Errors.ShouldHaveSingleItem();
        error.Type.ShouldBe(ErrorType.Conflict);
        error.Message.ShouldBe("The entity was modified or deleted. Reload it and try again.");
        error.Metadata.ShouldBeEmpty();
        originalResult?.IsSuccess.ShouldBeTrue();
    }

    [Theory(DisplayName = "ConcurrencyExceptionBehavior supports requests returning Result of T")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleExceptionAsync_WhenRequestReturnsGenericResult_ReturnsConflict(bool hasResult)
    {
        var behavior = new ConcurrencyExceptionBehavior<ValueCommand, Result<string>>();
        var originalResult = hasResult ? Result.Success("saved value") : null;

        var result = await behavior.HandleExceptionAsync(
            new ValueCommand(), originalResult, new ConcurrencyException(), TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
        originalResult?.Value.ShouldBe("saved value");
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior leaves existing failure results unchanged")]
    public async Task HandleExceptionAsync_WhenResultAlreadyFailed_DeclinesHandling()
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var error = Error.Validation("The order name is required.");
        var originalResult = Result.Failure(error);

        var result = await behavior.HandleExceptionAsync(
            new TestCommand(), originalResult, new ConcurrencyException(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
        originalResult.Errors.ShouldHaveSingleItem().ShouldBeSameAs(error);
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior leaves generic failure results unchanged")]
    public async Task HandleExceptionAsync_WhenGenericResultAlreadyFailed_DeclinesHandling()
    {
        var behavior = new ConcurrencyExceptionBehavior<ValueCommand, Result<string>>();
        var error = Error.NotFound("The order was not found.");
        var originalResult = Result.Failure<string>(error);

        var result = await behavior.HandleExceptionAsync(
            new ValueCommand(), originalResult, new ConcurrencyException(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
        originalResult.Errors.ShouldHaveSingleItem().ShouldBeSameAs(error);
    }

    [Theory(DisplayName = "ConcurrencyExceptionBehavior declines unrelated, wrapped, aggregate and cancellation failures")]
    [InlineData("unexpected")]
    [InlineData("canceled")]
    [InlineData("task-canceled")]
    [InlineData("wrapped")]
    [InlineData("aggregate")]
    [InlineData("mixed-aggregate")]
    public async Task HandleExceptionAsync_WhenExceptionIsNotDirectConcurrency_DeclinesHandling(string scenario)
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        Exception exception = scenario switch
        {
            "unexpected" => new InvalidOperationException("Unexpected failure."),
            "canceled" => new OperationCanceledException(),
            "task-canceled" => new TaskCanceledException(),
            "wrapped" => new InvalidOperationException("Wrapper failure.", new ConcurrencyException()),
            "aggregate" => new AggregateException(new ConcurrencyException()),
            "mixed-aggregate" => new AggregateException(new ConcurrencyException(), new InvalidOperationException("Rollback failed.")),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown exception scenario.")
        };

        var result = await behavior.HandleExceptionAsync(
            new TestCommand(), null, exception, TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior rejects a null request")]
    public async Task HandleExceptionAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();

        var exception = await Should.ThrowAsync<ArgumentNullException>(() => behavior.HandleExceptionAsync(
            null!, null, new ConcurrencyException(), TestContext.Current.CancellationToken));

        exception.ParamName.ShouldBe("request");
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior rejects a null exception")]
    public async Task HandleExceptionAsync_WhenExceptionIsNull_ThrowsArgumentNullException()
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();

        var exception = await Should.ThrowAsync<ArgumentNullException>(() => behavior.HandleExceptionAsync(
            new TestCommand(), null, null!, TestContext.Current.CancellationToken));

        exception.ParamName.ShouldBe("exception");
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior maps a conflict after successful transaction cleanup")]
    public async Task HandleExceptionAsync_WhenTransactionCleanupSucceeds_ReturnsConflict()
    {
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = true };
        var cleanup = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var request = new TestCommand();
        var exception = new ConcurrencyException();

        await cleanup.FinallyAsync(request, null, exception, TestContext.Current.CancellationToken);
        var result = await behavior.HandleExceptionAsync(request, null, exception, TestContext.Current.CancellationToken);

        unitOfWork.RollbackTransactionCalls.ShouldBe(1);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
        result.ShouldNotBeNull().FirstError.Type.ShouldBe(ErrorType.Conflict);
    }

    [Theory(DisplayName = "ConcurrencyExceptionBehavior does not hide transaction cleanup failures")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task HandleExceptionAsync_WhenCleanupAggregatesFailures_DeclinesHandling(bool failRollback, bool failDispose)
    {
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true,
            RollbackException = failRollback ? new InvalidOperationException("Rollback failed.") : null,
            DisposeException = failDispose ? new InvalidOperationException("Dispose failed.") : null
        };
        var cleanup = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var request = new TestCommand();
        var originalException = new ConcurrencyException();

        var exception = await Should.ThrowAsync<AggregateException>(() => cleanup.FinallyAsync(
            request, null, originalException, TestContext.Current.CancellationToken));
        var result = await behavior.HandleExceptionAsync(request, null, exception, TestContext.Current.CancellationToken);

        result.ShouldBeNull();
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
        exception.InnerExceptions.ShouldBe(new[]
        {
            originalException,
            unitOfWork.RollbackException,
            unitOfWork.DisposeException
        }.OfType<Exception>());
    }

    private sealed record ValueCommand : ICommand<Result<string>>;
}
