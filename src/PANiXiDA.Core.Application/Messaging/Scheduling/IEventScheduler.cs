namespace PANiXiDA.Core.Application.Messaging.Scheduling;

/// <summary>
/// Schedules domain events for deferred publication to their subscribers.
/// </summary>
/// <remarks>
/// Events describe facts that have already occurred; scheduling delays their publication.
/// Persistence and transaction boundaries are defined by the infrastructure implementation.
/// </remarks>
public interface IEventScheduler
{
    /// <summary>
    /// Schedules a domain event for publication after the specified delay.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event to schedule.</typeparam>
    /// <param name="event">The domain event to publish.</param>
    /// <param name="delay">The non-negative delay before the event becomes eligible for publication.</param>
    /// <param name="cancellationToken">The token used to cancel scheduling, not subsequent event handling.</param>
    /// <returns>A task that represents the scheduling operation.</returns>
    /// <exception cref="ArgumentNullException">The event is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The delay is negative.</exception>
    /// <exception cref="OperationCanceledException">Scheduling is canceled.</exception>
    Task ScheduleAsync<TEvent>(
        TEvent @event,
        TimeSpan delay,
        CancellationToken cancellationToken)
        where TEvent : DomainEvent;

    /// <summary>
    /// Schedules a domain event for publication at or after the specified time.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event to schedule.</typeparam>
    /// <param name="event">The domain event to publish.</param>
    /// <param name="deliverAt">The earliest requested publication time.</param>
    /// <param name="cancellationToken">The token used to cancel scheduling, not subsequent event handling.</param>
    /// <returns>A task that represents the scheduling operation.</returns>
    /// <exception cref="ArgumentNullException">The event is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Scheduling is canceled.</exception>
    Task ScheduleAtAsync<TEvent>(
        TEvent @event,
        DateTimeOffset deliverAt,
        CancellationToken cancellationToken)
        where TEvent : DomainEvent;
}
