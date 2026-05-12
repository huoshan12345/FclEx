namespace FclEx.Utils;

public delegate ref int GetRefInt<T>(List<T> list);

public static class ListAccessor<T>
{
    public static readonly Func<List<T>, T[]> Items = BuildItemsAccessor();
    public static readonly GetRefInt<T> Size = BuildSizeAccessor();
    public static readonly GetRefInt<T> Version = BuildVersionAccessor();

    private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes)
    {
        return new DynamicMethod(
            name: name,
            attributes: MethodAttributes.Static | MethodAttributes.Public,
            callingConvention: CallingConventions.Standard,
            returnType: returnType,
            parameterTypes: parameterTypes,
            owner: typeof(ListAccessor<T>),
            skipVisibility: true);
    }

    private static Func<List<T>, T[]> BuildItemsAccessor()
    {
        var method = CreateDynamicMethod("_items", typeof(T[]), [typeof(List<T>)]);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
        il.Emit(OpCodes.Ldfld, typeof(List<T>).GetRequiredField("_items")); // Replace argument by field
        il.Emit(OpCodes.Ret); // Return field
        return method.CreateDelegate<Func<List<T>, T[]>>();
    }

    private static GetRefInt<T> BuildSizeAccessor()
    {
        var method = CreateDynamicMethod("_size", typeof(int).MakeByRefType(), [typeof(List<T>)]);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
        il.Emit(OpCodes.Ldflda, typeof(List<T>).GetRequiredField("_size")); // Replace argument by field
        il.Emit(OpCodes.Ret); // Return field
        return method.CreateDelegate<GetRefInt<T>>();
    }

    private static GetRefInt<T> BuildVersionAccessor()
    {
        var method = CreateDynamicMethod("_version", typeof(int).MakeByRefType(), [typeof(List<T>)]);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
        il.Emit(OpCodes.Ldflda, typeof(List<T>).GetRequiredField("_version")); // Replace argument by field
        il.Emit(OpCodes.Ret); // Return field
        return method.CreateDelegate<GetRefInt<T>>();
    }
}