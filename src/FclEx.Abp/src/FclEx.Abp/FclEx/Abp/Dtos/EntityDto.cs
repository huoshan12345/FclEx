namespace FclEx.Abp.Dtos;

public abstract class EntityDto<TPrimaryKey> : IEntityDto<TPrimaryKey>
{
    public TPrimaryKey Id { get; set; } = default!;

    protected EntityDto() { }
    protected EntityDto(TPrimaryKey id) { Id = id; }
}