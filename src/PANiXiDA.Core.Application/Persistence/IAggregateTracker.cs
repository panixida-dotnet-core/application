namespace PANiXiDA.Core.Application.Persistence;

/// <summary>
/// Tracks aggregate roots touched during an application request.
/// </summary>
public interface IAggregateTracker
{
    /// <summary>
    /// Adds an aggregate root to the current tracking scope.
    /// </summary>
    /// <param name="aggregateRoot">The aggregate root to track.</param>
    void Track(IAggregateRoot aggregateRoot);

    /// <summary>
    /// Gets all aggregate roots tracked in the current scope.
    /// </summary>
    /// <returns>The tracked aggregate roots.</returns>
    IReadOnlyCollection<IAggregateRoot> GetAll();

    /// <summary>
    /// Clears the current aggregate tracking scope.
    /// </summary>
    void Clear();
}
