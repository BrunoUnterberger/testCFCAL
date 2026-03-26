using FastEndpoints;
using PartageTexte.Application.DTOs;
using PartageTexte.Application.Exceptions;
using PartageTexte.Application.Services;

namespace PartageTexte.Api.Endpoints;

/// <summary>
/// Requête pour accéder au contenu d'un partage depuis l'URL.
/// </summary>
public sealed class AccederPartageUrlRequete
{
    /// <summary>Identifiant du partage (segment de route).</summary>
    public Guid Id { get; init; }

    /// <summary>Mot de passe (optionnel, envoyé dans le corps).</summary>
    public string? MotDePasse { get; init; }
}

/// <summary>
/// Endpoint POST /api/partages/{id}/acceder — Accède au contenu déchiffré d'un partage.
/// </summary>
public sealed class AccederPartageEndpoint : Endpoint<AccederPartageUrlRequete, ContenuPartageReponse>
{
    private readonly ServicePartage _service;

    public AccederPartageEndpoint(ServicePartage service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("/api/partages/{id}/acceder");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Accède au contenu déchiffré d'un partage.";
            s.Description = "Fournir le mot de passe si le partage est protégé.";
        });
    }

    public override async Task HandleAsync(AccederPartageUrlRequete req, CancellationToken ct)
    {
        try
        {
            var requete = new AccederPartageRequete
            {
                Id = req.Id,
                MotDePasse = req.MotDePasse
            };

            var reponse = await _service.AccederAsync(requete, ct);
            await SendAsync(reponse, 200, ct);
        }
        catch (PartageException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(400, ct);
        }
    }
}
