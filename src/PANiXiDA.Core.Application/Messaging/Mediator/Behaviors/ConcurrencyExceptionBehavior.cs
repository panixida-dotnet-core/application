using PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence.Errors;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Converts a direct optimistic concurrency exception to a conflict failure result.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
/// <remarks>
/// Existing failure results are preserved. Other exception types, including aggregates and cancellation,
/// are not handled. Diagnostic exception messages are not exposed in the returned error.
/// The mediator must accept exception handling only after all finally behaviors succeed.
/// </remarks>
public sealed class ConcurrencyExceptionBehavior<TRequest, TResult> : IFinallyMessageBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    /// <summary>
    /// Offers a conflict result for a concurrency exception when there is no existing failure result.
    /// </summary>
    /// <param name="request">The request that was processed.</param>
    /// <param name="result">The current request result, or null when no result was produced.</param>
    /// <param name="exception">The processing exception to inspect, if any.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A conflict failure result for a handled exception, or the supplied result unchanged.</returns>
    public Task<Result?> FinallyAsync(
        TRequest request,
        TResult? result,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        if (result is { IsFailure: true } || exception is not ConcurrencyException)
        {
            return Task.FromResult<Result?>(result);
        }

        return Task.FromResult<Result?>(Result.Failure(
            Error.Conflict("The entity was modified or deleted. Reload it and try again.")));
    }
}
