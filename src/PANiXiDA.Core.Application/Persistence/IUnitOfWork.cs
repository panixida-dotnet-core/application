namespace PANiXiDA.Core.Application.Persistence;

/// <summary>
/// Coordinates persistence changes and transaction boundaries for an application request.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Executes an action inside a transaction managed by the unit of work.
    /// </summary>
    /// <param name="action">The action to execute inside the transaction.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Commits the active transaction.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back the active transaction.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Releases resources owned by the active transaction.
    /// </summary>
    /// <returns>A value task that represents the asynchronous operation.</returns>
    ValueTask DisposeTransactionAsync();

    /// <summary>
    /// Gets a value indicating whether the unit of work has an active transaction.
    /// </summary>
    bool HasActiveTransaction { get; }
}
