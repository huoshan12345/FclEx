namespace FclEx.Abp.Domain.Entities.Interfaces
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
