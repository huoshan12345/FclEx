namespace FclEx.Helpers;

public static class ExpressionHelper
{
    public static PropertyInfo GetProperty<T, TMember>(Expression<Func<T, TMember>> selector)
    {
        var member = GetMember(selector, false);
        if (member is PropertyInfo info) return info;
        throw new ArgumentException($"Expression '{selector}' does not refer to a property.");
    }

    public static FieldInfo GetField<T, TMember>(Expression<Func<T, TMember>> selector)
    {
        var member = GetMember(selector, false);
        if (member is FieldInfo info) return info;
        throw new ArgumentException($"Expression '{selector}' does not refer to a field.");
    }

    public static MethodInfo GetMethod<T>(Expression<Action<T>> selector)
    {
        var member = GetMember(selector);
        if (member is MethodInfo info) return info;
        throw new ArgumentException($"Expression '{selector}' does not refer to a method.");
    }

    public static MethodInfo GetMethod<T, TMember>(Expression<Func<T, TMember>> selector)
    {
        var member = GetMember(selector);
        if (member is MethodInfo info) return info;
        throw new ArgumentException($"Expression '{selector}' does not refer to a method.");
    }

    public static MemberInfo GetMember(Expression expression, bool allowNested = true)
    {
        Check.NotNull(expression);

        return expression switch
        {
            MethodCallExpression methodCall => methodCall.Method,
            LambdaExpression lambda => GetMember(lambda.Body, allowNested),
            UnaryExpression unary => GetMember(unary.Operand, allowNested),
            MemberExpression member => GetMemberInfo(member),
            _ => throw new ArgumentException($"Expression '{expression}' does not refer to a member."),
        };

        MemberInfo GetMemberInfo(MemberExpression member)
        {
            if (allowNested)
                return member.Member;

            var level = GetMemberNestingLevel(member.Expression);
            return level > 0
                ? throw new ArgumentException($"Expression '{expression}' must not reference a nested member.")
                : member.Member;
        }

        static int GetMemberNestingLevel(Expression? expression)
        {
            return expression switch
            {
                MemberExpression memberExp => 1 + GetMemberNestingLevel(memberExp.Expression),
                LambdaExpression lambda => GetMemberNestingLevel(lambda.Body),
                UnaryExpression unary => GetMemberNestingLevel(unary.Operand),
                _ => 0,
            };
        }
    }

    public static MemberInfo GetMember(Expression expression, Type type)
    {
        var member = GetMember(expression);

        var reflectedType = member.ReflectedType;

        // If the MemberInfo object is a global member (that is, if it was obtained from the Module.GetMethods method,
        // which returns global methods on a module), the returned DeclaringType will be null.
        if (reflectedType == null)
            throw new ArgumentException($"Expression '{expression}' does not refer to a member of a type.");

        if (type != reflectedType && !type.IsSubclassOf(reflectedType))
            throw new ArgumentException($"Expression '{expression}' refers to a member that is not from type {type.LongName()}.");

        return member;
    }

    public static MemberInfo GetMember<T>(Expression<Func<T, object?>> selector)
    {
        return GetMember(selector, typeof(T));
    }

    public static MemberInfo GetMember<T>(Expression<Action<T>> selector)
    {
        return GetMember(selector, typeof(T));
    }

    public static MemberInfo GetDataMember<T, TMember>(Expression<Func<T, TMember>> selector)
    {
        var member = GetMember(selector, false);
        return member switch
        {
            PropertyInfo prop => prop,
            FieldInfo field => field,
            _ => throw new ArgumentException($"Expression '{selector}' refers to neither a field nor a property.")
        };
    }

    public static DataMemberInfo GetDataMemberInfo<T, TMember>(Expression<Func<T, TMember>> selector)
    {
        var member = GetMember(selector, false);
        return member.ToDataMemberInfo();
    }

    public static Action<T, TMember> GetSetter<T, TMember>(Expression<Func<T, TMember>> selector)
    {
        var member = GetDataMemberInfo(selector);
        return (o, v) => member.SetValue(o, v);
    }

    public static Func<T, TMember?> GetGetter<T, TMember>(Expression<Func<T, TMember?>> selector)
    {
        var member = GetDataMemberInfo(selector);
        return o => member.GetValue(o).CastTo<TMember>();
    }

    /// <summary>
    /// Converts a strongly typed selector expression (<typeparamref name="TMember"/>) 
    /// into an expression returning <see cref="object"/>.<br/>
    /// This is useful when you need a generic property accessor without knowing the member type at compile time.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TMember">The member type of the selector.</typeparam>
    /// <param name="selector">An expression selecting a member of <typeparamref name="T"/>.</param>
    /// <returns>
    /// An equivalent expression returning <see cref="object"/> instead of <typeparamref name="TMember"/>.
    /// </returns>
    public static Expression<Func<T, object?>> ToObjectSelector<T, TMember>(Expression<Func<T, TMember>> selector)
    {
        var type = typeof(TMember);
        if (type != typeof(object))
        {
            var e = Expression.Convert(selector.Body, typeof(object));
            return Expression.Lambda<Func<T, object?>>(e, selector.Parameters);
        }
        else
        {
            return Expression.Lambda<Func<T, object?>>(selector.Body, selector.Parameters);
        }
    }

    public static Type GetDataMemberType(this MemberInfo member)
    {
        return member switch
        {
            PropertyInfo propInfo => propInfo.PropertyType,
            FieldInfo fieldInfo => fieldInfo.FieldType,
            DataMemberInfo dataMemberInfo => dataMemberInfo.DataMemberType,
            _ => throw new ArgumentException($"MemberInfo '{member.Name}' refers to neither a field nor a property.")
        };
    }

    public static (string Name, TMember value) GetNamedValue<T, TMember>(T obj, Expression<Func<T, TMember>> selector)
    {
        var member = GetDataMemberInfo(selector);
        return (member.Name, member.GetValue(obj).CastTo<TMember>())!;
    }

    /// <summary>
    /// Creates a strongly-typed <see cref="Expression{TDelegate}"/> that selects
    /// a property or field of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object that contains the target property or field.
    /// </typeparam>
    /// <typeparam name="TMember">
    /// The type of the property or field being selected.
    /// </typeparam>
    /// <param name="propertyOrFieldName">
    /// The name of the property or field to access. Case-sensitive.
    /// </param>
    /// <returns>
    /// A lambda expression of the form <c>x => x.PropertyOrField</c>.
    /// </returns>
    public static Expression<Func<T, TMember>> CreateSelector<T, TMember>(string propertyOrFieldName)
    {
        var type = typeof(T);
        var parameter = Expression.Parameter(type);
        var access = Expression.PropertyOrField(parameter, propertyOrFieldName);
        return Expression.Lambda<Func<T, TMember>>(access, parameter);
    }

    /// <summary>
    /// Creates a lambda expression that selects the specified property or field from
    /// an instance of <typeparamref name="T"/>, converting the result to <see cref="object"/>.
    /// </summary>
    /// <typeparam name="T">The type that contains the member to access.</typeparam>
    /// <param name="propertyOrFieldName">The name of the property or field to select.</param>
    /// <returns>
    /// A lambda expression of the form <c>x => (object)x.Member</c>.
    /// </returns>
    public static Expression<Func<T, object?>> CreateSelector<T>(string propertyOrFieldName)
    {
        var type = typeof(T);
        var parameter = Expression.Parameter(type);
        var access = Expression.PropertyOrField(parameter, propertyOrFieldName);
        var convert = Expression.Convert(access, typeof(object));
        return Expression.Lambda<Func<T, object?>>(convert, parameter);
    }

    public static IEnumerable<MemberInfo> GetDataMembers<T>(Expression<Func<T, object?>> selector)
    {
        var body = selector.Body;

        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            body = unary.Operand;
        }

        return body switch
        {
            MemberExpression member => [GetDataMember(member)],
            NewExpression newExpr => newExpr.Arguments
                .Select(arg => arg is not MemberExpression m
                    ? throw new ArgumentException("Only simple member access is allowed: " + arg)
                    : GetDataMember(m))
                .ToArray(),
            _ => throw new ArgumentException("Selector must be a member access or new expression.")
        };
    }

    private static MemberInfo GetDataMember(MemberExpression member)
    {
        var m = member.Member;
        return m is not PropertyInfo && m is not FieldInfo
            ? throw new ArgumentException("Only property or field is allowed: " + m.MemberType)
            : m;
    }
}