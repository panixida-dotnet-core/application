using FluentValidation;

using PANiXiDA.Core.Application.Messaging.Behaviors.Abstractions;
using PANiXiDA.Core.Application.Messaging.Mediator.Contracts;
using PANiXiDA.Core.Application.Messaging.Mediator.Handlers;

namespace PANiXiDA.Core.Application.Messaging.Mediator.Behaviors;

/// <summary>
/// Validates a message before the message handler executes.
/// </summary>
/// <typeparam name="TRequest">The message type processed by the behavior.</typeparam>
/// <typeparam name="TResult">The result type returned by the message.</typeparam>
/// <typeparam name="THandler">The handler type processing the message.</typeparam>
/// <param name="validators">The validators used to validate the message.</param>
/// <remarks>
/// Creates a validation failure result when one or more FluentValidation validators report failures.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResult, THandler>(
    IEnumerable<IValidator<TRequest>> validators) : IBeforeMessageBehavior<TRequest>
    where TRequest : IRequest<TResult>
    where TResult : Result
    where THandler : IRequestHandler<TRequest, TResult>
{
    /// <summary>
    /// Validates the message and returns a failure result when validation fails.
    /// </summary>
    /// <param name="message">The message being processed.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>
    /// A task that returns a successful result when validation passes, or a validation failure result when validation fails.
    /// </returns>
    public async Task<Result> BeforeAsync(
        TRequest message,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        foreach (var validator in validators)
        {
            var validationResult = await validator.ValidateAsync(message, cancellationToken);

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
