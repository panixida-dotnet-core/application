using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Publishes domain events collected from tracked aggregate roots after a successful command result.
/// </summary>
/// <typeparam name="TCommand">The command type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
/// <param name="eventBus">The event bus used to publish domain events.</param>
/// <param name="aggregateTracker">The tracker that stores aggregate roots touched by the command.</param>
public sealed class PublishDomainEventsBehavior<TCommand, TResult>(
    IEventBus eventBus,
    IAggregateTracker aggregateTracker) : IAfterRequestBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    private readonly PublishDomainEventsBehavior publication = new(eventBus, aggregateTracker);

    /// <summary>
    /// Publishes domain events when the command succeeded and clears tracked events after completed publication.
    /// </summary>
    /// <param name="request">The command that was processed.</param>
    /// <param name="result">The command execution result.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AfterAsync(
        TCommand request,
        TResult result,
        CancellationToken cancellationToken)
    {
        if (result.IsFailure)
        {
            publication.Clear();
            return Task.CompletedTask;
        }

        return publication.AfterAsync(cancellationToken);
    }

}

/// <summary>
/// Publishes tracked aggregate events after a handler succeeds, before its transaction is saved and committed.
/// </summary>
/// <remarks>
/// Command and event pipelines share this publication logic. The caller owns transaction and outbox management
/// and must invoke the behavior only after successful handler completion.
/// </remarks>
public sealed class PublishDomainEventsBehavior
{
    private readonly IEventBus eventBus;
    private readonly IAggregateTracker aggregateTracker;

    /// <summary>
    /// Initializes the publication behavior for the current aggregate tracking scope.
    /// </summary>
    /// <param name="eventBus">The event bus used to publish pending domain events.</param>
    /// <param name="aggregateTracker">The tracker containing aggregates touched by the handler.</param>
    /// <exception cref="ArgumentNullException">A dependency is null.</exception>
    public PublishDomainEventsBehavior(IEventBus eventBus, IAggregateTracker aggregateTracker)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(aggregateTracker);
        this.eventBus = eventBus;
        this.aggregateTracker = aggregateTracker;
    }

    /// <summary>
    /// Publishes pending events and clears tracking only after all publications complete successfully.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel publication.</param>
    /// <returns>A task that represents the asynchronous publication operation.</returns>
    public async Task AfterAsync(CancellationToken cancellationToken)
    {
        var aggregateRoots = aggregateTracker.GetAll();

        foreach (var aggregateRoot in aggregateRoots)
        {
            var domainEvents = aggregateRoot.GetDomainEvents();

            foreach (var domainEvent in domainEvents)
            {
                await eventBus.PublishAsync(domainEvent, cancellationToken);
            }
        }

        ClearDomainEvents(aggregateRoots);
        aggregateTracker.Clear();
    }

    internal void Clear()
    {
        ClearDomainEvents(aggregateTracker.GetAll());
        aggregateTracker.Clear();
    }

    private static void ClearDomainEvents(IReadOnlyCollection<IAggregateRoot> aggregateRoots)
    {
        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }
    }
}
