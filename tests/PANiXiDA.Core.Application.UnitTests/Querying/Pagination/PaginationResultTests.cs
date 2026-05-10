using PANiXiDA.Core.Application.Querying.Pagination;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Pagination;

public sealed class PaginationResultTests
{
    [Fact(DisplayName = "Create returns calculated page metadata")]
    public void Create_WhenParametersAreValid_ReturnsCalculatedMetadata()
    {
        IReadOnlyList<int> items = [3, 4];

        var result = PaginationResult<int>.Create(
            items,
            pageNumber: 2,
            pageSize: 2,
            totalCount: 5);

        result.Items.Should().BeSameAs(items);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
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

        result.Items.Should().Equal(1, 2);
        result.Items.Should().BeAssignableTo<IReadOnlyList<int>>();
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact(DisplayName = "Empty returns an empty page result")]
    public void Empty_WhenParametersAreValid_ReturnsEmptyResult()
    {
        var result = PaginationResult<string>.Empty(pageNumber: 1, pageSize: 25);

        result.Items.Should().BeEmpty();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(25);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact(DisplayName = "Create throws when items are null")]
    public void Create_WhenItemsAreNull_Throws()
    {
        Action act = () => PaginationResult<int>.Create(
            items: null!,
            pageNumber: 1,
            pageSize: 10,
            totalCount: 0);

        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("items");
    }

    [Fact(DisplayName = "Create throws when page number is not positive")]
    public void Create_WhenPageNumberIsNotPositive_Throws()
    {
        Action act = () => PaginationResult<int>.Create(
            items: [],
            pageNumber: 0,
            pageSize: 10,
            totalCount: 0);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("pageNumber")
            .WithMessage("*Номер страницы должен быть больше 0.*");
    }

    [Fact(DisplayName = "Create throws when page size is not positive")]
    public void Create_WhenPageSizeIsNotPositive_Throws()
    {
        Action act = () => PaginationResult<int>.Create(
            items: [],
            pageNumber: 1,
            pageSize: 0,
            totalCount: 0);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("pageSize")
            .WithMessage("*Размер страницы должен быть больше 0.*");
    }

    [Fact(DisplayName = "Create throws when total count is negative")]
    public void Create_WhenTotalCountIsNegative_Throws()
    {
        Action act = () => PaginationResult<int>.Create(
            items: [],
            pageNumber: 1,
            pageSize: 10,
            totalCount: -1);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("totalCount")
            .WithMessage("*Общее количество элементов не может быть отрицательным.*");
    }
}
