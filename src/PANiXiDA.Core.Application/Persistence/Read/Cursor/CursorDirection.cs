using System.ComponentModel.DataAnnotations;

namespace PANiXiDA.Core.Application.Persistence.Read.Cursor;

/// <summary>
/// Направление курсорной пагинации.
/// </summary>
public enum CursorDirection
{
    /// <summary>
    /// Выполняет чтение вперёд от текущего курсора.
    /// </summary>
    [Display(Name = "Вперёд")]
    Forward = 1,

    /// <summary>
    /// Выполняет чтение назад от текущего курсора.
    /// </summary>
    [Display(Name = "Назад")]
    Backward = 2
}
