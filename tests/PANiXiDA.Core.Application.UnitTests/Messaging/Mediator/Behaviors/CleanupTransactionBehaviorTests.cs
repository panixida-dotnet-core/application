using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors;

public sealed class CleanupTransactionBehaviorTests
{
    [Theory(DisplayName = "Cleanup preserves generic results with and without an active transaction")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task FinallyAsync_WhenGenericResultExists_ReturnsOriginalResult(bool hasActiveTransaction, bool isFailure)
    {
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = hasActiveTransaction };
        var behavior = new CleanupTransactionBehavior<ValueCommand, Result<string>>(unitOfWork);
        var originalResult = isFailure ? Result.Failure<string>(Error.Validation("Invalid value.")) : Result.Success("saved value");

        var result = await behavior.FinallyAsync(new ValueCommand(), originalResult, null, TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(originalResult);
        unitOfWork.RollbackTransactionCalls.ShouldBe(hasActiveTransaction && isFailure ? 1 : 0);
        unitOfWork.DisposeTransactionCalls.ShouldBe(hasActiveTransaction ? 1 : 0);
    }

    [Fact(DisplayName = "Cleanup preserves a missing result when there is no active transaction")]
    public async Task FinallyAsync_WhenResultAndTransactionAreMissing_ReturnsNull()
    {
        var unitOfWork = new TestUnitOfWork();
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        var result = await behavior.FinallyAsync(new TestCommand(), null, null, TestContext.Current.CancellationToken);

        result.ShouldBeNull();
        unitOfWork.RollbackTransactionCalls.ShouldBe(0);
        unitOfWork.DisposeTransactionCalls.ShouldBe(0);
    }

    [Theory(DisplayName = "Cleanup preserves single failures and aggregates multiple failures")]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task FinallyAsync_PreservesAllFailures(
        bool failOperation,
        bool failRollback,
        bool failDispose)
    {
        var originalException = failOperation ? new InvalidOperationException("Operation failed.") : null;
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true,
            RollbackException = failRollback ? new InvalidOperationException("Rollback failed.") : null,
            DisposeException = failDispose ? new InvalidOperationException("Dispose failed.") : null
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);
        var expectedExceptions = new[] { originalException, unitOfWork.RollbackException, unitOfWork.DisposeException }
            .OfType<Exception>().ToArray();

        var actualException = await Record.ExceptionAsync(async () =>
        {
            Exception? caughtException = null;
            try
            {
                if (originalException is not null)
                {
                    throw originalException;
                }
            }
            catch (Exception exception)
            {
                caughtException = exception;
                throw;
            }
            finally
            {
                await behavior.FinallyAsync(new TestCommand(), null, caughtException, CancellationToken.None);
            }
        });

        unitOfWork.RollbackTransactionCalls.ShouldBe(1);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
        if (expectedExceptions.Length == 0)
        {
            actualException.ShouldBeNull();
        }
        else if (expectedExceptions.Length == 1)
        {
            actualException.ShouldBeSameAs(expectedExceptions[0]);
        }
        else
        {
            var aggregate = actualException.ShouldBeOfType<AggregateException>();
            aggregate.InnerExceptions.ShouldBe(expectedExceptions);
        }

        if (originalException is not null)
        {
            originalException.StackTrace.ShouldNotBeNull();
            originalException.StackTrace.ShouldContain(nameof(FinallyAsync_PreservesAllFailures));
        }

        if (unitOfWork.RollbackException is not null)
        {
            unitOfWork.RollbackException.StackTrace.ShouldNotBeNull();
            unitOfWork.RollbackException.StackTrace.ShouldContain(nameof(TestUnitOfWork.RollbackTransactionAsync));
        }

        if (unitOfWork.DisposeException is not null)
        {
            unitOfWork.DisposeException.StackTrace.ShouldNotBeNull();
            unitOfWork.DisposeException.StackTrace.ShouldContain(nameof(TestUnitOfWork.DisposeTransactionAsync));
        }
    }

    [Theory(DisplayName = "Cleanup preserves single cancellation failures and their tokens")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FinallyAsync_WhenOnlyCleanupIsCanceled_PreservesCancellation(bool cancelDispose)
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var exception = new OperationCanceledException(cancellation.Token);
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true,
            RollbackException = cancelDispose ? null : exception,
            DisposeException = cancelDispose ? exception : null
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        var task = behavior.FinallyAsync(new TestCommand(), null, null, cancellation.Token);
        var actualException = await Record.ExceptionAsync(() => task);

        actualException.ShouldBeSameAs(exception);
        task.IsCanceled.ShouldBeTrue();
        unitOfWork.LastCancellationToken.ShouldBe(cancellation.Token);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "Cleanup aggregates rollback cancellation with the original failure")]
    public async Task FinallyAsync_WhenRollbackIsCanceledAndOperationFailed_AggregatesFailures()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var originalException = new InvalidOperationException("Operation failed.");
        var rollbackException = new OperationCanceledException(cancellation.Token);
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true,
            RollbackException = rollbackException
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        var task = behavior.FinallyAsync(new TestCommand(), null, originalException, cancellation.Token);
        var actualException = await Record.ExceptionAsync(() => task);

        actualException.ShouldBeOfType<AggregateException>().InnerExceptions
            .ShouldBe(new Exception[] { originalException, rollbackException });
        task.IsFaulted.ShouldBeTrue();
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "Cleanup preserves an existing aggregate when it is the only failure")]
    public async Task FinallyAsync_WhenOnlyRollbackThrowsAggregate_PreservesInstance()
    {
        var rollbackException = new AggregateException(
            new InvalidOperationException("Rollback failed."),
            new InvalidOperationException("Transaction disposal failed."));
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true,
            RollbackException = rollbackException
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        var actualException = await Record.ExceptionAsync(
            () => behavior.FinallyAsync(new TestCommand(), null, null, CancellationToken.None));

        actualException.ShouldBeSameAs(rollbackException);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "Cleanup retains failures already aggregated by the unit of work")]
    public async Task FinallyAsync_WhenRollbackThrowsAggregate_PreservesNestedFailures()
    {
        var originalException = new InvalidOperationException("Operation failed.");
        var rollbackException = new InvalidOperationException("Rollback failed.");
        var disposeException = new InvalidOperationException("Transaction disposal failed.");
        var cleanupException = new AggregateException(rollbackException, disposeException);
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true,
            RollbackException = cleanupException
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        var actualException = await Record.ExceptionAsync(
            () => behavior.FinallyAsync(new TestCommand(), null, originalException, CancellationToken.None));

        var aggregate = actualException.ShouldBeOfType<AggregateException>();
        aggregate.InnerExceptions.ShouldBe(new Exception[] { originalException, cleanupException });
        aggregate.Flatten().InnerExceptions.ShouldBe(new Exception[] { originalException, rollbackException, disposeException });
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "Cleanup propagates disposal failures after successful commands without rolling back")]
    public async Task FinallyAsync_WhenSuccessfulCommandDisposalFails_PreservesFailure()
    {
        var exception = new InvalidOperationException("Dispose failed.");
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = true, DisposeException = exception };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        var actualException = await Record.ExceptionAsync(
            () => behavior.FinallyAsync(new TestCommand(), Result.Success(), null, CancellationToken.None));

        actualException.ShouldBeSameAs(exception);
        unitOfWork.RollbackTransactionCalls.ShouldBe(0);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    private sealed record ValueCommand : ICommand<Result<string>>;
}
