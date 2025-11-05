using backend.Models.ApiResponses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace backend.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, errorCode, message) = MapExceptionToResponse(exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Error = errorCode,
            Message = message,
            TraceId = httpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        if (exception is ValidationException validationEx)
        {
            errorResponse.Errors = new Dictionary<string, string[]>
            {
                { "validation", new[] { validationEx.Message } }
            };
        }

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
        return true;
    }

    private static (int StatusCode, string ErrorCode, string Message) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            DbUpdateException dbEx when IsDuplicateKeyException(dbEx)
                => (StatusCodes.Status409Conflict, "DUPLICATE_RESOURCE", "A resource with the same unique value already exists"),

            DbUpdateException dbEx when IsForeignKeyException(dbEx)
                => (StatusCodes.Status422UnprocessableEntity, "INVALID_REFERENCE", "The request references a resource that does not exist"),

            DbUpdateConcurrencyException
                => (StatusCodes.Status409Conflict, "CONCURRENCY_CONFLICT", "The resource has been modified by another user"),

            ValidationException
                => (StatusCodes.Status400BadRequest, "VALIDATION_ERROR", exception.Message),

            UnauthorizedAccessException
                => (StatusCodes.Status401Unauthorized, "UNAUTHORIZED", "Authentication is required to access this resource"),

            KeyNotFoundException
                => (StatusCodes.Status404NotFound, "NOT_FOUND", exception.Message),

            InvalidOperationException
                => (StatusCodes.Status400BadRequest, "INVALID_OPERATION", exception.Message),

            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred. Please contact support if the problem persists.")
        };
    }

    private static bool IsDuplicateKeyException(DbUpdateException exception)
    {
        var innerMessage = exception.InnerException?.Message ?? string.Empty;
        return innerMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
               innerMessage.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               innerMessage.Contains("unique index", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsForeignKeyException(DbUpdateException exception)
    {
        var innerMessage = exception.InnerException?.Message ?? string.Empty;
        return innerMessage.Contains("foreign key", StringComparison.OrdinalIgnoreCase) ||
               innerMessage.Contains("violates foreign key constraint", StringComparison.OrdinalIgnoreCase);
    }
}
