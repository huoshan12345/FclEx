namespace FclEx.Utils;

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

    /// <summary>
    /// Gets the memory address of a given reference to an instance as an <see cref="IntPtr"/>.
    /// </summary>
    /// <typeparam name="T">The type of the instance whose address is being retrieved.</typeparam>
    /// <param name="instance">A reference to the instance whose memory address is to be obtained.</param>
    /// <returns>An <see cref="IntPtr"/> representing the memory address of the instance.</returns>
    /// <remarks>
    /// This method uses unsafe code and the <see cref="Unsafe"/> class to 
    /// convert the reference to a raw pointer. Note that even null references have an address, so no null-check 
    /// is performed on the input.
    /// </remarks>
    public static unsafe IntPtr GetAddress<T>(ref T instance)
    {
        // null variables also have addresses, so we don't do null-check here.
        var pointer = Unsafe.AsPointer(ref instance);
        return new IntPtr(pointer);
    }

    /// <summary>
    /// Gets the memory addresses of all instance fields of the given instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the instance whose field addresses are to be retrieved.</typeparam>
    /// <param name="instance">A reference to the instance whose field addresses are to be obtained.</param>
    /// <returns>
    /// An array of <see cref="IntPtr"/> representing the memory addresses of all instance fields of the specified instance.
    /// </returns>
    /// <remarks>
    /// This method validates the instance and utilizes the <see cref="ObjectAccessor{T}"/> to access the addresses of 
    /// the instance fields. It is useful for scenarios involving low-level memory inspection or manipulation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the instance is null.</exception>
    public static IntPtr[] GetAllFieldAddresses<T>(ref T instance)
    {
        Check.NotNull(instance);
        return ObjectAccessor<T>.GetAllFieldAddresses(ref instance);
    }

    /// <summary>
    /// Gets the memory addresses of all instance fields of a given object instance and type.
    /// </summary>
    /// <param name="instance">A reference to the object instance whose field addresses are to be obtained.</param>
    /// <param name="type">The type of the instance whose field addresses are to be retrieved.</param>
    /// <returns>
    /// An array of <see cref="IntPtr"/> representing the memory addresses of all instance fields of the specified object.
    /// </returns>
    /// <remarks>
    /// This method dynamically caches and retrieves a delegate for accessing field addresses based on the provided type.
    /// The delegate is built using <see cref="BuildAllFieldAddressesAccessor"/> and invoked with the instance to return 
    /// the field addresses. This method supports polymorphic scenarios where the exact type is determined at runtime.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the instance is null.</exception>
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

    /// <summary>
    /// Builds a delegate that retrieves the memory addresses of all instance fields of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>
    /// A delegate of type <see cref="GetAllFieldAddresses{T}"/> that, when invoked, returns an array of <see cref="IntPtr"/> 
    /// representing the addresses of all instance fields in the given instance of type <typeparamref name="T"/>.
    /// </returns>
    /// <remarks>
    /// This method uses a <see cref="DynamicMethod"/> to dynamically generate IL code for accessing 
    /// the field addresses of an instance. The method utilizes the `Ldflda` (Load Field Address) opcode to load the address 
    /// of each field and stores these addresses in an array. Special handling is included for reference types to ensure 
    /// the dereferencing of the `ref` parameter to get the actual object reference before field addresses are obtained.
    ///
    /// The resulting method, when executed, returns an array of pointers, each corresponding to the address of an instance field. 
    /// This function can be used for advanced scenarios that require direct memory manipulation or introspection.
    /// </remarks>
    private static GetAllFieldAddresses<T> BuildAllFieldAddressesAccessor()
    {
        var type = typeof(T);
        var fields = type.GetAllInstanceFields();

        // Ldflda means "Load Field Address", which is used to load the address of a field.
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
        il.Emit(OpCodes.Ldc_I4, fields.Count);
        il.Emit(OpCodes.Newarr, typeof(IntPtr));
        il.Emit(OpCodes.Stloc_0);

        // addresses[index] = address of field[index];
        for (var index = 0; index < fields.Count; index++)
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