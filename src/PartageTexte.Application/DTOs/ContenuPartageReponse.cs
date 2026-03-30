using PartageTexte.Domain.Enumerations;

namespace PartageTexte.Application.DTOs;

/// <summary>Contenu déchiffré retourné lors de l'accès à un partage.</summary>
public sealed class ContenuPartageReponse
{
    /// <summary>Contenu en clair déchiffré.</summary>
    public string Contenu { get; init; } = string.Empty;

    /// <summary>Type du contenu (Texte ou MotDePasse).</summary>
    public TypeContenu TypeContenu { get; init; }
}
