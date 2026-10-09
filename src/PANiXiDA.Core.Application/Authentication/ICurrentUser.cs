using System.Diagnostics.CodeAnalysis;

namespace PANiXiDA.Core.Application.Authentication;

/// <summary>
/// Provides transport-independent information about the current caller and its permissions.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets whether the current caller has been authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the user account identifier, or null when no user account is associated with the caller.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the user account name, or null when it is unavailable.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the current user's role names, or an empty collection when no roles are available.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Attempts to parse the authenticated caller's first claim of the specified type.
    /// </summary>
    /// <typeparam name="T">The claim value type that supports string parsing.</typeparam>
    /// <param name="claimType">The claim type to read.</param>
    /// <param name="value">The parsed value, or the default value when parsing fails.</param>
    /// <returns>True when parsing with invariant culture succeeds; otherwise, false.</returns>
    bool TryGetClaimValue<T>(string claimType, [MaybeNullWhen(false)] out T value)
        where T : IParsable<T>;

    /// <summary>
    /// Checks whether the authenticated caller has the specified permission.
    /// </summary>
    /// <param name="permission">The complete permission name.</param>
    /// <returns>True when the caller is authenticated and has the permission; otherwise, false.</returns>
    bool HasPermission(string permission);
}
