namespace PANiXiDA.Core.Application.Authentication.Abstractions;

/// <summary>
/// Requires authentication and declares the permissions needed to execute a request handler.
/// </summary>
public interface IRequireAuthorization
{
    /// <summary>
    /// Gets the permissions that must all be granted, or an empty collection.
    /// </summary>
    static virtual IReadOnlyCollection<string> AllPermissions => [];

    /// <summary>
    /// Gets alternative permissions, at least one of which must be granted, or an empty collection.
    /// </summary>
    static virtual IReadOnlyCollection<string> AnyPermissions => [];
}
