namespace FclEx.Utils;

public class ExpressionReplacer : ExpressionVisitor
{
    private Expression? _oldValue;
    private Expression? _newValue;

    private ExpressionReplacer Init(Expression oldExp, Expression newExp)
    {
        _oldValue = oldExp;
        _newValue = newExp;
        return this;
    }

    public override Expression? Visit(Expression? node)
    {
        return node == _oldValue
            ? _newValue
            : base.Visit(node);
    }

    public static Expression Replace(Expression exp, Expression oldExp, Expression newExp)
    {
        using var disposable = ObjectPoolHelper.GetPool<ExpressionReplacer>().GetPooled();
        var replacer = disposable.Value.Init(oldExp, newExp);
        return replacer.Visit(exp)!;
    }
}