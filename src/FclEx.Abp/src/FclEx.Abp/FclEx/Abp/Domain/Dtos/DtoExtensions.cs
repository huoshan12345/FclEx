namespace FclEx.Abp.Domain.Dtos;

public static class DtoExtensions
{
    public static T Map<T>(this IEntityDto dto, IObjectMapper mapper)
    {
        return mapper.Map<T>(dto);
    }
}