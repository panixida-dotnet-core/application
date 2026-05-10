using System.ComponentModel.DataAnnotations;

namespace PANiXiDA.Core.Application.Persistence.Read.Sorting;

/// <summary>
/// Направление сортировки.
/// </summary>
public enum SortOrder
{
    [Display(Name = "По возрастанию")]
    Ascending = 0,

    [Display(Name = "По убыванию")]
    Descending = 1,
}
