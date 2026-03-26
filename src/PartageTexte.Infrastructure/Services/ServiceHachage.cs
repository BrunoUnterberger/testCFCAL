using System.Security.Cryptography;
using PartageTexte.Application.Interfaces;

namespace PartageTexte.Infrastructure.Services;

/// <summary>
/// Implémentation du hachage de mots de passe via PBKDF2 (Rfc2898DeriveBytes).
/// Aucun package NuGet externe requis.
/// </summary>
/// <example>
/// Format stocké : Base64(salt[16 octets] + hash[32 octets])
/// Paramètres : SHA-256, 350 000 itérations (recommandation OWASP 2023)
/// </example>
public sealed class ServiceHachage : IServiceHachage
{
    private const int TailleSalt = 16;        // 128 bits
    private const int TailleHash = 32;        // 256 bits
    private const int NombreIterations = 350_000;
    private static readonly HashAlgorithmName AlgorithmeHash = HashAlgorithmName.SHA256;

    /// <inheritdoc/>
    public string Hacher(string motDePasse)
    {
        // Génération d'un salt aléatoire
        var salt = RandomNumberGenerator.GetBytes(TailleSalt);

        // Dérivation PBKDF2
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            motDePasse,
            salt,
            NombreIterations,
            AlgorithmeHash,
            TailleHash);

        // Concaténation salt + hash
        var resultat = new byte[TailleSalt + TailleHash];
        Buffer.BlockCopy(salt, 0, resultat, 0, TailleSalt);
        Buffer.BlockCopy(hash, 0, resultat, TailleSalt, TailleHash);

        return Convert.ToBase64String(resultat);
    }

    /// <inheritdoc/>
    public bool Verifier(string motDePasse, string hashStocke)
    {
        var donnees = Convert.FromBase64String(hashStocke);

        // Extraction du salt et du hash
        var salt = new byte[TailleSalt];
        var hashAttendu = new byte[TailleHash];
        Buffer.BlockCopy(donnees, 0, salt, 0, TailleSalt);
        Buffer.BlockCopy(donnees, TailleSalt, hashAttendu, 0, TailleHash);

        // Re-dérivation avec le même salt
        var hashCalcule = Rfc2898DeriveBytes.Pbkdf2(
            motDePasse,
            salt,
            NombreIterations,
            AlgorithmeHash,
            TailleHash);

        // Comparaison en temps constant (protection contre les timing attacks)
        return CryptographicOperations.FixedTimeEquals(hashCalcule, hashAttendu);
    }
}
