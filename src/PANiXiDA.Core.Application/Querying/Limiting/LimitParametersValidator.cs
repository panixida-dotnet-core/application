using FluentValidation;

namespace PANiXiDA.Core.Application.Querying.Limiting;

/// <summary>
/// Validates that a requested limit is positive and does not exceed a use case's maximum.
/// </summary>
public sealed class LimitParametersValidator : AbstractValidator<LimitParameters>
{
    /// <summary>
    /// Initializes a validator with an inclusive upper bound.
    /// </summary>
    /// <param name="maxLimit">The positive maximum number of items allowed by the use case.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLimit"/> is not positive.</exception>
    public LimitParametersValidator(int maxLimit)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLimit);

        RuleFor(parameters => parameters.Limit)
            .InclusiveBetween(1, maxLimit);
    }
}
