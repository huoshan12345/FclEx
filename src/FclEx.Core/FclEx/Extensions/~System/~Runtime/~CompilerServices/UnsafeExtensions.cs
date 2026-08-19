namespace FclEx.Extensions;

public static class UnsafeExtensions
{
    private static readonly MethodInfo _sizeof = typeof(Unsafe).GetRequiredMethod(nameof(Unsafe.SizeOf), 1);
    private static readonly ConditionalWeakTable<Type, ValueBox<int>> _cache = new();
    private static readonly ConditionalWeakTable<Type, ConcurrentDictionary<string, MethodInfo>> _methods = new();

    extension(Unsafe)
    {
        public static int SizeOf(Type type)
        {
            return _cache.GetValue(type, m =>
            {
                var method = _sizeof.MakeGenericMethod(m);
                return method.Invoke<int>(null, null);
            });
        }

        /// <summary>
        /// Dereferences a pointer and returns the value at the specified memory address.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value being dereferenced.</typeparam>
        /// <param name="ptr">A pointer to the memory address containing the value.</param>
        /// <returns>
        /// The value located at the memory address pointed to by <paramref name="ptr"/>.
        /// </returns>
        /// <remarks>
        /// This function interprets the memory address as a pointer to a value of type <typeparamref name="T"/>.
        /// <typeparamref name="T"/> is restricted to unmanaged types so an arbitrary address cannot be interpreted as a
        /// managed object reference. The caller remains responsible for ensuring that <paramref name="ptr"/> is non-null,
        /// suitably aligned, readable for <typeparamref name="T"/>, and valid for the duration of this call.
        /// </remarks>
        public static unsafe T? GetValue<T>(IntPtr ptr) where T : unmanaged
        {
            var pointer = ptr.ToPointer();
            return *(T*)pointer;
        }

        /// <summary>
        /// Dereferences a pointer and returns its value as the specified unmanaged runtime type.
        /// </summary>
        /// <param name="ptr">A pointer to the memory address containing the value.</param>
        /// <param name="type">The unmanaged runtime type of the value.</param>
        /// <returns>The value located at <paramref name="ptr"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an unmanaged type.</exception>
        /// <remarks>Has the same pointer validity, alignment, and readability requirements as <see cref="GetValue{T}(IntPtr)"/>.</remarks>
        public static object? GetValue(IntPtr ptr, Type type)
        {
            var methods = _methods.GetValue(type, _ => new());
            var method = methods.GetOrAdd(nameof(GetValue), name =>
            {
                var methodDef = typeof(UnsafeExtensions).GetRequiredMethod(name, 1, typeof(IntPtr));
                return methodDef.MakeGenericMethod(type);
            });
            return method.Invoke(null, [ptr]);
        }
    }
}
