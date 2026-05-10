namespace PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

/// <summary>
/// Defines an application request that returns a result.
/// </summary>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
public interface IRequest<out TResult>
{
}
