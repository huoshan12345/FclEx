namespace FclEx.Utils;

/// <summary>
/// Temporarily set a member's value of an object and restore it when disposing.
/// </summary>
public class TemporarySetter
{
    public static TemporarySetterBuilder<T> For<T>(T? obj)
    {
        return new(obj);
    }
}

/// <summary>
/// Build a <see cref="TemporarySetter"/>.
/// </summary>
public class TemporarySetterBuilder<T>
{
    private readonly T? _obj;

    public TemporarySetterBuilder(T? obj)
    {
        _obj = obj;
    }

    public TemporarySetter<T, TMember> Set<TMember>(Expression<Func<T, TMember>> selector, TMember tempValue)
    {
        return new(_obj, selector, tempValue);
    }
}

/// <summary>
/// Temporarily set a member's value of an object and restore it when disposing.
/// </summary>
public class TemporarySetter<T, TMember> : IDisposable
{
    private readonly DataMemberInfo _member;
    private readonly TMember? _value;
    private readonly T? _obj;

    public TemporarySetter(T? obj, Expression<Func<T, TMember>> selector, TMember tempValue)
    {
        _obj = obj;
        _member = ExpressionHelper.GetMember(selector).ToDataMemberInfo();
        _value = _member.GetValue<TMember>(obj);
        _member.SetValue(_obj, tempValue);
    }

    public void Dispose()
    {
        _member.SetValue(_obj, _value);
    }
}