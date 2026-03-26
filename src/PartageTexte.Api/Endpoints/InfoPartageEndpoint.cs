using FastEndpoints;
using PartageTexte.Application.DTOs;
using PartageTexte.Application.Exceptions;
using PartageTexte.Application.Services;

namespace PartageTexte.Api.Endpoints;

/// <summary>
/// Requête pour obtenir les méta-données d'un partage.
/// </summary>
public sealed class InfoPartageUrlRequete
{
    /// <summary>Identifiant du partage (segment de route).</summary>
    public Guid Id { get; init; }
}

/// <summary>
/// Endpoint GET /api/partages/{id}/info — Retourne les méta-données d'un partage (sans le contenu).
/// </summary>
public sealed class InfoPartageEndpoint : Endpoint<InfoPartageUrlRequete, InfoPartageReponse>
{
    private readonly ServicePartage _service;

    public InfoPartageEndpoint(ServicePartage service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("/api/partages/{id}/info");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Retourne les méta-données d'un partage (expiration, protection, lectures).";
        });
    }

    public override async Task HandleAsync(InfoPartageUrlRequete req, CancellationToken ct)
    {
        try
        {
            var reponse = await _service.ObtenirInfoAsync(req.Id, ct);
            await SendAsync(reponse, 200, ct);
        }
        catch (PartageException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(404, ct);
        }
    }
}
