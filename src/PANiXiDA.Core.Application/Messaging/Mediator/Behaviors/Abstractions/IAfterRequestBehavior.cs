using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;

/// <summary>
/// Defines behavior that runs after a request handler returns a result.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
public interface IAfterRequestBehavior<TRequest, in TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    /// <summary>
    /// Executes behavior after the request handler returns a result.
    /// </summary>
    /// <param name="request">The request that was processed.</param>
    /// <param name="result">The result returned by the request handler.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AfterAsync(
        TRequest request,
        TResult result,
        CancellationToken cancellationToken);
}
