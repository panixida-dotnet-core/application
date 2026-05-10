namespace PANiXiDA.Core.Application.Querying.Sorting;

/// <summary>
/// Represents sorting parameters.
/// </summary>
/// <param name="Field">The field to sort by.</param>
/// <param name="Order">The sort direction.</param>
public sealed record SortParameters(
    string? Field = null,
    SortOrder Order = SortOrder.Ascending)
{
    /// <summary>
    /// Creates default sorting parameters.
    /// </summary>
    /// <returns>Default sorting parameters.</returns>
    public static SortParameters Default()
    {
        return new SortParameters();
    }
}
