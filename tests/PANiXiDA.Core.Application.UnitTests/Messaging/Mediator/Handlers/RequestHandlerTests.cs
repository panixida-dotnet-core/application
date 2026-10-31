using PANiXiDA.Core.Application.Messaging.EventBus.Handlers;
using PANiXiDA.Core.Application.Messaging.Handlers;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.ResultPattern;
using PANiXiDA.Core.Domain.DomainEvents;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Handlers;

public sealed class RequestHandlerTests
{
    [Fact(DisplayName = "Command handlers execute through the common request handler contract")]
    public async Task HandleAsync_WhenCommandHandlerIsUsed_ReturnsCommandResult()
    {
        ICommandHandler<TestCommand, Result<string>> commandHandler = new TestHandler();
        IRequestHandler<TestCommand, Result<string>> requestHandler = commandHandler;
        IMessageHandler<TestCommand> messageHandler = requestHandler;
        var command = new TestCommand("command");

        var result = await requestHandler.HandleAsync(command, TestContext.Current.CancellationToken);

        result.Value.ShouldBe("command");
        messageHandler.ShouldBeSameAs(commandHandler);
    }

    [Fact(DisplayName = "Query handlers execute through the common request handler contract")]
    public async Task HandleAsync_WhenQueryHandlerIsUsed_ReturnsQueryResult()
    {
        IQueryHandler<TestQuery, Result<string>> queryHandler = new TestHandler();
        IRequestHandler<TestQuery, Result<string>> requestHandler = queryHandler;
        IMessageHandler<TestQuery> messageHandler = requestHandler;
        var query = new TestQuery("query");

        var result = await requestHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        result.Value.ShouldBe("query");
        messageHandler.ShouldBeSameAs(queryHandler);
    }

    [Fact(DisplayName = "Event handlers share the message handler contract without changing their Task result")]
    public async Task HandleAsync_WhenEventHandlerIsUsed_CompletesEventHandling()
    {
        var handler = new TestHandler();
        IEventHandler<TestEvent> eventHandler = handler;
        IMessageHandler<TestEvent> messageHandler = eventHandler;

        await eventHandler.HandleAsync(new TestEvent(), TestContext.Current.CancellationToken);

        handler.EventHandled.ShouldBeTrue();
        messageHandler.ShouldBeSameAs(eventHandler);
    }

    private sealed record TestCommand(string Value) : ICommand<Result<string>>;

    private sealed record TestQuery(string Value) : IQuery<Result<string>>;

    private sealed record TestEvent : DomainEvent;

    private sealed class TestHandler :
        ICommandHandler<TestCommand, Result<string>>,
        IQueryHandler<TestQuery, Result<string>>,
        IEventHandler<TestEvent>
    {
        public bool EventHandled { get; private set; }

        public Task<Result<string>> HandleAsync(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(request.Value));
        }

        public Task<Result<string>> HandleAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(request.Value));
        }

        public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken)
        {
            EventHandled = true;
            return Task.CompletedTask;
        }
    }
}
