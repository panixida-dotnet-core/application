using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    Task<TResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken);
}
