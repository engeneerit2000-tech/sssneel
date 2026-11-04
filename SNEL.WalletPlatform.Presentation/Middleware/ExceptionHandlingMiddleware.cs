using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Common.Exceptions;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;
    private readonly IStringLocalizer<SharedResource> localizer;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        this.next = next;
        this.logger = logger;
        this.localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, logLevel) = MapException(exception, localizer);

        logger.Log(
            logLevel,
            exception,
            "Unhandled exception for {Method} {Path}. Returning {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            statusCode);

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>(false, null, message);
        await context.Response.WriteAsJsonAsync(response);
    }

    private static (int StatusCode, string Message, LogLevel LogLevel) MapException(
        Exception exception,
        IStringLocalizer<SharedResource> localizer)
    {
        return exception switch
        {
            AppException appException => ((int)appException.StatusCode, appException.Message, LogLevel.Warning),
            ValidationException validationException => (StatusCodes.Status400BadRequest, validationException.Message, LogLevel.Warning),
            ArgumentException argumentException => (StatusCodes.Status400BadRequest, argumentException.Message, LogLevel.Warning),
            UnauthorizedAccessException unauthorizedAccessException => (StatusCodes.Status401Unauthorized, unauthorizedAccessException.Message, LogLevel.Warning),
            KeyNotFoundException keyNotFoundException => (StatusCodes.Status404NotFound, keyNotFoundException.Message, LogLevel.Warning),
            _ => (
                StatusCodes.Status500InternalServerError,
                string.IsNullOrWhiteSpace(exception.Message)
                    ? localizer[LocalizationKeys.UnexpectedError]
                    : exception.Message,
                LogLevel.Error)
        };
    }
}
