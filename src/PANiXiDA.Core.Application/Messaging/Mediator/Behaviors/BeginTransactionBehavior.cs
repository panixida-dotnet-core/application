using System.Diagnostics.CodeAnalysis;

using PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Begins a unit-of-work transaction before a command handler executes.
/// </summary>
/// <typeparam name="TMessage">The message type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
/// <typeparam name="THandler">The handler type processing the command.</typeparam>
/// <param name="unitOfWork">The unit of work used to manage transactions.</param>
[SuppressMessage("Design", "S2326", Justification = "THandler restricts pipeline binding to request handlers that require a transaction.")]
public sealed class BeginTransactionBehavior<TMessage, TResult, THandler>(IUnitOfWork unitOfWork)
    : IBeforeMessageBehavior<TMessage>
    where TMessage : ICommand<TResult>
    where TResult : Result
    where THandler : IRequestHandler<TMessage, TResult>, IRequireTransaction
{
    /// <summary>
    /// Begins a transaction before the command handler runs.
    /// </summary>
    /// <param name="message">The command being processed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A task that returns a successful result when message processing should continue to the handler.
    /// </returns>
    public async Task<Result> BeforeAsync(
        TMessage message,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        return Result.Success();
    }
}
