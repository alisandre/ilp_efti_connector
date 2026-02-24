using ilp_efti_connector.Application.Customers.Queries.GetAutoCreatedCustomers;
using ilp_efti_connector.Application.Customers.Queries.GetCustomerByCode;
using MediatR;

namespace ilp_efti_connector.FormInputService.Endpoints;

public static class CustomerEndpoints
{
    public static WebApplication MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/forms/customers")
            .WithTags("FormInput - Customers")
            .RequireAuthorization();

        // ─── GET / — lista clienti per autocomplete ────────────────────────
        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var customers = await mediator.Send(new GetAutoCreatedCustomersQuery(), ct);
            return Results.Ok(customers);
        })
        .WithName("GetCustomers")
        .WithSummary("Restituisce la lista clienti (auto-creati) per il dropdown del form.");

        // ─── GET /{code} — singolo cliente per codice ──────────────────────
        group.MapGet("/{code}", async (string code, IMediator mediator, CancellationToken ct) =>
        {
            var customer = await mediator.Send(new GetCustomerByCodeQuery(code), ct);
            return customer is null
                ? Results.NotFound(new { error = $"Cliente '{code}' non trovato." })
                : Results.Ok(customer);
        })
        .WithName("GetCustomerByCode")
        .WithSummary("Restituisce un cliente per codice.");

        return app;
    }
}
