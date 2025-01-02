namespace FclEx.Domain;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}