using FluentValidation;
using GymRoutineGenerator.Application.Common;
using MediatR;

namespace GymRoutineGenerator.Application.Behaviors;

/// <summary>
/// Pipeline Behavior para validación automática con FluentValidation
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessages = string.Join("; ", failures.Select(f => f.ErrorMessage));

            // Usar reflexión para crear el tipo de Result correcto
            var resultType = typeof(TResponse);
            if (resultType.IsGenericType)
            {
                var genericArg = resultType.GetGenericArguments()[0];
                var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure))!
                    .MakeGenericMethod(genericArg);
                return (TResponse)failureMethod.Invoke(null, new object[] { errorMessages })!;
            }
            else
            {
                return (TResponse)(object)Result.Failure(errorMessages);
            }
        }

        return await next();
    }
}
