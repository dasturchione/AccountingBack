using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Infrastructure
{
    public sealed class FluentValidationFilter : IAsyncActionFilter
    {
        private const string ValidationTypeUrl = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var failures = new List<ValidationFailure>();

            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg is null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                var validator = context.HttpContext.RequestServices.GetService(validatorType);
                if (validator is null) continue;

                Type contextType = typeof(ValidationContext<>).MakeGenericType(arg.GetType());

                var validationContext = (IValidationContext)Activator.CreateInstance(contextType, arg)!;

                ValidationResult result = await ((IValidator)validator).ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                failures.AddRange(result.Errors);
            }

            if (failures.Count == 0)
            {
                await next();
                return;
            }

            var problem = new ProblemDetails
            {
                Type = ValidationTypeUrl,
                Title = "Validation.General",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred"
            };

            var errors = failures
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => ToCamelCase(g.Key),
                    g => g
                        .Select(x => x.ErrorMessage)
                        .Distinct()
                        .ToArray());

            problem.Extensions["errors"] = errors;

            context.Result = new BadRequestObjectResult(problem);
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return char.ToLowerInvariant(value[0]) + value[1..];
        }
    }
}
