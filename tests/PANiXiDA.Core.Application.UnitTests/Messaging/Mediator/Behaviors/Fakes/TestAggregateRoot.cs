using PANiXiDA.Core.Domain.AggregateRoots;
using PANiXiDA.Core.Domain.DomainEvents;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;

internal sealed class TestAggregateRoot(Guid id) : AggregateRoot<Guid>(id)
{
    public void Raise(DomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }
}
