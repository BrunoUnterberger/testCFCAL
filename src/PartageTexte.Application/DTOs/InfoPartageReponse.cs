namespace PartageTexte.Application.DTOs;

/// <summary>Méta-données d'un partage (sans le contenu).</summary>
public sealed class InfoPartageReponse
{
    /// <summary>Date d'expiration du partage (null = jamais).</summary>
    public DateTime? DateExpiration { get; init; }

    /// <summary>Indique si le partage est protégé par un mot de passe.</summary>
    public bool EstProtege { get; init; }

    /// <summary>Nombre de lectures déjà effectuées.</summary>
    public int NombreLectures { get; init; }

    /// <summary>Nombre de lectures maximum (null = illimité).</summary>
    public int? NombreLecturesMax { get; init; }
}
