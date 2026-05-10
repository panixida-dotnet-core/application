using PANiXiDA.Core.Application.Querying.Pagination;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Pagination;

public sealed class PaginationParametersTests
{
    [Fact(DisplayName = "Skip and Take use the requested positive page values")]
    public void SkipAndTake_WhenValuesArePositive_UseRequestedValues()
    {
        var parameters = new PaginationParameters(PageNumber: 3, PageSize: 20);

        var skip = parameters.Skip;
        var take = parameters.Take;

        skip.Should().Be(40);
        take.Should().Be(20);
    }

    [Fact(DisplayName = "Skip and Take clamp invalid page values to one")]
    public void SkipAndTake_WhenValuesAreInvalid_ClampToOne()
    {
        var parameters = new PaginationParameters(PageNumber: 0, PageSize: 0);

        var skip = parameters.Skip;
        var take = parameters.Take;

        skip.Should().Be(0);
        take.Should().Be(1);
    }

    [Fact(DisplayName = "With expression copies and updates pagination parameters")]
    public void WithExpression_WhenValuesAreChanged_CopiesAndUpdatesParameters()
    {
        var parameters = new PaginationParameters(PageNumber: 1, PageSize: 10);

        var updated = parameters with
        {
            PageNumber = 2,
            PageSize = 20
        };

        updated.PageNumber.Should().Be(2);
        updated.PageSize.Should().Be(20);
    }
}
