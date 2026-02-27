using ilp_efti_connector.Infrastructure.DependencyInjection;
using ilp_efti_connector.Shared.Contracts.Dtos;
using ilp_efti_connector.Shared.Contracts.Events;
using ilp_efti_connector.Shared.Infrastructure.Extensions;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Text.Json;
using ilp_efti_connector.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Registrazione servizi infrastrutturali e autenticazione
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIlpEftiAuth(builder.Configuration);
builder.Services.AddIlpEftiMessaging(builder.Configuration);
builder.Services.AddOpenApi();

// Abilita i controller MVC
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opts => opts.Title = "ILP eFTI — API Gateway");
}

app.UseAuthentication();
app.UseAuthorization();

// Mappa i controller MVC
app.MapControllers();

app.Run();
