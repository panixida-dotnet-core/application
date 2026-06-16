using PANiXiDA.Core.Application.Querying.Cursor;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Cursor;

public sealed class CursorPaginationParametersTests
{
    [Fact(DisplayName = "FirstPage returns forward parameters without a cursor")]
    public void FirstPage_WhenLimitIsProvided_ReturnsForwardParametersWithoutCursor()
    {
        var parameters = CursorPaginationParameters.FirstPage(limit: 50);

        parameters.Cursor.ShouldBeNull();
        parameters.Limit.ShouldBe(50);
        parameters.Direction.ShouldBe(CursorDirection.Forward);
    }

    [Fact(DisplayName = "Constructor stores cursor pagination values")]
    public void Constructor_WhenValuesAreProvided_StoresValues()
    {
        var parameters = new CursorPaginationParameters(
            Cursor: "cursor-1",
            Limit: 10,
            Direction: CursorDirection.Backward);

        parameters.Cursor.ShouldBe("cursor-1");
        parameters.Limit.ShouldBe(10);
        parameters.Direction.ShouldBe(CursorDirection.Backward);
    }

    [Fact(DisplayName = "With expression copies and updates cursor pagination parameters")]
    public void WithExpression_WhenValuesAreChanged_CopiesAndUpdatesParameters()
    {
        var parameters = CursorPaginationParameters.FirstPage(limit: 10);

        var updated = parameters with
        {
            Cursor = "cursor-2",
            Limit = 20,
            Direction = CursorDirection.Backward
        };

        updated.Cursor.ShouldBe("cursor-2");
        updated.Limit.ShouldBe(20);
        updated.Direction.ShouldBe(CursorDirection.Backward);
    }

    [Theory(DisplayName = "CursorDirection has localized display names")]
    [InlineData(CursorDirection.Forward, "Вперёд")]
    [InlineData(CursorDirection.Backward, "Назад")]
    public void DisplayName_WhenCursorDirectionIsProvided_ReturnsLocalizedName(
        CursorDirection direction,
        string expectedDisplayName)
    {
        var member = typeof(CursorDirection).GetMember(direction.ToString()).Single();
        var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();

        displayAttribute.ShouldNotBeNull();
        displayAttribute.Name.ShouldBe(expectedDisplayName);
    }
}
