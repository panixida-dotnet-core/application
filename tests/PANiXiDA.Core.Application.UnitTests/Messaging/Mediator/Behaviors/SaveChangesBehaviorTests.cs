using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Persistence.Errors;
using PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors;

public sealed class SaveChangesBehaviorTests
{
    [Theory(DisplayName = "SaveChangesBehavior saves only successful commands in an active transaction")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task AfterAsync_SavesOnlySuccessfulTransactionalCommands(bool active, bool success)
    {
        // Arrange
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = active };
        var behavior = new SaveChangesBehavior<TestCommand, Result>(unitOfWork);
        var result = success ? Result.Success() : Result.Failure(Error.Validation("Rejected."));
        using var cancellation = new CancellationTokenSource();

        // Act
        await behavior.AfterAsync(new TestCommand(), result, cancellation.Token);

        // Assert
        unitOfWork.SaveChangesCalls.ShouldBe(active && success ? 1 : 0);
        unitOfWork.CommitTransactionCalls.ShouldBe(0);
        if (active && success)
        {
            unitOfWork.LastCancellationToken.ShouldBe(cancellation.Token);
        }
    }

    [Theory(DisplayName = "SaveChangesBehavior preserves save failures and cancellation for the request pipeline")]
    [InlineData("save")]
    [InlineData("concurrency")]
    [InlineData("canceled")]
    public async Task AfterAsync_PropagatesOriginalFailure(string failure)
    {
        // Arrange
        Exception expected = failure switch
        {
            "concurrency" => new ConcurrencyException(),
            "canceled" => new OperationCanceledException(),
            _ => new InvalidOperationException("Save failed.")
        };
        var unitOfWork = new TestUnitOfWork { HasActiveTransaction = true, SaveException = expected };
        var behavior = new SaveChangesBehavior<TestCommand, Result>(unitOfWork);

        // Act
        var actual = await Record.ExceptionAsync(() => behavior.AfterAsync(
            new TestCommand(), Result.Success(), TestContext.Current.CancellationToken));

        // Assert
        actual.ShouldBeSameAs(expected);
        unitOfWork.SaveChangesCalls.ShouldBe(1);
        unitOfWork.CommitTransactionCalls.ShouldBe(0);
    }
}
