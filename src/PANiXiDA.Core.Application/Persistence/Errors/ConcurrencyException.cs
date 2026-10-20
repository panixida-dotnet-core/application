namespace PANiXiDA.Core.Application.Persistence.Errors;

/// <summary>
/// Indicates that a persistence operation could not match the expected entity version.
/// </summary>
/// <remarks>
/// Persistence adapters use this exception for optimistic concurrency conflicts, including concurrent deletion.
/// Provider-specific exceptions can be retained as the inner exception.
/// </remarks>
public sealed class ConcurrencyException : Exception
{
    /// <summary>
    /// Initializes a concurrency exception with a default message.
    /// </summary>
    public ConcurrencyException()
        : this("The entity was modified or deleted by another operation.")
    {
    }

    /// <summary>
    /// Initializes a concurrency exception with a diagnostic message.
    /// </summary>
    /// <param name="message">A nonblank diagnostic message.</param>
    /// <exception cref="ArgumentNullException">The message is null.</exception>
    /// <exception cref="ArgumentException">The message is empty or whitespace.</exception>
    public ConcurrencyException(string message)
        : this(message, null)
    {
    }

    /// <summary>
    /// Initializes a concurrency exception with a diagnostic message and the original failure.
    /// </summary>
    /// <param name="message">A nonblank diagnostic message.</param>
    /// <param name="innerException">The original failure, or null when the adapter detected the conflict directly.</param>
    /// <exception cref="ArgumentNullException">The message is null.</exception>
    /// <exception cref="ArgumentException">The message is empty or whitespace.</exception>
    public ConcurrencyException(string message, Exception? innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
    }
}
