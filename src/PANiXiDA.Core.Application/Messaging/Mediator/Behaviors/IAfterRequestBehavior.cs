using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

public interface IAfterRequestBehavior<TRequest, in TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    Task AfterAsync(
        TRequest request,
        TResult result,
        CancellationToken cancellationToken);
}
