namespace FclEx.Domain;

public interface IHasDeletedAt
{
    DateTimeOffset DeletedAt { get; set; }
}