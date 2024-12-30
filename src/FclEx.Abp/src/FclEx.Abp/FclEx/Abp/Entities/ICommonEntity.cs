namespace FclEx.Abp.Entities;

public interface ICommonEntity<TPrimaryKey> :
    IEntity<TPrimaryKey>,
    IHasCreatedAt,
    IHasUpdatedAt,
    IHasDeletedAt,
    IDeletable,
    IDisableable;