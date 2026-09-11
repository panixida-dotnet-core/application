using FluentValidation;

namespace PANiXiDA.Core.Application.Querying.Limiting;

/// <summary>
/// Validates that a requested limit is between 1 and 200.
/// </summary>
public sealed class LimitParametersValidator : AbstractValidator<LimitParameters>
{
    /// <summary>
    /// Initializes a validator for limits from 1 through 200.
    /// </summary>
    public LimitParametersValidator()
    {
        RuleFor(parameters => parameters.Limit)
            .InclusiveBetween(1, 200);
    }
}
