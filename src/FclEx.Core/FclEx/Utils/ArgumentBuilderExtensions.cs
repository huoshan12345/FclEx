namespace FclEx.Utils;

public static class ArgumentBuilderExtensions
{
    public static T CreateObject<T>(this ArgumentBuilder args)
    {
        Check.NotNull(args);
        return (T)args.CreateObject(typeof(T));
    }

    internal static int GetInheritDepthFromInterfaceTo(this Type? ifaceType, Type? inheritType)
    {
        if (ifaceType == null || inheritType == null)
            return -1;

        if (!ifaceType.IsInterface)
            return -1;

        if (ifaceType == inheritType)
            return 0;

        if (inheritType.GetInterfaces().Any(t1 => t1 == ifaceType))
            return 1;

        return -1;
    }

    internal static int GetInheritDepthFromClassTo(this Type? classType, Type? inheritType)
    {
        if (classType == null || inheritType == null)
            return -1;

        if (!classType.IsClass)
            return -1;

        if (!inheritType.IsClass)
            return -1;

        var t = inheritType;
        for (var i = 0; t != null; ++i)
        {
            if (t == classType)
                return i;
            t = t.BaseType;
        }
        return -1;
    }

    internal static int GetInheritDepthTo(this Type? baseType, Type? inheritType)
    {
        if (baseType == null || inheritType == null)
            return -1;

        if (baseType == inheritType)
            return 0;

        if (baseType.IsInterface)
            return baseType.GetInheritDepthFromInterfaceTo(inheritType);

        if(baseType.IsClass && inheritType.IsClass)
            return baseType.GetInheritDepthFromClassTo(inheritType);

        return -1;
    }
}