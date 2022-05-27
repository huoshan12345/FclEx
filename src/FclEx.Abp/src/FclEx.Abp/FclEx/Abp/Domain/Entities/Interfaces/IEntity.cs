namespace FclEx.Abp.Domain.Entities.Interfaces
{
    public interface IEntity
    {
    }

    public interface IEntity<TPrimaryKey> : IEntity
    {
        TPrimaryKey Id { get; set; }
    }
}