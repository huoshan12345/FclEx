namespace Volo.Abp.ObjectMapping;

public static class Extensions
{
    public static TDestination Map<TDestination>(this IObjectMapper mapper, object source)
    {
        return mapper.GetMapper().Map<TDestination>(source);
    }
}