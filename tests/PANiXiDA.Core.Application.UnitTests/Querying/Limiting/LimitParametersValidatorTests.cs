using FluentValidation;
using PANiXiDA.Core.Application.Querying.Limiting;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Limiting;

public sealed class LimitParametersValidatorTests
{
    [Theory(DisplayName = "Constructor rejects a nonpositive maximum")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_WhenMaximumIsNotPositive_Throws(int maxLimit)
    {
        void act() => _ = new LimitParametersValidator(maxLimit);

        var exception = Should.Throw<ArgumentOutOfRangeException>(act);

        exception.ParamName.ShouldBe(nameof(maxLimit));
    }

    [Theory(DisplayName = "Validate accepts positive limits up to the configured maximum")]
    [InlineData(1, 1)]
    [InlineData(1, 100)]
    [InlineData(20, 100)]
    [InlineData(100, 100)]
    [InlineData(500, 500)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void Validate_WhenLimitIsWithinBounds_Succeeds(int limit, int maxLimit)
    {
        var validator = new LimitParametersValidator(maxLimit);

        var result = validator.Validate(new LimitParameters(limit));

        result.IsValid.ShouldBeTrue();
    }

    [Theory(DisplayName = "Validate rejects limits outside the configured bounds")]
    [InlineData(int.MinValue, 100)]
    [InlineData(-1, 100)]
    [InlineData(0, 100)]
    [InlineData(2, 1)]
    [InlineData(51, 50)]
    [InlineData(101, 100)]
    public void Validate_WhenLimitIsOutsideBounds_ReturnsFailure(int limit, int maxLimit)
    {
        var validator = new LimitParametersValidator(maxLimit);
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
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void SetValidator_WhenUsedInQuery_AppliesLimitRules(int limit, bool isValid)
    {
        var validator = new OptionsQueryValidator();

        var result = validator.Validate(new OptionsQuery(new LimitParameters(limit)));

        result.IsValid.ShouldBe(isValid);
        result.Errors.Select(failure => failure.PropertyName)
            .ShouldBe(isValid ? [] : new[] { "Limit.Limit" });
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
                .SetValidator(new LimitParametersValidator(maxLimit: 100));
        }
    }
}
