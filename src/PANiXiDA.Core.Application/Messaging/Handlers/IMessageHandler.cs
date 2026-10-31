using PANiXiDA.Core.Domain.Abstractions;

namespace PANiXiDA.Core.Application.Messaging.Handlers;

/// <summary>
/// Associates a handler with the message type it processes.
/// </summary>
/// <typeparam name="TMessage">The message type handled by the handler.</typeparam>
public interface IMessageHandler<in TMessage>
    where TMessage : IMessage
{
}
