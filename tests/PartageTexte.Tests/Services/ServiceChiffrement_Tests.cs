using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PartageTexte.Infrastructure.Services;

namespace PartageTexte.Tests.Services;

/// <summary>
/// Tests unitaires du service de chiffrement AES-256.
/// </summary>
public sealed class ServiceChiffrement_Tests
{
    private readonly ServiceChiffrement _service;

    public ServiceChiffrement_Tests()
    {
        // Clé AES-256 de test (32 octets en Base64)
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Chiffrement:Cle"] = "RVybXs9k6SQw+n6ix3icQ1BkhbPU6mSm6dUkEJpg1Uc="
            })
            .Build();

        _service = new ServiceChiffrement(config);
    }

    [Fact]
    public void Chiffrer_AvecTexteValide_RetourneBase64NonVide()
    {
        // Arrange
        const string texte = "Bonjour monde !";

        // Act
        var resultat = _service.Chiffrer(texte);

        // Assert
        resultat.Should().NotBeNullOrEmpty();
        Convert.TryFromBase64String(resultat, new byte[resultat.Length], out _).Should().BeTrue();
    }

    [Fact]
    public void Dechiffrer_ApresChiffrement_RetourneTexteOriginal()
    {
        // Arrange
        const string texteOriginal = "Mon texte secret";

        // Act
        var chiffre = _service.Chiffrer(texteOriginal);
        var dechiffre = _service.Dechiffrer(chiffre);

        // Assert
        dechiffre.Should().Be(texteOriginal);
    }

    [Fact]
    public void Chiffrer_DeuxFois_ProduitsResultatsDifferents()
    {
        // Arrange - L'IV est aléatoire donc chaque chiffrement doit être différent
        const string texte = "Même texte";

        // Act
        var chiffre1 = _service.Chiffrer(texte);
        var chiffre2 = _service.Chiffrer(texte);

        // Assert
        chiffre1.Should().NotBe(chiffre2);
    }

    [Fact]
    public void Dechiffrer_AvecTexteVideChiffre_RetourneChaine()
    {
        // Arrange
        const string texte = "";

        // Act
        var chiffre = _service.Chiffrer(texte);
        var dechiffre = _service.Dechiffrer(chiffre);

        // Assert
        dechiffre.Should().Be(texte);
    }
}
