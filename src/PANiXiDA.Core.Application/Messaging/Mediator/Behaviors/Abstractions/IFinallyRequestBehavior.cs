using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;

/// <summary>
/// Defines behavior that runs after request processing completes or fails.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
/// <remarks>
/// Behaviors run in registration order, passing each returned result to the next behavior.
/// Return the supplied result unchanged when no replacement is needed; null means no result exists.
/// The mediator converts a non-generic failure to the declared result type before calling the next behavior.
/// Returning a new failure for a processing exception proposes handling that exception. Passing through an
/// existing result does not handle it. Existing failure results must be preserved, and a non-null result
/// must not be replaced with null. Successful replacements must be assignable to the declared result type.
/// The original processing exception remains available to all behaviors. The mediator must run every behavior,
/// collect cleanup failures separately, and accept exception handling only if all finally behaviors succeed.
/// Cleanup failures must propagate together with the original exception, even if a result was replaced.
/// Unhandled exceptions retain their original instance and stack trace. Background delivery requires an explicit policy.
/// </remarks>
public interface IFinallyRequestBehavior<TRequest, in TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    /// <summary>
    /// Executes behavior after request processing completes, including failure paths.
    /// </summary>
    /// <param name="request">The request that was processed.</param>
    /// <param name="result">The current result returned by the handler or a preceding finally behavior, if any.</param>
    /// <param name="exception">The exception thrown during request processing, if any.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The supplied result or its replacement, or null when no result exists.</returns>
    Task<Result?> FinallyAsync(
        TRequest request,
        TResult? result,
        Exception? exception,
        CancellationToken cancellationToken);
}
