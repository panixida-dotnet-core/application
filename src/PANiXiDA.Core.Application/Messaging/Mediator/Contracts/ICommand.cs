namespace PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

/// <summary>
/// Defines a state-changing application request.
/// </summary>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
public interface ICommand<out TResult> : IRequest<TResult>
    where TResult : Result
{
}
