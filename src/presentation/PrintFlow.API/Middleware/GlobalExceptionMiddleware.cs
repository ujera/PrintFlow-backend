using System.Text.Json;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Exceptions;

namespace PrintFlow.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            BadRequestException badRequest => HandleBadRequest(badRequest, traceId),
            UnauthorizedException unauthorized => HandleStandard(unauthorized, traceId),
            ForbiddenException forbidden => HandleStandard(forbidden, traceId),
            NotFoundException notFound => HandleStandard(notFound, traceId),
            PaymentException payment => HandlePayment(payment, traceId),
            ConflictException conflict => HandleStandard(conflict, traceId),
            AppException appException => HandleStandard(appException, traceId),
            _ => HandleUnknown(exception, traceId)
        };

        LogException(exception, errorResponse);

        response.StatusCode = errorResponse.StatusCode;
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
    }

    private ErrorResponse HandleBadRequest(BadRequestException ex, string traceId)
    {
        return new ErrorResponse
        {
            StatusCode = ex.StatusCode,
            Message = ex.Message,
            Errors = ex.Errors.Count > 1 ? ex.Errors : null,
            TraceId = traceId
        };
    }

    private ErrorResponse HandlePayment(PaymentException ex, string traceId)
    {
        return new ErrorResponse
        {
            StatusCode = ex.StatusCode,
            Message = ex.Message,
            Detail = ex.StripeErrorCode,
            TraceId = traceId
        };
    }

    private ErrorResponse HandleStandard(AppException ex, string traceId)
    {
        return new ErrorResponse
        {
            StatusCode = ex.StatusCode,
            Message = ex.Message,
            TraceId = traceId
        };
    }

    private ErrorResponse HandleUnknown(Exception ex, string traceId)
    {
        return new ErrorResponse
        {
            StatusCode = 500,
            Message = "An unexpected error occurred.",
            Detail = _environment.IsDevelopment() ? ex.ToString() : null,
            TraceId = traceId
        };
    }

    private void LogException(Exception exception, ErrorResponse errorResponse)
    {
        if (errorResponse.StatusCode >= 500)
        {
            _logger.LogError(exception,
                "Unhandled exception | TraceId: {TraceId} | Message: {Message}",
                errorResponse.TraceId, exception.Message);
        }
        else if (errorResponse.StatusCode == 401 || errorResponse.StatusCode == 403)
        {
            _logger.LogWarning(
                "Access violation | TraceId: {TraceId} | Status: {StatusCode} | Message: {Message}",
                errorResponse.TraceId, errorResponse.StatusCode, errorResponse.Message);
        }
        else
        {
            _logger.LogInformation(
                "Client error | TraceId: {TraceId} | Status: {StatusCode} | Message: {Message}",
                errorResponse.TraceId, errorResponse.StatusCode, errorResponse.Message);
        }
    }
}