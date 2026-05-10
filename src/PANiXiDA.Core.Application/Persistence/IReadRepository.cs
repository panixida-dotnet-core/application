namespace PANiXiDA.Core.Application.Persistence;

/// <summary>
/// Defines read-only persistence checks for an entity or aggregate root identifier.
/// </summary>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IReadRepository<TId>
    where TId : struct
{
    /// <summary>
    /// Determines whether an item with the specified identifier exists.
    /// </summary>
    /// <param name="id">The item identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if an item with the identifier exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsByIdAsync(
        TId id,
        CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the read repository contains any items.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if at least one item exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> AnyAsync(CancellationToken cancellationToken);
}
