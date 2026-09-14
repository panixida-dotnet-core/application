using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace PANiXiDA.Core.Application.Querying.Sorting;

/// <summary>
/// Contains public sortable field paths for a read model without inspecting its runtime type.
/// </summary>
/// <typeparam name="TReadModel">The read model described by these fields.</typeparam>
[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed",
    Justification = "The type parameter binds field metadata to its read model for typed validation and dependency injection.")]
public sealed class SortDefinition<TReadModel>
{
    /// <summary>
    /// Gets the immutable set of field paths, compared without regard to case.
    /// </summary>
    public IReadOnlySet<string> Fields { get; }

    /// <summary>
    /// Creates a definition from field paths supplied by generated code or a composition root.
    /// </summary>
    /// <param name="fields">The public sortable field paths.</param>
    /// <exception cref="ArgumentNullException">The array is null.</exception>
    /// <exception cref="ArgumentException">A path is invalid or occurs more than once, ignoring case.</exception>
    public SortDefinition(params string[] fields)
    {
        ArgumentNullException.ThrowIfNull(fields);

        var uniqueFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in fields)
        {
            if (!SortField.IsValidFieldPath(field))
            {
                throw new ArgumentException("Sorting field paths must be valid non-empty paths.", nameof(fields));
            }

            if (!uniqueFields.Add(field))
            {
                throw new ArgumentException($"Sorting field '{field}' is defined more than once.", nameof(fields));
            }
        }

        Fields = uniqueFields.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }
}
