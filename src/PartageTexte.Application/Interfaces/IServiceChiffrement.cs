namespace PartageTexte.Application.Interfaces;

/// <summary>
/// Contrat pour le chiffrement et déchiffrement de données (AES-256).
/// </summary>
public interface IServiceChiffrement
{
    /// <summary>
    /// Chiffre un texte en clair et retourne le résultat encodé en Base64.
    /// </summary>
    /// <param name="texteEnClair">Texte à chiffrer.</param>
    /// <returns>Texte chiffré encodé en Base64 (IV + données chiffrées).</returns>
    string Chiffrer(string texteEnClair);

    /// <summary>
    /// Déchiffre un texte chiffré (Base64) et retourne le texte en clair.
    /// </summary>
    /// <param name="texteChiffre">Texte chiffré encodé en Base64.</param>
    /// <returns>Texte en clair déchiffré.</returns>
    string Dechiffrer(string texteChiffre);
}
