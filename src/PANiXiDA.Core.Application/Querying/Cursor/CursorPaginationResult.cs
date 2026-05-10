namespace PANiXiDA.Core.Application.Querying.Cursor;

/// <summary>
/// Represents a cursor-based pagination result.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
public sealed class CursorPaginationResult<TItem>
{
    /// <summary>
    /// Gets the current page items.
    /// </summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>
    /// Gets the requested item limit.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Gets the cursor used to request the next page.
    /// </summary>
    public string? NextCursor { get; }

    /// <summary>
    /// Gets the cursor used to request the previous page.
    /// </summary>
    public string? PreviousCursor { get; }

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage { get; }

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage { get; }

    private CursorPaginationResult(
        IReadOnlyList<TItem> items,
        int limit,
        string? nextCursor,
        string? previousCursor,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                limit,
                "Лимит должен быть больше 0.");
        }

        Items = items;
        Limit = limit;
        NextCursor = nextCursor;
        PreviousCursor = previousCursor;
        HasNextPage = hasNextPage;
        HasPreviousPage = hasPreviousPage;
    }

    /// <summary>
    /// Creates a cursor-based pagination result.
    /// </summary>
    /// <param name="items">The current page items.</param>
    /// <param name="limit">The requested item limit.</param>
    /// <param name="nextCursor">The cursor used to request the next page.</param>
    /// <param name="previousCursor">The cursor used to request the previous page.</param>
    /// <param name="hasNextPage">A value indicating whether a next page exists.</param>
    /// <param name="hasPreviousPage">A value indicating whether a previous page exists.</param>
    /// <returns>A cursor-based pagination result.</returns>
    public static CursorPaginationResult<TItem> Create(
        IEnumerable<TItem> items,
        int limit,
        string? nextCursor = null,
        string? previousCursor = null,
        bool hasNextPage = false,
        bool hasPreviousPage = false)
    {
        ArgumentNullException.ThrowIfNull(items);

        return new CursorPaginationResult<TItem>(
            items is IReadOnlyList<TItem> readOnlyList
                ? readOnlyList
                : [.. items],
            limit,
            nextCursor,
            previousCursor,
            hasNextPage,
            hasPreviousPage);
    }

    /// <summary>
    /// Creates an empty cursor-based pagination result.
    /// </summary>
    /// <param name="limit">The requested item limit.</param>
    /// <returns>An empty cursor-based pagination result.</returns>
    public static CursorPaginationResult<TItem> Empty(int limit)
    {
        return new CursorPaginationResult<TItem>(
            items: [],
            limit: limit,
            nextCursor: null,
            previousCursor: null,
            hasNextPage: false,
            hasPreviousPage: false);
    }
}
