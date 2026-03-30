using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PartageTexte.Application.Interfaces;
using PartageTexte.Domain.Entites;
using PartageTexte.Domain.Enumerations;

namespace PartageTexte.Infrastructure.Persistance;

/// <summary>
/// Implémentation de <see cref="IDepotPartage"/> avec persistance sur le système de fichiers.
/// Chaque partage est sérialisé en JSON dans un fichier dédié.
/// Compatible avec les déploiements multi-conteneurs via un volume partagé.
/// </summary>
public sealed class DepotPartageFichier : IDepotPartage
{
    private readonly string _dossier;

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    public DepotPartageFichier(IConfiguration configuration)
    {
        _dossier = configuration["Stockage:Chemin"] ?? "/data/partages";
        Directory.CreateDirectory(_dossier);
    }

    /// <inheritdoc/>
    public Task AjouterAsync(Partage partage, CancellationToken annulation = default)
    {
        var chemin = CheminFichier(partage.Id);
        var dto = VersDto(partage);
        var json = JsonSerializer.Serialize(dto, _options);
        File.WriteAllText(chemin, json);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<Partage?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default)
    {
        var chemin = CheminFichier(id);
        if (!File.Exists(chemin))
            return Task.FromResult<Partage?>(null);

        var json = File.ReadAllText(chemin);
        var dto = JsonSerializer.Deserialize<PartageDto>(json, _options);
        return Task.FromResult<Partage?>(dto is null ? null : DepuisDto(dto));
    }

    /// <inheritdoc/>
    public Task MettreAJourAsync(Partage partage, CancellationToken annulation = default)
        => AjouterAsync(partage, annulation);

    /// <inheritdoc/>
    public Task SupprimerAsync(Guid id, CancellationToken annulation = default)
    {
        var chemin = CheminFichier(id);
        if (File.Exists(chemin))
            File.Delete(chemin);
        return Task.CompletedTask;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private string CheminFichier(Guid id) => Path.Combine(_dossier, $"{id:N}.json");

    private static PartageDto VersDto(Partage p) => new(
        p.Id,
        p.ContenuChiffre,
        p.TypeContenu,
        p.DateCreation,
        p.DateExpiration,
        p.EstProtege,
        p.MotDePasseHash,
        p.NombreLecturesMax,
        p.NombreLectures);

    private static Partage DepuisDto(PartageDto d)
    {
        var p = Partage.Creer(
            d.ContenuChiffre,
            d.TypeContenu,
            d.DateExpiration,
            d.MotDePasseHash,
            d.NombreLecturesMax);

        // Restaurer l'id et le compteur via la réflexion est évitable :
        // on passe par un constructeur interne exposé uniquement pour la désérialisation.
        return p.AvecEtat(d.Id, d.DateCreation, d.NombreLectures);
    }

    // ── DTO de sérialisation ─────────────────────────────────────────────────

    private record PartageDto(
        Guid Id,
        string ContenuChiffre,
        TypeContenu TypeContenu,
        DateTime DateCreation,
        DateTime? DateExpiration,
        bool EstProtege,
        string? MotDePasseHash,
        int? NombreLecturesMax,
        int NombreLectures);
}
