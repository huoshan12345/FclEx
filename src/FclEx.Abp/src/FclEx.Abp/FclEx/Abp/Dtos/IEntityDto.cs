namespace FclEx.Abp.Dtos;

public interface IEntityDto;

public interface IEntityDto<TPrimaryKey> : IEntityDto, IHasKey<TPrimaryKey>;