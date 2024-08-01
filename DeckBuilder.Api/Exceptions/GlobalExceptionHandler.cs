using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DeckBuilder.Api.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails();
        
        if (exception is KnownException)
        {
            var knownException = (KnownException)exception;
            problemDetails.Title = knownException.Message;
            problemDetails.Status = knownException.StatusCode;
            problemDetails.Detail = knownException.InnerException?.Message;
        }
        else
        {
            problemDetails.Title = "An internal server error occured.";
            problemDetails.Status = StatusCodes.Status500InternalServerError;
        }
        
        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}