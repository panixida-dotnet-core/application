using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

/// <summary>
/// Handles a command and returns its execution result.
/// </summary>
/// <typeparam name="TCommand">The command type handled by the handler.</typeparam>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
}
