namespace FclEx.Abp.Domain;

public interface IEntity
{
}

public interface IEntity<TPrimaryKey> : IEntity
{
    TPrimaryKey Id { get; set; }
}