using PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Saves pending changes after a successful command result.
/// </summary>
/// <typeparam name="TMessage">The message type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
/// <param name="unitOfWork">The unit of work used to save changes.</param>
public sealed class SaveChangesBehavior<TMessage, TResult>(IUnitOfWork unitOfWork)
    : IAfterMessageBehavior<TMessage, TResult>
    where TMessage : ICommand<TResult>
    where TResult : Result
{
    /// <inheritdoc />
    public Task AfterAsync(TMessage message, TResult result, CancellationToken cancellationToken)
    {
        if (!result.IsSuccess)
        {
            return Task.CompletedTask;
        }

        return unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
