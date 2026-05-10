namespace PANiXiDA.Core.Application.Querying.Pagination;

/// <summary>
/// Represents a page-based pagination result.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
public sealed class PaginationResult<TItem>
{
    /// <summary>
    /// Gets the current page items.
    /// </summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the current page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total item count.
    /// </summary>
    public long TotalCount { get; }

    /// <summary>
    /// Gets the total page count.
    /// </summary>
    public long TotalPages { get; }

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage { get; }

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage { get; }

    private PaginationResult(
        IReadOnlyList<TItem> items,
        int pageNumber,
        int pageSize,
        long totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (pageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "Номер страницы должен быть больше 0.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                "Размер страницы должен быть больше 0.");
        }

        if (totalCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalCount),
                totalCount,
                "Общее количество элементов не может быть отрицательным.");
        }

        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalCount == 0
            ? 0
            : (totalCount + pageSize - 1) / pageSize;

        HasPreviousPage = pageNumber > 1;
        HasNextPage = pageNumber < TotalPages;
    }

    /// <summary>
    /// Creates a page-based pagination result.
    /// </summary>
    /// <param name="items">The current page items.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The current page size.</param>
    /// <param name="totalCount">The total item count.</param>
    /// <returns>A page-based pagination result.</returns>
    public static PaginationResult<TItem> Create(
        IEnumerable<TItem> items,
        int pageNumber,
        int pageSize,
        long totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        return new PaginationResult<TItem>(
            items is IReadOnlyList<TItem> readOnlyList
                ? readOnlyList
                : [.. items],
            pageNumber,
            pageSize,
            totalCount);
    }

    /// <summary>
    /// Creates an empty page-based pagination result.
    /// </summary>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The current page size.</param>
    /// <returns>An empty page-based pagination result.</returns>
    public static PaginationResult<TItem> Empty(int pageNumber, int pageSize)
    {
        return new PaginationResult<TItem>(
            items: [],
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalCount: 0);
    }
}
