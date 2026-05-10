namespace PANiXiDA.Core.Application.Querying.Cursor;

/// <summary>
/// Represents cursor-based pagination request parameters.
/// </summary>
/// <param name="Cursor">The cursor used as the page boundary.</param>
/// <param name="Limit">The maximum number of items to return.</param>
/// <param name="Direction">The read direction relative to the cursor.</param>
public sealed record CursorPaginationParameters(
    string? Cursor,
    int Limit = 10,
    CursorDirection Direction = CursorDirection.Forward)
{
    /// <summary>
    /// Creates parameters for the first page.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <returns>Cursor pagination parameters for the first page.</returns>
    public static CursorPaginationParameters FirstPage(int limit)
    {
        return new CursorPaginationParameters(
            null,
            limit,
            CursorDirection.Forward);
    }
}
