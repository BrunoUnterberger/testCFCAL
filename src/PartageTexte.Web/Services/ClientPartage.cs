using Flurl;
using Flurl.Http;
using PartageTexte.Application.DTOs;

namespace PartageTexte.Web.Services;

/// <summary>
/// Implémentation du client HTTP utilisant Flurl pour appeler l'API PartageTexte.
/// </summary>
public sealed class ClientPartage : IClientPartage
{
    private readonly Url _urlBase;

    public ClientPartage(IConfiguration configuration)
    {
        var urlBase = configuration["Api:UrlBase"]
            ?? throw new InvalidOperationException("La configuration 'Api:UrlBase' est manquante.");
        _urlBase = new Url(urlBase);
    }

    /// <inheritdoc/>
    public async Task<PartageReponse?> CreerAsync(
        CreerPartageRequete requete,
        CancellationToken annulation = default)
    {
        try
        {
            return await _urlBase.Clone()
                .AppendPathSegment("api/partages")
                .PostJsonAsync(requete, cancellationToken: annulation)
                .ReceiveJson<PartageReponse>();
        }
        catch (FlurlHttpException ex)
        {
            var message = await ExtraireMessageErreurAsync(ex);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <inheritdoc/>
    public async Task<ContenuPartageReponse?> AccederAsync(
        AccederPartageRequete requete,
        CancellationToken annulation = default)
    {
        try
        {
            return await _urlBase.Clone()
                .AppendPathSegments("api/partages", requete.Id, "acceder")
                .PostJsonAsync(new { requete.MotDePasse }, cancellationToken: annulation)
                .ReceiveJson<ContenuPartageReponse>();
        }
        catch (FlurlHttpException)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<InfoPartageReponse?> ObtenirInfoAsync(
        Guid id,
        CancellationToken annulation = default)
    {
        try
        {
            return await _urlBase.Clone()
                .AppendPathSegments("api/partages", id, "info")
                .GetJsonAsync<InfoPartageReponse>(cancellationToken: annulation);
        }
        catch (FlurlHttpException)
        {
            return null;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async Task<string> ExtraireMessageErreurAsync(FlurlHttpException ex)
    {
        try
        {
            // FastEndpoints retourne les erreurs de validation sous forme de JSON structuré
            var erreurs = await ex.GetResponseJsonAsync<ErreurApiDto>();
            if (erreurs?.Errors is { Count: > 0 })
            {
                var messages = erreurs.Errors.Values
                    .SelectMany(liste => liste)
                    .Where(m => !string.IsNullOrWhiteSpace(m));
                return string.Join(" — ", messages);
            }
            if (!string.IsNullOrWhiteSpace(erreurs?.Message))
                return erreurs.Message;
        }
        catch { /* corps non JSON, on utilisera le message HTTP */ }

        return ex.StatusCode switch
        {
            400 => "Les données saisies sont invalides.",
            404 => "Ce partage est introuvable.",
            _ => "Une erreur est survenue. Réessayez."
        };
    }

    private sealed class ErreurApiDto
    {
        public string? Message { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();
    }
}
