using PartageTexte.Domain.Enumerations;

namespace PartageTexte.Domain.Entites;

/// <summary>
/// Représente un partage de texte ou de mot de passe chiffré.
/// </summary>
public class Partage
{
    /// <summary>Identifiant unique du partage (utilisé dans l'URL).</summary>
    public Guid Id { get; private set; }

    /// <summary>Contenu chiffré en AES-256 (Base64).</summary>
    public string ContenuChiffre { get; private set; } = string.Empty;

    /// <summary>Type du contenu partagé (texte ou mot de passe).</summary>
    public TypeContenu TypeContenu { get; private set; }

    /// <summary>Date et heure de création du partage (UTC).</summary>
    public DateTime DateCreation { get; private set; }

    /// <summary>Date d'expiration du partage. Null = jamais expiré.</summary>
    public DateTime? DateExpiration { get; private set; }

    /// <summary>Indique si le partage est protégé par un mot de passe.</summary>
    public bool EstProtege { get; private set; }

    /// <summary>Hash PBKDF2 du mot de passe (null si non protégé).</summary>
    public string? MotDePasseHash { get; private set; }

    /// <summary>Nombre de lectures maximum autorisées. Null = illimité.</summary>
    public int? NombreLecturesMax { get; private set; }

    /// <summary>Nombre de fois que le contenu a été lu.</summary>
    public int NombreLectures { get; private set; }

    /// <summary>Constructeur privé pour EF Core.</summary>
    private Partage() { }

    /// <summary>
    /// Crée un nouveau partage.
    /// </summary>
    /// <param name="contenuChiffre">Contenu chiffré en AES-256.</param>
    /// <param name="typeContenu">Type du contenu (Texte ou MotDePasse).</param>
    /// <param name="dateExpiration">Date d'expiration optionnelle (UTC).</param>
    /// <param name="motDePasseHash">Hash du mot de passe (null si non protégé).</param>
    /// <param name="nombreLecturesMax">Nombre maximum de lectures (null = illimité).</param>
    public static Partage Creer(
        string contenuChiffre,
        TypeContenu typeContenu,
        DateTime? dateExpiration,
        string? motDePasseHash,
        int? nombreLecturesMax)
    {
        return new Partage
        {
            Id = Guid.NewGuid(),
            ContenuChiffre = contenuChiffre,
            TypeContenu = typeContenu,
            DateCreation = DateTime.UtcNow,
            DateExpiration = dateExpiration,
            EstProtege = motDePasseHash is not null,
            MotDePasseHash = motDePasseHash,
            NombreLecturesMax = nombreLecturesMax,
            NombreLectures = 0
        };
    }

    /// <summary>
    /// Restaure l'état complet d'un partage (utilisé uniquement lors de la désérialisation).
    /// </summary>
    public Partage AvecEtat(Guid id, DateTime dateCreation, int nombreLectures)
    {
        Id = id;
        DateCreation = dateCreation;
        NombreLectures = nombreLectures;
        return this;
    }

    /// <summary>
    /// Indique si le partage a expiré.
    /// </summary>
    public bool EstExpire()
        => DateExpiration.HasValue && DateTime.UtcNow > DateExpiration.Value;

    /// <summary>
    /// Indique si le nombre de lectures maximum a été atteint.
    /// </summary>
    public bool EstEpuise()
        => NombreLecturesMax.HasValue && NombreLectures >= NombreLecturesMax.Value;

    /// <summary>
    /// Incrémente le compteur de lectures.
    /// </summary>
    public void EnregistrerLecture()
        => NombreLectures++;
}
