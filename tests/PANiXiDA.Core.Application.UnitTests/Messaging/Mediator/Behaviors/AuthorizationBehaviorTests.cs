using PANiXiDA.Core.Application.Authentication;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors;

public sealed class AuthorizationBehaviorTests
{
    [Fact(DisplayName = "AuthorizationBehavior rejects a null current user")]
    public void Constructor_WhenCurrentUserIsNull_ThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() =>
            new AuthorizationBehavior<TestCommand, Result, AuthenticatedHandler>(null!));

        exception.ParamName.ShouldBe("currentUser");
    }

    [Fact(DisplayName = "AuthorizationBehavior rejects a null request")]
    public async Task BeforeAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var behavior = new AuthorizationBehavior<TestCommand, Result, AuthenticatedHandler>(
            new RecordingCurrentUser(true));

        var exception = await Should.ThrowAsync<ArgumentNullException>(() =>
            behavior.BeforeAsync(null!, TestContext.Current.CancellationToken));

        exception.ParamName.ShouldBe("request");
    }

    [Theory(DisplayName = "AuthorizationBehavior requires a matching handler that declares authorization")]
    [InlineData(typeof(TestCommandHandler))]
    [InlineData(typeof(UnrelatedHandler))]
    public void AuthorizationBehavior_WhenHandlerDoesNotMatch_RejectsHandler(Type handlerType)
    {
        var behaviorType = typeof(AuthorizationBehavior<,,>);

        void act() => behaviorType.MakeGenericType(typeof(TestCommand), typeof(Result), handlerType);

        Should.Throw<ArgumentException>(act);
    }

    [Fact(DisplayName = "Roles alone do not grant permissions")]
    public async Task BeforeAsync_WhenOnlyRoleIsGranted_ReturnsForbidden()
    {
        var currentUser = new RecordingCurrentUser(true) { Roles = ["Administrator"] };

        var result = await AuthorizeAsync<AllPermissionsHandler>(currentUser);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
    }

    [Fact(DisplayName = "Authentication is required for every combination of permission collections")]
    public async Task BeforeAsync_Should_Reject_Anonymous_User_Before_Checking_Permissions()
    {
        var currentUser = new RecordingCurrentUser(false,
            "tasks.read", "tasks.export", "tasks.manage", "reports.create");

        Result[] results =
        [
            await AuthorizeAsync<AuthenticatedHandler>(currentUser),
            await AuthorizeAsync<AllPermissionsHandler>(currentUser),
            await AuthorizeAsync<AnyPermissionsHandler>(currentUser),
            await AuthorizeAsync<CombinedPermissionsHandler>(currentUser),
            await AuthorizeAsync<NullAllPermissionsHandler>(currentUser),
            await AuthorizeAsync<NullAnyPermissionsHandler>(currentUser)
        ];

        foreach (var result in results)
        {
            var error = result.Errors.ShouldHaveSingleItem();
            error.Type.ShouldBe(ErrorType.Unauthorized);
            error.Message.ShouldBe("Authentication is required.");
        }

        currentUser.CheckedPermissions.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Empty collections authorize authenticated callers without a UserId for commands and queries")]
    public async Task BeforeAsync_Should_Authorize_Without_Permissions_Or_User_Id()
    {
        var currentUser = new RecordingCurrentUser(true);
        IBeforeRequestBehavior<TestCommand, Result, AuthenticatedHandler> behavior =
            new AuthorizationBehavior<TestCommand, Result, AuthenticatedHandler>(currentUser);

        var commandResult = await behavior.BeforeAsync(new TestCommand(), TestContext.Current.CancellationToken);
        var queryResult = await new AuthorizationBehavior<TestQuery, Result<Guid>, AuthenticatedHandler>(currentUser)
            .BeforeAsync(new TestQuery(), TestContext.Current.CancellationToken);

        currentUser.UserId.ShouldBeNull();
        commandResult.IsSuccess.ShouldBeTrue();
        queryResult.IsSuccess.ShouldBeTrue();
        currentUser.CheckedPermissions.ShouldBeEmpty();
    }

    [Theory(DisplayName = "AllPermissions requires every permission and reports only missing permissions")]
    [InlineData("tasks.read, tasks.export")]
    [InlineData("tasks.export", "tasks.read")]
    [InlineData("tasks.read", "tasks.export")]
    [InlineData(null, "tasks.read", "tasks.export")]
    public async Task BeforeAsync_Should_Require_All_Permissions(
        string? missingPermissions,
        params string[] grantedPermissions)
    {
        var currentUser = new RecordingCurrentUser(true, grantedPermissions);

        var result = await AuthorizeAsync<AllPermissionsHandler>(currentUser);

        result.IsSuccess.ShouldBe(missingPermissions is null);
        if (missingPermissions is not null)
        {
            var error = result.Errors.ShouldHaveSingleItem();
            error.Type.ShouldBe(ErrorType.Forbidden);
            error.Message.ShouldBe($"The caller is missing required permissions: {missingPermissions}.");
        }

        currentUser.CheckedPermissions.ShouldBe(["tasks.read", "tasks.export"]);
    }

    [Theory(DisplayName = "AnyPermissions grants access when at least one alternative permission is present")]
    [InlineData(false)]
    [InlineData(false, "tasks.other")]
    [InlineData(true, "tasks.read")]
    [InlineData(true, "tasks.manage")]
    [InlineData(true, "tasks.read", "tasks.manage")]
    public async Task BeforeAsync_Should_Require_Any_Permission(
        bool expectedSuccess,
        params string[] grantedPermissions)
    {
        var currentUser = new RecordingCurrentUser(true, grantedPermissions);

        var result = await AuthorizeAsync<AnyPermissionsHandler>(currentUser);

        result.IsSuccess.ShouldBe(expectedSuccess);
        if (!expectedSuccess)
        {
            var error = result.Errors.ShouldHaveSingleItem();
            error.Type.ShouldBe(ErrorType.Forbidden);
            error.Message.ShouldBe("At least one of the following permissions is required: tasks.read, tasks.manage.");
        }
    }

    [Theory(DisplayName = "Combined collections require all mandatory permissions and at least one alternative")]
    [InlineData(false)]
    [InlineData(false, "tasks.read", "tasks.export")]
    [InlineData(false, "reports.create")]
    [InlineData(false, "tasks.read", "reports.create")]
    [InlineData(false, "tasks.read", "reports.create", "reports.manage")]
    [InlineData(true, "tasks.read", "tasks.export", "reports.create")]
    [InlineData(true, "tasks.read", "tasks.export", "reports.manage")]
    public async Task BeforeAsync_Should_Combine_All_And_Any_With_And(
        bool expectedSuccess,
        params string[] grantedPermissions)
    {
        var currentUser = new RecordingCurrentUser(true, grantedPermissions);

        var result = await AuthorizeAsync<CombinedPermissionsHandler>(currentUser);

        result.IsSuccess.ShouldBe(expectedSuccess);
        if (!expectedSuccess)
        {
            result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
        }
    }

    [Fact(DisplayName = "Null permission collections return a configuration error")]
    public async Task BeforeAsync_Should_Reject_Null_Collections()
    {
        var currentUser = new RecordingCurrentUser(true, "tasks.read");

        Result[] results =
        [
            await AuthorizeAsync<NullAllPermissionsHandler>(currentUser),
            await AuthorizeAsync<NullAnyPermissionsHandler>(currentUser)
        ];

        foreach (var result in results)
        {
            var error = result.Errors.ShouldHaveSingleItem();
            error.Type.ShouldBe(ErrorType.Unexpected);
            error.Message.ShouldBe("Permission collections cannot be null.");
        }

        currentUser.CheckedPermissions.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Invalid permission names in either collection are rejected before checking access")]
    public async Task BeforeAsync_Should_Reject_Invalid_Permission_Names_Before_Checking_Access()
    {
        var currentUser = new RecordingCurrentUser(true, "tasks.read");

        Result[] results =
        [
            await AuthorizeAsync<NullPermissionHandler>(currentUser),
            await AuthorizeAsync<EmptyPermissionHandler>(currentUser),
            await AuthorizeAsync<WhitespacePermissionHandler>(currentUser)
        ];

        foreach (var result in results)
        {
            var error = result.Errors.ShouldHaveSingleItem();
            error.Type.ShouldBe(ErrorType.Unexpected);
            error.Message.ShouldBe("A permission cannot be empty or whitespace.");
        }

        currentUser.CheckedPermissions.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Both collections support explicit static handler interface implementations")]
    [InlineData(false)]
    [InlineData(false, "tasks.update")]
    [InlineData(true, "tasks.update", "tasks.manage")]
    public async Task BeforeAsync_Should_Read_Explicit_Handler_Permissions(
        bool expectedSuccess,
        params string[] grantedPermissions)
    {
        var currentUser = new RecordingCurrentUser(true, grantedPermissions);

        var result = await AuthorizeAsync<ExplicitPermissionsHandler>(currentUser);

        result.IsSuccess.ShouldBe(expectedSuccess);
        if (!expectedSuccess)
        {
            result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Forbidden);
        }

        currentUser.CheckedPermissions.ShouldContain("tasks.update");
    }

    private static Task<Result> AuthorizeAsync<THandler>(RecordingCurrentUser currentUser)
        where THandler : IRequestHandler<TestCommand, Result>, IRequireAuthorization
    {
        return new AuthorizationBehavior<TestCommand, Result, THandler>(currentUser)
            .BeforeAsync(new TestCommand(), TestContext.Current.CancellationToken);
    }

    private sealed record TestCommand : ICommand<Result>;

    private sealed record TestQuery : IQuery<Result<Guid>>;

    private sealed class UnrelatedHandler : IQueryHandler<TestQuery, Result<Guid>>, IRequireAuthorization
    {
        public Task<Result<Guid>> HandleAsync(TestQuery request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private abstract class TestCommandHandler : ICommandHandler<TestCommand, Result>
    {
        public Task<Result> HandleAsync(TestCommand request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class AuthenticatedHandler
        : TestCommandHandler, IQueryHandler<TestQuery, Result<Guid>>, IRequireAuthorization
    {
        public Task<Result<Guid>> HandleAsync(TestQuery request, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class AllPermissionsHandler : TestCommandHandler, IRequireAuthorization
    {
        public static IReadOnlyCollection<string> AllPermissions { get; } = ["tasks.read", "tasks.export"];
    }

    private sealed class AnyPermissionsHandler : TestCommandHandler, IRequireAuthorization
    {
        public static IReadOnlyCollection<string> AnyPermissions { get; } = ["tasks.read", "tasks.manage"];
    }

    private sealed class CombinedPermissionsHandler : TestCommandHandler, IRequireAuthorization
    {
        public static IReadOnlyCollection<string> AllPermissions { get; } = ["tasks.read", "tasks.export"];

        public static IReadOnlyCollection<string> AnyPermissions { get; } = ["reports.create", "reports.manage"];
    }

    private sealed class ExplicitPermissionsHandler : TestCommandHandler, IRequireAuthorization
    {
        static IReadOnlyCollection<string> IRequireAuthorization.AllPermissions => ["tasks.update"];

        static IReadOnlyCollection<string> IRequireAuthorization.AnyPermissions => ["tasks.manage"];
    }

    private sealed class NullAllPermissionsHandler : TestCommandHandler, IRequireAuthorization
    {
        public static IReadOnlyCollection<string> AllPermissions => null!;
    }

    private sealed class NullAnyPermissionsHandler : TestCommandHandler, IRequireAuthorization
    {
        public static IReadOnlyCollection<string> AnyPermissions => null!;
    }

    private sealed class NullPermissionHandler : TestCommandHandler, IRequireAuthorization
    {
        public static IReadOnlyCollection<string> AllPermissions { get; } = ["tasks.read", null!];
    }

    private sealed class EmptyPermissionHandler : TestCommandHandler, IRequireAuthorization
    {
        public static IReadOnlyCollection<string> AllPermissions { get; } = ["tasks.read", ""];
    }

    private sealed class WhitespacePermissionHandler : TestCommandHandler, IRequireAuthorization
    {
        public static IReadOnlyCollection<string> AllPermissions { get; } = ["tasks.read"];

        public static IReadOnlyCollection<string> AnyPermissions { get; } = ["tasks.read", " \t\r\n"];
    }

    private sealed class RecordingCurrentUser(bool isAuthenticated, params string[] permissions) : ICurrentUser
    {
        public bool IsAuthenticated => isAuthenticated;

        public Guid? UserId => null;

        public string? UserName => null;

        public IReadOnlyCollection<string> Roles { get; init; } = [];

        public List<string> CheckedPermissions { get; } = [];

        public bool TryGetClaimValue<T>(string claimType, out T value)
            where T : IParsable<T>
        {
            throw new NotSupportedException();
        }

        public bool HasPermission(string permission)
        {
            CheckedPermissions.Add(permission);
            return permissions.Contains(permission, StringComparer.Ordinal);
        }
    }
}
