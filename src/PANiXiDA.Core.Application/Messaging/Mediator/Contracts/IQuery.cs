namespace PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

public interface IQuery<out TResult> : IRequest<TResult>
    where TResult : Result
{
}
