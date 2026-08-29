using PANiXiDA.Core.Application.Querying.Pagination;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Pagination;

public sealed class PaginationResultTests
{
    private static readonly int[] ExpectedItems = [1, 2];

    [Fact(DisplayName = "Create returns calculated page metadata")]
    public void Create_WhenParametersAreValid_ReturnsCalculatedMetadata()
    {
        IReadOnlyList<int> items = [3, 4];

        var result = PaginationResult<int>.Create(
            items,
            pageNumber: 2,
            pageSize: 2,
            totalCount: 5);

        result.Items.ShouldBeSameAs(items);
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(2);
        result.TotalCount.ShouldBe(5);
        result.TotalPages.ShouldBe(3);
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact(DisplayName = "Create copies enumerable items into a read-only list")]
    public void Create_WhenItemsAreEnumerable_CopiesItemsIntoReadOnlyList()
    {
        var items = Enumerable.Range(1, 2).Where(item => item > 0);

        var result = PaginationResult<int>.Create(
            items,
            pageNumber: 1,
            pageSize: 10,
            totalCount: 2);

        result.Items.ShouldBe(ExpectedItems);
        result.Items.ShouldBeAssignableTo<IReadOnlyList<int>>();
        result.HasPreviousPage.ShouldBeFalse();
        result.HasNextPage.ShouldBeFalse();
    }

    [Fact(DisplayName = "Empty returns an empty page result")]
    public void Empty_WhenParametersAreValid_ReturnsEmptyResult()
    {
        var result = PaginationResult<string>.Empty(pageNumber: 1, pageSize: 25);

        result.Items.ShouldBeEmpty();
        result.PageNumber.ShouldBe(1);
        result.PageSize.ShouldBe(25);
        result.TotalCount.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
        result.HasPreviousPage.ShouldBeFalse();
        result.HasNextPage.ShouldBeFalse();
    }

    [Fact(DisplayName = "Create throws when items are null")]
    public void Create_WhenItemsAreNull_Throws()
    {
        static void act() => PaginationResult<int>.Create(
            items: null!,
            pageNumber: 1,
            pageSize: 10,
            totalCount: 0);

        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("items");
    }

    [Fact(DisplayName = "Create throws when page number is not positive")]
    public void Create_WhenPageNumberIsNotPositive_Throws()
    {
        static void act() => PaginationResult<int>.Create(
            items: [],
            pageNumber: 0,
            pageSize: 10,
            totalCount: 0);

        var exception = Should.Throw<ArgumentOutOfRangeException>(act);

        exception.ParamName.ShouldBe("pageNumber");
        exception.Message.ShouldContain("Номер страницы должен быть больше 0.");
    }

    [Fact(DisplayName = "Create throws when page size is not positive")]
    public void Create_WhenPageSizeIsNotPositive_Throws()
    {
        static void act() => PaginationResult<int>.Create(
            items: [],
            pageNumber: 1,
            pageSize: 0,
            totalCount: 0);

        var exception = Should.Throw<ArgumentOutOfRangeException>(act);

        exception.ParamName.ShouldBe("pageSize");
        exception.Message.ShouldContain("Размер страницы должен быть больше 0.");
    }

    [Fact(DisplayName = "Create throws when total count is negative")]
    public void Create_WhenTotalCountIsNegative_Throws()
    {
        static void act() => PaginationResult<int>.Create(
            items: [],
            pageNumber: 1,
            pageSize: 10,
            totalCount: -1);

        var exception = Should.Throw<ArgumentOutOfRangeException>(act);

        exception.ParamName.ShouldBe("totalCount");
        exception.Message.ShouldContain("Общее количество элементов не может быть отрицательным.");
    }
}
