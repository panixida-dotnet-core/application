using FluentValidation;
using PANiXiDA.Core.Application.Querying.Limiting;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Limiting;

public sealed class LimitParametersValidatorTests
{
    [Theory(DisplayName = "Validate accepts limits from one through two hundred")]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(200)]
    public void Validate_WhenLimitIsWithinBounds_Succeeds(int limit)
    {
        var validator = new LimitParametersValidator();

        var result = validator.Validate(new LimitParameters(limit));

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Validate rejects limits outside one through two hundred")]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(201)]
    [InlineData(int.MaxValue)]
    public void Validate_WhenLimitIsOutsideBounds_ReturnsFailure(int limit)
    {
        var validator = new LimitParametersValidator();
        var parameters = new LimitParameters(limit);

        var result = validator.Validate(parameters);

        var failure = result.Errors.ShouldHaveSingleItem();
        failure.PropertyName.ShouldBe(nameof(LimitParameters.Limit));
        failure.ErrorCode.ShouldBe("InclusiveBetweenValidator");
        parameters.Limit.ShouldBe(limit);
    }

    [Theory(DisplayName = "SetValidator applies limit rules inside a query validator")]
    [InlineData(0, false)]
    [InlineData(20, true)]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void SetValidator_WhenUsedInQuery_AppliesLimitRules(int limit, bool isValid)
    {
        var validator = new OptionsQueryValidator();

        var result = validator.Validate(new OptionsQuery(new LimitParameters(limit)));

        result.IsValid.ShouldBe(isValid);
        result.Errors.Count.ShouldBe(isValid ? 0 : 1);
        result.Errors.ShouldAllBe(failure => failure.PropertyName == "Limit.Limit");
    }

    [Fact(DisplayName = "Query validator rejects missing limit parameters")]
    public void Validate_WhenQueryLimitIsNull_ReturnsFailure()
    {
        var validator = new OptionsQueryValidator();

        var result = validator.Validate(new OptionsQuery(null!));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(nameof(OptionsQuery.Limit));
    }

    private sealed record OptionsQuery(LimitParameters Limit);

    private sealed class OptionsQueryValidator : AbstractValidator<OptionsQuery>
    {
        public OptionsQueryValidator()
        {
            RuleFor(query => query.Limit)
                .NotNull()
                .SetValidator(new LimitParametersValidator());
        }
    }
}
