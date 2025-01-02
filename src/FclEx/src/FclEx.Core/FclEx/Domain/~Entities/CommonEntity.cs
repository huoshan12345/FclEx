namespace FclEx.Domain;

public abstract class CommonEntity<TPrimaryKey> : ICommonEntity<TPrimaryKey>
{
    public TPrimaryKey Id { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsDisabled { get; set; }
}