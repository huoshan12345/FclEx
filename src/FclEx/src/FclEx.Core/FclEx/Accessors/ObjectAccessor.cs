namespace FclEx.Accessors;

public delegate IntPtr[] GetAllFieldAddresses<T>(ref T instance);

public delegate IntPtr[] GetAllFieldAddresses(ref object instance);

public static class ObjectAccessor
{
    /// <summary>
    /// Gets the address of the first field in the specified object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object whose field address is to be retrieved.</param>
    /// <returns>
    /// A pointer to the address of the first field of the object. If <typeparamref name="T"/> is a reference type,
    /// the function adjusts the pointer to return the address of the first field within the object, rather than the reference itself.
    /// Note that the first field may not be the first declared field, due to potential field rearrangement by the CLR.
    /// </returns>
    public static unsafe IntPtr GetFirstFieldAddress<T>(ref T obj)
    {
        Check.NotNull(obj);

        var pointer = Unsafe.AsPointer(ref obj);
        return typeof(T).IsValueType
            ? new IntPtr(pointer)
            : *(IntPtr*)pointer + IntPtr.Size;
    }

    public static unsafe IntPtr GetAddress<T>(ref T instance)
    {
        // null variables also have addresses, so we don't do null-check here.
        var pointer = Unsafe.AsPointer(ref instance);
        return new IntPtr(pointer);
    }

    public static IntPtr[] GetAllFieldAddresses<T>(ref T instance)
    {
        Check.NotNull(instance);
        return ObjectAccessor<T>.GetAllFieldAddresses(ref instance);
    }

    public static IntPtr[] GetAllFieldAddresses(ref object instance, Type type)
    {
        Check.NotNull(instance);
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

        var field = typeof(ObjectAccessor<>)
            .MakeGenericType(type)
            .GetRequiredField(nameof(GetAllFieldAddresses));

        var invoke = typeof(GetAllFieldAddresses<>)
            .MakeGenericType(type)
            .GetRequiredMethod(nameof(MethodInfo.Invoke));

        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldsfld, field);

        if (type.IsValueType)
        {
            // unbox for value type.
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldind_Ref);
            il.Emit(OpCodes.Unbox, type);
            il.Emit(OpCodes.Starg, 0);
        }

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, invoke);
        il.Emit(OpCodes.Ret);

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
        var il = method.GetILGenerator();
        var start = il.DefineLabel();

        il.DeclareLocal(typeof(IntPtr[]));
        il.DeclareLocal(typeof(Type));

        if (type.IsValueType == false)
        {
            // for reference types, we need to dereference the ref and get the actual object reference.
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldind_Ref);
            il.Emit(OpCodes.Starg, 0);
        }

        il.MarkLabel(start);
        // var addresses = new long[fields.Length];
        il.Emit(OpCodes.Ldc_I4, fields.Length);
        il.Emit(OpCodes.Newarr, typeof(IntPtr));
        il.Emit(OpCodes.Stloc_0);

        // addresses[index] = address of field[index];
        for (var index = 0; index < fields.Length; index++)
        {
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, fields[index]);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Stelem_I);
        }

        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ret);

        return method.CreateDelegate<GetAllFieldAddresses<T>>();
    }
}