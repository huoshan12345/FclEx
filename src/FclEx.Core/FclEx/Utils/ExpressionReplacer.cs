namespace FclEx.Utils;

public class ExpressionReplacer : ExpressionVisitor
{
    private readonly Expression? _oldValue;
    private readonly Expression? _newValue;

    private ExpressionReplacer(Expression oldExp, Expression newExp)
    {
        _oldValue = oldExp;
        _newValue = newExp;
    }

    public override Expression? Visit(Expression? node)
    {
        return node == _oldValue
            ? _newValue
            : base.Visit(node);
    }

    public static Expression Replace(Expression exp, Expression oldExp, Expression newExp)
    {
        var replacer = new ExpressionReplacer(oldExp, newExp);
        return replacer.Visit(exp)!;
    }
}