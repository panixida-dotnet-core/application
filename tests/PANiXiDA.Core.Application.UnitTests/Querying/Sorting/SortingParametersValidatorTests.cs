using FluentValidation;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Querying;
using PANiXiDA.Core.Application.Querying.Sorting;
using PANiXiDA.Core.ResultPattern;
using SortDirection = PANiXiDA.Core.Application.Querying.Sorting.SortDirection;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Sorting;

public sealed class SortingParametersValidatorTests
{
    [Theory(DisplayName = "Validate accepts empty sorting and distinct criteria without a count limit")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(6)]
    public void Validate_WhenCriteriaAreValid_Succeeds(int count)
    {
        string[] fields = ["name", "email", "createdAt", "isActive", "role", "department.name"];
        var parameters = new SortingParameters([.. fields.Take(count).Select(field => new SortField(field))]);
        var validator = new SortingTestReadModelSortingValidator();

        var result = validator.Validate(parameters);

        result.IsValid.ShouldBeTrue();
        parameters.Fields.Length.ShouldBe(count);
    }

    [Fact(DisplayName = "Validate reports null criteria arrays through FluentValidation")]
    public void Validate_WhenArrayIsNull_ReturnsFieldFailure()
    {
        var parameters = new SortingParameters(null!);
        var validator = new SortingTestReadModelSortingValidator();

        var result = validator.Validate(parameters);

        var failure = result.Errors.ShouldHaveSingleItem();
        failure.PropertyName.ShouldBe("Fields");
        failure.ErrorCode.ShouldBe("NotNullValidator");
    }

    [Fact(DisplayName = "Validate reports null criteria and preserves subsequent field indexes")]
    public void Validate_WhenCriterionIsNull_ReturnsIndexedFailures()
    {
        var parameters = new SortingParameters([new SortField("name"), null!, new SortField("NAME")]);
        var validator = new SortingTestReadModelSortingValidator();

        var result = validator.Validate(parameters);

        result.Errors.Select(failure => failure.PropertyName).ShouldBe(["Fields[1]", "Fields[2].Field"]);
        result.Errors[0].ErrorCode.ShouldBe("NotNullValidator");
    }

    [Theory(DisplayName = "Validate reports invalid field paths without changing the input")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" name")]
    [InlineData("name:asc")]
    [InlineData("department..name")]
    [InlineData("name,age")]
    public void Validate_WhenPathIsInvalid_ReturnsIndexedFailure(string? field)
    {
        var parameters = SortingParameters.Of(new SortField(field!));
        var validator = new SortingTestReadModelSortingValidator();

        var result = validator.Validate(parameters);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Fields[0].Field");
        parameters.Fields[0].Field.ShouldBe(field);
    }

    [Theory(DisplayName = "Validate rejects invalid enum values instead of treating them as ascending")]
    [InlineData(-1)]
    [InlineData(2)]
    [InlineData(99)]
    public void Validate_WhenOrderIsInvalid_ReturnsIndexedFailure(int order)
    {
        var validator = new SortingTestReadModelSortingValidator();
        var parameters = SortingParameters.Of(new SortField("name", (SortDirection)order));

        var result = validator.Validate(parameters);

        var failure = result.Errors.ShouldHaveSingleItem();
        failure.PropertyName.ShouldBe("Fields[0].Order");
        failure.ErrorMessage.ShouldBe("Sort order must be Asc or Desc.");
    }

    [Fact(DisplayName = "Validate rejects repeated field paths regardless of casing or direction")]
    public void Validate_WhenFieldIsRepeated_ReturnsDuplicateFailure()
    {
        var parameters = SortingParameters.Of(new SortField("department.name"), new SortField("DEPARTMENT.Name", SortDirection.Desc));
        var validator = new SortingTestReadModelSortingValidator();

        var result = validator.Validate(parameters);

        var failure = result.Errors.ShouldHaveSingleItem();
        failure.PropertyName.ShouldBe("Fields[1].Field");
        failure.ErrorMessage.ShouldBe("Sorting field 'DEPARTMENT.Name' must not occur more than once.");
    }

    [Theory(DisplayName = "Read model validator accepts only fields from its generated property paths ignoring case")]
    [InlineData("name", true)]
    [InlineData(nameof(SortingTestReadModel.Name), true)]
    [InlineData("DEPARTMENT.Name", true)]
    [InlineData(nameof(SortingTestReadModel.Department) + "." + nameof(SortingDepartment.Name), true)]
    [InlineData("SortingTestReadModel.Name", false)]
    [InlineData("department", false)]
    [InlineData("id", false)]
    [InlineData("unknown", false)]
    public void Validate_WhenReadModelIsProvided_ChecksAllowedFields(string field, bool isValid)
    {
        var validator = new SortingTestReadModelSortingValidator();

        var result = validator.Validate(SortingParameters.Ascending(field));

        result.IsValid.ShouldBe(isValid);
        if (!isValid)
        {
            var failure = result.Errors.ShouldHaveSingleItem();
            failure.PropertyName.ShouldBe("Fields[0].Field");
            failure.ErrorMessage.ShouldBe($"Sorting field '{field}' is not supported.");
        }
    }

    [Fact(DisplayName = "Read model validator reports malformed paths once and retains structural rules")]
    public void Validate_WhenReadModelIsProvided_AppliesStructuralRules()
    {
        var validator = new SortingTestReadModelSortingValidator();
        var parameters = SortingParameters.Of(new SortField("", (SortDirection)99), new SortField("name"));

        var result = validator.Validate(parameters);

        result.Errors.Select(failure => failure.PropertyName).ShouldBe(["Fields[0].Field", "Fields[0].Order"]);
    }

    [Fact(DisplayName = "Read model validator requires a field set and accepts empty sorting")]
    public void Constructor_WhenFieldSetIsMissing_Throws()
    {
        var action = () => new TestSortingValidator(null!);
        var validator = new EmptySortingReadModelSortingValidator();

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("fields");
        validator.Validate(SortingParameters.None).IsValid.ShouldBeTrue();
        validator.Validate(SortingParameters.Ascending("id")).IsValid.ShouldBeFalse();
    }

    [Fact(DisplayName = "SetValidator preserves nested paths for unsupported and repeated fields")]
    public void SetValidator_WhenFieldsAreInvalid_ReturnsNestedPaths()
    {
        var validator = new SortQueryValidator();
        var query = new SortQuery(SortingParameters.Of(new SortField("name"), new SortField("NAME"), new SortField("id")));

        var result = validator.Validate(query);

        result.Errors.Select(failure => failure.PropertyName).ShouldBe(["Sorting.Fields[1].Field", "Sorting.Fields[2].Field"]);
        validator.Validate(new SortQuery(null!)).Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Sorting");
    }

    [Fact(DisplayName = "ValidationBehavior exposes indexed sorting fields in validation errors")]
    public async Task BeforeAsync_WhenSortingIsUnsupported_ReturnsFieldMetadata()
    {
        var behavior = new ValidationBehavior<SortQuery, Result>([new SortQueryValidator()]);
        var query = new SortQuery(SortingParameters.Ascending("id"));

        var result = await behavior.BeforeAsync(query, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        var error = result.Errors.ShouldHaveSingleItem();
        error.Type.ShouldBe(ErrorType.Validation);
        error.Metadata[Error.FieldMetadataKey].ShouldBe("Sorting.Fields[0].Field");
    }

    private sealed class TestSortingValidator(IReadOnlySet<string> fields) : SortingParametersValidator(fields);

    private sealed record SortQuery(SortingParameters Sorting) : IRequest<Result>;

    private sealed class SortQueryValidator : AbstractValidator<SortQuery>
    {
        public SortQueryValidator()
        {
            RuleFor(query => query.Sorting)
                .NotNull()
                .SetValidator(new SortingTestReadModelSortingValidator());
        }
    }
}

internal sealed record SortingTestReadModel(
    string Name,
    string Email,
    DateTime CreatedAt,
    bool IsActive,
    string Role,
    SortingDepartment Department) : IReadModel;

internal sealed record SortingDepartment(string Name);

internal sealed record EmptySortingReadModel : IReadModel;
