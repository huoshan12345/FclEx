namespace FclEx.Abp;

public static class ObjectMapperExtensions
{
    public static TDestination Map<TDestination>(this IObjectMapper mapper, object source)
    {
        return mapper.GetMapper().Map<TDestination>(source);
    }
}