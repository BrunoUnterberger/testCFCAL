using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PartageTexte.Application.DTOs;
using PartageTexte.Application.Exceptions;
using PartageTexte.Application.Services;
using PartageTexte.Application.Validators;
using PartageTexte.Domain.Enumerations;
using PartageTexte.Infrastructure.Persistance;
using PartageTexte.Infrastructure.Services;

namespace PartageTexte.Tests.Services;

/// <summary>
/// Tests unitaires de ServicePartage.AccederAsync.
/// </summary>
public sealed class ServicePartage_AccederAsync_Tests : IDisposable
{
    private readonly string _dossierTemp;
    private readonly DepotPartageFichier _depot;
    private readonly ServicePartage _service;
    private readonly ServiceChiffrement _chiffrement;

    public ServicePartage_AccederAsync_Tests()
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
        _chiffrement = new ServiceChiffrement(config);
        var hachage = new ServiceHachage();
        var validateur = new CreerPartageValidateur();

        _service = new ServicePartage(_depot, _chiffrement, hachage, validateur);
    }

    [Fact]
    public async Task AccederAsync_AvecIdValide_RetourneContenuDechiffre()
    {
        const string contenuOriginal = "Texte secret de test";
        var creation = await _service.CreerAsync(new CreerPartageRequete
        {
            Contenu = contenuOriginal,
            TypeContenu = TypeContenu.Texte,
            DateExpiration = DateTime.UtcNow.AddDays(7)
        });

        var reponse = await _service.AccederAsync(new AccederPartageRequete { Id = creation.Id });

        reponse.Contenu.Should().Be(contenuOriginal);
        reponse.TypeContenu.Should().Be(TypeContenu.Texte);
    }

    [Fact]
    public async Task AccederAsync_AvecIdInexistant_LevePartageException()
    {
        var requete = new AccederPartageRequete { Id = Guid.NewGuid() };

        var action = () => _service.AccederAsync(requete);

        await action.Should().ThrowAsync<PartageException>()
            .WithMessage("*introuvable*");
    }

    [Fact]
    public async Task AccederAsync_AvecPartageExpire_LevePartageException()
    {
        var contenuChiffre = _chiffrement.Chiffrer("Contenu expiré");
        var partageExpire = Domain.Entites.Partage.Creer(
            contenuChiffre,
            TypeContenu.Texte,
            DateTime.UtcNow.AddDays(-1),
            null,
            null);

        await _depot.AjouterAsync(partageExpire);

        var action = () => _service.AccederAsync(new AccederPartageRequete { Id = partageExpire.Id });

        await action.Should().ThrowAsync<PartageException>()
            .WithMessage("*expiré*");
    }

    [Fact]
    public async Task AccederAsync_AvecMotDePasseCorrect_RetourneContenu()
    {
        const string motDePasse = "secret123";
        const string contenu = "Texte protégé";

        var creation = await _service.CreerAsync(new CreerPartageRequete
        {
            Contenu = contenu,
            DateExpiration = DateTime.UtcNow.AddDays(7),
            MotDePasse = motDePasse
        });

        var reponse = await _service.AccederAsync(new AccederPartageRequete
        {
            Id = creation.Id,
            MotDePasse = motDePasse
        });

        reponse.Contenu.Should().Be(contenu);
    }

    [Fact]
    public async Task AccederAsync_AvecMotDePasseIncorrect_LevePartageException()
    {
        var creation = await _service.CreerAsync(new CreerPartageRequete
        {
            Contenu = "Contenu protégé",
            DateExpiration = DateTime.UtcNow.AddDays(7),
            MotDePasse = "bonmotdepasse"
        });

        var action = () => _service.AccederAsync(new AccederPartageRequete
        {
            Id = creation.Id,
            MotDePasse = "mauvaimotdepasse"
        });

        await action.Should().ThrowAsync<PartageException>()
            .WithMessage("*incorrect*");
    }

    [Fact]
    public async Task AccederAsync_IncrementeLeCompteurDeLectures()
    {
        var creation = await _service.CreerAsync(new CreerPartageRequete
        {
            Contenu = "Texte à compter",
            DateExpiration = DateTime.UtcNow.AddDays(7)
        });

        await _service.AccederAsync(new AccederPartageRequete { Id = creation.Id });
        await _service.AccederAsync(new AccederPartageRequete { Id = creation.Id });

        var partage = await _depot.ObtenirParIdAsync(creation.Id);
        partage!.NombreLectures.Should().Be(2);
    }

    [Fact]
    public async Task AccederAsync_QuandNombreLecturesMaxAtteint_LevePartageException()
    {
        var creation = await _service.CreerAsync(new CreerPartageRequete
        {
            Contenu = "Lecture unique",
            DateExpiration = DateTime.UtcNow.AddDays(7),
            NombreLecturesMax = 1
        });

        await _service.AccederAsync(new AccederPartageRequete { Id = creation.Id });

        var action = () => _service.AccederAsync(new AccederPartageRequete { Id = creation.Id });

        await action.Should().ThrowAsync<PartageException>()
            .WithMessage("*lectures maximum*");
    }

    public void Dispose()
    {
        if (Directory.Exists(_dossierTemp))
            Directory.Delete(_dossierTemp, recursive: true);
    }
}
