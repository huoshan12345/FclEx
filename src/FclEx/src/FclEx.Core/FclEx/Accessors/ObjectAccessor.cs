namespace FclEx.Accessors;

public delegate IntPtr GetInstanceAddress<T>(ref T instance);
public delegate IntPtr[] GetAllFieldAddresses<T>(ref T instance);

public static class ObjectAccessor
{
    public static IntPtr GetInstanceAddress<T>(ref T instance)
    {
        return ObjectAccessor<T>.GetInstanceAddress(ref instance);
    }

    public static IntPtr[] GetAllFieldAddresses<T>(ref T instance)
    {
        return ObjectAccessor<T>.GetAllFieldAddresses(ref instance);
    }
}

public static class ObjectAccessor<T>
{
    public static readonly GetInstanceAddress<T> GetInstanceAddress = BuildInstanceAddressAccessor();

    private static GetInstanceAddress<T> BuildInstanceAddressAccessor()
    {
        var method = new DynamicMethod(
            name: nameof(GetInstanceAddress),
            returnType: typeof(IntPtr),
            parameterTypes: [typeof(T).MakeByRefType()],
            m: typeof(ObjectAccessor).Module,
            skipVisibility: true);
        var ilGen = method.GetILGenerator();

        ilGen.Emit(OpCodes.Ldarg_0);
        ilGen.Emit(OpCodes.Conv_I);
        ilGen.Emit(OpCodes.Ret);
        return method.CreateDelegate<GetInstanceAddress<T>>();
    }

    public static readonly GetAllFieldAddresses<T> GetAllFieldAddresses = BuildAllFieldAddressesAccessor();

    private static GetAllFieldAddresses<T> BuildAllFieldAddressesAccessor()
    {
        var type = typeof(T);
        var fields = type.GetAllInstanceFields();

        // Ldflda表示Load Field Address，它可以帮助我们得到实例某个字段的地址
        var method = new DynamicMethod(
            name: nameof(GetAllFieldAddresses),
            returnType: typeof(IntPtr[]),
            parameterTypes: [type.MakeByRefType()],
            m: typeof(ObjectAccessor).Module,
            skipVisibility: true);
        var ilGen = method.GetILGenerator();

        // var addresses = new long[fields.Length];
        ilGen.DeclareLocal(typeof(IntPtr[]));
        ilGen.Emit(OpCodes.Ldc_I4, fields.Length);
        ilGen.Emit(OpCodes.Newarr, typeof(IntPtr));
        ilGen.Emit(OpCodes.Stloc_0);

        // addresses[index] = address of field[index];
        for (var index = 0; index < fields.Length; index++)
        {
            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Ldc_I4, index);
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldflda, fields[index]);
            ilGen.Emit(OpCodes.Conv_I);
            ilGen.Emit(OpCodes.Stelem_I);
        }

        ilGen.Emit(OpCodes.Ldloc_0);
        ilGen.Emit(OpCodes.Ret);

        return method.CreateDelegate<GetAllFieldAddresses<T>>();
    }
}