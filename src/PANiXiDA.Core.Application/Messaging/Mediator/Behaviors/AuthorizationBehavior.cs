using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Checks authentication and required permissions before a request handler executes.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
/// <typeparam name="THandler">The handler type declaring the authorization requirements.</typeparam>
/// <param name="currentUser">The current caller.</param>
public sealed class AuthorizationBehavior<TRequest, TResult, THandler>(ICurrentUser currentUser)
    : IBeforeRequestBehavior<TRequest, TResult, THandler>
    where TRequest : IRequest<TResult>
    where TResult : Result
    where THandler : IRequestHandler<TRequest, TResult>, IRequireAuthorization
{
    /// <summary>
    /// Requires authentication, all mandatory permissions, and at least one configured alternative permission.
    /// </summary>
    /// <param name="request">The request whose handler is being authorized.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// Success when access is granted, Unauthorized for an unauthenticated caller,
    /// Forbidden for missing permissions, or Unexpected for invalid permission declarations.
    /// </returns>
    public Task<Result> BeforeAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Task.FromResult(
                Result.Failure(Error.Unauthorized("Authentication is required.")));
        }

        var allPermissions = THandler.AllPermissions;
        var anyPermissions = THandler.AnyPermissions;

        if (allPermissions is null || anyPermissions is null)
        {
            return Task.FromResult(
                Result.Failure(Error.Unexpected("Permission collections cannot be null.")));
        }

        if (allPermissions.Concat(anyPermissions).Any(string.IsNullOrWhiteSpace))
        {
            return Task.FromResult(
                Result.Failure(Error.Unexpected("A permission cannot be empty or whitespace.")));
        }

        var missingPermissions = allPermissions
            .Where(permission => !currentUser.HasPermission(permission))
            .ToArray();

        if (missingPermissions.Length > 0)
        {
            return Task.FromResult(
                Result.Failure(Error.Forbidden(
                    $"The caller is missing required permissions: {string.Join(", ", missingPermissions)}.")));
        }

        if (anyPermissions.Count > 0 && !anyPermissions.Any(currentUser.HasPermission))
        {
            return Task.FromResult(
                Result.Failure(Error.Forbidden(
                    $"At least one of the following permissions is required: {string.Join(", ", anyPermissions)}.")));
        }

        return Task.FromResult(Result.Success());
    }
}
