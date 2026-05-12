namespace FclEx.Utils;

public static class Accessor<T>
{
    public static DynamicMethod CreateDynamicMethod(Type owner, string name, Type returnType, Type[] parameterTypes)
    {
        return new DynamicMethod(
            name: name,
            attributes: MethodAttributes.Static | MethodAttributes.Public,
            callingConvention: CallingConventions.Standard,
            returnType: returnType,
            parameterTypes: parameterTypes,
            owner: owner,
            skipVisibility: true);
    }

    public static RefGetter<T, TField> BuildRefGetter<TField>(Type owner, string fieldName)
    {
        var method = CreateDynamicMethod(owner, fieldName,
#if NET5_0_OR_GREATER
            typeof(TField).MakeByRefType(),
#else
            // netfx does not support ref return type for DynamicMethod, so we return a pointer instead.
            typeof(TField*),
#endif
            [typeof(T)]);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldflda, typeof(T).GetRequiredField(fieldName));
        il.Emit(OpCodes.Ret);
#if NET5_0_OR_GREATER
        return method.CreateDelegate<RefGetter<T, TField>>();
#else
        return method.CreateDelegate<PtrGetter<T, TField>>().AsRef();
#endif
    }

    public static Func<T, TField> BuildGetter<TField>(Type owner, string fieldName)
    {
        var method = CreateDynamicMethod(owner, fieldName, typeof(TField), [typeof(T)]);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, typeof(T).GetRequiredField(fieldName));
        il.Emit(OpCodes.Ret);
        return method.CreateDelegate<Func<T, TField>>();
    }
}
