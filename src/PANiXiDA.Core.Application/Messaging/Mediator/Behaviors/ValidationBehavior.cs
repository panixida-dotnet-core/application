using FluentValidation;

using PANiXiDA.Core.Application.Messaging.Mediator.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Validates a request before the request handler executes.
/// </summary>
/// <typeparam name="TRequest">The request type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the request.</typeparam>
/// <param name="validators">The validators used to validate the request.</param>
/// <remarks>
/// Creates a validation failure result when one or more FluentValidation validators report failures.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResult>(
    IEnumerable<IValidator<TRequest>> validators) : IBeforeRequestBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    /// <summary>
    /// Validates the request and returns a failure result when validation fails.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A task that returns a successful result when validation passes, or a validation failure result when validation fails.
    /// </returns>
    public async Task<Result> BeforeAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        foreach (var validator in validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            foreach (var failure in validationResult.Errors)
            {
                var error = Error.Validation(failure.ErrorMessage);

                if (!string.IsNullOrWhiteSpace(failure.PropertyName))
                {
                    error = error.WithField(failure.PropertyName);
                }

                errors.Add(error);
            }
        }

        if (errors.Count == 0)
        {
            return Result.Success();
        }

        return Result.Failure(errors);
    }
}
