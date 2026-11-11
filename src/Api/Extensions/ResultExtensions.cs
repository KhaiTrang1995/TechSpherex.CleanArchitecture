
using TechSpherex.CleanArchitecture.Domain.Common;
namespace TechSpherex.CleanArchitecture.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert a successful result to a problem.");

        // Error is a closed hierarchy (C# 15): no discard arm, so the compiler flags
        // this switch when a new error kind is added without an HTTP mapping.
        var error = result.Error!;
        var (statusCode, title) = error switch
        {
            NotFoundError => (StatusCodes.Status404NotFound, "Not Found"),
            ValidationError => (StatusCodes.Status400BadRequest, "Validation Error"),
            ConflictError => (StatusCodes.Status409Conflict, "Conflict"),
            FailureError => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        return Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Message,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = error.Code
            });
    }
}
