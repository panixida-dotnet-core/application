namespace PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

public interface ICommand<out TResult> : IRequest<TResult>
    where TResult : Result
{
}
