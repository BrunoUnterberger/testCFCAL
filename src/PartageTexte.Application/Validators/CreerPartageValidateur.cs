using FluentValidation;
using PartageTexte.Application.DTOs;

namespace PartageTexte.Application.Validators;

/// <summary>
/// Validateur FluentValidation pour la création d'un partage.
/// </summary>
public sealed class CreerPartageValidateur : AbstractValidator<CreerPartageRequete>
{
    public CreerPartageValidateur()
    {
        // Le contenu est obligatoire et ne peut pas dépasser 64 Ko
        RuleFor(r => r.Contenu)
            .NotEmpty().WithMessage("Le contenu ne peut pas être vide.")
            .MaximumLength(65_536).WithMessage("Le contenu ne peut pas dépasser 64 Ko.");

        // La date d'expiration est obligatoire et doit être dans le futur
        RuleFor(r => r.DateExpiration)
            .NotNull().WithMessage("La date d'expiration est obligatoire.")
            .GreaterThan(DateTime.UtcNow).WithMessage("La date d'expiration doit être dans le futur.");

        // Le mot de passe, si renseigné, doit avoir au moins 4 caractères
        When(r => !string.IsNullOrEmpty(r.MotDePasse), () =>
        {
            RuleFor(r => r.MotDePasse!)
                .MinimumLength(4).WithMessage("Le mot de passe doit contenir au moins 4 caractères.");
        });

        // Le nombre de lectures max, si renseigné, doit être positif
        When(r => r.NombreLecturesMax.HasValue, () =>
        {
            RuleFor(r => r.NombreLecturesMax!.Value)
                .GreaterThan(0).WithMessage("Le nombre de lectures maximum doit être supérieur à 0.");
        });
    }
}
