namespace PANiXiDA.Core.Application.Persistence.Read.Paged;

/// <summary>
/// Результат пагинации по схеме page/size.
/// </summary>
/// <typeparam name="TItem">Тип элемента.</typeparam>
public sealed class PagedResult<TItem>
{
    /// <summary>
    /// Элементы текущей страницы.
    /// </summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>
    /// Номер текущей страницы.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Размер страницы.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Общее количество элементов.
    /// </summary>
    public long TotalCount { get; }

    /// <summary>
    /// Общее количество страниц.
    /// </summary>
    public long TotalPages { get; }

    /// <summary>
    /// Признак наличия предыдущей страницы.
    /// </summary>
    public bool HasPreviousPage { get; }

    /// <summary>
    /// Признак наличия следующей страницы.
    /// </summary>
    public bool HasNextPage { get; }

    private PagedResult(
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
    /// Создаёт результат пагинации по схеме page/size.
    /// </summary>
    public static PagedResult<TItem> Create(
        IEnumerable<TItem> items,
        int pageNumber,
        int pageSize,
        long totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        return new PagedResult<TItem>(
            items is IReadOnlyList<TItem> readOnlyList
                ? readOnlyList
                : [.. items],
            pageNumber,
            pageSize,
            totalCount);
    }

    /// <summary>
    /// Создаёт пустой результат пагинации по схеме page/size.
    /// </summary>
    public static PagedResult<TItem> Empty(int pageNumber, int pageSize)
    {
        return new PagedResult<TItem>(
            items: [],
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalCount: 0);
    }
}
