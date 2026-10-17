using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;

/// <summary>
/// Offers a failure result for an exception after request cleanup has completed successfully.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
/// <remarks>
/// A consuming mediator invokes this stage only for a processing exception after all finally behaviors succeed.
/// Cleanup failures must propagate without invoking this stage. A failed request result must not be replaced.
/// Behaviors run in registration order until one returns a non-null failure result. The mediator converts its errors
/// to the declared request result type, preserving support for both Result and Result&lt;T&gt;.
/// A successful returned result is a contract violation. When all behaviors decline, the original exception
/// must propagate with its original instance and stack trace. Background delivery requires an explicit handling policy.
/// </remarks>
public interface IExceptionRequestBehavior<TRequest, in TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    /// <summary>
    /// Attempts to convert an expected processing exception to a failure result.
    /// </summary>
    /// <param name="request">The request that was processed.</param>
    /// <param name="result">The last request result, or null when no result was produced.</param>
    /// <param name="exception">The non-null processing exception.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A failure result when the exception is handled, or null to leave it unhandled.</returns>
    Task<Result?> HandleExceptionAsync(
        TRequest request,
        TResult? result,
        Exception exception,
        CancellationToken cancellationToken);
}
