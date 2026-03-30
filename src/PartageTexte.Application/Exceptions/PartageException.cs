namespace PartageTexte.Application.Exceptions;

/// <summary>Exception métier levée lors d'une violation des règles de partage.</summary>
public sealed class PartageException : Exception
{
    public PartageException(string message) : base(message) { }
}
