using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors;

public sealed class SaveChangesBehaviorTests
{
    [Theory(DisplayName = "SaveChangesBehavior saves successful commands regardless of an active transaction")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task AfterAsync_SavesOnlySuccessfulCommands(bool active, bool success)
    {
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = active };
        var behavior = new SaveChangesBehavior<TestCommand, Result>(unitOfWork);
        var result = success ? Result.Success() : Result.Failure(Error.Validation("Rejected."));
        using var cancellation = new CancellationTokenSource();

        await behavior.AfterAsync(new TestCommand(), result, cancellation.Token);

        unitOfWork.SaveChangesCalls.ShouldBe(success ? 1 : 0);
        unitOfWork.BeginTransactionCalls.ShouldBe(0);
        unitOfWork.CommitTransactionCalls.ShouldBe(0);
        unitOfWork.HasActiveTransaction.ShouldBe(active);
        if (success)
        {
            unitOfWork.LastCancellationToken.ShouldBe(cancellation.Token);
        }
    }

    [Theory(DisplayName = "SaveChangesBehavior preserves save failures and cancellation for the request pipeline")]
    [InlineData(false, "save")]
    [InlineData(true, "save")]
    [InlineData(false, "concurrency")]
    [InlineData(true, "concurrency")]
    [InlineData(false, "canceled")]
    [InlineData(true, "canceled")]
    public async Task AfterAsync_PropagatesOriginalFailure(bool active, string failure)
    {
        Exception expected = failure switch
        {
            "concurrency" => new ConcurrencyException(),
            "canceled" => new OperationCanceledException(),
            _ => new InvalidOperationException("Save failed.")
        };
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = active, SaveException = expected };
        var behavior = new SaveChangesBehavior<TestCommand, Result>(unitOfWork);

        var actual = await Record.ExceptionAsync(() => behavior.AfterAsync(
            new TestCommand(), Result.Success(), TestContext.Current.CancellationToken));

        actual.ShouldBeSameAs(expected);
        unitOfWork.SaveChangesCalls.ShouldBe(1);
        unitOfWork.CommitTransactionCalls.ShouldBe(0);
    }
}
