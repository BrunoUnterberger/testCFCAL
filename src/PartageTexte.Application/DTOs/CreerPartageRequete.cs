using PartageTexte.Domain.Enumerations;

namespace PartageTexte.Application.DTOs;

/// <summary>Données nécessaires à la création d'un partage.</summary>
public sealed class CreerPartageRequete
{
    /// <summary>Contenu en clair à partager (texte ou mot de passe).</summary>
    public string Contenu { get; init; } = string.Empty;

    /// <summary>Type du contenu (Texte ou MotDePasse).</summary>
    public TypeContenu TypeContenu { get; init; } = TypeContenu.Texte;

    /// <summary>Date d'expiration en UTC. Null = jamais expiré.</summary>
    public DateTime? DateExpiration { get; init; }

    /// <summary>Mot de passe de protection. Null ou vide = pas de protection.</summary>
    public string? MotDePasse { get; init; }

    /// <summary>Nombre de lectures maximum. Null = illimité.</summary>
    public int? NombreLecturesMax { get; init; }
}
