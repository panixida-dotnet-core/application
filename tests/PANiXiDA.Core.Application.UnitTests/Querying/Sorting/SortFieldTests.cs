using PANiXiDA.Core.Application.Querying.Sorting;
using SortDirection = PANiXiDA.Core.Application.Querying.Sorting.SortDirection;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Sorting;

public sealed class SortFieldTests
{
    [Theory(DisplayName = "TryParse accepts field paths and case-insensitive short directions")]
    [InlineData("name", "name", SortDirection.Asc)]
    [InlineData("name:asc", "name", SortDirection.Asc)]
    [InlineData("department.name:desc", "department.name", SortDirection.Desc)]
    [InlineData(" Name : ASC ", "Name", SortDirection.Asc)]
    [InlineData("department.name:DeSc", "department.name", SortDirection.Desc)]
    [InlineData("full-name", "full-name", SortDirection.Asc)]
    [InlineData("отдел.название", "отдел.название", SortDirection.Asc)]
    public void TryParse_WhenValueIsValid_ReturnsCriterion(string value, string field, SortDirection order)
    {
        var success = SortField.TryParse(value, out var result);

        success.ShouldBeTrue();
        result.ShouldBe(new SortField(field, order));
    }

    [Theory(DisplayName = "TryParse rejects malformed paths and directions without throwing")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(":asc")]
    [InlineData("name:")]
    [InlineData("name:ascending")]
    [InlineData("name:descending")]
    [InlineData("name:0")]
    [InlineData("name:1")]
    [InlineData("name:-1")]
    [InlineData("name:asc,desc")]
    [InlineData("name:desc:asc")]
    [InlineData("name desc")]
    [InlineData("name,age")]
    [InlineData(".name")]
    [InlineData("name.")]
    [InlineData("department..name")]
    [InlineData("department. name")]
    [InlineData("name\0")]
    public void TryParse_WhenValueIsInvalid_ReturnsFalseAndNull(string? value)
    {
        var success = SortField.TryParse(value, out var result);

        success.ShouldBeFalse();
        result.ShouldBeNull();
    }
}
