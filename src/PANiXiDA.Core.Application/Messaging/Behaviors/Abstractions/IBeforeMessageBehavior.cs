using PANiXiDA.Core.Domain.Abstractions;

namespace PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;

/// <summary>
/// Defines behavior that runs before a message handler is executed.
/// </summary>
/// <typeparam name="TMessage">The message type processed by the behavior.</typeparam>
public interface IBeforeMessageBehavior<in TMessage>
    where TMessage : IMessage
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
