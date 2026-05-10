using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Begins a unit-of-work transaction before a command handler executes.
/// </summary>
/// <typeparam name="TCommand">The command type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
/// <param name="unitOfWork">The unit of work used to manage transactions.</param>
public sealed class BeginTransactionBehavior<TCommand, TResult>(IUnitOfWork unitOfWork)
    : IBeforeRequestBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    /// <summary>
    /// Begins a transaction before the command handler runs.
    /// </summary>
    /// <param name="request">The command being processed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task BeforeAsync(
        TCommand request,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
    }
}
