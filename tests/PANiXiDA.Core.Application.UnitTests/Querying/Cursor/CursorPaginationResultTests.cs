using PANiXiDA.Core.Application.Querying.Cursor;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Cursor;

public sealed class CursorPaginationResultTests
{
    [Fact(DisplayName = "Create returns cursor page metadata")]
    public void Create_WhenParametersAreValid_ReturnsCursorMetadata()
    {
        IReadOnlyList<int> items = [1, 2];

        var result = CursorPaginationResult<int>.Create(
            items,
            limit: 2,
            nextCursor: "next",
            previousCursor: "previous",
            hasNextPage: true,
            hasPreviousPage: true);

        result.Items.Should().BeSameAs(items);
        result.Limit.Should().Be(2);
        result.NextCursor.Should().Be("next");
        result.PreviousCursor.Should().Be("previous");
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact(DisplayName = "Create copies enumerable cursor items into a read-only list")]
    public void Create_WhenItemsAreEnumerable_CopiesItemsIntoReadOnlyList()
    {
        var items = Enumerable.Range(1, 2).Where(item => item > 0);

        var result = CursorPaginationResult<int>.Create(items, limit: 10);

        result.Items.Should().Equal(1, 2);
        result.Items.Should().BeAssignableTo<IReadOnlyList<int>>();
        result.NextCursor.Should().BeNull();
        result.PreviousCursor.Should().BeNull();
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact(DisplayName = "Empty returns an empty cursor result")]
    public void Empty_WhenLimitIsValid_ReturnsEmptyResult()
    {
        var result = CursorPaginationResult<string>.Empty(limit: 25);

        result.Items.Should().BeEmpty();
        result.Limit.Should().Be(25);
        result.NextCursor.Should().BeNull();
        result.PreviousCursor.Should().BeNull();
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact(DisplayName = "Create throws when cursor items are null")]
    public void Create_WhenItemsAreNull_Throws()
    {
        Action act = () => CursorPaginationResult<int>.Create(items: null!, limit: 10);

        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("items");
    }

    [Fact(DisplayName = "Create throws when cursor limit is not positive")]
    public void Create_WhenLimitIsNotPositive_Throws()
    {
        Action act = () => CursorPaginationResult<int>.Create(items: [], limit: 0);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("limit")
            .WithMessage("*Лимит должен быть больше 0.*");
    }
}
