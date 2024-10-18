namespace FclEx;

public readonly record struct Unit
{
    public static readonly Unit Default = default;

    public override string ToString() => "()";
}