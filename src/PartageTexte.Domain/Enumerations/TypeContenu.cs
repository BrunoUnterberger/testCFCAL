namespace PartageTexte.Domain.Enumerations;

/// <summary>Type du contenu partagé.</summary>
public enum TypeContenu
{
    /// <summary>Texte libre (note, snippet de code, etc.).</summary>
    Texte = 0,

    /// <summary>Mot de passe ou secret sensible.</summary>
    MotDePasse = 1
}
