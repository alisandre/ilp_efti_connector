using FluentValidation;
using ilp_efti_connector.Application.DependencyInjection;
using ilp_efti_connector.FormInputService.Endpoints;
using ilp_efti_connector.FormInputService.Validators;
using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.Shared.Contracts.Dtos;
using ilp_efti_connector.Shared.Infrastructure.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Infrastruttura ───────────────────────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Application (MediatR + behaviours + validators) ─────────────────────────
builder.Services.AddApplicationServices();

// ─── FluentValidation (validator specifico del servizio) ─────────────────────
builder.Services.AddScoped<IValidator<SourcePayloadDto>, SourcePayloadDtoValidator>();

// ─── Autenticazione JWT (Keycloak) — sovrascrive ICurrentUserService ──────────
builder.Services.AddIlpEftiAuth(builder.Configuration);

// ─── MassTransit / RabbitMQ ───────────────────────────────────────────────────
builder.Services.AddIlpEftiMessaging(builder.Configuration);

// ─── CORS per frontend React ──────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("ReactFrontend", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000"])
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ─── OpenAPI (.NET 9 built-in) + Scalar ──────────────────────────────────────
builder.Services.AddOpenApi();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opts =>
    {
        opts.Title = "ILP eFTI — Form Input Service";
    });
}

app.UseCors("ReactFrontend");
app.UseAuthentication();
app.UseAuthorization();

// ─── Endpoints ────────────────────────────────────────────────────────────────
app.MapTransportOperationEndpoints();
app.MapCustomerEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "FormInputService" }))
   .WithTags("Health")
   .AllowAnonymous();

app.Run();
