using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;

internal sealed record TestCommand : ICommand<Result>;
