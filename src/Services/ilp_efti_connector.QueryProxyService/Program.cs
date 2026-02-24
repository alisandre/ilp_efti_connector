using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.QueryProxyService.Endpoints;
using ilp_efti_connector.Shared.Infrastructure.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Infrastruttura ───────────────────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Autenticazione JWT (Keycloak) ────────────────────────────────────────────
builder.Services.AddIlpEftiAuth(builder.Configuration);

// ─── MassTransit — solo publish (nessun consumer) ────────────────────────────
builder.Services.AddIlpEftiMessaging(builder.Configuration);

// ─── CORS per frontend React ──────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("ReactFrontend", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000"])
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ─── OpenAPI + Scalar ─────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opts => opts.Title = "ILP eFTI — Query Proxy Service");
}

app.UseCors("ReactFrontend");
app.UseAuthentication();
app.UseAuthorization();

// ─── Endpoints ────────────────────────────────────────────────────────────────
app.MapOperationQueryEndpoints();
app.MapMessageQueryEndpoints();
app.MapSseEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "QueryProxyService" }))
   .AllowAnonymous();

app.Run();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
