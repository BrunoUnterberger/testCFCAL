using PartageTexte.Domain.Entites;

namespace PartageTexte.Application.Interfaces;

/// <summary>
/// Contrat d'accès aux données pour l'entité <see cref="Partage"/>.
/// </summary>
public interface IDepotPartage
{
    /// <summary>Persiste un nouveau partage.</summary>
    Task AjouterAsync(Partage partage, CancellationToken annulation = default);

    /// <summary>Recherche un partage par son identifiant.</summary>
    /// <returns>Le partage trouvé, ou null s'il n'existe pas.</returns>
    Task<Partage?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default);

    /// <summary>Persiste les modifications d'un partage existant.</summary>
    Task MettreAJourAsync(Partage partage, CancellationToken annulation = default);

    /// <summary>Supprime un partage par son identifiant.</summary>
    Task SupprimerAsync(Guid id, CancellationToken annulation = default);
}
