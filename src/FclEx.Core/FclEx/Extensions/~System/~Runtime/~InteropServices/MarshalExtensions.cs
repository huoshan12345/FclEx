namespace FclEx.Extensions;

public static class MarshalExtensions
{
    private static readonly ConditionalWeakTable<Type, ValueBox<int>> _sizes = new();

    internal static int SizeOf<T>()
    {
        return _sizes.GetValue(typeof(T), static t => Marshal.SizeOf(t));
    }

    extension(Marshal)
    {
        public static DisposableValue<IntPtr> AllocHGlobalDisposable(int cb)
        {
            return Marshal.AllocHGlobal(cb).ToDisposable(Marshal.FreeHGlobal);
        }

        public static unsafe T ReadAs<T>(ReadOnlySpan<byte> bytes)
        {
            var size = SizeOf<T>();
            Check.NotLessThan(bytes.Length, size);

            using var memory = Marshal.AllocHGlobalDisposable(size);
            bytes[..size].CopyTo(new Span<byte>(memory.Value.ToPointer(), size));
            return Marshal.PtrToStructure<T>(memory.Value)!;
        }

        public static unsafe T[] ReadAsArray<T>(ReadOnlySpan<byte> bytes, int count)
        {
            Check.NotLessThan(count, 0);

            var size = SizeOf<T>();
            var totalLength = checked(size * count);
            Check.NotLessThan(bytes.Length, totalLength);

            if (count == 0)
                return [];

            var result = new T[count];
            using var memory = Marshal.AllocHGlobalDisposable(size);
            var buffer = new Span<byte>(memory.Value.ToPointer(), size);
            for (var i = 0; i < count; i++)
            {
                bytes.Slice(i * size, size).CopyTo(buffer);
                result[i] = Marshal.PtrToStructure<T>(memory.Value)!;
            }

            return result;
        }
        
        /// <summary>
        /// Marshals an object to its unmanaged byte representation.
        /// </summary>
        /// <typeparam name="T">The type of object to marshal.</typeparam>
        /// <param name="obj">The object to marshal.</param>
        /// <param name="clearNativeBuffer">
        /// Whether to initialize the native buffer to zero before marshaling. This prevents bytes left in structure padding
        /// from a previous native allocation from appearing in the returned representation.
        /// </param>
        /// <returns>The unmanaged representation of <paramref name="obj"/>.</returns>
        /// <remarks>
        /// The returned bytes are native-layout data, not a portable serialization format. Pointer-based marshaling can
        /// include temporary or external addresses rather than the pointed-to values.
        /// </remarks>
        public static byte[] ToBytes<T>(T obj, bool clearNativeBuffer = false)
        {
            Check.NotNull(obj);

            var length = Marshal.SizeOf<T>();
            var bufByte = new byte[length];
            using var disposable = Marshal.AllocHGlobalDisposable(length);
            var ptr = disposable.Value;
            var structureInitialized = false;
            try
            {
                if (clearNativeBuffer)
                    Marshal.Copy(bufByte, 0, ptr, length);

                Marshal.StructureToPtr(obj, ptr, false);
                structureInitialized = true;
                Marshal.Copy(ptr, bufByte, 0, length);
                return bufByte;
            }
            finally
            {
                if (structureInitialized)
                    Marshal.DestroyStructure<T>(ptr);
            }
        }

        public static DisposableValue<IntPtr> SecureStringToBSTRDisposable(SecureString str)
        {
            return Marshal.SecureStringToBSTR(str).ToDisposable(Marshal.ZeroFreeBSTR);
        }

    }
}
