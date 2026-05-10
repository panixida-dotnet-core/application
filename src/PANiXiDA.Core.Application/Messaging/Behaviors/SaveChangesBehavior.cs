using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.Messaging.Behaviors;

public sealed class SaveChangesBehavior<TCommand, TResult>(IUnitOfWork unitOfWork)
    : IAfterRequestBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    /// <summary>
    /// Сохраняет изменения после успешного выполнения команды.
    /// </summary>
    public Task AfterAsync(
        TCommand request,
        TResult result,
        CancellationToken cancellationToken)
    {
        if (!unitOfWork.HasActiveTransaction || !result.IsSuccess)
        {
            return Task.CompletedTask;
        }

        return unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
