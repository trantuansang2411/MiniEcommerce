using ApplicationCore.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using PublicApi.DTOs.Responses;

namespace PublicApi.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
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
        if (httpContext.Response.HasStarted)
            return false;

        var (statusCode, message) = exception switch
        {
            BadRequestException => (
                StatusCodes.Status400BadRequest,
                exception.Message),

            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                exception.Message),

            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                exception.Message),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                exception.Message),

            ConflictException => (
                StatusCodes.Status409Conflict,
                exception.Message),

            TooManyRequestsException => (
                StatusCodes.Status429TooManyRequests,
                exception.Message),

            ServiceUnavailableException => (
                StatusCodes.Status503ServiceUnavailable,
                exception.Message),

            GatewayTimeoutException => (
                StatusCodes.Status504GatewayTimeout,
                exception.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Đã xảy ra lỗi không mong muốn.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Request {Method} {Path} failed with {StatusCode}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                statusCode,
                exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ApiResponse<object>
            {
                StatusCode = statusCode,
                Message = message,
                Data = null
            },
            cancellationToken);

        return true;
    }
}