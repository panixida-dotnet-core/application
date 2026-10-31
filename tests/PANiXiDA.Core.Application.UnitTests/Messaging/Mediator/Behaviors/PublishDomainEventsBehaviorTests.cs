using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.ResultPattern;
using PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors;

public sealed class PublishDomainEventsBehaviorTests
{
    [Fact(DisplayName = "Event completion publishes each tracked event once and clears tracking")]
    public async Task AfterAsync_WhenHandlerCompleted_PublishesEventsOnceAndClearsTracking()
    {
        var eventBus = new TestEventBus();
        var tracker = new TestAggregateTracker();
        var first = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        var second = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        var firstEvent = new TestDomainEvent();
        var secondEvent = new TestDomainEvent();
        first.Raise(firstEvent);
        second.Raise(secondEvent);
        tracker.Track(first);
        tracker.Track(second);
        var behavior = new PublishDomainEventsBehavior<TestDomainEvent, Result>(eventBus, tracker);
        using var cancellation = new CancellationTokenSource();

        await behavior.AfterAsync(new TestDomainEvent(), Result.Success(), cancellation.Token);
        await behavior.AfterAsync(new TestDomainEvent(), Result.Success(), cancellation.Token);

        eventBus.PublishedEvents.ShouldBe([firstEvent, secondEvent]);
        eventBus.LastCancellationToken.ShouldBe(cancellation.Token);
        first.GetDomainEvents().ShouldBeEmpty();
        second.GetDomainEvents().ShouldBeEmpty();
        tracker.GetAll().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Event completion preserves pending events when publication fails")]
    public async Task AfterAsync_WhenPublicationFails_PreservesEventsAndRethrows()
    {
        var failure = new InvalidOperationException("Publication failed.");
        var eventBus = new TestEventBus { Exception = failure };
        var tracker = new TestAggregateTracker();
        var aggregate = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        var domainEvent = new TestDomainEvent();
        aggregate.Raise(domainEvent);
        tracker.Track(aggregate);
        var behavior = new PublishDomainEventsBehavior<TestDomainEvent, Result>(eventBus, tracker);
        using var cancellation = new CancellationTokenSource();

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => behavior.AfterAsync(new TestDomainEvent(), Result.Success(), cancellation.Token));

        exception.ShouldBeSameAs(failure);
        aggregate.GetDomainEvents().ShouldHaveSingleItem().ShouldBeSameAs(domainEvent);
        tracker.GetAll().ShouldHaveSingleItem().ShouldBeSameAs(aggregate);
        tracker.ClearCalls.ShouldBe(0);
    }

    [Fact(DisplayName = "Event completion does not publish when no aggregates are tracked")]
    public async Task AfterAsync_WhenTrackerIsEmpty_DoesNotPublish()
    {
        var eventBus = new TestEventBus();
        var tracker = new TestAggregateTracker();
        var behavior = new PublishDomainEventsBehavior<TestDomainEvent, Result>(eventBus, tracker);
        using var cancellation = new CancellationTokenSource();

        await behavior.AfterAsync(new TestDomainEvent(), Result.Success(), cancellation.Token);

        eventBus.PublishedEvents.ShouldBeEmpty();
        tracker.GetAll().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Publication behavior rejects a missing event bus")]
    public void Constructor_WhenEventBusIsNull_ThrowsArgumentNullException()
    {
        var tracker = new TestAggregateTracker();

        var exception = Should.Throw<ArgumentNullException>(() => new PublishDomainEventsBehavior<TestDomainEvent, Result>(null!, tracker));

        exception.ParamName.ShouldBe("eventBus");
    }

    [Fact(DisplayName = "Publication behavior rejects a missing aggregate tracker")]
    public void Constructor_WhenAggregateTrackerIsNull_ThrowsArgumentNullException()
    {
        var eventBus = new TestEventBus();

        var exception = Should.Throw<ArgumentNullException>(() => new PublishDomainEventsBehavior<TestDomainEvent, Result>(eventBus, null!));

        exception.ParamName.ShouldBe("aggregateTracker");
    }

    [Theory(DisplayName = "Queries preserve pending events and tracking regardless of their result")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AfterAsync_WhenQueryCompletes_DoesNotPublishOrClearTracking(bool succeeds)
    {
        var eventBus = new TestEventBus();
        var tracker = new TestAggregateTracker();
        var aggregate = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        var domainEvent = new TestDomainEvent();
        aggregate.Raise(domainEvent);
        tracker.Track(aggregate);
        var behavior = new PublishDomainEventsBehavior<TestQuery, Result<string>>(eventBus, tracker);
        var result = succeeds ? Result.Success("query") : Result.Failure<string>(Error.Failure("Failed query."));

        await behavior.AfterAsync(new TestQuery(), result, TestContext.Current.CancellationToken);

        eventBus.PublishedEvents.ShouldBeEmpty();
        aggregate.GetDomainEvents().ShouldHaveSingleItem().ShouldBeSameAs(domainEvent);
        tracker.GetAll().ShouldHaveSingleItem().ShouldBeSameAs(aggregate);
        tracker.ClearCalls.ShouldBe(0);
    }

    [Fact(DisplayName = "Plain requests do not publish or clear tracked aggregate events")]
    public async Task AfterAsync_WhenMessageIsPlainRequest_LeavesPendingEventsUntouched()
    {
        var eventBus = new TestEventBus();
        var tracker = new TestAggregateTracker();
        var aggregate = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        aggregate.Raise(new TestDomainEvent());
        tracker.Track(aggregate);
        var behavior = new PublishDomainEventsBehavior<TestRequest, Result>(eventBus, tracker);

        await behavior.AfterAsync(new TestRequest(), Result.Success(), TestContext.Current.CancellationToken);

        eventBus.PublishedEvents.ShouldBeEmpty();
        aggregate.GetDomainEvents().ShouldHaveSingleItem();
        tracker.GetAll().ShouldHaveSingleItem();
        tracker.ClearCalls.ShouldBe(0);
    }

    [Fact(DisplayName = "Query exclusion takes precedence over a command contract")]
    public async Task AfterAsync_WhenMessageAlsoImplementsQuery_DoesNotPublishOrClearTracking()
    {
        var eventBus = new TestEventBus();
        var tracker = new TestAggregateTracker();
        var aggregate = new TestAggregateRoot(new TestAggregateRootId(Guid.NewGuid()));
        aggregate.Raise(new TestDomainEvent());
        tracker.Track(aggregate);
        var behavior = new PublishDomainEventsBehavior<MixedMessage, Result>(eventBus, tracker);

        await behavior.AfterAsync(new MixedMessage(), Result.Success(), TestContext.Current.CancellationToken);

        eventBus.PublishedEvents.ShouldBeEmpty();
        tracker.GetAll().ShouldHaveSingleItem();
        aggregate.GetDomainEvents().ShouldHaveSingleItem();
        tracker.ClearCalls.ShouldBe(0);
    }

    [Fact(DisplayName = "Publication rejects a missing message")]
    public async Task AfterAsync_WhenMessageIsNull_ThrowsArgumentNullException()
    {
        var behavior = new PublishDomainEventsBehavior<TestDomainEvent, Result>(new TestEventBus(), new TestAggregateTracker());

        var exception = await Should.ThrowAsync<ArgumentNullException>(() =>
            behavior.AfterAsync(null!, Result.Success(), TestContext.Current.CancellationToken));

        exception.ParamName.ShouldBe("request");
    }

    [Fact(DisplayName = "Publication rejects a missing result for a supported message")]
    public async Task AfterAsync_WhenResultIsNull_ThrowsArgumentNullException()
    {
        var behavior = new PublishDomainEventsBehavior<TestDomainEvent, Result>(new TestEventBus(), new TestAggregateTracker());

        var exception = await Should.ThrowAsync<ArgumentNullException>(() =>
            behavior.AfterAsync(new TestDomainEvent(), null!, TestContext.Current.CancellationToken));

        exception.ParamName.ShouldBe("result");
    }

    private sealed record TestQuery : IQuery<Result<string>>;

    private sealed record MixedMessage : ICommand<Result>, IQuery<Result>;
}
