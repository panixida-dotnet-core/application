using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Behaviors;

public sealed class PublishDomainEventsBehavior<TRequest, TResult>(
    IEventBus eventBus,
    IAggregateTracker aggregateTracker) : IAfterRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
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
