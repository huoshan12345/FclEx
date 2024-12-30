namespace FclEx.Abp.Entities;

public interface IDeletable
{
    bool IsDeleted { get; set; }
}