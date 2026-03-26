using PartageTexte.Application.DTOs;

namespace PartageTexte.Web.Services;

/// <summary>
/// Contrat du client HTTP pour accéder à l'API de partage.
/// </summary>
public interface IClientPartage
{
    /// <summary>Crée un partage via l'API et retourne les informations du partage créé.</summary>
    Task<PartageReponse?> CreerAsync(CreerPartageRequete requete, CancellationToken annulation = default);

    /// <summary>Accède au contenu d'un partage (avec mot de passe optionnel).</summary>
    Task<ContenuPartageReponse?> AccederAsync(AccederPartageRequete requete, CancellationToken annulation = default);

    /// <summary>Récupère les méta-données d'un partage (sans le contenu).</summary>
    Task<InfoPartageReponse?> ObtenirInfoAsync(Guid id, CancellationToken annulation = default);
}
