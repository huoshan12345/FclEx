namespace FclEx.Accessors;

public delegate IntPtr GetInstanceAddress<T>(ref T instance);
public delegate IntPtr[] GetAllFieldAddresses<T>(ref T instance);

public delegate IntPtr[] GetAllFieldAddresses(ref object instance);

public static class ObjectAccessor
{
    public static unsafe IntPtr GetInstanceAddress<T>(ref T instance)
    {
        var pointer = Unsafe.AsPointer(ref instance);
        return new IntPtr(pointer);
    }

    public static IntPtr[] GetAllFieldAddresses<T>(ref T instance)
    {
        return ObjectAccessor<T>.GetAllFieldAddresses(ref instance);
    }

    public static IntPtr[] GetAllFieldAddresses(ref object instance, Type type)
    {
        var @delegate = _cache.GetOrAdd(type, BuildAllFieldAddressesAccessor);
        return @delegate.Invoke(ref instance);
    }

    private static readonly ConcurrentDictionary<Type, GetAllFieldAddresses> _cache = new();
    
    private static GetAllFieldAddresses BuildAllFieldAddressesAccessor(Type type)
    {
        var method = new DynamicMethod(
            name: nameof(GetAllFieldAddresses),
            returnType: typeof(IntPtr[]),
            parameterTypes: [typeof(object).MakeByRefType()],
            m: typeof(ObjectAccessor).Module,
            skipVisibility: true);

        var field = typeof(ObjectAccessor<>).MakeGenericType(type).GetRequiredField(nameof(GetAllFieldAddresses));
        var invoke = typeof(GetAllFieldAddresses).GetRequiredMethod(nameof(MethodInfo.Invoke));

        var ilGen = method.GetILGenerator();
        ilGen.Emit(OpCodes.Ldsfld, field);
        ilGen.Emit(OpCodes.Ldarg_0); // ref object
        ilGen.Emit(OpCodes.Conv_U);  // Unsafe.AsPointer
        ilGen.Emit(OpCodes.Callvirt, invoke);
        ilGen.Emit(OpCodes.Ret);

        return method.CreateDelegate<GetAllFieldAddresses>();
    }
}

public static class ObjectAccessor<T>
{
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
            m: typeof(ObjectAccessor<T>).Module,
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