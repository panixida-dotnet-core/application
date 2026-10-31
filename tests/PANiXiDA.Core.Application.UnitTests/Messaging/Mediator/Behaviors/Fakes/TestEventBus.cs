using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Domain.DomainEvents;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;

internal sealed class TestEventBus : IEventBus
{
    public List<DomainEvent> PublishedEvents { get; } = [];
    public Exception? Exception { get; init; }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : DomainEvent
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        PublishedEvents.Add(@event);

        return Task.CompletedTask;
    }
}
