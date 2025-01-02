namespace FclEx.Domain;

public interface ICommonEntity<TPrimaryKey> :
    IEntity<TPrimaryKey>,
    IHasCreatedAt,
    IHasUpdatedAt,
    IDisableable;