using System.Diagnostics.CodeAnalysis;

namespace PANiXiDA.Core.Application.Querying.Sorting;

/// <summary>
/// Describes one sorting criterion using a public read model field path.
/// </summary>
/// <param name="Field">The field path, such as <c>department.name</c>.</param>
/// <param name="Order">The sort direction.</param>
public sealed record SortField(string Field, SortDirection Order = SortDirection.Asc)
{
    /// <summary>
    /// Parses a field path with an optional <c>:asc</c> or <c>:desc</c> suffix, ignoring case.
    /// </summary>
    /// <param name="value">The query parameter value. Surrounding whitespace is ignored.</param>
    /// <param name="result">The parsed criterion when successful; otherwise, null.</param>
    /// <returns>Whether the value has a valid field path and direction.</returns>
    public static bool TryParse(string? value, [NotNullWhen(true)] out SortField? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split(':', 2, StringSplitOptions.TrimEntries);

        if (!IsValidFieldPath(parts[0]))
        {
            return false;
        }

        var order = SortDirection.Asc;

        if (parts.Length == 2)
        {
            if (string.Equals(parts[1], nameof(SortDirection.Desc), StringComparison.OrdinalIgnoreCase))
            {
                order = SortDirection.Desc;
            }
            else if (!string.Equals(parts[1], nameof(SortDirection.Asc), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        result = new SortField(parts[0], order);
        return true;
    }

    internal static bool IsValidFieldPath(string? field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return false;
        }

        var segmentLength = 0;

        foreach (var character in field)
        {
            if (character == '.')
            {
                if (segmentLength == 0)
                {
                    return false;
                }

                segmentLength = 0;
            }
            else
            {
                if (char.IsWhiteSpace(character) || char.IsControl(character) || character is ':' or ',')
                {
                    return false;
                }

                segmentLength++;
            }
        }

        return segmentLength > 0;
    }
}
