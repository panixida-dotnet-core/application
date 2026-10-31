using PANiXiDA.Core.Domain.Abstractions;
using PANiXiDA.Core.Application.Messaging.Handlers;

namespace PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;

/// <summary>
/// Defines behavior that runs before a message handler is executed.
/// </summary>
/// <typeparam name="TMessage">The message type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type used by the message processing pipeline.</typeparam>
/// <typeparam name="THandler">The handler type processing the message.</typeparam>
public interface IBeforeMessageBehavior<TMessage, TResult, THandler>
    where TMessage : IMessage
    where TResult : Result
    where THandler : IMessageHandler<TMessage>
{
    /// <summary>
    /// Executes behavior before the message handler runs.
    /// </summary>
    /// <param name="message">The message being processed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A task that returns a successful result to continue message processing, or a failed result to stop it.
    /// </returns>
    Task<Result> BeforeAsync(
        TMessage message,
        CancellationToken cancellationToken);
}
