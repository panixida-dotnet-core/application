namespace PANiXiDA.Core.Application.Querying.Sorting;

/// <summary>
/// Contains sorting criteria in their order of precedence without adding implicit fields.
/// </summary>
/// <param name="Fields">The sorting criteria in their order of precedence.</param>
public sealed record SortingParameters(SortField[] Fields)
{
    /// <summary>
    /// Gets empty sorting parameters.
    /// </summary>
    public static SortingParameters None { get; } = new([]);

    /// <summary>
    /// Gets whether at least one sorting criterion is present.
    /// </summary>
    public bool HasSorting => Fields.Length > 0;

    /// <summary>
    /// Returns empty sorting parameters without an implicit identifier criterion.
    /// </summary>
    /// <returns>Empty sorting parameters.</returns>
    public static SortingParameters Default()
    {
        return None;
    }

    /// <summary>
    /// Creates sorting parameters from criteria in their order of precedence.
    /// </summary>
    /// <param name="fields">The sorting criteria.</param>
    /// <returns>The sorting parameters.</returns>
    public static SortingParameters Of(params SortField[] fields)
    {
        return new SortingParameters(fields);
    }

    /// <summary>
    /// Creates an ascending sorting criterion.
    /// </summary>
    /// <param name="field">The public read model field path.</param>
    /// <returns>The sorting parameters.</returns>
    public static SortingParameters Ascending(string field)
    {
        return new SortingParameters([new SortField(field)]);
    }

    /// <summary>
    /// Creates a descending sorting criterion.
    /// </summary>
    /// <param name="field">The public read model field path.</param>
    /// <returns>The sorting parameters.</returns>
    public static SortingParameters Descending(string field)
    {
        return new SortingParameters([new SortField(field, SortDirection.Desc)]);
    }

    /// <summary>
    /// Appends default criteria whose field paths are not already present, ignoring case.
    /// </summary>
    /// <param name="defaults">Optional default criteria, in their order of precedence.</param>
    /// <returns>The combined criteria, retaining the explicit directions and precedence.</returns>
    /// <remarks>Validate both inputs before merging. No implicit identifier criterion is added.</remarks>
    public SortingParameters WithDefault(SortingParameters? defaults)
    {
        if (defaults is null || !defaults.HasSorting)
        {
            return this;
        }

        var fields = Fields.Select(static field => field.Field).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = defaults.Fields.Where(field => fields.Add(field.Field)).ToArray();

        return missing.Length == 0 ? this : Of([.. Fields, .. missing]);
    }
}
