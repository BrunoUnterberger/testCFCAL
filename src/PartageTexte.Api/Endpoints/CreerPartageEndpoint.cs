using FastEndpoints;
using FluentValidation;
using PartageTexte.Application.DTOs;
using PartageTexte.Application.Services;

namespace PartageTexte.Api.Endpoints;

/// <summary>
/// Endpoint POST /api/partages — Crée un nouveau partage chiffré.
/// </summary>
public sealed class CreerPartageEndpoint : Endpoint<CreerPartageRequete, PartageReponse>
{
    private readonly ServicePartage _service;

    public CreerPartageEndpoint(ServicePartage service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("/api/partages");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Crée un partage sécurisé de texte ou de mot de passe.";
            s.Description = "Le contenu est chiffré en AES-256. La protection par mot de passe et la date d'expiration sont optionnelles.";
        });
    }

    public override async Task HandleAsync(CreerPartageRequete req, CancellationToken ct)
    {
        try
        {
            var reponse = await _service.CreerAsync(req, ct);
            await SendAsync(reponse, 201, ct);
        }
        catch (ValidationException ex)
        {
            AddError(string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
            await SendErrorsAsync(400, ct);
        }
    }
}
