using PANiXiDA.Core.Application.Querying.Cursor;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Cursor;

public sealed class CursorPaginationResultTests
{
    private static readonly int[] ExpectedItems = [1, 2];

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

        result.Items.ShouldBeSameAs(items);
        result.Limit.ShouldBe(2);
        result.NextCursor.ShouldBe("next");
        result.PreviousCursor.ShouldBe("previous");
        result.HasNextPage.ShouldBeTrue();
        result.HasPreviousPage.ShouldBeTrue();
    }

    [Fact(DisplayName = "Create copies enumerable cursor items into a read-only list")]
    public void Create_WhenItemsAreEnumerable_CopiesItemsIntoReadOnlyList()
    {
        var items = Enumerable.Range(1, 2).Where(item => item > 0);

        var result = CursorPaginationResult<int>.Create(items, limit: 10);

        result.Items.ShouldBe(ExpectedItems);
        result.Items.ShouldBeAssignableTo<IReadOnlyList<int>>();
        result.NextCursor.ShouldBeNull();
        result.PreviousCursor.ShouldBeNull();
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact(DisplayName = "Empty returns an empty cursor result")]
    public void Empty_WhenLimitIsValid_ReturnsEmptyResult()
    {
        var result = CursorPaginationResult<string>.Empty(limit: 25);

        result.Items.ShouldBeEmpty();
        result.Limit.ShouldBe(25);
        result.NextCursor.ShouldBeNull();
        result.PreviousCursor.ShouldBeNull();
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact(DisplayName = "Create throws when cursor items are null")]
    public void Create_WhenItemsAreNull_Throws()
    {
        static void act() => CursorPaginationResult<int>.Create(items: null!, limit: 10);

        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("items");
    }

    [Fact(DisplayName = "Create throws when cursor limit is not positive")]
    public void Create_WhenLimitIsNotPositive_Throws()
    {
        static void act() => CursorPaginationResult<int>.Create(items: [], limit: 0);

        var exception = Should.Throw<ArgumentOutOfRangeException>(act);

        exception.ParamName.ShouldBe("limit");
        exception.Message.ShouldContain("Лимит должен быть больше 0.");
    }
}
