using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PartageTexte.Application.Services;

namespace PartageTexte.Application.Extensions;

/// <summary>
/// Extensions pour l'enregistrement des services de la couche Application.
/// </summary>
public static class InscriptionApplication
{
    /// <summary>
    /// Enregistre tous les services de la couche Application dans le conteneur DI.
    /// </summary>
    public static IServiceCollection AjouterApplication(this IServiceCollection services)
    {
        services.AddScoped<ServicePartage>();
        services.AddValidatorsFromAssemblyContaining<ServicePartage>();
        return services;
    }
}
