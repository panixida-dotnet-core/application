using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

/// <summary>
/// Handles a query and returns its execution result.
/// </summary>
/// <typeparam name="TQuery">The query type handled by the handler.</typeparam>
/// <typeparam name="TResult">The result type returned by the query.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
    where TResult : Result
{
    /// <summary>
    /// Handles the specified query.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The query execution result.</returns>
    Task<TResult> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken);
}
