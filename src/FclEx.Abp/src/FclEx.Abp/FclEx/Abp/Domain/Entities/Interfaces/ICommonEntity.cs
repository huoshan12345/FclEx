namespace FclEx.Abp.Domain.Entities.Interfaces
{
    public interface ICommonEntity<TPrimaryKey> : IEntity<TPrimaryKey>, 
        IHasCreationTime, 
        IHasModificationTime, 
        ISoftDelete, 
        IPassivable
    {
    }
}
