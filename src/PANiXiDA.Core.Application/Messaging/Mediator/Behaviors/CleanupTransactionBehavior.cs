using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Rolls back failed command transactions and releases transaction resources.
/// </summary>
/// <typeparam name="TCommand">The command type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
/// <param name="unitOfWork">The unit of work used to manage transactions.</param>
public sealed class CleanupTransactionBehavior<TCommand, TResult>(IUnitOfWork unitOfWork)
    : IFinallyRequestBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    /// <summary>
    /// Rolls back the active transaction on failure and releases transaction resources.
    /// </summary>
    /// <param name="request">The command that was processed.</param>
    /// <param name="result">The command execution result, if any.</param>
    /// <param name="exception">The exception thrown during command processing, if any.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task FinallyAsync(
        TCommand request,
        TResult? result,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        if (!unitOfWork.HasActiveTransaction)
        {
            return;
        }

        if (exception is not null || result is null || !result.IsSuccess)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
        }

        await unitOfWork.DisposeTransactionAsync();
    }
}
