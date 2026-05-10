namespace PANiXiDA.Core.Application.Querying.Pagination;

/// <summary>
/// Represents page-based pagination request parameters.
/// </summary>
/// <param name="PageNumber">The requested page number.</param>
/// <param name="PageSize">The number of items per page.</param>
public sealed record PaginationParameters(int PageNumber = 1, int PageSize = 10)
{
    /// <summary>
    /// Gets the number of items to skip for the current page.
    /// </summary>
    public int Skip => (Math.Max(PageNumber, 1) - 1) * Math.Max(PageSize, 1);

    /// <summary>
    /// Gets the number of items to take for the current page.
    /// </summary>
    public int Take => Math.Max(PageSize, 1);
}
