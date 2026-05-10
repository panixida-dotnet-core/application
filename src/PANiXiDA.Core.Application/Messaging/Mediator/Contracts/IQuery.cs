namespace PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

/// <summary>
/// Defines a read-only application request.
/// </summary>
/// <typeparam name="TResult">The result type returned by the query.</typeparam>
public interface IQuery<out TResult> : IRequest<TResult>
    where TResult : Result
{
}
