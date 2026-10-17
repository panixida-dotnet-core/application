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
    public async Task FinallyAsync_WhenResultIsMissingOrSuccessful_ReturnsConflict(bool hasResult)
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var exception = new ConcurrencyException("Private provider diagnostics.");
        var originalResult = hasResult ? Result.Success() : null;

        var result = await behavior.FinallyAsync(
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
    public async Task FinallyAsync_WhenRequestReturnsGenericResult_ReturnsConflict(bool hasResult)
    {
        var behavior = new ConcurrencyExceptionBehavior<ValueCommand, Result<string>>();
        var originalResult = hasResult ? Result.Success("saved value") : null;

        var result = await behavior.FinallyAsync(
            new ValueCommand(), originalResult, new ConcurrencyException(), TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
        originalResult?.Value.ShouldBe("saved value");
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior leaves existing failure results unchanged")]
    public async Task FinallyAsync_WhenResultAlreadyFailed_ReturnsOriginalResult()
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var error = Error.Validation("The order name is required.");
        var originalResult = Result.Failure(error);

        var result = await behavior.FinallyAsync(
            new TestCommand(), originalResult, new ConcurrencyException(), TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(originalResult);
        originalResult.Errors.ShouldHaveSingleItem().ShouldBeSameAs(error);
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior leaves generic failure results unchanged")]
    public async Task FinallyAsync_WhenGenericResultAlreadyFailed_ReturnsOriginalResult()
    {
        var behavior = new ConcurrencyExceptionBehavior<ValueCommand, Result<string>>();
        var error = Error.NotFound("The order was not found.");
        var originalResult = Result.Failure<string>(error);

        var result = await behavior.FinallyAsync(
            new ValueCommand(), originalResult, new ConcurrencyException(), TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(originalResult);
        originalResult.Errors.ShouldHaveSingleItem().ShouldBeSameAs(error);
    }

    [Theory(DisplayName = "ConcurrencyExceptionBehavior declines unrelated, wrapped, aggregate and cancellation failures")]
    [InlineData("unexpected", false)]
    [InlineData("unexpected", true)]
    [InlineData("canceled", false)]
    [InlineData("canceled", true)]
    [InlineData("task-canceled", false)]
    [InlineData("task-canceled", true)]
    [InlineData("wrapped", false)]
    [InlineData("wrapped", true)]
    [InlineData("aggregate", false)]
    [InlineData("aggregate", true)]
    [InlineData("mixed-aggregate", false)]
    [InlineData("mixed-aggregate", true)]
    public async Task FinallyAsync_WhenExceptionIsNotDirectConcurrency_ReturnsOriginalResult(string scenario, bool hasResult)
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var originalResult = hasResult ? Result.Success() : null;
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

        var result = await behavior.FinallyAsync(
            new TestCommand(), originalResult, exception, TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(originalResult);
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior rejects a null request")]
    public async Task FinallyAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();

        var exception = await Should.ThrowAsync<ArgumentNullException>(() => behavior.FinallyAsync(
            null!, null, new ConcurrencyException(), TestContext.Current.CancellationToken));

        exception.ParamName.ShouldBe("request");
    }

    [Theory(DisplayName = "ConcurrencyExceptionBehavior passes through results when no exception occurred")]
    [InlineData("missing")]
    [InlineData("success")]
    [InlineData("failure")]
    public async Task FinallyAsync_WhenExceptionIsNull_ReturnsOriginalResult(string scenario)
    {
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var originalResult = scenario switch
        {
            "missing" => null,
            "success" => Result.Success(),
            "failure" => Result.Failure(Error.Validation("The order name is required.")),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown result scenario.")
        };

        var result = await behavior.FinallyAsync(
            new TestCommand(), originalResult, null, TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(originalResult);
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior preserves a successful generic result and its value")]
    public async Task FinallyAsync_WhenGenericRequestSucceeds_ReturnsOriginalResult()
    {
        var behavior = new ConcurrencyExceptionBehavior<ValueCommand, Result<string>>();
        var originalResult = Result.Success("saved value");

        var result = await behavior.FinallyAsync(
            new ValueCommand(), originalResult, null, TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(originalResult);
        originalResult.Value.ShouldBe("saved value");
    }

    [Fact(DisplayName = "ConcurrencyExceptionBehavior maps a conflict after successful transaction cleanup")]
    public async Task FinallyAsync_WhenTransactionCleanupSucceeds_ReturnsConflict()
    {
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = true };
        var cleanup = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var request = new TestCommand();
        var exception = new ConcurrencyException();

        var currentResult = await cleanup.FinallyAsync(request, null, exception, TestContext.Current.CancellationToken);
        var result = await behavior.FinallyAsync(request, currentResult, exception, TestContext.Current.CancellationToken);

        unitOfWork.RollbackTransactionCalls.ShouldBe(1);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
        result.ShouldNotBeNull().FirstError.Type.ShouldBe(ErrorType.Conflict);
    }

    [Fact(DisplayName = "Transaction cleanup passes through a conflict returned by a preceding finally behavior")]
    public async Task FinallyAsync_WhenConcurrencyIsHandledBeforeCleanup_PreservesConflict()
    {
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = true };
        var cleanup = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);
        var behavior = new ConcurrencyExceptionBehavior<TestCommand, Result>();
        var request = new TestCommand();
        var exception = new ConcurrencyException();

        var conflict = await behavior.FinallyAsync(request, null, exception, TestContext.Current.CancellationToken);
        var result = await cleanup.FinallyAsync(request, conflict, exception, TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(conflict);
        result.ShouldNotBeNull().FirstError.Type.ShouldBe(ErrorType.Conflict);
        unitOfWork.RollbackTransactionCalls.ShouldBe(1);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Theory(DisplayName = "A conflict result does not hide later transaction cleanup failures")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task FinallyAsync_WhenCleanupFailsAfterConcurrencyHandling_PreservesExceptions(bool failRollback, bool failDispose)
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

        var conflict = await behavior.FinallyAsync(request, null, originalException, TestContext.Current.CancellationToken);
        var exception = await Should.ThrowAsync<AggregateException>(() => cleanup.FinallyAsync(
            request, conflict, originalException, TestContext.Current.CancellationToken));

        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
        exception.InnerExceptions.ShouldBe(new[]
        {
            originalException,
            unitOfWork.RollbackException,
            unitOfWork.DisposeException
        }.OfType<Exception>());
    }

    [Theory(DisplayName = "ConcurrencyExceptionBehavior does not hide transaction cleanup failures")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task FinallyAsync_WhenCleanupAggregatesFailures_DeclinesHandling(bool failRollback, bool failDispose)
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
        var result = await behavior.FinallyAsync(request, null, exception, TestContext.Current.CancellationToken);

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
