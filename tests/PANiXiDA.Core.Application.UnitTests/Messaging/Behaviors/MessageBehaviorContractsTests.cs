using PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.EventBus.Handlers;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Behaviors;

public sealed class MessageBehaviorContractsTests
{
    [Theory(DisplayName = "After and finally message contracts accept domain events")]
    [InlineData(typeof(IAfterMessageBehavior<,>))]
    [InlineData(typeof(IFinallyMessageBehavior<,>))]
    public void MessageContracts_WhenClosedForDomainEvent_AcceptEvent(Type contract)
    {
        var closed = contract.MakeGenericType(typeof(TestDomainEvent), typeof(Result));

        closed.IsConstructedGenericType.ShouldBeTrue();
    }

    [Theory(DisplayName = "Message behavior contracts reject types without a message contract")]
    [InlineData(typeof(IAfterMessageBehavior<,>))]
    [InlineData(typeof(IFinallyMessageBehavior<,>))]
    public void MessageContracts_WhenMessageContractIsMissing_RejectType(Type contract)
    {
        var exception = Should.Throw<ArgumentException>(() => contract.MakeGenericType(typeof(string), typeof(Result)));

        exception.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Before message contracts accept handlers for domain events")]
    public void BeforeContract_WhenClosedForEventHandler_AcceptsHandler()
    {
        var contract = typeof(IBeforeMessageBehavior<,,>).MakeGenericType(
            typeof(TestDomainEvent), typeof(Result), typeof(IEventHandler<TestDomainEvent>));

        contract.IsConstructedGenericType.ShouldBeTrue();
    }

    [Fact(DisplayName = "Before message contracts reject handlers for another message type")]
    public void BeforeContract_WhenHandlerProcessesAnotherMessage_RejectsHandler()
    {
        var exception = Should.Throw<ArgumentException>(() => typeof(IBeforeMessageBehavior<,,>).MakeGenericType(
            typeof(TestDomainEvent), typeof(Result), typeof(IRequestHandler<TestCommand, Result>)));

        exception.ShouldNotBeNull();
    }
}
