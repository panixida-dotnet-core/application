using System.ComponentModel.DataAnnotations;

namespace PANiXiDA.Core.Application.Querying.Sorting;

/// <summary>
/// Defines sort directions.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Sorts values in ascending order.
    /// </summary>
    [Display(Name = "По возрастанию")]
    Asc = 0,

    /// <summary>
    /// Sorts values in descending order.
    /// </summary>
    [Display(Name = "По убыванию")]
    Desc = 1,
}
