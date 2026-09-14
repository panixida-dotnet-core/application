using PANiXiDA.Core.Application.Querying.Sorting;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Sorting;

public sealed class SortFieldTests
{
    [Theory(DisplayName = "TryParse accepts field paths and case-insensitive short directions")]
    [InlineData("name", "name", SortOrder.Asc)]
    [InlineData("name:asc", "name", SortOrder.Asc)]
    [InlineData("department.name:desc", "department.name", SortOrder.Desc)]
    [InlineData(" Name : ASC ", "Name", SortOrder.Asc)]
    [InlineData("department.name:DeSc", "department.name", SortOrder.Desc)]
    [InlineData("full-name", "full-name", SortOrder.Asc)]
    [InlineData("отдел.название", "отдел.название", SortOrder.Asc)]
    public void TryParse_WhenValueIsValid_ReturnsCriterion(string value, string field, SortOrder order)
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
