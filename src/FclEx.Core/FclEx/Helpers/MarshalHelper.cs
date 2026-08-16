namespace FclEx.Helpers;

public static class MarshalHelper
{
    private static readonly ConditionalWeakTable<Type, ValueBox<int>> _sizes = new();

    public static DisposableValue<IntPtr> AllocHGlobal(int cb)
    {
        return Marshal.AllocHGlobal(cb).ToDisposable(Marshal.FreeHGlobal);
    }

    public static DisposableValue<IntPtr> SecureStringToBSTR(SecureString str)
    {
        return Marshal.SecureStringToBSTR(str).ToDisposable(Marshal.ZeroFreeBSTR);
    }

    internal static int SizeOf<T>() where T : struct
    {
        return _sizes.GetValue(typeof(T), static type =>
        {
            ValidateInlineStructure(type, [], "$");
            return Marshal.SizeOf(type);
        });
    }

    internal static unsafe T Read<T>(ReadOnlySpan<byte> bytes) where T : struct
    {
        var size = SizeOf<T>();
        Check.NotLessThan(bytes.Length, size);

        using var memory = AllocHGlobal(size);
        bytes[..size].CopyTo(new Span<byte>(memory.Value.ToPointer(), size));
        return Marshal.PtrToStructure<T>(memory.Value);
    }

    internal static unsafe T[] ReadArray<T>(ReadOnlySpan<byte> bytes, int count) where T : struct
    {
        Check.NotLessThan(count, 0);

        var size = SizeOf<T>();
        var totalLength = checked(size * count);
        Check.NotLessThan(bytes.Length, totalLength);

        if (count == 0)
            return [];

        var result = new T[count];
        using var memory = AllocHGlobal(size);
        var buffer = new Span<byte>(memory.Value.ToPointer(), size);
        for (var i = 0; i < count; i++)
        {
            bytes.Slice(i * size, size).CopyTo(buffer);
            result[i] = Marshal.PtrToStructure<T>(memory.Value);
        }

        return result;
    }

    private static void ValidateInlineStructure(Type type, HashSet<Type> visiting, string path)
    {
        if (type.IsEnum || type.IsPrimitive || type.IsPointer
            || type == typeof(decimal) || type == typeof(IntPtr) || type == typeof(UIntPtr))
        {
            return;
        }

        if (type.IsValueType == false)
            throw Unsupported(path, type, "the field is a managed reference without a supported inline representation");

        if (type.IsAutoLayout)
            throw Unsupported(path, type, "the structure uses automatic layout");

        if (visiting.Add(type) == false)
            throw Unsupported(path, type, "the structure contains a recursive value layout");

        try
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                ValidateField(field, visiting, path + "." + field.Name);
        }
        finally
        {
            visiting.Remove(type);
        }
    }

    private static void ValidateField(FieldInfo field, HashSet<Type> visiting, string path)
    {
        var fieldType = field.FieldType;
        if (fieldType.IsArray)
        {
            var marshalAs = field.GetCustomAttribute<MarshalAsAttribute>();
            if (marshalAs is not { Value: UnmanagedType.ByValArray, SizeConst: > 0 })
                throw Unsupported(path, fieldType, "arrays require MarshalAs(ByValArray) with a positive SizeConst");

            if (fieldType.GetArrayRank() != 1)
                throw Unsupported(path, fieldType, "only one-dimensional ByValArray fields are supported");

            var elementType = fieldType.GetElementType()!;
            ValidateInlineStructure(elementType, visiting, path + "[]");
            return;
        }

        if (fieldType == typeof(string))
        {
            var marshalAs = field.GetCustomAttribute<MarshalAsAttribute>();
            if (marshalAs is not { Value: UnmanagedType.ByValTStr, SizeConst: > 0 })
                throw Unsupported(path, fieldType, "strings require MarshalAs(ByValTStr) with a positive SizeConst");

            return;
        }

        ValidateInlineStructure(fieldType, visiting, path);
    }

    private static NotSupportedException Unsupported(string path, Type type, string reason)
    {
        return new NotSupportedException($"Cannot marshal bytes as the requested structure: {path} ({type}) {reason}.");
    }
}
