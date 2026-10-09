using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;

/// <summary>
/// Defines behavior that runs before a request handler is executed.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
/// <typeparam name="THandler">The handler type processing the request.</typeparam>
public interface IBeforeRequestBehavior<TRequest, TResult, THandler>
    where TRequest : IRequest<TResult>
    where TResult : Result
    where THandler : IRequestHandler<TRequest, TResult>
{
    /// <summary>
    /// Executes behavior before the request handler runs.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A task that returns a successful result to continue request processing, or a failed result to stop it.
    /// </returns>
    Task<Result> BeforeAsync(
        TRequest request,
        CancellationToken cancellationToken);
}
