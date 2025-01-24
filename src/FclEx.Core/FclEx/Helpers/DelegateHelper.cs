namespace FclEx.Helpers;

public static class DelegateHelper
{
    private const MethodAttributes CtorAttributes = MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;
    private const MethodImplAttributes ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;
    private const MethodAttributes InvokeAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
    private static readonly Type[] _delegateCtorSignature = [typeof(object), typeof(IntPtr)];

    public static readonly Type TypeOfAssemblyGen = typeof(Expression).Assembly.GetRequiredType("System.Linq.Expressions.Compiler.AssemblyGen");
    public static readonly Func<string, TypeBuilder> DefineDelegateType = TypeOfAssemblyGen
        .GetRequiredMethod("DefineDelegateType")
        .CreateDelegate<Func<string, TypeBuilder>>();

    public static Type MakeNewCustomDelegate(Type returnType, IEnumerable<Type> parameterTypes)
    {
        var paras = parameterTypes.AsArray();
        var builder = DefineDelegateType("Delegate" + paras.Length + 1);
        builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _delegateCtorSignature).SetImplementationFlags(ImplAttributes);
        builder.DefineMethod("Invoke", InvokeAttributes, returnType, paras).SetImplementationFlags(ImplAttributes);
        return builder.CreateTypeInfo()!;
    }
}