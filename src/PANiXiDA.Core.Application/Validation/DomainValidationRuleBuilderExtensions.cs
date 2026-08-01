using FluentValidation;

namespace PANiXiDA.Core.Application.Validation;

/// <summary>
/// Provides FluentValidation rules for validating values with domain factories.
/// </summary>
public static class DomainValidationRuleBuilderExtensions
{
    /// <summary>
    /// Adds a FluentValidation custom rule that treats failed domain factory results as validation failures.
    /// </summary>
    /// <typeparam name="TRequest">The request type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type being validated.</typeparam>
    /// <typeparam name="TValue">The domain value type returned by the factory.</typeparam>
    /// <param name="ruleBuilder">The FluentValidation rule builder.</param>
    /// <param name="factory">The factory used to validate and create a domain value from the property value.</param>
    /// <returns>The FluentValidation rule builder options.</returns>
    public static IRuleBuilderOptionsConditions<TRequest, TProperty> MustBeValidDomainValue<TRequest, TProperty, TValue>(
        this IRuleBuilder<TRequest, TProperty> ruleBuilder,
        Func<TProperty, Result<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(ruleBuilder);
        ArgumentNullException.ThrowIfNull(factory);

        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValue> result = factory(value);
            if (result.IsSuccess)
            {
                return;
            }

            foreach (Error error in result.Errors)
            {
                context.AddFailure(context.PropertyPath, error.Message);
            }
        });
    }

    /// <summary>
    /// Adds a FluentValidation custom rule that maps failed domain factory results to their domain field paths.
    /// </summary>
    /// <typeparam name="TRequest">The request type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type being validated.</typeparam>
    /// <typeparam name="TValue">The domain value type returned by the factory.</typeparam>
    /// <param name="ruleBuilder">The FluentValidation rule builder.</param>
    /// <param name="factory">The factory used to validate and create a domain value from the property value.</param>
    /// <returns>The FluentValidation rule builder options.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ruleBuilder"/> or <paramref name="factory"/> is null.</exception>
    public static IRuleBuilderOptionsConditions<TRequest, TProperty> MustBeValidDomainResult<TRequest, TProperty, TValue>(
        this IRuleBuilder<TRequest, TProperty> ruleBuilder,
        Func<TProperty, Result<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(ruleBuilder);
        ArgumentNullException.ThrowIfNull(factory);

        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValue> result = factory(value);
            if (result.IsSuccess)
            {
                return;
            }

            foreach (Error error in result.Errors)
            {
                string propertyPath = error.Metadata.TryGetValue(
                        Error.FieldMetadataKey,
                        out object? field) &&
                    field is string fieldName &&
                    !string.IsNullOrWhiteSpace(fieldName)
                        ? fieldName
                        : context.PropertyPath;

                context.AddFailure(propertyPath, error.Message);
            }
        });
    }
}
