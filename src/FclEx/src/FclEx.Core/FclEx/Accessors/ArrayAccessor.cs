namespace FclEx.Accessors;

public static class ArrayAccessor<T>
{
    public static readonly Func<List<T>, T[]> ItemsAccessor = BuildItemsAccessor();

    public static Func<List<T>, T[]> BuildItemsAccessor()
    {
        var method = new DynamicMethod(
            name: "get",
            attributes: MethodAttributes.Static | MethodAttributes.Public,
            callingConvention: CallingConventions.Standard,
            returnType: typeof(T[]),
            parameterTypes: [typeof(List<T>)], owner: typeof(ArrayAccessor<T>),
            skipVisibility: true);

        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
        il.Emit(OpCodes.Ldfld, typeof(List<T>).GetRequiredField("_items")); // Replace argument by field
        il.Emit(OpCodes.Ret); // Return field
        return (Func<List<T>, T[]>)method.CreateDelegate(typeof(Func<List<T>, T[]>));
    }
}