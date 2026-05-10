namespace PANiXiDA.Core.Application.Persistence.Read.Paged;

/// <summary>
/// Представляет параметры запроса страницы для постраничной выборки.
/// </summary>
/// <param name="PageNumber">Номер запрашиваемой страницы.</param>
/// <param name="PageSize">Количество элементов на странице.</param>
public sealed record PaginationParameters(int PageNumber = 1, int PageSize = 10)
{
    /// <summary>
    /// Возвращает количество элементов, которое нужно пропустить для текущей страницы.
    /// </summary>
    public int Skip => (Math.Max(PageNumber, 1) - 1) * Math.Max(PageSize, 1);

    /// <summary>
    /// Возвращает количество элементов, которое нужно взять для текущей страницы.
    /// </summary>
    public int Take => Math.Max(PageSize, 1);
}
