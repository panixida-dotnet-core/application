using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Handlers;

public sealed class RequestHandlerTests
{
    [Fact(DisplayName = "Command handlers execute through the common request handler contract")]
    public async Task HandleAsync_WhenCommandHandlerIsUsed_ReturnsCommandResult()
    {
        ICommandHandler<TestCommand, Result<string>> commandHandler = new TestHandler();
        IRequestHandler<TestCommand, Result<string>> requestHandler = commandHandler;
        var command = new TestCommand("command");

        var result = await requestHandler.HandleAsync(command, TestContext.Current.CancellationToken);

        result.Value.ShouldBe("command");
    }

    [Fact(DisplayName = "Query handlers execute through the common request handler contract")]
    public async Task HandleAsync_WhenQueryHandlerIsUsed_ReturnsQueryResult()
    {
        IQueryHandler<TestQuery, Result<string>> queryHandler = new TestHandler();
        IRequestHandler<TestQuery, Result<string>> requestHandler = queryHandler;
        var query = new TestQuery("query");

        var result = await requestHandler.HandleAsync(query, TestContext.Current.CancellationToken);

        result.Value.ShouldBe("query");
    }

    private sealed record TestCommand(string Value) : ICommand<Result<string>>;

    private sealed record TestQuery(string Value) : IQuery<Result<string>>;

    private sealed class TestHandler : ICommandHandler<TestCommand, Result<string>>, IQueryHandler<TestQuery, Result<string>>
    {
        public Task<Result<string>> HandleAsync(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(request.Value));
        }

        public Task<Result<string>> HandleAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(request.Value));
        }
    }
}
