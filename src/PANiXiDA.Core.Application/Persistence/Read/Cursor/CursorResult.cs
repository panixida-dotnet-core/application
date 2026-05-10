namespace PANiXiDA.Core.Application.Persistence.Read.Cursor;

/// <summary>
/// Результат курсорной пагинации.
/// </summary>
/// <typeparam name="TItem">Тип элемента.</typeparam>
public sealed class CursorResult<TItem>
{
    /// <summary>
    /// Элементы текущей страницы.
    /// </summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>
    /// Лимит элементов, который запрашивался.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Курсор для получения следующей страницы.
    /// </summary>
    public string? NextCursor { get; }

    /// <summary>
    /// Курсор для получения предыдущей страницы.
    /// </summary>
    public string? PreviousCursor { get; }

    /// <summary>
    /// Признак наличия следующей страницы.
    /// </summary>
    public bool HasNextPage { get; }

    /// <summary>
    /// Признак наличия предыдущей страницы.
    /// </summary>
    public bool HasPreviousPage { get; }

    private CursorResult(
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
    /// Создаёт результат курсорной пагинации.
    /// </summary>
    public static CursorResult<TItem> Create(
        IEnumerable<TItem> items,
        int limit,
        string? nextCursor = null,
        string? previousCursor = null,
        bool hasNextPage = false,
        bool hasPreviousPage = false)
    {
        ArgumentNullException.ThrowIfNull(items);

        return new CursorResult<TItem>(
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
    /// Создаёт пустой результат курсорной пагинации.
    /// </summary>
    public static CursorResult<TItem> Empty(int limit)
    {
        return new CursorResult<TItem>(
            items: [],
            limit: limit,
            nextCursor: null,
            previousCursor: null,
            hasNextPage: false,
            hasPreviousPage: false);
    }
}
