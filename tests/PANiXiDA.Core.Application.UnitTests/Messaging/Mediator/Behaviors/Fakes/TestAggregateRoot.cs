using PANiXiDA.Core.Domain.AggregateRoots;
using PANiXiDA.Core.Domain.DomainEvents;
using PANiXiDA.Core.Domain.Identifiers;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;

internal readonly record struct TestAggregateRootId(Guid Value) : IStronglyTypedId;

internal sealed class TestAggregateRoot(TestAggregateRootId id) : AggregateRoot<TestAggregateRootId>(id)
{
    public void Raise(DomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }
}
