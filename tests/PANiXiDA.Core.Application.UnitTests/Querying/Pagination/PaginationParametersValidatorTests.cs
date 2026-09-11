using FluentValidation;
using PANiXiDA.Core.Application.Querying.Pagination;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Pagination;

public sealed class PaginationParametersValidatorTests
{
    [Theory(DisplayName = "Validate accepts valid page sizes and representable offsets")]
    [InlineData(1, 1)]
    [InlineData(1, 200)]
    [InlineData(2, 20)]
    [InlineData(10737419, 200)]
    [InlineData(int.MaxValue, 1)]
    public void Validate_WhenPaginationIsWithinBounds_Succeeds(int pageNumber, int pageSize)
    {
        var validator = new PaginationParametersValidator();
        var parameters = new PaginationParameters(pageNumber, pageSize);

        var result = validator.Validate(parameters);

        result.IsValid.ShouldBeTrue();
        parameters.Skip.ShouldBe((int)(((long)pageNumber - 1) * pageSize));
    }

    [Theory(DisplayName = "Validate rejects invalid page numbers and page sizes")]
    [InlineData(0, 20, "PageNumber")]
    [InlineData(-1, 20, "PageNumber")]
    [InlineData(int.MinValue, 20, "PageNumber")]
    [InlineData(1, 0, "PageSize")]
    [InlineData(1, -1, "PageSize")]
    [InlineData(1, 201, "PageSize")]
    [InlineData(1, int.MaxValue, "PageSize")]
    public void Validate_WhenPaginationIsOutsideBounds_ReturnsFailure(int pageNumber, int pageSize, string propertyName)
    {
        var validator = new PaginationParametersValidator();

        var result = validator.Validate(new PaginationParameters(pageNumber, pageSize));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(propertyName);
    }

    [Theory(DisplayName = "Validate rejects page numbers that overflow the offset")]
    [InlineData(10737420, 200)]
    [InlineData(int.MaxValue, 2)]
    public void Validate_WhenOffsetWouldOverflow_ReturnsFailure(int pageNumber, int pageSize)
    {
        var validator = new PaginationParametersValidator();

        var result = validator.Validate(new PaginationParameters(pageNumber, pageSize));

        var failure = result.Errors.ShouldHaveSingleItem();
        failure.PropertyName.ShouldBe(nameof(PaginationParameters.PageNumber));
        failure.ErrorMessage.ShouldBe("The requested page offset must not exceed 2147483647.");
    }

    [Fact(DisplayName = "Query validator accepts default pagination parameters")]
    public void SetValidator_WhenPaginationIsDefault_Succeeds()
    {
        var validator = new PageQueryValidator();

        var result = validator.Validate(new PageQuery(new PaginationParameters()));

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "SetValidator preserves pagination property paths inside a query")]
    [InlineData(0, 20, "Pagination.PageNumber")]
    [InlineData(1, 201, "Pagination.PageSize")]
    public void SetValidator_WhenUsedInQuery_AppliesPaginationRules(int pageNumber, int pageSize, string propertyName)
    {
        var validator = new PageQueryValidator();

        var result = validator.Validate(new PageQuery(new PaginationParameters(pageNumber, pageSize)));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(propertyName);
    }

    [Fact(DisplayName = "Query validator rejects missing pagination parameters")]
    public void Validate_WhenQueryPaginationIsNull_ReturnsFailure()
    {
        var validator = new PageQueryValidator();

        var result = validator.Validate(new PageQuery(null!));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(nameof(PageQuery.Pagination));
    }

    private sealed record PageQuery(PaginationParameters Pagination);

    private sealed class PageQueryValidator : AbstractValidator<PageQuery>
    {
        public PageQueryValidator()
        {
            RuleFor(query => query.Pagination)
                .NotNull()
                .SetValidator(new PaginationParametersValidator());
        }
    }
}
