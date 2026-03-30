using FluentAssertions;
using PartageTexte.Infrastructure.Services;

namespace PartageTexte.Tests.Services;

/// <summary>
/// Tests unitaires du service de hachage PBKDF2.
/// </summary>
public sealed class ServiceHachage_Tests
{
    private readonly ServiceHachage _service = new();

    [Fact]
    public void Hacher_AvecMotDePasseValide_RetourneHashNonVide()
    {
        // Arrange
        const string motDePasse = "MonMotDePasse123!";

        // Act
        var hash = _service.Hacher(motDePasse);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Verifier_AvecBonMotDePasse_RetourneTrue()
    {
        // Arrange
        const string motDePasse = "MonMotDePasse123!";
        var hash = _service.Hacher(motDePasse);

        // Act
        var resultat = _service.Verifier(motDePasse, hash);

        // Assert
        resultat.Should().BeTrue();
    }

    [Fact]
    public void Verifier_AvecMauvaisMotDePasse_RetourneFalse()
    {
        // Arrange
        const string motDePasse = "MonMotDePasse123!";
        const string mauvaisMotDePasse = "AutreMotDePasse!";
        var hash = _service.Hacher(motDePasse);

        // Act
        var resultat = _service.Verifier(mauvaisMotDePasse, hash);

        // Assert
        resultat.Should().BeFalse();
    }

    [Fact]
    public void Hacher_DeuxFois_ProduitsHashsDifferents()
    {
        // Arrange - Le salt est aléatoire donc chaque hash doit être différent
        const string motDePasse = "MemeMotDePasse";

        // Act
        var hash1 = _service.Hacher(motDePasse);
        var hash2 = _service.Hacher(motDePasse);

        // Assert
        hash1.Should().NotBe(hash2);
    }
}
