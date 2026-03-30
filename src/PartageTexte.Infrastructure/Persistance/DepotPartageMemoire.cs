using System.Collections.Concurrent;
using PartageTexte.Application.Interfaces;
using PartageTexte.Domain.Entites;

namespace PartageTexte.Infrastructure.Persistance;

/// <summary>
/// Implémentation de <see cref="IDepotPartage"/> en mémoire (pas de base de données).
/// </summary>
public sealed class DepotPartageMemoire : IDepotPartage
{
    private readonly ConcurrentDictionary<Guid, Partage> _stockage = new();

    /// <inheritdoc/>
    public Task AjouterAsync(Partage partage, CancellationToken annulation = default)
    {
        _stockage[partage.Id] = partage;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<Partage?> ObtenirParIdAsync(Guid id, CancellationToken annulation = default)
    {
        _stockage.TryGetValue(id, out var partage);
        return Task.FromResult(partage);
    }

    /// <inheritdoc/>
    public Task MettreAJourAsync(Partage partage, CancellationToken annulation = default)
    {
        _stockage[partage.Id] = partage;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SupprimerAsync(Guid id, CancellationToken annulation = default)
    {
        _stockage.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
