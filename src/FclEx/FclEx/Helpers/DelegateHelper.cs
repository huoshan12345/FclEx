using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Helpers
{
    public static class DelegateHelper
    {
        private const MethodAttributes CtorAttributes = MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public;
        private const MethodImplAttributes ImplAttributes = MethodImplAttributes.Runtime | MethodImplAttributes.Managed;
        private const MethodAttributes InvokeAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        private static readonly Type[] _delegateCtorSignature = { typeof(object), typeof(IntPtr) };

        public static readonly Type TypeOfAssemblyGen = typeof(Expression).Assembly.GetType("System.Linq.Expressions.Compiler.AssemblyGen")!;
        public static readonly MethodInfo MethodOfDefineDelegateType = TypeOfAssemblyGen.GetMethod("DefineDelegateType", BindingFlags.NonPublic | BindingFlags.Static)!;
        public static readonly Func<string, TypeBuilder> DefineDelegateType = CreateDelegate<Func<string, TypeBuilder>>(MethodOfDefineDelegateType);

        public static T CreateDelegate<T>(MethodInfo method) where T : Delegate
        {
            return (T)Delegate.CreateDelegate(typeof(T), method);
        }

        public static T CreateDelegate<T>(object @this, MethodInfo method) where T : Delegate
        {
            return (T)Delegate.CreateDelegate(typeof(T), @this, method);
        }

        public static Type MakeNewCustomDelegate(Type returnType, IEnumerable<Type> parameterTypes)
        {
            var paras = parameterTypes.AsArray();
            var builder = DefineDelegateType("Delegate" + paras.Length + 1);
            builder.DefineConstructor(CtorAttributes, CallingConventions.Standard, _delegateCtorSignature).SetImplementationFlags(ImplAttributes);
            builder.DefineMethod("Invoke", InvokeAttributes, returnType, paras).SetImplementationFlags(ImplAttributes);
            return builder.CreateTypeInfo()!;
        }
    }
}
