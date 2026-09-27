using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Scheduling;

/// <summary>
/// Schedules application commands for deferred dispatch and domain events for deferred publication.
/// </summary>
/// <remarks>
/// Scheduling does not wait for command execution or return its result.
/// Events describe facts that have already occurred; scheduling delays their publication.
/// Persistence and transaction boundaries are defined by the infrastructure implementation.
/// </remarks>
public interface IScheduler
{
    /// <summary>
    /// Schedules a command for dispatch after the specified delay.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="delay">The non-negative delay before the command becomes eligible for dispatch.</param>
    /// <param name="cancellationToken">The token used to cancel scheduling, not subsequent command execution.</param>
    /// <returns>A task that represents the scheduling operation.</returns>
    /// <exception cref="ArgumentNullException">The command is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The delay is negative.</exception>
    /// <exception cref="OperationCanceledException">Scheduling is canceled.</exception>
    Task ScheduleAsync(
        ICommand<Result> command,
        TimeSpan delay,
        CancellationToken cancellationToken);

    /// <summary>
    /// Schedules a domain event for publication after the specified delay.
    /// </summary>
    /// <param name="event">The domain event to publish.</param>
    /// <param name="delay">The non-negative delay before the event becomes eligible for publication.</param>
    /// <param name="cancellationToken">The token used to cancel scheduling, not subsequent event handling.</param>
    /// <returns>A task that represents the scheduling operation.</returns>
    /// <exception cref="ArgumentNullException">The event is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The delay is negative.</exception>
    /// <exception cref="OperationCanceledException">Scheduling is canceled.</exception>
    Task ScheduleAsync(
        DomainEvent @event,
        TimeSpan delay,
        CancellationToken cancellationToken);

    /// <summary>
    /// Schedules a command for dispatch at or after the specified time.
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="deliverAt">The earliest requested dispatch time.</param>
    /// <param name="cancellationToken">The token used to cancel scheduling, not subsequent command execution.</param>
    /// <returns>A task that represents the scheduling operation.</returns>
    /// <exception cref="ArgumentNullException">The command is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Scheduling is canceled.</exception>
    Task ScheduleAtAsync(
        ICommand<Result> command,
        DateTimeOffset deliverAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Schedules a domain event for publication at or after the specified time.
    /// </summary>
    /// <param name="event">The domain event to publish.</param>
    /// <param name="deliverAt">The earliest requested publication time.</param>
    /// <param name="cancellationToken">The token used to cancel scheduling, not subsequent event handling.</param>
    /// <returns>A task that represents the scheduling operation.</returns>
    /// <exception cref="ArgumentNullException">The event is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Scheduling is canceled.</exception>
    Task ScheduleAtAsync(
        DomainEvent @event,
        DateTimeOffset deliverAt,
        CancellationToken cancellationToken);
}
