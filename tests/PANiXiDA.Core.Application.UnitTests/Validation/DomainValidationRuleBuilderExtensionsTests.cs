using FluentValidation;
using PANiXiDA.Core.Application.Validation;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Validation;

public sealed class DomainValidationRuleBuilderExtensionsTests
{
    [Fact(DisplayName = "MustBeValidDomainValue throws when rule builder is null")]
    public void MustBeValidDomainValue_WhenRuleBuilderIsNull_Throws()
    {
        IRuleBuilder<TestRequest, string> ruleBuilder = null!;

        Action act = () => ruleBuilder.MustBeValidDomainValue(static value => TestDomainValue.Create(value));

        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("ruleBuilder");
    }

    [Fact(DisplayName = "MustBeValidDomainValue throws when factory is null")]
    public void MustBeValidDomainValue_WhenFactoryIsNull_Throws()
    {
        var validator = new InlineValidator<TestRequest>();
        IRuleBuilderInitial<TestRequest, string> ruleBuilder = validator.RuleFor(request => request.Value);

        Action act = () => ruleBuilder.MustBeValidDomainValue((Func<string, Result<TestDomainValue>>)null!);

        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("factory");
    }

    [Fact(DisplayName = "MustBeValidDomainValue does not add failures when factory succeeds")]
    public void MustBeValidDomainValue_WhenFactorySucceeds_DoesNotAddFailures()
    {
        var validator = new TestRequestValidator();

        var result = validator.Validate(new TestRequest("valid"));

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact(DisplayName = "MustBeValidDomainValue adds factory errors as validation failures")]
    public void MustBeValidDomainValue_WhenFactoryFails_AddsValidationFailures()
    {
        var validator = new TestRequestValidator();

        var result = validator.Validate(new TestRequest(""));

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(2);
        result.Errors.Select(failure => failure.PropertyName).ShouldBe(["Value", "Value"]);
        result.Errors.Select(failure => failure.ErrorMessage).ShouldBe(["Value is required.", "Value is too short."]);
    }

    private sealed record TestRequest(string Value);

    private sealed record TestDomainValue(string Value)
    {
        public static Result<TestDomainValue> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result.Failure<TestDomainValue>(
                    [
                        Error.Validation("Value is required."),
                        Error.Validation("Value is too short.")
                    ]);
            }

            return Result.Success(new TestDomainValue(value));
        }
    }

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(request => request.Value)
                .MustBeValidDomainValue(TestDomainValue.Create);
        }
    }
}
