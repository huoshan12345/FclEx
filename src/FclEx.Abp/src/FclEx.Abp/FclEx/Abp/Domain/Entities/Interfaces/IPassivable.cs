namespace FclEx.Abp.Domain.Entities.Interfaces
{
    public interface IPassivable
    {
        bool IsActive { get; set; }
    }
}