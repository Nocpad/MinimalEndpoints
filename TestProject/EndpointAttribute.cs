//using System;

//namespace TestProject;

//[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
//public sealed class EndpointAttribute : Attribute
//{
//    public bool Active { get; set; }
//}



//using FluentValidation;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.DependencyInjection;
//using System.Linq;
//using System.Threading.Tasks;

//public sealed class AbstractValidationFilter<T> : IEndpointFilter
//{
//    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
//    {
//        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

//        if (validator is not null)
//        {
//            var entity = context.Arguments.OfType<T>().FirstOrDefault();

//            return entity switch
//            {
//                null => Results.Problem("Could not find type to validate."),
//                _ => await ValidateEntity(context, next, validator, entity)
//            };
//        }

//        return await next(context);
//    }


//    private static async ValueTask<object?> ValidateEntity(EndpointFilterInvocationContext context, EndpointFilterDelegate next, IValidator<T>? validator, T? entity)
//    {
//        var validation = await validator.ValidateAsync(entity);
//        if (validation.IsValid)
//        {
//            return await next(context);
//        }

//        return Results.ValidationProblem(validation.ToDictionary());
//    }
//}