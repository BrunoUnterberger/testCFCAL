namespace PartageTexte.Application.DTOs;

/// <summary>Données nécessaires pour accéder au contenu d'un partage.</summary>
public sealed class AccederPartageRequete
{
    /// <summary>Identifiant du partage.</summary>
    public Guid Id { get; init; }

    /// <summary>Mot de passe (requis uniquement si le partage est protégé).</summary>
    public string? MotDePasse { get; init; }
}
