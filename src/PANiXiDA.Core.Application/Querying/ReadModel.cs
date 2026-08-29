namespace PANiXiDA.Core.Application.Querying;

/// <summary>
/// Represents the base type for immutable read-side result models.
/// </summary>
#pragma warning disable S2094 // Derived read models provide the actual result state.
public abstract record ReadModel;
#pragma warning restore S2094
