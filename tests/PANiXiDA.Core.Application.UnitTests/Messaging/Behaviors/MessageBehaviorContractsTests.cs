using PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;
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

    [Fact(DisplayName = "Before message contracts accept domain events")]
    public void BeforeContract_WhenClosedForDomainEvent_AcceptsMessage()
    {
        var contract = typeof(IBeforeMessageBehavior<>).MakeGenericType(typeof(TestDomainEvent));

        contract.IsConstructedGenericType.ShouldBeTrue();
    }

    [Fact(DisplayName = "Before message contracts reject types without a message contract")]
    public void BeforeContract_WhenMessageContractIsMissing_RejectsType()
    {
        static void act() => typeof(IBeforeMessageBehavior<>).MakeGenericType(typeof(string));

        Should.Throw<ArgumentException>(act);
    }
}
