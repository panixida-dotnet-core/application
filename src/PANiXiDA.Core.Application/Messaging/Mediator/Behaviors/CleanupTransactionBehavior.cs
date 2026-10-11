using System.Runtime.ExceptionServices;

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
    /// <remarks>
    /// Disposal is attempted even when rollback fails. A single cleanup failure is rethrown unchanged.
    /// Multiple failures, including the supplied exception, are reported in an <see cref="AggregateException" />.
    /// If cleanup succeeds, propagating the supplied exception remains the caller's responsibility.
    /// </remarks>
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

        List<Exception>? cleanupExceptions = null;

        if (exception is not null || result is null || !result.IsSuccess)
        {
            try
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackException)
            {
                cleanupExceptions = [rollbackException];
            }
        }

        try
        {
            await unitOfWork.DisposeTransactionAsync();
        }
        catch (Exception disposeException)
        {
            (cleanupExceptions ??= []).Add(disposeException);
        }

        if (cleanupExceptions is null)
        {
            return;
        }

        if (exception is not null)
        {
            cleanupExceptions.Insert(0, exception);
        }

        if (cleanupExceptions.Count == 1)
        {
            ExceptionDispatchInfo.Throw(cleanupExceptions[0]);
        }

        throw new AggregateException("Multiple errors occurred while cleaning up the transaction.", cleanupExceptions);
    }
}
