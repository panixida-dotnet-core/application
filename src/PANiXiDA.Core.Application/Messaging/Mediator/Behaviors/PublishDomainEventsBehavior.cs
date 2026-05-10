using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Publishes domain events collected from tracked aggregate roots after a successful request result.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
/// <param name="eventBus">The event bus used to publish domain events.</param>
/// <param name="aggregateTracker">The tracker that stores aggregate roots touched by the request.</param>
public sealed class PublishDomainEventsBehavior<TRequest, TResult>(
    IEventBus eventBus,
    IAggregateTracker aggregateTracker) : IAfterRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    /// <summary>
    /// Publishes domain events when the request succeeded and clears tracked events after completed publication.
    /// </summary>
    /// <param name="request">The request that was processed.</param>
    /// <param name="result">The request execution result.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AfterAsync(
        TRequest request,
        TResult result,
        CancellationToken cancellationToken)
    {
        var aggregateRoots = aggregateTracker.GetAll();

        if (result.IsFailure)
        {
            ClearDomainEvents(aggregateRoots);
            aggregateTracker.Clear();
            return;
        }

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

    private static void ClearDomainEvents(IReadOnlyCollection<IAggregateRoot> aggregateRoots)
    {
        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }
    }
}
