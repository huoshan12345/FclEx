namespace FclEx.Domain;

public interface ISoftDeletableEntity<TPrimaryKey> :
    ICommonEntity<TPrimaryKey>,
    IHasDeletedAt,
    ISoftDeletable;
