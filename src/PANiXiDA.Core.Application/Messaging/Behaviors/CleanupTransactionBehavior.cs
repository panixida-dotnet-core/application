using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Behaviors;

public sealed class CleanupTransactionBehavior<TCommand, TResult>(IUnitOfWork unitOfWork)
    : IFinallyRequestBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    /// <summary>
    /// Выполняет откат транзакции при ошибке и освобождает её ресурсы.
    /// </summary>
    public Task FinallyAsync(
        TCommand request,
        TResult? result,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        var shouldRollback =
            unitOfWork.HasActiveTransaction &&
            (exception is not null ||
            result is null ||
            !result.IsSuccess);

        if (shouldRollback)
        {
            return unitOfWork.RollbackTransactionAsync(cancellationToken);
        }

        return Task.CompletedTask;
    }
}
