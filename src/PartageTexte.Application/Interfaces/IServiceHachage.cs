namespace PartageTexte.Application.Interfaces;

/// <summary>
/// Contrat pour le hachage et la vérification de mots de passe (PBKDF2).
/// </summary>
public interface IServiceHachage
{
    /// <summary>
    /// Calcule le hash PBKDF2 d'un mot de passe.
    /// </summary>
    /// <param name="motDePasse">Mot de passe en clair.</param>
    /// <returns>Hash encodé en Base64 (salt + hash).</returns>
    string Hacher(string motDePasse);

    /// <summary>
    /// Vérifie si un mot de passe en clair correspond à un hash stocké.
    /// </summary>
    /// <param name="motDePasse">Mot de passe en clair à vérifier.</param>
    /// <param name="hash">Hash stocké (produit par <see cref="Hacher"/>).</param>
    /// <returns>True si le mot de passe correspond au hash.</returns>
    bool Verifier(string motDePasse, string hash);
}
