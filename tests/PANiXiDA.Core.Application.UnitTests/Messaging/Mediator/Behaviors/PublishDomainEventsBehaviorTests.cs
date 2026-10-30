using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
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
        var behavior = new PublishDomainEventsBehavior(eventBus, tracker);
        using var cancellation = new CancellationTokenSource();

        await behavior.AfterAsync(cancellation.Token);
        await behavior.AfterAsync(cancellation.Token);

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
        var behavior = new PublishDomainEventsBehavior(eventBus, tracker);
        using var cancellation = new CancellationTokenSource();

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => behavior.AfterAsync(cancellation.Token));

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
        var behavior = new PublishDomainEventsBehavior(eventBus, tracker);
        using var cancellation = new CancellationTokenSource();

        await behavior.AfterAsync(cancellation.Token);

        eventBus.PublishedEvents.ShouldBeEmpty();
        tracker.GetAll().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Shared publication behavior rejects a missing event bus")]
    public void Constructor_WhenEventBusIsNull_ThrowsArgumentNullException()
    {
        var tracker = new TestAggregateTracker();

        var exception = Should.Throw<ArgumentNullException>(() => new PublishDomainEventsBehavior(null!, tracker));

        exception.ParamName.ShouldBe("eventBus");
    }

    [Fact(DisplayName = "Shared publication behavior rejects a missing aggregate tracker")]
    public void Constructor_WhenAggregateTrackerIsNull_ThrowsArgumentNullException()
    {
        var eventBus = new TestEventBus();

        var exception = Should.Throw<ArgumentNullException>(() => new PublishDomainEventsBehavior(eventBus, null!));

        exception.ParamName.ShouldBe("aggregateTracker");
    }
}
