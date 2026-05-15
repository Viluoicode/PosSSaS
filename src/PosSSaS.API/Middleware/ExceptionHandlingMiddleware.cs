using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PosSSaS.Application.Common.Exceptions;

namespace PosSSaS.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException ex)
        {
            await WriteAsync(ctx, HttpStatusCode.BadRequest, "validation_failed",
                ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (NotFoundException ex)
        {
            await WriteAsync(ctx, HttpStatusCode.NotFound, "not_found", ex.Message);
        }
        catch (InsufficientStockException ex)
        {
            await WriteAsync(ctx, HttpStatusCode.Conflict, "insufficient_stock", ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await WriteAsync(ctx, HttpStatusCode.Conflict, "concurrency_conflict",
                "The resource was modified by another request. Please retry.");
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteAsync(ctx, HttpStatusCode.Unauthorized, "unauthorized", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteAsync(ctx, HttpStatusCode.BadRequest, "invalid_operation", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteAsync(ctx, HttpStatusCode.InternalServerError, "internal_error", "An unexpected error occurred.");
        }
    }

    private static Task WriteAsync(HttpContext ctx, HttpStatusCode status, string code, object detail)
    {
        ctx.Response.StatusCode = (int)status;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { code, detail }));
    }
}
