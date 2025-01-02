namespace FclEx.Domain;

public interface IHasCreatedAt
{
    DateTimeOffset CreatedAt { get; set; }
}