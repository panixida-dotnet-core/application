using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

/// <summary>
/// Handles a command or query and returns its execution result.
/// </summary>
/// <typeparam name="TRequest">The request type handled by the handler.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
public interface IRequestHandler<in TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The request execution result.</returns>
    Task<TResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken);
}
