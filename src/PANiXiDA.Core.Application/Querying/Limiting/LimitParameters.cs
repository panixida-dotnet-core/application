namespace PANiXiDA.Core.Application.Querying.Limiting;

/// <summary>
/// Represents a requested result count limit without pagination.
/// </summary>
/// <param name="Limit">The maximum number of items to return.</param>
public sealed record LimitParameters(int Limit = 20);
