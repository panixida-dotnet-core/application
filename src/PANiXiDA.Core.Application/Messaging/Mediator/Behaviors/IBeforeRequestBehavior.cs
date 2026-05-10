using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

public interface IBeforeRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    Task BeforeAsync(
        TRequest request,
        CancellationToken cancellationToken);
}
