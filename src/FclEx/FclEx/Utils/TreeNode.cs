using System.Collections.Generic;
using MoreLinq.Extensions;

namespace FclEx.Utils
{
    public class TreeNode<T>
    {
        public TreeNode(T value)
        {
            Value = value;
        }

        public T Value { get; }
        public List<TreeNode<T>> Children { get; } = new List<TreeNode<T>>();
        public TreeNode<T>? Parent { get; private set; }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value);
            Children.Add(node);
            node.Parent = this;
            return node;
        }

        public void AddChildren(IEnumerable<T> values)
        {
            values.ForEach(m => AddChild(m));
        }

        public bool DeepEquals(TreeNode<T> y, IEqualityComparer<T>? comparer = null)
        {
            if (y == null) return false;
            comparer ??= EqualityComparer<T>.Default;
            var x = this;
            var map = new Dictionary<TreeNode<T>, TreeNode<T>> { { x, y } };
            var queue = new Queue<TreeNode<T>>();
            queue.Enqueue(x);
            while (queue.Count != 0)
            {
                var left = queue.Dequeue();
                if (!map.TryGetValue(left, out var right)) return false;
                if (!comparer.Equals(left.Value, right.Value)) return false;
                if (left.Children.Count != right.Children.Count) return false;
                left.Children.ForEach((m, i) =>
                {
                    queue.Enqueue(m);
                    map.Add(m, right.Children[i]);
                });
            }
            return true;
        }
    }
}
