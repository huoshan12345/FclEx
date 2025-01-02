namespace FclEx.Domain;

public interface IHasUpdatedAt
{
    DateTimeOffset UpdatedAt { get; set; }
}