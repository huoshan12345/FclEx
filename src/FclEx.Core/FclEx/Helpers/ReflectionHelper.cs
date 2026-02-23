namespace FclEx.Helpers;

public static class ReflectionHelper
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<DataMemberInfo>> TypeDataMemberDic = [];

    public static IReadOnlyList<DataMemberInfo> GetDataMembers(Type type)
    {
        return TypeDataMemberDic.GetOrAdd(type, GetDataMembersInternal);

        static IReadOnlyList<DataMemberInfo> GetDataMembersInternal(Type type)
        {
            var types = GetVisibleDataMembers(type);

            var list = new List<DataMemberInfo>(types);

            var baseType = type.BaseType;
            while (baseType is not null)
            {
                var members = GetNotVisibleToDerivedDataMembers(baseType);
                list.AddRange(members);
                baseType = baseType.BaseType;
            }

            return list.ToReadOnlyList();
        }

        static IEnumerable<DataMemberInfo> GetVisibleDataMembers(Type type)
        {
            return type.GetMembers(BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                            | BindingFlags.Static
                                            | BindingFlags.FlattenHierarchy)
                .Where(m => m is PropertyInfo or FieldInfo)
                .Select(m => m.ToDataMemberInfo());
        }

        static IEnumerable<DataMemberInfo> GetNotVisibleToDerivedDataMembers(Type type)
        {
            return type.GetMembers(BindingFlags.NonPublic
                                   | BindingFlags.DeclaredOnly
                                   | BindingFlags.Static
                                   | BindingFlags.Instance)
                .Where(m => m is PropertyInfo property && property.IsNotVisibleToDerived()
                            || m is FieldInfo field && field.IsNotVisibleToDerived())
                .Select(m => m.ToDataMemberInfo());
        }
    }

    private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<FieldInfo>> TypeBackingFieldDic = [];

    /// <summary>
    /// Attempts to extract the single <see cref="FieldInfo"/> accessed by a 
    /// compiler-generated property accessor method.
    /// 
    /// <para>
    /// For a C# auto-property, Roslyn always emits IL in one of the following forms:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Getter:
    /// <code>
    /// ldarg.0
    /// ldfld &lt;backingField&gt;
    /// ret
    /// </code>
    /// </description>
    /// </item>
    /// 
    /// <item>
    /// <description>
    /// Setter / init:
    /// <code>
    /// ldarg.0
    /// ldarg.1
    /// stfld &lt;backingField&gt;
    /// ret
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// 
    /// <para>
    /// Where:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>ldfld</c> = 0x7B</description></item>
    /// <item><description><c>stfld</c> = 0x7D</description></item>
    /// </list>
    /// 
    /// <para>
    /// The metadata token immediately following the opcode identifies
    /// the accessed field.
    /// </para>
    /// 
    /// <para>
    /// We REQUIRE exactly one field access:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Multiple field accesses indicate a manually implemented property
    /// (e.g. <c>get =&gt; _a + _b</c>)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Zero field accesses indicate computed or forwarded properties
    /// </description>
    /// </item>
    /// </list>
    /// 
    /// <para>
    /// Returns <see langword="null"/> if:
    /// </para>
    /// <list type="bullet">
    /// <item><description>The method has no body (e.g. abstract/interface)</description></item>
    /// <item><description>Multiple field accesses are detected</description></item>
    /// <item><description>No field access is detected</description></item>
    /// </list>
    /// </summary>
    private static FieldInfo? GetSingleFieldAccess(MethodInfo method, byte opcode)
    {
        var body = method.GetMethodBody();
        var il = body?.GetILAsByteArray();
        if (il == null)
            return null;

        var typeArgs = method.DeclaringType is { IsGenericType: true } type
            ? type.GetGenericArguments()
            : null;

        var methodArgs = method.IsGenericMethod
            ? method.GetGenericArguments()
            : null;

        FieldInfo? field = null;

        var module = method.ReflectedType?.Module ?? method.Module;

        for (var i = 0; i < il.Length - 4; i++)
        {
            if (il[i] != opcode)
                continue;

            var token =
                il[i + 1] |
                (il[i + 2] << 8) |
                (il[i + 3] << 16) |
                (il[i + 4] << 24);

            try
            {
                var f = module.ResolveField(
                metadataToken: token,
                genericTypeArguments: typeArgs,
                genericMethodArguments: methodArgs);

                // must be exactly one access
                if (field != null)
                    return null;

                field = f;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to resolve field token 0x{token:X8} in method {method.DeclaringType?.ShortName()}.{method.Name} from module {module.Name}", e);
            }
        }

        return field;
    }

    /// <summary>
    /// Builds a set of auto-property backing fields declared on the specified type.
    /// 
    /// <para>
    /// Detection is performed at metadata level (no name matching).
    /// </para>
    /// 
    /// <para>
    /// A field is considered an auto-property backing field iff:
    /// </para>
    /// 
    /// <list type="number">
    /// <item><description>Property getter exists</description></item>
    /// <item><description>Property setter / init exists</description></item>
    /// <item><description>Both accessor methods are compiler-generated</description></item>
    /// <item><description>Getter performs exactly one <c>ldfld</c></description></item>
    /// <item><description>Setter performs exactly one <c>stfld</c></description></item>
    /// <item><description>Both access the SAME <see cref="FieldInfo"/></description></item>
    /// <item><description>The field itself is compiler-generated</description></item>
    /// </list>
    /// 
    /// <para>
    /// This matches the IL pattern emitted by Roslyn for:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Auto-properties</description></item>
    /// <item><description>Init-only properties</description></item>
    /// <item><description>Record primary constructor properties</description></item>
    /// </list>
    /// 
    /// <para>
    /// <b>BindingFlags.DeclaredOnly is REQUIRED</b>
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Backing fields are always declared in the same type as the property.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Inherited properties must NOT be scanned, otherwise base-type backing
    /// fields may be incorrectly cached under the derived type key.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    private static IReadOnlyCollection<FieldInfo> BuildBackingFieldMap(Type type)
    {
        var result = new List<FieldInfo>();

        var props = type.GetProperties(BindingAttributes.AllDeclared);

        foreach (var prop in props)
        {
            var getter = prop.GetGetMethod(true);
            var setter = prop.GetSetMethod(true);

            if (getter == null || setter == null)
                continue;

            if (!getter.IsCompilerGenerated()
                || !setter.IsCompilerGenerated())
                continue;

            // ldfld = 0x7B
            // stfld = 0x7D
            var gField = GetSingleFieldAccess(getter, 0x7B);
            var sField = GetSingleFieldAccess(setter, 0x7D);

            if (gField == null || sField == null)
                continue;

            // MUST be same field
            if (!ReferenceEquals(gField, sField))
                continue;

            if (!gField.IsDefined(typeof(CompilerGeneratedAttribute), false))
                continue;

            result.Add(gField);
        }

        return result.ToReadOnlySet();
    }

    public static IReadOnlyCollection<FieldInfo> GetAutoPropertyBackingFields(Type type)
    {
        return TypeBackingFieldDic.GetOrAdd(type, BuildBackingFieldMap);
    }
}