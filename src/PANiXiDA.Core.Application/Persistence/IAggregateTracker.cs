namespace PANiXiDA.Core.Application.Persistence;

public interface IAggregateTracker
{
    void Track(IAggregateRoot aggregateRoot);
    IReadOnlyCollection<IAggregateRoot> GetAll();
    void Clear();
}
