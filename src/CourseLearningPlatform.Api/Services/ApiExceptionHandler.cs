using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CourseLearningPlatform.Api.Services;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted ||
            (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested))
        {
            return false;
        }

        var (status, title, detail) = GetResponse(exception);
        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Request {Method} {Path} failed with trace ID {TraceId}.",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(exception, "Request {Method} {Path} was rejected with status {StatusCode}.",
                context.Request.Method, context.Request.Path, status);
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int Status, string Title, string Detail) GetResponse(Exception exception)
    {
        if (exception is DbUpdateException dbUpdateException &&
            dbUpdateException.GetBaseException() is SqlException sqlException)
        {
            if (sqlException.Number is 2601 or 2627)
            {
                return (StatusCodes.Status409Conflict, "This information is already in use.",
                    "A user or course record with the same unique details already exists.");
            }

            if (sqlException.Number == 547)
            {
                return (StatusCodes.Status409Conflict, "This item is still in use.",
                    "It is linked to other records. Remove those links before deleting it.");
            }
        }

        if (exception is SqlException { Number: -2 or 2 or 53 or 121 or 233 or 10053 or 10054 or 10060 or 4060 } ||
            exception is RedisConnectionException or RedisTimeoutException)
        {
            return (StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.",
                "Please try again in a few minutes.");
        }

        return (StatusCodes.Status500InternalServerError, "We couldn't complete that request.",
            "Please try again. If the problem continues, contact support and share the reference code shown below.");
    }
}
