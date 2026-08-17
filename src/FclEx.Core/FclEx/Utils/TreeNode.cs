namespace FclEx.Utils;

public static class TreeNode
{
    public static TreeNode<T> Create<T>(T value) => new(value);
}

public sealed class TreeNode<T>
{
    private readonly List<TreeNode<T>> _children = [];

    public TreeNode(T value)
    {
        Value = value;
    }

    public T Value { get; }
    public IReadOnlyList<TreeNode<T>> Children => _children;
    public TreeNode<T>? Parent { get; private set; }

    public TreeNode<T> AddChild(T value)
    {
        var node = new TreeNode<T>(value);
        AddChild(node);
        return node;
    }

    public TreeNode<T> AddChild(TreeNode<T> child)
    {
        Check.NotNull(child);
        EnsureCanAttach(child);

        _children.Add(child);
        child.Parent = this;
        return child;
    }

    public void AddChildren(IEnumerable<T> values)
    {
        values.ForEach(m => AddChild(m));
    }

    public bool RemoveChild(TreeNode<T> child)
    {
        Check.NotNull(child);

        if (_children.Remove(child) == false)
            return false;

        child.Parent = null;
        return true;
    }

    public bool Detach()
    {
        return Parent?.RemoveChild(this) == true;
    }

    public void MoveTo(TreeNode<T> newParent)
    {
        Check.NotNull(newParent);

        if (ReferenceEquals(Parent, newParent))
            return;

        newParent.EnsureCanAttach(this, allowExistingParent: true);

        // Reserve the destination slot before changing the existing relationship. If allocation
        // fails, this node remains attached to its original parent.
        newParent._children.Add(this);
        Parent?._children.Remove(this);
        Parent = newParent;
    }

    public void SortChildren(Comparison<TreeNode<T>> comparison)
    {
        Check.NotNull(comparison);
        _children.Sort(comparison);
    }

    public bool DeepEquals(TreeNode<T>? other, IEqualityComparer<T>? comparer = null)
    {
        if (other == null)
            return false;

        comparer ??= EqualityComparer<T>.Default;
        var queue = new Queue<(TreeNode<T> Left, TreeNode<T> Right)>();
        queue.Enqueue((this, other));
        while (queue.Count != 0)
        {
            var (left, right) = queue.Dequeue();

            if (comparer.Equals(left.Value, right.Value) == false)
                return false;

            if (left.Children.Count != right.Children.Count)
                return false;

            for (var i = 0; i < left.Children.Count; i++)
                queue.Enqueue((left.Children[i], right.Children[i]));
        }
        return true;
    }

    private void EnsureCanAttach(TreeNode<T> child, bool allowExistingParent = false)
    {
        if (ReferenceEquals(child, this))
            throw new InvalidOperationException("A tree node cannot be its own child.");
        if (!allowExistingParent && child.Parent is not null)
            throw new InvalidOperationException("The child already has a parent. Use MoveTo to move an attached node.");

        for (var ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ReferenceEquals(ancestor, child))
                throw new InvalidOperationException("Attaching the node would create a cycle.");
        }
    }
}

public static class TreeNodeExtensions
{
    public static IEnumerable<TreeNode<T>> GetPathToRoot<T>(this TreeNode<T> node)
    {
        var p = node;
        while (p != null)
        {
            yield return p;
            p = p.Parent;
        }
    }

    public static IEnumerable<T> TraversalByLevel<T>(this TreeNode<T>? root)
    {
        if (root == null)
            yield break;

        var queue = new Queue<TreeNode<T>>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            yield return cur.Value;

            foreach (var child in cur.Children)
            {
                queue.Enqueue(child);
            }
        }
    }
}
