namespace FclEx.Utils;

public class TreeNodeTests
{
    [Fact]
    public void AddRemoveAndMove_MaintainParentRelationship()
    {
        var firstParent = new TreeNode<int>(0);
        var secondParent = new TreeNode<int>(1);
        var child = firstParent.AddChild(2);

        child.MoveTo(secondParent);

        Assert.Empty(firstParent.Children);
        Assert.Same(secondParent, child.Parent);
        Assert.Same(child, Assert.Single(secondParent.Children));

        Assert.True(secondParent.RemoveChild(child));
        Assert.Null(child.Parent);
        Assert.Empty(secondParent.Children);
        Assert.False(secondParent.RemoveChild(child));
    }

    [Fact]
    public void AddChild_RejectsMultipleParentsAndCycles()
    {
        var root = new TreeNode<int>(0);
        var child = root.AddChild(1);
        var grandchild = child.AddChild(2);
        var otherParent = new TreeNode<int>(3);

        Assert.Throws<InvalidOperationException>(() => otherParent.AddChild(child));
        Assert.Throws<InvalidOperationException>(() => grandchild.AddChild(root));
        Assert.Throws<InvalidOperationException>(() => root.AddChild(root));

        Assert.Same(root, child.Parent);
        Assert.Same(child, grandchild.Parent);
    }

    [Fact]
    public void MoveTo_RejectsCycleWithoutChangingExistingTree()
    {
        var root = new TreeNode<int>(0);
        var child = root.AddChild(1);
        var grandchild = child.AddChild(2);

        Assert.Throws<InvalidOperationException>(() => child.MoveTo(grandchild));

        Assert.Same(root, child.Parent);
        Assert.Same(child, Assert.Single(root.Children));
        Assert.Same(grandchild, Assert.Single(child.Children));
    }

    [Fact]
    public void TraversalByLevel_PreservesTheGenericNullability()
    {
        var root = new TreeNode<string?>(null);
        root.AddChild("child");

        IEnumerable<string?> values = root.TraversalByLevel();

        Assert.Equal([null, "child"], values);
    }
}
