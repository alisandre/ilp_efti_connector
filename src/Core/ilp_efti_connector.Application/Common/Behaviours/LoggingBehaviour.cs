using MediatR;
using Microsoft.Extensions.Logging;

namespace ilp_efti_connector.Application.Common.Behaviours;

public sealed class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Elaborazione {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogInformation("{RequestName} completato con successo", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{RequestName} fallito: {Message}", requestName, ex.Message);
            throw;
        }
    }
}
