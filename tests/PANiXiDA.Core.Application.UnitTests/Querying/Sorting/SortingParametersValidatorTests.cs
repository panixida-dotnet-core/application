using FluentValidation;
using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Querying.Sorting;
using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Sorting;

public sealed class SortingParametersValidatorTests
{
    [Theory(DisplayName = "Validate accepts empty sorting and up to five distinct criteria")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Validate_WhenCriteriaAreValid_Succeeds(int count)
    {
        var parameters = new SortingParameters(Enumerable.Range(0, count).Select(index => new SortField($"field{index}")).ToArray());
        var validator = new SortingParametersValidator();

        var result = validator.Validate(parameters);

        result.IsValid.ShouldBeTrue();
        parameters.Fields.Length.ShouldBe(count);
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
        var parameters = new SortingParameters(new SortField(field!));
        var validator = new SortingParametersValidator();

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
        var validator = new SortingParametersValidator();
        var parameters = new SortingParameters(new SortField("name", (SortDirection)order));

        var result = validator.Validate(parameters);

        var failure = result.Errors.ShouldHaveSingleItem();
        failure.PropertyName.ShouldBe("Fields[0].Order");
        failure.ErrorMessage.ShouldBe("Sort order must be Asc or Desc.");
    }

    [Fact(DisplayName = "Validate rejects repeated field paths regardless of casing or direction")]
    public void Validate_WhenFieldIsRepeated_ReturnsDuplicateFailure()
    {
        var parameters = new SortingParameters(new SortField("department.name"), new SortField("DEPARTMENT.Name", SortDirection.Desc));
        var validator = new SortingParametersValidator();

        var result = validator.Validate(parameters);

        var failure = result.Errors.ShouldHaveSingleItem();
        failure.PropertyName.ShouldBe("Fields[1].Field");
        failure.ErrorMessage.ShouldBe("Sorting field 'DEPARTMENT.Name' must not occur more than once.");
    }

    [Theory(DisplayName = "Validate respects the configured maximum number of criteria")]
    [InlineData(5, 6, false)]
    [InlineData(6, 6, true)]
    [InlineData(1, 2, false)]
    public void Validate_WhenMaximumIsConfigured_UsesIt(int maxFields, int count, bool isValid)
    {
        var validator = new SortingParametersValidator(maxFields);
        var parameters = new SortingParameters(Enumerable.Range(0, count).Select(index => new SortField($"field{index}")).ToArray());

        var result = validator.Validate(parameters);

        result.IsValid.ShouldBe(isValid);
        if (!isValid)
        {
            var failure = result.Errors.ShouldHaveSingleItem();
            failure.PropertyName.ShouldBe("Fields");
            failure.ErrorMessage.ShouldBe($"Sorting must contain no more than {maxFields} fields.");
        }
    }

    [Theory(DisplayName = "Validator rejects non-positive maximum values")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WhenMaximumIsInvalid_Throws(int maxFields)
    {
        var action = () => new SortingParametersValidator(maxFields);

        action.ShouldThrow<ArgumentOutOfRangeException>().ParamName.ShouldBe(nameof(maxFields));
    }

    [Theory(DisplayName = "Read model validator accepts only fields from its definition ignoring case")]
    [InlineData("name", true)]
    [InlineData("DEPARTMENT.Name", true)]
    [InlineData("department", false)]
    [InlineData("id", false)]
    [InlineData("unknown", false)]
    public void Validate_WhenDefinitionIsProvided_ChecksAllowedFields(string field, bool isValid)
    {
        var definition = new SortDefinition<TestReadModel>("name", "department.name");
        var validator = new SortingParametersValidator<TestReadModel>(definition);

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
    public void Validate_WhenDefinitionIsProvided_AppliesStructuralRules()
    {
        var validator = new SortingParametersValidator<TestReadModel>(new SortDefinition<TestReadModel>("name"), maxFields: 1);
        var parameters = new SortingParameters(new SortField("", (SortDirection)99), new SortField("name"));

        var result = validator.Validate(parameters);

        result.Errors.Select(failure => failure.PropertyName).ShouldBe(["Fields", "Fields[0].Field", "Fields[0].Order"]);
    }

    [Fact(DisplayName = "Read model validator requires a definition and accepts empty sorting")]
    public void Constructor_WhenDefinitionIsMissing_Throws()
    {
        var action = () => new SortingParametersValidator<TestReadModel>(null!);
        var validator = new SortingParametersValidator<TestReadModel>(new SortDefinition<TestReadModel>());

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("definition");
        validator.Validate(SortingParameters.None).IsValid.ShouldBeTrue();
        validator.Validate(SortingParameters.Ascending("id")).IsValid.ShouldBeFalse();
    }

    [Fact(DisplayName = "SetValidator preserves nested paths for unsupported and repeated fields")]
    public void SetValidator_WhenFieldsAreInvalid_ReturnsNestedPaths()
    {
        var validator = new SortQueryValidator();
        var query = new SortQuery(new SortingParameters(new SortField("name"), new SortField("NAME"), new SortField("id")));

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

    private sealed record TestReadModel(string Name);

    private sealed record SortQuery(SortingParameters Sorting) : IRequest<Result>;

    private sealed class SortQueryValidator : AbstractValidator<SortQuery>
    {
        public SortQueryValidator()
        {
            RuleFor(query => query.Sorting)
                .NotNull()
                .SetValidator(new SortingParametersValidator<TestReadModel>(new SortDefinition<TestReadModel>("name")));
        }
    }
}
