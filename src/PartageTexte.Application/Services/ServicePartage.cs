using FluentValidation;
using PartageTexte.Application.DTOs;
using PartageTexte.Application.Exceptions;
using PartageTexte.Application.Interfaces;
using PartageTexte.Domain.Entites;

namespace PartageTexte.Application.Services;

/// <summary>
/// Service métier gérant la création et l'accès aux partages.
/// </summary>
public sealed class ServicePartage
{
    private readonly IDepotPartage _depot;
    private readonly IServiceChiffrement _chiffrement;
    private readonly IServiceHachage _hachage;
    private readonly IValidator<CreerPartageRequete> _validateur;

    public ServicePartage(
        IDepotPartage depot,
        IServiceChiffrement chiffrement,
        IServiceHachage hachage,
        IValidator<CreerPartageRequete> validateur)
    {
        _depot = depot;
        _chiffrement = chiffrement;
        _hachage = hachage;
        _validateur = validateur;
    }

    /// <summary>
    /// Crée un nouveau partage chiffré.
    /// </summary>
    /// <param name="requete">Données de création du partage.</param>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Informations du partage créé (id, expiration, protection).</returns>
    /// <exception cref="ValidationException">Si les données sont invalides.</exception>
    public async Task<PartageReponse> CreerAsync(
        CreerPartageRequete requete,
        CancellationToken annulation = default)
    {
        // Validation FluentValidation
        var resultatValidation = await _validateur.ValidateAsync(requete, annulation);
        if (!resultatValidation.IsValid)
            throw new ValidationException(resultatValidation.Errors);

        // Chiffrement du contenu
        var contenuChiffre = _chiffrement.Chiffrer(requete.Contenu);

        // Hachage du mot de passe si fourni
        string? motDePasseHash = null;
        if (!string.IsNullOrEmpty(requete.MotDePasse))
            motDePasseHash = _hachage.Hacher(requete.MotDePasse);

        // Création de l'entité
        var partage = Partage.Creer(
            contenuChiffre,
            requete.TypeContenu,
            requete.DateExpiration,
            motDePasseHash,
            requete.NombreLecturesMax);

        await _depot.AjouterAsync(partage, annulation);

        return new PartageReponse
        {
            Id = partage.Id,
            DateExpiration = partage.DateExpiration,
            EstProtege = partage.EstProtege,
            NombreLecturesMax = partage.NombreLecturesMax
        };
    }

    /// <summary>
    /// Accède au contenu déchiffré d'un partage.
    /// </summary>
    /// <param name="requete">Identifiant et mot de passe optionnel.</param>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Contenu déchiffré et type du contenu.</returns>
    /// <exception cref="PartageException">
    /// Si le partage est introuvable, expiré, épuisé ou si le mot de passe est incorrect.
    /// </exception>
    public async Task<ContenuPartageReponse> AccederAsync(
        AccederPartageRequete requete,
        CancellationToken annulation = default)
    {
        var partage = await _depot.ObtenirParIdAsync(requete.Id, annulation)
            ?? throw new PartageException("Ce partage est introuvable ou a été supprimé.");

        if (partage.EstExpire())
            throw new PartageException("Ce partage a expiré.");

        if (partage.EstEpuise())
            throw new PartageException("Ce partage a atteint son nombre de lectures maximum.");

        // Vérification du mot de passe
        if (partage.EstProtege)
        {
            if (string.IsNullOrEmpty(requete.MotDePasse))
                throw new PartageException("Ce partage est protégé par un mot de passe.");

            if (!_hachage.Verifier(requete.MotDePasse, partage.MotDePasseHash!))
                throw new PartageException("Mot de passe incorrect.");
        }

        // Déchiffrement
        var contenuEnClair = _chiffrement.Dechiffrer(partage.ContenuChiffre);

        // Incrémenter le compteur de lectures
        partage.EnregistrerLecture();
        await _depot.MettreAJourAsync(partage, annulation);

        return new ContenuPartageReponse
        {
            Contenu = contenuEnClair,
            TypeContenu = partage.TypeContenu
        };
    }

    /// <summary>
    /// Retourne les méta-données d'un partage sans son contenu.
    /// </summary>
    /// <param name="id">Identifiant du partage.</param>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Méta-données du partage.</returns>
    /// <exception cref="PartageException">Si le partage est introuvable.</exception>
    public async Task<InfoPartageReponse> ObtenirInfoAsync(
        Guid id,
        CancellationToken annulation = default)
    {
        var partage = await _depot.ObtenirParIdAsync(id, annulation)
            ?? throw new PartageException("Ce partage est introuvable ou a été supprimé.");

        return new InfoPartageReponse
        {
            DateExpiration = partage.DateExpiration,
            EstProtege = partage.EstProtege,
            NombreLectures = partage.NombreLectures,
            NombreLecturesMax = partage.NombreLecturesMax
        };
    }
}
