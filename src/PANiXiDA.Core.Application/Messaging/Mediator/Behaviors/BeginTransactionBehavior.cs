using System.Diagnostics.CodeAnalysis;

using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Begins a unit-of-work transaction before a command handler executes.
/// </summary>
/// <typeparam name="TCommand">The command type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
/// <typeparam name="THandler">The handler type processing the command.</typeparam>
/// <param name="unitOfWork">The unit of work used to manage transactions.</param>
[SuppressMessage(
    "Major Code Smell",
    "S2436:Types and methods should not have too many generic parameters",
    Justification = "The behavior preserves the command, result, and handler types required by the before-request pipeline contract.")]
public sealed class BeginTransactionBehavior<TCommand, TResult, THandler>(IUnitOfWork unitOfWork)
    : IBeforeRequestBehavior<TCommand, TResult, THandler>
    where TCommand : ICommand<TResult>
    where TResult : Result
    where THandler : IRequestHandler<TCommand, TResult>
{
    /// <summary>
    /// Begins a transaction before the command handler runs.
    /// </summary>
    /// <param name="request">The command being processed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A task that returns a successful result when request processing should continue to the handler.
    /// </returns>
    public async Task<Result> BeforeAsync(
        TCommand request,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        return Result.Success();
    }
}
