namespace FclEx.Utils;

public delegate ref int GetRefInt<T>(List<T> list);

public static class ListAccessor<T>
{
    public static readonly Func<List<T>, T[]> Items = BuildItemsAccessor();
    public static readonly GetRefInt<T> Size = BuildSizeAccessor();
    public static readonly GetRefInt<T> Version = BuildVersionAccessor();
    public static readonly Action<List<T>, int> Grow = BuildGrowAccessor();

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

    private static Action<List<T>, int> BuildGrowAccessor()
    {
        var type = typeof(List<T>);
        var method = type.GetMethod(
            "Grow",
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(int)],
            modifiers: null);

        if (method == null)
            throw new InvalidOperationException("Grow method not found.");

        var dynamicMethod = CreateDynamicMethod(method.Name, method.ReturnType, [typeof(List<T>), typeof(int)]);
        var il = dynamicMethod.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate<Action<List<T>, int>>();
    }
}