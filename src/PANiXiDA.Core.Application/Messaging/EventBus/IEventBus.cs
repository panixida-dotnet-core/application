namespace PANiXiDA.Core.Application.Messaging.EventBus;

public interface IEventBus
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : DomainEvent;
}
