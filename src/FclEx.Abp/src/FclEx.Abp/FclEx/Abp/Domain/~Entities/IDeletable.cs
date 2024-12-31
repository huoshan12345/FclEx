namespace FclEx.Abp.Domain;

public interface IDeletable
{
    bool IsDeleted { get; set; }
}