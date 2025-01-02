namespace FclEx.Domain;

public class SoftDeletableEntity<TPrimaryKey> : 
    CommonEntity<TPrimaryKey>, 
    ISoftDeletableEntity<TPrimaryKey>;
