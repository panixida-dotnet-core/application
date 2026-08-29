using FluentValidation;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors;

public sealed class RequestBehaviorsTests
{
    [Fact(DisplayName = "BeginTransactionBehavior begins a transaction")]
    public async Task BeforeAsync_WhenCalled_BeginsTransaction()
    {
        var unitOfWork = new TestUnitOfWork();
        var behavior = new BeginTransactionBehavior<TestCommand, Result>(unitOfWork);
        using var cancellationTokenSource = new CancellationTokenSource();

        var result = await behavior.BeforeAsync(new TestCommand(), cancellationTokenSource.Token);

        result.IsSuccess.ShouldBeTrue();
        unitOfWork.BeginTransactionCalls.ShouldBe(1);
        unitOfWork.HasActiveTransaction.ShouldBeTrue();
        unitOfWork.LastCancellationToken.ShouldBe(cancellationTokenSource.Token);
    }

    [Fact(DisplayName = "ValidationBehavior returns success when validators are missing")]
    public async Task BeforeAsync_WhenValidatorsAreMissing_ReturnsSuccess()
    {
        var behavior = new ValidationBehavior<TestValidatedCommand, Result>([]);

        var result = await behavior.BeforeAsync(new TestValidatedCommand(Name: ""), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact(DisplayName = "ValidationBehavior returns success when validation succeeds")]
    public async Task BeforeAsync_WhenValidationSucceeds_ReturnsSuccess()
    {
        var behavior = new ValidationBehavior<TestValidatedCommand, Result>(
            [new TestValidatedCommandValidator()]);

        var result = await behavior.BeforeAsync(new TestValidatedCommand(Name: "name"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact(DisplayName = "ValidationBehavior returns validation failure for invalid commands")]
    public async Task BeforeAsync_WhenCommandValidationFails_ReturnsValidationFailure()
    {
        var behavior = new ValidationBehavior<TestValidatedCommand, Result>(
            [new TestValidatedCommandValidator()]);

        var result = await behavior.BeforeAsync(new TestValidatedCommand(Name: ""), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        var error = result.Errors.ShouldHaveSingleItem();
        error.Type.ShouldBe(ErrorType.Validation);
        error.Message.ShouldBe("Name is required.");
        error.Metadata[Error.FieldMetadataKey].ShouldBe(nameof(TestValidatedCommand.Name));
    }

    [Fact(DisplayName = "ValidationBehavior returns validation failure for invalid queries")]
    public async Task BeforeAsync_WhenQueryValidationFails_ReturnsValidationFailure()
    {
        var behavior = new ValidationBehavior<TestValidatedQuery, Result<string>>(
            [new TestValidatedQueryValidator()]);

        var result = await behavior.BeforeAsync(new TestValidatedQuery(Name: ""), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        var error = result.Errors.ShouldHaveSingleItem();
        error.Type.ShouldBe(ErrorType.Validation);
        error.Message.ShouldBe("Name is required.");
        error.Metadata[Error.FieldMetadataKey].ShouldBe(nameof(TestValidatedQuery.Name));
    }

    [Fact(DisplayName = "CommitTransactionBehavior commits successful commands in an active transaction")]
    public async Task AfterAsync_WhenCommandSucceededInActiveTransaction_CommitsTransaction()
    {
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true
        };
        var behavior = new CommitTransactionBehavior<TestCommand, Result>(unitOfWork);

        await behavior.AfterAsync(new TestCommand(), Result.Success(), CancellationToken.None);

        unitOfWork.CommitTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "CommitTransactionBehavior skips when there is no active transaction")]
    public async Task AfterAsync_WhenTransactionIsNotActive_DoesNotCommitTransaction()
    {
        var unitOfWork = new TestUnitOfWork();
        var behavior = new CommitTransactionBehavior<TestCommand, Result>(unitOfWork);

        await behavior.AfterAsync(new TestCommand(), Result.Success(), CancellationToken.None);

        unitOfWork.CommitTransactionCalls.ShouldBe(0);
    }

    [Fact(DisplayName = "CommitTransactionBehavior skips failed command results")]
    public async Task AfterAsync_WhenCommandFailed_DoesNotCommitTransaction()
    {
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true
        };
        var behavior = new CommitTransactionBehavior<TestCommand, Result>(unitOfWork);

        await behavior.AfterAsync(new TestCommand(), CreateFailureResult(), CancellationToken.None);

        unitOfWork.CommitTransactionCalls.ShouldBe(0);
    }

    [Fact(DisplayName = "CleanupTransactionBehavior skips when there is no active transaction")]
    public async Task FinallyAsync_WhenTransactionIsNotActive_DoesNotRollbackOrDispose()
    {
        var unitOfWork = new TestUnitOfWork();
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        await behavior.FinallyAsync(new TestCommand(), Result.Success(), null, CancellationToken.None);

        unitOfWork.RollbackTransactionCalls.ShouldBe(0);
        unitOfWork.DisposeTransactionCalls.ShouldBe(0);
    }

    [Fact(DisplayName = "CleanupTransactionBehavior disposes successful active transactions")]
    public async Task FinallyAsync_WhenCommandSucceededInActiveTransaction_DisposesTransaction()
    {
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        await behavior.FinallyAsync(new TestCommand(), Result.Success(), null, CancellationToken.None);

        unitOfWork.RollbackTransactionCalls.ShouldBe(0);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "CleanupTransactionBehavior rolls back failed command results")]
    public async Task FinallyAsync_WhenCommandFailedInActiveTransaction_RollsBackAndDisposesTransaction()
    {
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        await behavior.FinallyAsync(new TestCommand(), CreateFailureResult(), null, CancellationToken.None);

        unitOfWork.RollbackTransactionCalls.ShouldBe(1);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "CleanupTransactionBehavior rolls back when the command result is missing")]
    public async Task FinallyAsync_WhenResultIsNullInActiveTransaction_RollsBackAndDisposesTransaction()
    {
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        await behavior.FinallyAsync(new TestCommand(), null, null, CancellationToken.None);

        unitOfWork.RollbackTransactionCalls.ShouldBe(1);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "CleanupTransactionBehavior rolls back when an exception is provided")]
    public async Task FinallyAsync_WhenExceptionIsProvidedInActiveTransaction_RollsBackAndDisposesTransaction()
    {
        var unitOfWork = new TestUnitOfWork
        {
            HasActiveTransaction = true
        };
        var behavior = new CleanupTransactionBehavior<TestCommand, Result>(unitOfWork);

        await behavior.FinallyAsync(
            new TestCommand(),
            Result.Success(),
            new InvalidOperationException(),
            CancellationToken.None);

        unitOfWork.RollbackTransactionCalls.ShouldBe(1);
        unitOfWork.DisposeTransactionCalls.ShouldBe(1);
    }

    [Fact(DisplayName = "PublishDomainEventsBehavior publishes events from tracked aggregates")]
    public async Task AfterAsync_WhenRequestSucceeded_PublishesDomainEventsAndClearsTracking()
    {
        var eventBus = new TestEventBus();
        var aggregateTracker = new TestAggregateTracker();
        var aggregateRoot = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        var firstEvent = new TestDomainEvent();
        var secondEvent = new TestDomainEvent();
        aggregateRoot.Raise(firstEvent);
        aggregateRoot.Raise(secondEvent);
        aggregateTracker.Track(aggregateRoot);

        var behavior = new PublishDomainEventsBehavior<TestRequest, Result>(eventBus, aggregateTracker);

        await behavior.AfterAsync(new TestRequest(), Result.Success(), CancellationToken.None);

        eventBus.PublishedEvents.ShouldBe(new[] { firstEvent, secondEvent });
        aggregateRoot.GetDomainEvents().ShouldBeEmpty();
        aggregateTracker.ClearCalls.ShouldBe(1);
        aggregateTracker.GetAll().ShouldBeEmpty();
    }

    [Fact(DisplayName = "PublishDomainEventsBehavior clears tracking without publishing failed request results")]
    public async Task AfterAsync_WhenRequestFailed_ClearsTrackingWithoutPublishing()
    {
        var eventBus = new TestEventBus();
        var aggregateTracker = new TestAggregateTracker();
        var aggregateRoot = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        aggregateRoot.Raise(new TestDomainEvent());
        aggregateTracker.Track(aggregateRoot);

        var behavior = new PublishDomainEventsBehavior<TestRequest, Result>(eventBus, aggregateTracker);

        await behavior.AfterAsync(new TestRequest(), CreateFailureResult(), CancellationToken.None);

        eventBus.PublishedEvents.ShouldBeEmpty();
        aggregateRoot.GetDomainEvents().ShouldBeEmpty();
        aggregateTracker.ClearCalls.ShouldBe(1);
        aggregateTracker.GetAll().ShouldBeEmpty();
    }

    [Fact(DisplayName = "PublishDomainEventsBehavior does not clear tracking when publishing throws")]
    public async Task AfterAsync_WhenPublishingThrows_DoesNotClearTrackingAndRethrows()
    {
        var eventBus = new TestEventBus
        {
            Exception = new InvalidOperationException("Publish failed.")
        };
        var aggregateTracker = new TestAggregateTracker();
        var aggregateRoot = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        aggregateRoot.Raise(new TestDomainEvent());
        aggregateTracker.Track(aggregateRoot);

        var behavior = new PublishDomainEventsBehavior<TestRequest, Result>(eventBus, aggregateTracker);

        Task act() => behavior.AfterAsync(new TestRequest(), Result.Success(), CancellationToken.None);

        var exception = await Should.ThrowAsync<InvalidOperationException>(act);

        exception.Message.ShouldBe("Publish failed.");
        aggregateRoot.GetDomainEvents().ShouldNotBeEmpty();
        aggregateTracker.ClearCalls.ShouldBe(0);
        aggregateTracker.GetAll().ShouldHaveSingleItem();
    }

    private static Result CreateFailureResult()
    {
        return Result.Failure(Error.Failure("Failure."));
    }

    private sealed record TestValidatedCommand(string Name) : ICommand<Result>;

    private sealed record TestValidatedQuery(string Name) : IQuery<Result<string>>;

    private sealed class TestValidatedCommandValidator : AbstractValidator<TestValidatedCommand>
    {
        public TestValidatedCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .WithMessage("Name is required.");
        }
    }

    private sealed class TestValidatedQueryValidator : AbstractValidator<TestValidatedQuery>
    {
        public TestValidatedQueryValidator()
        {
            RuleFor(query => query.Name)
                .NotEmpty()
                .WithMessage("Name is required.");
        }
    }
}
