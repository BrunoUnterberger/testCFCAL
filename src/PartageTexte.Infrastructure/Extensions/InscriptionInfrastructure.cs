using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PartageTexte.Application.Interfaces;
using PartageTexte.Infrastructure.Persistance;
using PartageTexte.Infrastructure.Services;

namespace PartageTexte.Infrastructure.Extensions;

/// <summary>
/// Extensions pour l'enregistrement des services de la couche Infrastructure.
/// </summary>
public static class InscriptionInfrastructure
{
    /// <summary>
    /// Enregistre le dépôt fichier et les services d'infrastructure dans le conteneur DI.
    /// </summary>
    public static IServiceCollection AjouterInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Dépôt persisté sur le système de fichiers (Singleton — partage unique du dossier)
        services.AddSingleton<IDepotPartage, DepotPartageFichier>();

        // Services techniques
        services.AddSingleton<IServiceChiffrement, ServiceChiffrement>();
        services.AddSingleton<IServiceHachage, ServiceHachage>();

        return services;
    }
}
