namespace PANiXiDA.Core.Application.Persistence.Read.Sorting;

/// <summary>
/// Параметры сортировки.
/// </summary>
/// <param name="Field">Поле сортировки.</param>
/// <param name="Order">Направление сортировки.</param>
public sealed record SortParameters(
    string? Field = null,
    SortOrder Order = SortOrder.Ascending)
{
    /// <summary>
    /// Возвращает параметры сортировки по умолчанию.
    /// </summary>
    public static SortParameters Default()
    {
        return new SortParameters();
    }
}
