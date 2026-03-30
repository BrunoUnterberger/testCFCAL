using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using PartageTexte.Application.DTOs;
using PartageTexte.Application.Services;
using PartageTexte.Application.Validators;
using PartageTexte.Domain.Enumerations;
using PartageTexte.Infrastructure.Persistance;
using PartageTexte.Infrastructure.Services;

namespace PartageTexte.Tests.Services;

/// <summary>
/// Tests unitaires de ServicePartage.CreerAsync.
/// </summary>
public sealed class ServicePartage_CreerAsync_Tests : IDisposable
{
    private readonly string _dossierTemp;
    private readonly DepotPartageFichier _depot;
    private readonly ServicePartage _service;

    public ServicePartage_CreerAsync_Tests()
    {
        _dossierTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Chiffrement:Cle"] = "RVybXs9k6SQw+n6ix3icQ1BkhbPU6mSm6dUkEJpg1Uc=",
                ["Stockage:Chemin"] = _dossierTemp
            })
            .Build();

        _depot = new DepotPartageFichier(config);
        var chiffrement = new ServiceChiffrement(config);
        var hachage = new ServiceHachage();
        var validateur = new CreerPartageValidateur();

        _service = new ServicePartage(_depot, chiffrement, hachage, validateur);
    }

    [Fact]
    public async Task CreerAsync_AvecRequeteValide_RetourneReponseAvecId()
    {
        var requete = new CreerPartageRequete
        {
            Contenu = "Mon texte secret",
            TypeContenu = TypeContenu.Texte,
            DateExpiration = DateTime.UtcNow.AddDays(7)
        };

        var reponse = await _service.CreerAsync(requete);

        reponse.Id.Should().NotBeEmpty();
        reponse.EstProtege.Should().BeFalse();
        reponse.DateExpiration.Should().NotBeNull();
    }

    [Fact]
    public async Task CreerAsync_AvecMotDePasse_PersisteLesPartagesProteges()
    {
        var requete = new CreerPartageRequete
        {
            Contenu = "Secret protégé",
            DateExpiration = DateTime.UtcNow.AddDays(7),
            MotDePasse = "motdepasse123"
        };

        var reponse = await _service.CreerAsync(requete);

        reponse.EstProtege.Should().BeTrue();
        var partage = await _depot.ObtenirParIdAsync(reponse.Id);
        partage!.MotDePasseHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreerAsync_AvecDateExpiration_PersisteLaDate()
    {
        var expiration = DateTime.UtcNow.AddDays(7);
        var requete = new CreerPartageRequete
        {
            Contenu = "Texte avec expiration",
            DateExpiration = expiration
        };

        var reponse = await _service.CreerAsync(requete);

        reponse.DateExpiration.Should().BeCloseTo(expiration, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CreerAsync_AvecContenuVide_LeveValidationException()
    {
        var requete = new CreerPartageRequete { Contenu = "", DateExpiration = DateTime.UtcNow.AddDays(7) };

        var action = () => _service.CreerAsync(requete);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreerAsync_ChiffreLe_ContenuEnBase()
    {
        const string contenuClair = "Texte non chiffré";
        var requete = new CreerPartageRequete { Contenu = contenuClair, DateExpiration = DateTime.UtcNow.AddDays(7) };

        var reponse = await _service.CreerAsync(requete);

        var partage = await _depot.ObtenirParIdAsync(reponse.Id);
        partage!.ContenuChiffre.Should().NotBe(contenuClair);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dossierTemp))
            Directory.Delete(_dossierTemp, recursive: true);
    }
}
