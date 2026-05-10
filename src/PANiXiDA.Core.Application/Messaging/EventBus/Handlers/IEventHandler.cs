namespace PANiXiDA.Core.Application.Messaging.EventBus.Handlers;

public interface IEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
