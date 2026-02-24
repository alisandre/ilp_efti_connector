using ilp_efti_connector.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ilp_efti_connector.Shared.Infrastructure.Middleware;

/// <summary>
/// Intercetta le eccezioni non gestite e le converte in risposte <c>application/problem+json</c>
/// conformi a RFC 7807.
/// </summary>
public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            DomainException de              => (StatusCodes.Status422UnprocessableEntity, de.Message),
            KeyNotFoundException            => (StatusCodes.Status404NotFound,            "Risorsa non trovata."),
            UnauthorizedAccessException     => (StatusCodes.Status401Unauthorized,        "Non autorizzato."),
            OperationCanceledException      => (StatusCodes.Status499ClientClosedRequest, "Richiesta annullata."),
            _                               => (StatusCodes.Status500InternalServerError, "Errore interno del server.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Eccezione non gestita: {Message}", ex.Message);
        else
            _logger.LogWarning(ex, "Eccezione di dominio: {Message}", ex.Message);

        var problem = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = ex.Message,
            Instance = context.Request.Path
        };

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, _jsonOptions));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
        => services.AddTransient<ExceptionHandlingMiddleware>();

    public static IApplicationBuilder UseIlpEftiExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
