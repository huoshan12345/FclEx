namespace FclEx.Abp.Domain.Dtos
{
    public interface IEntityDto
    {
    }

    public interface IEntityDto<TPrimaryKey> : IEntityDto, IHasKey<TPrimaryKey>
    {
    }
}