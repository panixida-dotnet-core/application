using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Publishes tracked aggregate events after successful commands and event handlers, excluding queries.
/// </summary>
/// <typeparam name="TMessage">The command or domain event processed by the handler.</typeparam>
/// <typeparam name="TResult">The command result type, or Result for a completed event handler.</typeparam>
public sealed class PublishDomainEventsBehavior<TMessage, TResult> : IAfterRequestBehavior<TMessage, TResult>
    where TResult : Result
{
    private readonly IEventBus eventBus;
    private readonly IAggregateTracker aggregateTracker;

    /// <summary>
    /// Initializes the behavior for the current aggregate tracking scope.
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
    /// Publishes events for successful commands and event handlers, or discards them for failed command results.
    /// Queries and messages without a command or domain-event contract leave events and tracking untouched.
    /// </summary>
    /// <param name="request">The message that was processed.</param>
    /// <param name="result">The command result, or a successful Result supplied after event-handler completion.</param>
    /// <param name="cancellationToken">The token used to cancel publication.</param>
    /// <returns>A task that represents the asynchronous publication operation.</returns>
    public async Task AfterAsync(TMessage request, TResult result, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request is IQuery<TResult> || request is not ICommand<TResult> && request is not DomainEvent)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(result);
        var aggregateRoots = aggregateTracker.GetAll();

        if (result.IsSuccess)
        {
            foreach (var aggregateRoot in aggregateRoots)
            {
                foreach (var domainEvent in aggregateRoot.GetDomainEvents())
                {
                    await eventBus.PublishAsync(domainEvent, cancellationToken);
                }
            }
        }

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        aggregateTracker.Clear();
    }
}
