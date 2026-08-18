namespace FclEx.Extensions;

public static class DelegateExtensions
{
    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
    private const MethodImplAttributes ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;
    private const MethodAttributes CtorAttributes = MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;
    private const MethodAttributes InvokeAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
    private const TypeAttributes DelegateTypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass;
    private static readonly Type[] _delegateCtorSignature = [typeof(object), typeof(IntPtr)];
    private static readonly ModuleBuilder _moduleBuilder = CreateModuleBuilder();

    private static ModuleBuilder CreateModuleBuilder()
    {
        var assemblyName = new AssemblyName("RuntimeDelegateTypes");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        return assemblyBuilder.DefineDynamicModule("RuntimeDelegateTypes");
    }

    private static volatile int _typeCounter;

    private static readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
    _lock = new();
    
    extension(Delegate)
    {
        /// <summary>
        /// Creates a delegate type with the specified return and parameter types.
        /// </summary>
        /// <remarks>
        /// The type is created with <see cref="AssemblyBuilder"/> and exists only at runtime. This API requires a runtime that
        /// supports dynamic code generation and is not suitable for Native AOT scenarios. Prefer
        /// <see cref="Expression.GetDelegateType(Type[])"/> when its supported signature set is sufficient; it cannot
        /// represent every CLR delegate signature, including signatures with by-reference parameters. Each call to this
        /// method defines a new non-collectible runtime type, so callers should avoid repeatedly creating the same signature
        /// or passing types from collectible assembly-load contexts.
        /// </remarks>
        public static Type MakeNewCustomDelegate(Type returnType, IEnumerable<Type> parameterTypes)
        {
            var paras = parameterTypes.AsArray();
            var typeName = "Delegate" + Interlocked.Increment(ref _typeCounter);

            TypeBuilder typeBuilder;
            lock (_lock)
            {
                typeBuilder = _moduleBuilder.DefineType(
                    typeName,
                    DelegateTypeAttributes,
                    typeof(MulticastDelegate));

                typeBuilder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _delegateCtorSignature)
                    .SetImplementationFlags(ImplAttributes);

                typeBuilder.DefineMethod("Invoke", InvokeAttributes, returnType, paras)
                    .SetImplementationFlags(ImplAttributes);
            }

            return typeBuilder.CreateTypeInfo()!;
        }
    }
}
