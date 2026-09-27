using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Scheduling;

/// <summary>
/// Schedules application commands for deferred dispatch.
/// </summary>
/// <remarks>
/// Scheduling does not wait for command execution or return its result.
/// Persistence and transaction boundaries are defined by the infrastructure implementation.
/// </remarks>
public interface ICommandScheduler
{
    /// <summary>
    /// Schedules a command for dispatch after the specified delay.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to schedule.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="delay">The non-negative delay before the command becomes eligible for dispatch.</param>
    /// <param name="cancellationToken">The token used to cancel scheduling, not subsequent command execution.</param>
    /// <returns>A task that represents the scheduling operation.</returns>
    /// <exception cref="ArgumentNullException">The command is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The delay is negative.</exception>
    /// <exception cref="OperationCanceledException">Scheduling is canceled.</exception>
    Task ScheduleAsync<TCommand>(
        TCommand command,
        TimeSpan delay,
        CancellationToken cancellationToken)
        where TCommand : ICommand<Result>;

    /// <summary>
    /// Schedules a command for dispatch at or after the specified time.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to schedule.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="deliverAt">The earliest requested dispatch time.</param>
    /// <param name="cancellationToken">The token used to cancel scheduling, not subsequent command execution.</param>
    /// <returns>A task that represents the scheduling operation.</returns>
    /// <exception cref="ArgumentNullException">The command is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Scheduling is canceled.</exception>
    Task ScheduleAtAsync<TCommand>(
        TCommand command,
        DateTimeOffset deliverAt,
        CancellationToken cancellationToken)
        where TCommand : ICommand<Result>;
}
