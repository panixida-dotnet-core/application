namespace PANiXiDA.Core.Application.Messaging.EventBus;

/// <summary>
/// Publishes domain events to their subscribers.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes the specified domain event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event to publish.</typeparam>
    /// <param name="event">The domain event to publish.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : DomainEvent;
}
