using PANiXiDA.Core.Domain.Abstractions;

namespace PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;

/// <summary>
/// Defines behavior that runs after a message handler completes.
/// </summary>
/// <typeparam name="TMessage">The message type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type used by the message processing pipeline.</typeparam>
public interface IAfterMessageBehavior<in TMessage, in TResult>
    where TMessage : IMessage
    where TResult : Result
{
    /// <summary>
    /// Executes behavior after the message handler completes.
    /// </summary>
    /// <param name="message">The message that was processed.</param>
    /// <param name="result">The processing result provided by the pipeline.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AfterAsync(
        TMessage message,
        TResult result,
        CancellationToken cancellationToken);
}
