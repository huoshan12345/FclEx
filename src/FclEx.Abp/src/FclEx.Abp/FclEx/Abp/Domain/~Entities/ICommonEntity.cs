namespace FclEx.Abp.Domain;

public interface ICommonEntity<TPrimaryKey> :
    IEntity<TPrimaryKey>,
    IHasCreatedAt,
    IHasUpdatedAt,
    IHasDeletedAt,
    IDeletable,
    IDisableable;