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

        void act() => ruleBuilder.MustBeValidDomainValue(static value => TestDomainValue.Create(value));

        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("ruleBuilder");
    }

    [Fact(DisplayName = "MustBeValidDomainValue throws when factory is null")]
    public void MustBeValidDomainValue_WhenFactoryIsNull_Throws()
    {
        var validator = new InlineValidator<TestRequest>();
        IRuleBuilderInitial<TestRequest, string> ruleBuilder = validator.RuleFor(request => request.Value);

        void act() => ruleBuilder.MustBeValidDomainValue((Func<string, Result<TestDomainValue>>)null!);

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

    [Fact(DisplayName = "MustBeValidDomainResult throws when rule builder is null")]
    public void MustBeValidDomainResult_WhenRuleBuilderIsNull_Throws()
    {
        IRuleBuilder<ComplexTestRequest, ComplexTestRequest> ruleBuilder = null!;

        void act() => ruleBuilder.MustBeValidDomainResult(static request => TestDomainResult.Create(
            request.FirstValue,
            request.SecondValue));

        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("ruleBuilder");
    }

    [Fact(DisplayName = "MustBeValidDomainResult throws when factory is null")]
    public void MustBeValidDomainResult_WhenFactoryIsNull_Throws()
    {
        var validator = new InlineValidator<ComplexTestRequest>();
        IRuleBuilderInitial<ComplexTestRequest, ComplexTestRequest> ruleBuilder =
            validator.RuleFor(request => request);

        void act() => ruleBuilder.MustBeValidDomainResult(
            (Func<ComplexTestRequest, Result<TestDomainResult>>)null!);

        var exception = Should.Throw<ArgumentNullException>(act);

        exception.ParamName.ShouldBe("factory");
    }

    [Fact(DisplayName = "MustBeValidDomainResult does not add failures when factory succeeds")]
    public void MustBeValidDomainResult_WhenFactorySucceeds_DoesNotAddFailures()
    {
        var validator = new ComplexTestRequestValidator();

        var result = validator.Validate(new ComplexTestRequest("first", "second"));

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact(DisplayName = "MustBeValidDomainResult uses domain fields when factory fails")]
    public void MustBeValidDomainResult_WhenFactoryFails_UsesDomainFields()
    {
        var validator = new ComplexTestRequestValidator();

        var result = validator.Validate(new ComplexTestRequest("", ""));

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(2);
        result.Errors.Select(failure => failure.PropertyName).ShouldBe(["FirstValue", "SecondValue"]);
        result.Errors.Select(failure => failure.ErrorMessage).ShouldBe(
            ["First value is required.", "Second value is required."]);
    }

    [Fact(DisplayName = "MustBeValidDomainResult uses rule path when domain field is missing")]
    public void MustBeValidDomainResult_WhenDomainFieldIsMissing_UsesRulePath()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(request => request.Value)
            .MustBeValidDomainResult(static _ =>
                Result.Failure<TestDomainValue>(
                    Error.Validation("Value is invalid.")));

        var result = validator.Validate(new TestRequest("invalid"));

        var failure = result.Errors.ShouldHaveSingleItem();
        failure.PropertyName.ShouldBe("Value");
        failure.ErrorMessage.ShouldBe("Value is invalid.");
    }

    private sealed record TestRequest(string Value);

    private sealed record ComplexTestRequest(
        string FirstValue,
        string SecondValue);

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

    private sealed record TestDomainResult(
        string FirstValue,
        string SecondValue)
    {
        public static Result<TestDomainResult> Create(
            string firstValue,
            string secondValue)
        {
            var errors = new List<Error>();
            if (string.IsNullOrWhiteSpace(firstValue))
            {
                errors.Add(
                    Error.Validation("First value is required.")
                        .WithField(nameof(FirstValue)));
            }

            if (string.IsNullOrWhiteSpace(secondValue))
            {
                errors.Add(
                    Error.Validation("Second value is required.")
                        .WithField(nameof(SecondValue)));
            }

            return errors.Count == 0
                ? Result.Success(new TestDomainResult(firstValue, secondValue))
                : Result.Failure<TestDomainResult>(errors);
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

    private sealed class ComplexTestRequestValidator
        : AbstractValidator<ComplexTestRequest>
    {
        public ComplexTestRequestValidator()
        {
            RuleFor(request => request)
                .MustBeValidDomainResult(request => TestDomainResult.Create(
                    request.FirstValue,
                    request.SecondValue));
        }
    }
}
