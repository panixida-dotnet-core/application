using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator;

/// <summary>
/// Dispatches application commands and queries to their handlers.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a command to the matching command handler.
    /// </summary>
    /// <typeparam name="TResult">The result type returned by the command.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The command execution result.</returns>
    Task<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken)
        where TResult : Result;

    /// <summary>
    /// Sends a query to the matching query handler.
    /// </summary>
    /// <typeparam name="TResult">The result type returned by the query.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The query execution result.</returns>
    Task<TResult> QueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken)
        where TResult : Result;
}
