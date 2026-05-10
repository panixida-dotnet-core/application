namespace PANiXiDA.Core.Application.Persistence.Read.Cursor;

/// <summary>
/// Параметры курсорной пагинации.
/// </summary>
/// <param name="Cursor">Курсор, относительно которого строится выборка.</param>
/// <param name="Limit">Максимальное количество элементов в ответе.</param>
/// <param name="Direction">Направление чтения относительно курсора.</param>
public sealed record CursorPaginationParameters(
    string? Cursor,
    int Limit = 10,
    CursorDirection Direction = CursorDirection.Forward)
{
    /// <summary>
    /// Возвращает параметры для запроса первой страницы.
    /// </summary>
    public static CursorPaginationParameters FirstPage(int limit)
    {
        return new CursorPaginationParameters(
            null,
            limit,
            CursorDirection.Forward);
    }
}
