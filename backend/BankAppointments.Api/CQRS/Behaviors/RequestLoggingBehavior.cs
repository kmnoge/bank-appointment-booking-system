using MediatR;

namespace BankAppointments.Api.CQRS.Behaviors;

public sealed class RequestLoggingBehavior<TRequest, TResponse>(ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogDebug("Handling CQRS request {RequestType}", typeof(TRequest).Name);
        var started = System.Diagnostics.Stopwatch.StartNew();
        try { return await next(); }
        finally { logger.LogDebug("Handled CQRS request {RequestType} in {ElapsedMs}ms", typeof(TRequest).Name, started.ElapsedMilliseconds); }
    }
}
