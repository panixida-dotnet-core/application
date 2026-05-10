using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;

/// <summary>
/// Defines behavior that runs before a request handler is executed.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
public interface IBeforeRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    /// <summary>
    /// Executes behavior before the request handler runs.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeforeAsync(
        TRequest request,
        CancellationToken cancellationToken);
}
