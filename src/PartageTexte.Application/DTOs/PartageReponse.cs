namespace PartageTexte.Application.DTOs;

/// <summary>Réponse retournée après la création d'un partage.</summary>
public sealed class PartageReponse
{
    /// <summary>Identifiant unique du partage (à utiliser dans l'URL de partage).</summary>
    public Guid Id { get; init; }

    /// <summary>Date d'expiration du partage (null = jamais).</summary>
    public DateTime? DateExpiration { get; init; }

    /// <summary>Indique si le partage est protégé par un mot de passe.</summary>
    public bool EstProtege { get; init; }

    /// <summary>Nombre de lectures maximum (null = illimité).</summary>
    public int? NombreLecturesMax { get; init; }
}
