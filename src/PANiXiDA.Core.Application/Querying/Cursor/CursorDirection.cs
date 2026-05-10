using System.ComponentModel.DataAnnotations;

namespace PANiXiDA.Core.Application.Querying.Cursor;

/// <summary>
/// Defines cursor pagination directions.
/// </summary>
public enum CursorDirection
{
    /// <summary>
    /// Reads items after the current cursor.
    /// </summary>
    [Display(Name = "Вперёд")]
    Forward = 1,

    /// <summary>
    /// Reads items before the current cursor.
    /// </summary>
    [Display(Name = "Назад")]
    Backward = 2
}
