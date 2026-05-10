namespace PANiXiDA.Core.Application.Messaging.EventBus.Handlers;

/// <summary>
/// Handles a published domain event.
/// </summary>
/// <typeparam name="TEvent">The domain event type handled by the handler.</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="event">The domain event to handle.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken);
}
