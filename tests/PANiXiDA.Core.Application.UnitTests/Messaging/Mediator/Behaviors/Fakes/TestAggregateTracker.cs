using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Domain.AggregateRoots;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;

internal sealed class TestAggregateTracker : IAggregateTracker
{
    private readonly List<IAggregateRoot> aggregateRoots = [];

    public int ClearCalls { get; private set; }

    public void Track(IAggregateRoot aggregateRoot)
    {
        aggregateRoots.Add(aggregateRoot);
    }

    public IReadOnlyCollection<IAggregateRoot> GetAll()
    {
        return aggregateRoots;
    }

    public void Clear()
    {
        ClearCalls++;
        aggregateRoots.Clear();
    }
}
