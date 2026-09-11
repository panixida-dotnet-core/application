using FluentValidation;

namespace PANiXiDA.Core.Application.Querying.Pagination;

/// <summary>
/// Validates positive page numbers, page sizes from 1 through 200, and representable offsets.
/// </summary>
public sealed class PaginationParametersValidator : AbstractValidator<PaginationParameters>
{
    /// <summary>
    /// Initializes a validator for page-based pagination parameters.
    /// </summary>
    public PaginationParametersValidator()
    {
        RuleFor(parameters => parameters.PageNumber)
            .GreaterThan(0)
            .Must((parameters, pageNumber) => ((long)pageNumber - 1) * parameters.PageSize <= int.MaxValue)
            .WithMessage("The requested page offset must not exceed 2147483647.");

        RuleFor(parameters => parameters.PageSize)
            .InclusiveBetween(1, 200);
    }
}
