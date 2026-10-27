using BankAppointments.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
namespace BankAppointments.Api.Middleware;
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (ValidationException ex) { await WriteProblem(context, StatusCodes.Status400BadRequest, "Request validation failed", string.Join(" ", ex.Errors.Select(e => e.ErrorMessage))); }
        catch (BookingValidationException ex) { await WriteProblem(context, StatusCodes.Status400BadRequest, "Invalid booking", ex.Message); }
        catch (SlotUnavailableException ex) { await WriteProblem(context, StatusCodes.Status409Conflict, "Appointment slot unavailable", ex.Message); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled request error. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Unexpected error", "The request could not be completed. Please try again later.");
        }
    }
    private static async Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title, Detail = detail, Instance = context.Request.Path });
    }
}
