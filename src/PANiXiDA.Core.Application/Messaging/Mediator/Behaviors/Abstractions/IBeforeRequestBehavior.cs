using System.Diagnostics.CodeAnalysis;

using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;

/// <summary>
/// Defines behavior that runs before a request handler is executed.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
/// <typeparam name="THandler">The handler type processing the request.</typeparam>
[SuppressMessage(
    "Major Code Smell",
    "S2326:Unused type parameters should be removed",
    Justification = "THandler identifies the concrete handler so the pipeline can select behaviors by handler constraints.")]
[SuppressMessage(
    "Major Code Smell",
    "S2436:Types and methods should not have too many generic parameters",
    Justification = "The pipeline requires request, result, and handler types to select and construct compatible behaviors.")]
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
