using FluentValidation;
using FluentValidation.Results;

namespace PANiXiDA.Core.Application.Querying.Sorting;

/// <summary>
/// Validates sorting paths, directions, duplicate fields, and the number of criteria.
/// </summary>
public class SortingParametersValidator : AbstractValidator<SortingParameters>
{
    /// <summary>
    /// The default maximum number of sorting criteria.
    /// </summary>
    public const int DefaultMaxFields = 5;

    /// <summary>
    /// Creates a structural sorting validator. Empty sorting is valid.
    /// </summary>
    /// <param name="maxFields">The positive maximum number of criteria.</param>
    /// <exception cref="ArgumentOutOfRangeException">The maximum is not positive.</exception>
    public SortingParametersValidator(int maxFields = DefaultMaxFields)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxFields);

        RuleFor(parameters => parameters.Fields)
            .Must(fields => fields.Length <= maxFields)
            .WithMessage($"Sorting must contain no more than {maxFields} fields.");

        RuleForEach(parameters => parameters.Fields).ChildRules(field =>
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
                var field = fields[index].Field;

                if (SortField.IsValidFieldPath(field) && !names.Add(field))
                {
                    context.AddFailure(new ValidationFailure(
                        $"{context.PropertyPath}[{index}].Field",
                        $"Sorting field '{field}' must not occur more than once."));
                }
            }
        });
    }
}

/// <summary>
/// Also validates that each criterion belongs to the supplied read model sorting definition.
/// </summary>
/// <typeparam name="TReadModel">The read model being sorted.</typeparam>
public sealed class SortingParametersValidator<TReadModel> : SortingParametersValidator
{
    /// <summary>
    /// Creates a validator using a read model definition without runtime property discovery.
    /// </summary>
    /// <param name="definition">The definition supplied for the read model.</param>
    /// <param name="maxFields">The positive maximum number of criteria.</param>
    /// <exception cref="ArgumentNullException">The definition is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The maximum is not positive.</exception>
    public SortingParametersValidator(SortDefinition<TReadModel> definition, int maxFields = DefaultMaxFields)
        : base(maxFields)
    {
        ArgumentNullException.ThrowIfNull(definition);

        RuleForEach(parameters => parameters.Fields)
            .Where(field => SortField.IsValidFieldPath(field.Field))
            .ChildRules(field =>
            {
                field.RuleFor(criterion => criterion.Field)
                    .Must(definition.Fields.Contains)
                    .WithMessage("Sorting field '{PropertyValue}' is not supported.");
            });
    }
}
