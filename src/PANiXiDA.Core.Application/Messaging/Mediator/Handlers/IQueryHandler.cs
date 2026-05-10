using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
    where TResult : Result
{
    Task<TResult> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken);
}
