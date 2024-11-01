namespace FclEx.Utils;

public static class SizeCalculator
{
    /// <summary>
    /// 获取实例的自身以及各字段的地址
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    private static Func<object?, long[]> GenerateFieldAddressAccessor(FieldInfo[] fields)
    {
        // Ldflda表示Load Field Address，它可以帮助我们得到实例某个字段的地址
        var method = new DynamicMethod(
            name: "GetFieldAddresses",
            returnType: typeof(long[]),
            parameterTypes: [typeof(object)],
            m: typeof(SizeCalculator).Module,
            skipVisibility: true);
        var ilGen = method.GetILGenerator();

        // var addresses = new long[fields.Length + 1];
        ilGen.DeclareLocal(typeof(long[]));
        ilGen.Emit(OpCodes.Ldc_I4, fields.Length + 1);
        ilGen.Emit(OpCodes.Newarr, typeof(long));
        ilGen.Emit(OpCodes.Stloc_0);

        // addresses[0] = address of instance;
        ilGen.Emit(OpCodes.Ldloc_0);
        ilGen.Emit(OpCodes.Ldc_I4, 0);
        ilGen.Emit(OpCodes.Ldarg_0);
        ilGen.Emit(OpCodes.Conv_I8);
        ilGen.Emit(OpCodes.Stelem_I8);

        // addresses[index] = address of field[index + 1];
        for (var index = 0; index < fields.Length; index++)
        {
            ilGen.Emit(OpCodes.Ldloc_0);
            ilGen.Emit(OpCodes.Ldc_I4, index + 1);
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldflda, fields[index]);
            ilGen.Emit(OpCodes.Conv_I8);
            ilGen.Emit(OpCodes.Stelem_I8);
        }

        ilGen.Emit(OpCodes.Ldloc_0);
        ilGen.Emit(OpCodes.Ret);

        return (Func<object?, long[]>)method.CreateDelegate(typeof(Func<object, long[]>));
    }

    private const BindingFlags DeclaredInstance = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static object GetUninitializedObject(Type type)
    {
        // for string type, GetUninitializedObject will throw an ArgumentException:
        // Uninitialized Strings cannot be created.
        // so we need to do special handling for it.
        return type == typeof(string)
            ? string.Empty
            : ObjectHelper.GetUninitializedObject(type);
    }

    private static int CalculateValueTypeInstance(Type type)
    {
        var instance = GetUninitializedObject(type);
        var fields = type.GetFields(DeclaredInstance).ToArray();

        if (fields.Length == 0)
            return 0;

        // 由于结构体在内存中字节就是所有字段的内容，所有我们采用一种讨巧的计算方法。
        // 假设我们需要结算类型为T的结构体的字节数，那么我们创建一个ValueTuple<T,T>元组，它的第二个字段Item2的偏移量就是结构体T的字节数
        // 注：值类型的实例地址和第一个字段的地址相同。所以addresses[1]等于addresses[0]
        var tupleType = typeof(ValueTuple<,>).MakeGenericType(type, type);
        var tuple = tupleType.GetConstructors()[0].Invoke([instance, instance]);
        var addresses = GenerateFieldAddressAccessor(tupleType.GetFields()).Invoke(tuple).OrderBy(it => it).ToArray();
        return (int)(addresses[2] - addresses[1]);
    }

    private static int CalculateReferenceTypeInstance(Type type)
    {
        // var fields = type.GetAllInstanceFields();
        var fields = GetBaseTypesAndThis(type)
            .SelectMany(m => m.GetFields(DeclaredInstance))
            .ToArray();

        // 如果指定的类型没有定义任何字段，CalculateReferenceTypeInstance 返回引用类型实例的最小字节数：3倍地址指针字节数。
        // 对于x86架构，一个应用类型对象至少占用12字节，包括 Object Header（4 bytes）、方法表指针（4 bytes）和最少4字节的字段内容（即使没有类型没有定义任何字段，这个4个字节也是必需的）。
        // 对于x64架构，这个最小字节数会变成24，因为方法表指针和最小字段内容变成了8个字节，虽然 Object Header 的有效内容只占用4个字节，但是前面会添加4个字节的Padding。
        if (fields.Length == 0)
            return 3 * IntPtr.Size;

        // TODO: GetUninitializedObject does work for abstract types and delegate types.
        var instance = GetUninitializedObject(type);
        var addresses = GenerateFieldAddressAccessor(fields).Invoke(instance);
        var (instanceAddress, fieldAddresses) = (addresses[0], addresses.Skip(1));

        var ordered = fieldAddresses.Zip(fields).OrderBy(m => m.First).ToArray();
        var (firstAddress, firstField) = ordered.First();
        var (lastAddress, lastField) = ordered.Last();
        var lastFieldOffset = (int)(lastAddress - firstAddress);
        var lastFieldSize = lastField.FieldType.IsValueType
            ? CalculateValueTypeInstance(lastField.FieldType)
            : IntPtr.Size;

        var size = lastFieldOffset + lastFieldSize + IntPtr.Size * 2; // plus sizes of two pointers for ObjectHeader and MethodTableAddress
        // Round up to IntPtr.Size
        var round = IntPtr.Size - 1;
        return ((size + round) & (~round));

        static IEnumerable<Type> GetBaseTypesAndThis(Type? type)
        {
            while (type is not null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }

    private static readonly ConcurrentDictionary<Type, int> _sizes = new();

    /// <summary>
    /// Retrieves the size of the specified type <paramref name="type"/>.
    /// </summary>
    /// <returns>
    /// The size in bytes of the specified type.<br/>
    /// * For value types, the size is the sum of the sizes of all its members.<br/>
    /// * For reference types, the size is the sum of the sizes of all its members plus the size of two pointers:<br/>
    /// one pointing to the object header and the other pointing to the method table.<br/>
    /// * The size of a member refers to the total size if the member is a value type,
    /// or the size of a pointer if the member is a reference type.<br/>
    /// * The size will be aligned to the size of a pointer, as the CLR performs memory alignment.
    /// </returns>
    public static int SizeOf(Type type)
    {
        return _sizes.GetOrAdd(type, SizeOfImpl);

        static int SizeOfImpl(Type type)
        {
            return type.IsValueType
                ? CalculateValueTypeInstance(type)
                : CalculateReferenceTypeInstance(type);
        }
    }

    /// <summary>
    /// Retrieves the size of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type for which to determine the size.</typeparam>
    /// <returns>
    /// The size in bytes of the specified type.<br/>
    /// * For value types, the size is the sum of the sizes of all its members.<br/>
    /// * For reference types, the size is the sum of the sizes of all its members plus the size of two pointers:<br/>
    /// one pointing to the object header and the other pointing to the method table.<br/>
    /// * The size of a member refers to the total size if the member is a value type,
    /// or the size of a pointer if the member is a reference type.<br/>
    /// * The size will be aligned to the size of a pointer, as the CLR performs memory alignment.
    /// </returns>
    public static int SizeOf<T>()
    {
        return SizeOf(typeof(T));
    }
}