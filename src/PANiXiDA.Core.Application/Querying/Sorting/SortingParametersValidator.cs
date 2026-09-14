using FluentValidation;
using FluentValidation.Results;

namespace PANiXiDA.Core.Application.Querying.Sorting;

/// <summary>
/// Validates sorting paths, directions, and duplicate fields.
/// </summary>
public class SortingParametersValidator : AbstractValidator<SortingParameters>
{
    /// <summary>
    /// Creates a structural sorting validator. Empty sorting is valid.
    /// </summary>
    public SortingParametersValidator()
    {
        RuleFor(parameters => parameters.Fields).NotNull();

        RuleForEach(parameters => parameters.Fields).NotNull().ChildRules(field =>
        {
            field.RuleFor(criterion => criterion.Field)
                .Must(SortField.IsValidFieldPath)
                .WithMessage("Sorting field must be a non-empty path without whitespace, colons, commas, or empty segments.");

            field.RuleFor(criterion => criterion.Order)
                .Must(order => order is SortDirection.Asc or SortDirection.Desc)
                .WithMessage($"Sort order must be {nameof(SortDirection.Asc)} or {nameof(SortDirection.Desc)}.");
        });

        RuleFor(parameters => parameters.Fields).Custom((fields, context) =>
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index]?.Field;

                if (SortField.IsValidFieldPath(field) && !names.Add(field))
                {
                    context.AddFailure(new ValidationFailure(
                        $"{context.PropertyPath}[{index}].Field",
                        $"Sorting field '{field}' must not occur more than once."));
                }
            }
        }).When(parameters => parameters.Fields is not null);
    }

    /// <summary>
    /// Creates a model-specific validator using field paths supplied by generated code.
    /// </summary>
    /// <param name="fields">The supported field paths, compared by the supplied set.</param>
    /// <exception cref="ArgumentNullException">The field set is null.</exception>
    protected SortingParametersValidator(IReadOnlySet<string> fields)
        : this()
    {
        ArgumentNullException.ThrowIfNull(fields);

        RuleForEach(parameters => parameters.Fields)
            .Where(field => field is not null && SortField.IsValidFieldPath(field.Field))
            .ChildRules(field =>
            {
                field.RuleFor(criterion => criterion.Field)
                    .Must(fields.Contains)
                    .WithMessage("Sorting field '{PropertyValue}' is not supported.");
            });
    }
}
