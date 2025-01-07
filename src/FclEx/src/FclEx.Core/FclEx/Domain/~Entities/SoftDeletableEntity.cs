namespace FclEx.Domain;

public class SoftDeletableEntity<TPrimaryKey> :
    CommonEntity<TPrimaryKey>,
    ISoftDeletableEntity<TPrimaryKey>
{
    public DateTimeOffset DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}


public class SoftDeletableEntity : SoftDeletableEntity<long>;