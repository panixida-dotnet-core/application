namespace PANiXiDA.Core.Application.Persistence;

/// <summary>
/// Defines basic persistence operations for an aggregate root.
/// </summary>
/// <typeparam name="TId">The aggregate root identifier type.</typeparam>
/// <typeparam name="TAggregateRoot">The aggregate root type.</typeparam>
public interface IRepository<TId, TAggregateRoot>
    where TId : struct
    where TAggregateRoot : class, IAggregateRoot
{
    /// <summary>
    /// Gets an aggregate root by its identifier.
    /// </summary>
    /// <param name="id">The aggregate root identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The aggregate root when found; otherwise, <see langword="null"/>.</returns>
    Task<TAggregateRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the aggregate root for insertion.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to add.</param>
    void Add(TAggregateRoot aggregateRoot);

    /// <summary>
    /// Marks the aggregate root for update.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to update.</param>
    void Update(TAggregateRoot aggregateRoot);

    /// <summary>
    /// Marks the aggregate root for deletion.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to delete.</param>
    void Delete(TAggregateRoot aggregateRoot);
}
