using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using PartageTexte.Application.Interfaces;

namespace PartageTexte.Infrastructure.Services;

/// <summary>
/// Implémentation du chiffrement AES-256-CBC avec IV aléatoire.
/// La clé est stockée dans la configuration sous la clé "Chiffrement:Cle" (Base64, 32 octets).
/// </summary>
/// <example>
/// Génération d'une clé : Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
/// Le résultat chiffré = Base64(IV[16 octets] + données chiffrées)
/// </example>
public sealed class ServiceChiffrement : IServiceChiffrement
{
    private readonly byte[] _cle;

    public ServiceChiffrement(IConfiguration configuration)
    {
        var cleBase64 = configuration["Chiffrement:Cle"]
            ?? throw new InvalidOperationException("La clé de chiffrement 'Chiffrement:Cle' est manquante dans la configuration.");

        _cle = Convert.FromBase64String(cleBase64);

        if (_cle.Length != 32)
            throw new InvalidOperationException("La clé de chiffrement doit être de 256 bits (32 octets).");
    }

    /// <inheritdoc/>
    public string Chiffrer(string texteEnClair)
    {
        using var aes = Aes.Create();
        aes.Key = _cle;
        aes.GenerateIV(); // IV aléatoire à chaque chiffrement

        using var chiffreur = aes.CreateEncryptor();
        var octetsSource = Encoding.UTF8.GetBytes(texteEnClair);
        var octetsChiffres = chiffreur.TransformFinalBlock(octetsSource, 0, octetsSource.Length);

        // Concaténation IV (16 octets) + données chiffrées
        var resultat = new byte[aes.IV.Length + octetsChiffres.Length];
        Buffer.BlockCopy(aes.IV, 0, resultat, 0, aes.IV.Length);
        Buffer.BlockCopy(octetsChiffres, 0, resultat, aes.IV.Length, octetsChiffres.Length);

        return Convert.ToBase64String(resultat);
    }

    /// <inheritdoc/>
    public string Dechiffrer(string texteChiffre)
    {
        var donnees = Convert.FromBase64String(texteChiffre);

        // Extraction de l'IV (16 premiers octets) et des données chiffrées
        const int tailleIv = 16;
        var iv = new byte[tailleIv];
        var octetsChiffres = new byte[donnees.Length - tailleIv];
        Buffer.BlockCopy(donnees, 0, iv, 0, tailleIv);
        Buffer.BlockCopy(donnees, tailleIv, octetsChiffres, 0, octetsChiffres.Length);

        using var aes = Aes.Create();
        aes.Key = _cle;
        aes.IV = iv;

        using var dechiffreur = aes.CreateDecryptor();
        var octetsClairs = dechiffreur.TransformFinalBlock(octetsChiffres, 0, octetsChiffres.Length);

        return Encoding.UTF8.GetString(octetsClairs);
    }
}
