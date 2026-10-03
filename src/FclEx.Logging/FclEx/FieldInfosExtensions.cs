namespace FclEx;

public static class FieldInfosExtensions
{
    private static readonly FieldInfo _filterOptions = typeof(LoggerFactory).GetRequiredField("_filterOptions");

    extension(FieldInfos)
    {
        public static FieldInfo LoggerFactory_FilterOptions => _filterOptions;
    }
}
