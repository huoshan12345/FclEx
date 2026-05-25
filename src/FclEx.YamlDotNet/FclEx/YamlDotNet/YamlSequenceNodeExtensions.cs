namespace FclEx.YamlDotNet;

public static class YamlSequenceNodeExtensions
{
    public static (T? Child, int Index) FindChild<T>(this YamlSequenceNode node, Predicate<T> match) where T : YamlNode
    {
        var index = 0;
        foreach (var child in node.Children)
        {
            if (child is T t && match(t))
                return (t, index);

            index++;
        }
        return (null, -1);
    }

    public static List<(T Child, int Index)> FindChildren<T>(this YamlSequenceNode node, Predicate<T> match) where T : YamlNode
    {
        var index = 0;
        var list = new List<(T Child, int Index)>();
        foreach (var child in node.Children)
        {
            if (child is T t && match(t))
                list.Add((t, index));

            index++;
        }
        return list;
    }

    public static bool HasChild<T>(this YamlSequenceNode node, Predicate<T> match) where T : YamlNode
    {
        return node.FindChild(match).Child != null;
    }

    public static bool TryRemove<T>(this YamlSequenceNode node, Predicate<T> match, out List<T> removedNodes) where T : YamlNode
    {
        removedNodes = [];
        foreach (var child in node.Children)
        {
            if (child is T t && match(t))
                removedNodes.Add(t);
        }
        return removedNodes.IsNotEmpty();
    }

    public static bool TryRemove<T>(this YamlSequenceNode node, Predicate<T> match) where T : YamlNode
    {
        return node.TryRemove(match, out _);
    }
}
