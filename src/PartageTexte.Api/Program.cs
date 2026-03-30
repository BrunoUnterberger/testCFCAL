using FastEndpoints;
using PartageTexte.Application.Extensions;
using PartageTexte.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Couche Application (services métier + validators FluentValidation)
builder.Services.AjouterApplication();

// Couche Infrastructure (dépôt en mémoire + chiffrement + hachage)
builder.Services.AjouterInfrastructure(builder.Configuration);

// FastEndpoints
builder.Services.AddFastEndpoints();

// CORS pour autoriser le frontend Blazor
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://localhost:7210", "http://localhost:5110", "https://localhost:5002", "http://localhost:5002")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();
app.UseFastEndpoints();

app.Run();
