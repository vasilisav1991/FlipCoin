using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Flipcoin.Api.Validation;

/// <summary>
/// Runs any registered FluentValidation validator for each action argument
/// before the action executes. A failure throws a ValidationException, which the
/// global exception handler turns into a 400 ProblemDetails with the errors.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
            {
                var result = await validator.ValidateAsync(new ValidationContext<object>(argument));
                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }
            }
        }

        await next();
    }
}
