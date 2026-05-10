using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

public interface IFinallyRequestBehavior<TRequest, in TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    Task FinallyAsync(
        TRequest request,
        TResult? result,
        Exception? exception,
        CancellationToken cancellationToken);
}
