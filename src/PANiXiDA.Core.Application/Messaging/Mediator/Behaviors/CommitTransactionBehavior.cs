using PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Commits the active unit-of-work transaction after a successful command result.
/// </summary>
/// <typeparam name="TCommand">The command type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
/// <typeparam name="THandler">The handler requiring an automatically managed transaction.</typeparam>
/// <param name="unitOfWork">The unit of work used to manage transactions.</param>
public sealed class CommitTransactionBehavior<TCommand, TResult, THandler>(IUnitOfWork unitOfWork)
    : IAfterMessageBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
    where THandler : IRequestHandler<TCommand, TResult>, IRequireTransaction
{
    /// <summary>
    /// Commits the active transaction when the command succeeded.
    /// </summary>
    /// <param name="message">The command that was processed.</param>
    /// <param name="result">The command execution result.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AfterAsync(
        TCommand message,
        TResult result,
        CancellationToken cancellationToken)
    {
        if (!unitOfWork.HasActiveTransaction || !result.IsSuccess)
        {
            return Task.CompletedTask;
        }

        return unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}
