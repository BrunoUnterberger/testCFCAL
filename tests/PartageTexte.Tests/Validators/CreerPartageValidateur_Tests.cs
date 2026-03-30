using FluentAssertions;
using PartageTexte.Application.DTOs;
using PartageTexte.Application.Validators;
using PartageTexte.Domain.Enumerations;

namespace PartageTexte.Tests.Validators;

/// <summary>
/// Tests unitaires du validateur FluentValidation pour la création d'un partage.
/// </summary>
public sealed class CreerPartageValidateur_Tests
{
    private readonly CreerPartageValidateur _validateur = new();

    [Fact]
    public async Task Valider_AvecRequeteValide_RetourneSucces()
    {
        // Arrange
        var requete = new CreerPartageRequete
        {
            Contenu = "Texte valide",
            TypeContenu = TypeContenu.Texte,
            DateExpiration = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var resultat = await _validateur.ValidateAsync(requete);

        // Assert
        resultat.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Valider_AvecContenuVide_RetourneErreur()
    {
        // Arrange
        var requete = new CreerPartageRequete { Contenu = "", DateExpiration = DateTime.UtcNow.AddDays(7) };

        // Act
        var resultat = await _validateur.ValidateAsync(requete);

        // Assert
        resultat.IsValid.Should().BeFalse();
        resultat.Errors.Should().Contain(e => e.PropertyName == nameof(CreerPartageRequete.Contenu));
    }

    [Fact]
    public async Task Valider_SansDateExpiration_RetourneErreur()
    {
        // Arrange
        var requete = new CreerPartageRequete { Contenu = "Texte", DateExpiration = null };

        // Act
        var resultat = await _validateur.ValidateAsync(requete);

        // Assert
        resultat.IsValid.Should().BeFalse();
        resultat.Errors.Should().Contain(e => e.PropertyName == nameof(CreerPartageRequete.DateExpiration));
    }

    [Fact]
    public async Task Valider_AvecDateExpirationPassee_RetourneErreur()
    {
        // Arrange
        var requete = new CreerPartageRequete
        {
            Contenu = "Texte",
            DateExpiration = DateTime.UtcNow.AddDays(-1) // Dans le passé
        };

        // Act
        var resultat = await _validateur.ValidateAsync(requete);

        // Assert
        resultat.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Valider_AvecMotDePasseTropCourt_RetourneErreur()
    {
        // Arrange
        var requete = new CreerPartageRequete
        {
            Contenu = "Texte",
            DateExpiration = DateTime.UtcNow.AddDays(7),
            MotDePasse = "abc" // Moins de 4 caractères
        };

        // Act
        var resultat = await _validateur.ValidateAsync(requete);

        // Assert
        resultat.IsValid.Should().BeFalse();
        resultat.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreerPartageRequete.MotDePasse));
    }

    [Fact]
    public async Task Valider_AvecNombreLecturesMaxNegatif_RetourneErreur()
    {
        // Arrange
        var requete = new CreerPartageRequete
        {
            Contenu = "Texte",
            DateExpiration = DateTime.UtcNow.AddDays(7),
            NombreLecturesMax = 0 // Doit être > 0
        };

        // Act
        var resultat = await _validateur.ValidateAsync(requete);

        // Assert
        resultat.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Valider_AvecMotDePasseNull_RetourneSucces()
    {
        // Arrange - Le mot de passe est optionnel
        var requete = new CreerPartageRequete
        {
            Contenu = "Texte valide",
            DateExpiration = DateTime.UtcNow.AddDays(7),
            MotDePasse = null
        };

        // Act
        var resultat = await _validateur.ValidateAsync(requete);

        // Assert
        resultat.IsValid.Should().BeTrue();
    }
}
