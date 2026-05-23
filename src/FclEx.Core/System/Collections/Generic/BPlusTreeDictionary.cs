// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
namespace System.Collections.Generic;

[DebuggerDisplay("Count = {Count}")]
public class BPlusTreeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private const int DefaultMinDegree = 16;

    private readonly int _minKeyCount;
    private readonly int _maxKeyCount;
    private BPlusTreeNode? _root;
    private BPlusTreeNode? _firstLeaf;
    private int _level;
    private int _count;
    private int _version;
    private readonly IComparer<TKey> _comparer;
    private readonly EqualityComparer<TValue?> _valueComparer;

    public BPlusTreeDictionary(int minDegree = DefaultMinDegree, IComparer<TKey>? comparer = null)
    {
        _minKeyCount = Check.NotLessThan(minDegree, 2);
        _maxKeyCount = 2 * _minKeyCount;
        _comparer = comparer ?? Comparer<TKey>.Default;
        _valueComparer = EqualityComparer<TValue?>.Default;
    }

    public BPlusTreeDictionary(IEnumerable<KeyValuePair<TKey, TValue>> enumerable, IComparer<TKey>? comparer = null)
        : this(DefaultMinDegree, comparer)
    {
        Check.NotNull(enumerable);
        AddRange(enumerable);
    }

    private void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
        // TODO: optimize this for some special cases
        foreach (var item in enumerable)
        {
            Add(item.Key, item.Value);
        }
    }

    public IComparer<TKey> Comparer => _comparer;
    public int Level => _level;
    public int MinDegree => _minKeyCount;
    public int Count => _count;
    public bool IsReadOnly => false;
    public ICollection<TKey> Keys => field ??= new KeyCollection(this);
    public ICollection<TValue> Values => field ??= new ValueCollection(this);
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    public Enumerator GetEnumerator() => new(this);
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return _count == 0
            // use singleton empty enumerator to avoid unnecessary allocation when the dictionary is empty.
            ? GenericEmptyEnumerator<KeyValuePair<TKey, TValue>>.Instance
            : GetEnumerator();
    }

    public void Clear()
    {
        Clear(_root);
        _firstLeaf = null;
        _root = null;
        _count = 0;
        _level = 0;
        ++_version;

        static void Clear(BPlusTreeNode? root)
        {
            if (root == null)
                return;

            var queue = new Queue<BPlusTreeNode>();
            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                var item = queue.Dequeue();
                foreach (var t in item.Children.Take(item.KeyCount))
                {
                    queue.Enqueue(t);
                }
                item.Invalidate();
            }
        }
    }

    private (BPlusTreeNode? Node, int Index) Find(TKey key, bool checkValue = false, TValue? value = default)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        var node = _root;
        while (node != null)
        {
            var (found, index) = node.FindLastOfLessOrEqual(key);
            if (found)
            {
                var (leafNode, valueIndex) = node.IsLeafNode
                    ? (node, index)
                    : (node.Children[index].GetMinLeafNode(), 0); // if the key is found in an internal node, the corresponding value must be in the leftmost leaf node of its right child.

                return checkValue && !_valueComparer.Equals(leafNode.Values[valueIndex], value)
                    ? default
                    : (leafNode, valueIndex);
            }

            if (index < 0 || node.IsLeafNode)
                return default;

            node = node.Children[index];
        }
        return default;
    }

    private (BPlusTreeNode Node, int Index) FindLeafNodeToInsert(TKey key)
    {
        var node = _root;
        while (true)
        {
            Debug.Assert(node is not null);

            var (found, index) = node!.FindLastOfLessOrEqual(key);
            if (found)
                throw new ArgumentException($"An item with the same key has already been added. Key: {key}");

            if (index < 0)
                return (node.GetMinLeafNode(), 0);

            if (node.IsLeafNode)
                return (node, index + 1);

            node = node.Children[index];
        }
    }

    public bool ContainsValue(TValue value)
    {
        foreach (var (_, v) in this)
        {
            if (_valueComparer.Equals(v, value))
                return true;
        }
        return false;
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        Check.NotNull(item.Key, nameof(item)); // check here to set correct parameter name in exception
        return Find(item.Key, true, item.Value) != default;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        Check.CanCopyTo(array, arrayIndex, _count);

        foreach (var item in this)
        {
            array[arrayIndex++] = item;
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        var (node, index) = Find(item.Key, true, item.Value);
        if (node is null)
            return false;

        RemoveItem(node, index);
        return true;
    }

    private void RemoveItem(BPlusTreeNode node, int index)
    {
        Debug.Assert(node.IsLeafNode);

        node.RemoveKeyValue(index);

        // update the keys in its parent nodes when removing a node at the leftmost position.
        if (index == 0 && node != _root)
            UpdateKeys(node);

        if (node != _root && node.KeyCount < _minKeyCount)
            RebalanceForDeletion(node);

        --_count;
        ++_version;
    }

    private void RebalanceForDeletion(BPlusTreeNode node)
    {
        Debug.Assert(node != _root && node.KeyCount < _minKeyCount, $"{nameof(node)} does not need to rebalance");

        var parent = node.Parent;
        Debug.Assert(parent != null);

        var childIndex = node.ChildIndex;
        Debug.Assert(childIndex >= 0 && parent!.Children[childIndex] == node, "pointers between child and parent are not correct");

        var leftSiblingIndex = childIndex - 1;
        var rightSiblingIndex = childIndex + 1;
        // case 1: the deficient node's right sibling exists and has more than the minimum number of elements, then rotate left
        if (rightSiblingIndex < parent!.KeyCount && parent.Children[rightSiblingIndex].KeyCount > _minKeyCount)
        {
            var sibling = parent.Children[rightSiblingIndex];
            var borrowKey = sibling.Keys[0];
            // remove the first element of the right sibling
            if (sibling.IsLeafNode)
            {
                var borrowValue = sibling.Values[0];
                node.InsertKeyValue(borrowKey, borrowValue, node.KeyCount);
                sibling.RemoveKeyValue(0);
            }
            else
            {
                var borrowChild = sibling.Children[0];
                node.InsertKeyChild(borrowKey, borrowChild, node.KeyCount);
                sibling.RemoveKeyChild(0);
            }
            parent.Keys[rightSiblingIndex] = sibling.Keys[0];
        }
        // case 2: the deficient node's left sibling exists and has more than the minimum number of elements, then rotate right
        else if (leftSiblingIndex >= 0 && parent.Children[leftSiblingIndex].KeyCount > _minKeyCount)
        {
            var sibling = parent.Children[leftSiblingIndex];
            var borrowKey = sibling.Keys[sibling.KeyCount - 1];
            // remove the last element of the left sibling
            if (sibling.IsLeafNode)
            {
                var borrowValue = sibling.Values[sibling.KeyCount - 1];
                node.InsertKeyValue(borrowKey, borrowValue, 0);
                sibling.RemoveKeyValue(sibling.KeyCount - 1);
            }
            else
            {
                var borrowChild = sibling.Children[sibling.KeyCount - 1];
                node.InsertKeyChild(borrowKey, borrowChild, 0);
                sibling.RemoveKeyChild(sibling.KeyCount - 1);
            }
            parent.Keys[childIndex] = node.Keys[0];
        }
        // case 3: if both immediate siblings have only the minimum number of elements, then merge with a sibling sandwiching their separator taken off from their parent
        // case 3-a: the deficient node's right sibling exists
        else if (rightSiblingIndex < parent.KeyCount)
        {
            MergeNodes(node);
        }
        // case 3-b: the deficient node's left sibling exists
        else if (leftSiblingIndex >= 0)
        {
            MergeNodes(parent.Children[leftSiblingIndex]);
        }
        else
        {
            Debug.Assert(false, "should not reach here!");
        }
    }

    private void MergeNodes(BPlusTreeNode node)
    {
        var parent = node.Parent;
        Debug.Assert(parent is not null);

        var right = node.MergeWithRight();
        var rightIndex = right.ChildIndex;
        right.Invalidate();
        parent!.RemoveKeyChild(rightIndex);

        if (parent == _root && parent.KeyCount < 2)
        {
            _root = node;
            node.Parent = null;
            --_level;
        }
        else if (parent != _root && parent.KeyCount < _minKeyCount)
        {
            RebalanceForDeletion(parent);
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (_root == null)
        {
            _root = new BPlusTreeNode(this, true);
            _firstLeaf = _root;
        }
        var (node, index) = FindLeafNodeToInsert(key);
        node.InsertKeyValue(key, value, index);

        if (node.IsKeyFull)
            SplitNode(node);

        // update the keys in its parent nodes when insert a new node to the leftmost position.
        if (index == 0 && node != _root)
            UpdateKeys(node);

        ++_count;
        ++_version;
    }

    private void UpdateKeys(BPlusTreeNode node)
    {
        Debug.Assert(node.IsLeafNode);

        var p = node;
        while (p != _root)
        {
            Debug.Assert(p?.Parent != null);

            var parent = p!.Parent!;

            // nth key should be the minimum key of the nth child
            parent.Keys[p.ChildIndex] = p.Keys[0];
            p = p.Parent;
        }
    }

    private void SplitNode(BPlusTreeNode node)
    {
        Debug.Assert(node != null);

        var split = node!.Split();
        if (node == _root)
        {
            var newRoot = new BPlusTreeNode(this, false);
            newRoot.InsertKeyChild(_root.Keys[0], _root, 0);
            newRoot.InsertKeyChild(split.Keys[0], split, 1);
            _root = newRoot;
            ++_level;
        }
        else
        {
            var parent = node.Parent;
            Debug.Assert(parent != null);
            Debug.Assert(node.ChildIndex >= 0 && node.ChildIndex < parent!.KeyCount);

            parent!.InsertKeyChild(split.Keys[0], split, node.ChildIndex + 1);

            if (parent.IsKeyFull)
            {
                // ReSharper disable once TailRecursiveCall
                SplitNode(parent);
            }
        }
    }

    public bool ContainsKey(TKey key) => Find(key) != default;

    public bool Remove(TKey key)
    {
        var (node, index) = Find(key);
        if (node is null)
            return false;

        RemoveItem(node, index);
        return true;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        var (node, index) = Find(key);
        if (node is null)
        {
            value = default!;
            return false;
        }
        value = node.Values[index];
        return true;
    }

    public TValue this[TKey key]
    {
        get
        {
            var (node, index) = Find(key);

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (node is null)
                throw new KeyNotFoundException(key.ToString());

            return node.Values[index];
        }

        set
        {
            var (node, index) = Find(key);
            if (node is null)
            {
                Add(key, value);
                ++_version;
            }
            else
            {
                node.Values[index] = value;
                // update value does not increase version.
            }
        }
    }

    internal class BPlusTreeNode
    {
        public readonly BPlusTreeDictionary<TKey, TValue> Tree;
        public int KeyCount;
        public BPlusTreeNode? Parent;
        public BPlusTreeNode? Next;
        public int ChildIndex = -1;
        public readonly TKey[] Keys;
        public readonly TValue[] Values;
        public readonly BPlusTreeNode[] Children;

        public bool IsKeyFull => KeyCount >= Tree._maxKeyCount;
        public bool IsLeafNode => Values.IsNotEmpty();

        public BPlusTreeNode(BPlusTreeDictionary<TKey, TValue> tree, bool isLeaf)
        {
            var max = tree._maxKeyCount;
            Tree = tree;
            Keys = new TKey[max];
            Children = isLeaf ? [] : new BPlusTreeNode[max];
            Values = isLeaf ? new TValue[max] : [];
        }

        public void InsertKeyValue(TKey key, TValue value, int index)
        {
            Debug.Assert(index >= 0 && index <= KeyCount);
            Debug.Assert(IsLeafNode);
            Debug.Assert(KeyCount + 1 <= Tree._maxKeyCount);

            var diff = KeyCount - index;
            if (diff > 0)
            {
                Array.Copy(Keys, index, Keys, index + 1, diff);
                Array.Copy(Values, index, Values, index + 1, diff);
            }

            Keys[index] = key!;
            Values[index] = value;
            ++KeyCount;
        }

        public void RemoveKeyValue(int index)
        {
            Debug.Assert(index >= 0 && index < KeyCount);
            Debug.Assert(IsLeafNode);

            var diff = KeyCount - index - 1;
            if (diff > 0)
            {
                Array.Copy(Keys, index + 1, Keys, index, diff);
                Array.Copy(Values, index + 1, Values, index, diff);
            }
            Keys[KeyCount - 1] = default!;
            Values[KeyCount - 1] = default!;
            --KeyCount;
        }

        public BPlusTreeNode MergeWithRight()
        {
            Debug.Assert(Parent != null);
            Debug.Assert(ChildIndex >= 0 && ChildIndex < Parent!.KeyCount - 1);
            Debug.Assert(Parent!.Children[ChildIndex] == this);

            var rightIndex = ChildIndex + 1;
            var rightNode = Parent.Children[rightIndex];

            Array.Copy(rightNode.Keys, 0, Keys, KeyCount, rightNode.KeyCount);

            if (IsLeafNode)
            {
                Array.Copy(rightNode.Values, 0, Values, KeyCount, rightNode.KeyCount);
                Next = rightNode.Next;
            }
            else
            {
                Array.Copy(rightNode.Children, 0, Children, KeyCount, rightNode.KeyCount);
                for (var i = KeyCount; i < KeyCount + rightNode.KeyCount; i++)
                {
                    Children[i].Parent = this;
                    Children[i].ChildIndex = i;
                }
            }

            KeyCount += rightNode.KeyCount;

            return rightNode;
        }

        public void InsertKeyChild(TKey key, BPlusTreeNode node, int index)
        {
            Debug.Assert(node != null);
            Debug.Assert(index >= 0 && index <= KeyCount);
            Debug.Assert(!IsLeafNode);
            Debug.Assert(KeyCount + 1 <= Tree._maxKeyCount);

            var diff = KeyCount - index;
            if (diff > 0)
            {
                Array.Copy(Keys, index, Keys, index + 1, diff);
                Array.Copy(Children, index, Children, index + 1, diff);
            }

            Keys[index] = key!;
            Children[index] = node!;
            node!.ChildIndex = index;
            node.Parent = this;
            ++KeyCount;

            // update index of children
            for (var i = index + 1; i < KeyCount; i++)
            {
                ++Children[i].ChildIndex;
            }

        }

        public void RemoveKeyChild(int index)
        {
            Debug.Assert(index >= 0 && index < KeyCount);
            Debug.Assert(!IsLeafNode);
            Debug.Assert(KeyCount >= 2);

            var diff = KeyCount - index - 1;
            if (diff > 0)
            {
                Array.Copy(Keys, index + 1, Keys, index, diff);
                Array.Copy(Children, index + 1, Children, index, diff);
            }

            Keys[KeyCount - 1] = default!;
            Children[KeyCount - 1] = default!;
            --KeyCount;

            // update index of children
            for (var i = index; i < KeyCount; i++)
            {
                --Children[i].ChildIndex;
            }
        }

        public void Invalidate()
        {
            Next = null;
            Parent = null;
            KeyCount = 0;
            Children.Clear();
            Keys.Clear();
            Values.Clear();
        }

        public (bool Found, int Index) FindLastOfLessOrEqual(TKey key)
        {
            var low = -1;
            var high = KeyCount - 1;
            while (low < high)
            {
                var mid = (low + high + 1) >> 1;
                var cmp = Tree._comparer.Compare(Keys[mid], key);
                if (cmp < 0) low = mid;
                else if (cmp == 0) return (true, mid);
                else high = mid - 1;
            }
            return (low >= 0 && Tree._comparer.Compare(Keys[low], key) == 0, low);
        }

        public BPlusTreeNode GetMinLeafNode()
        {
            var p = this;
            while (!p.IsLeafNode)
            {
                p = p.Children[0];
            }
            return p;
        }

        public BPlusTreeNode Split()
        {
            Debug.Assert(IsKeyFull);

            var newNode = new BPlusTreeNode(Tree, IsLeafNode);
            Array.Copy(Keys, Tree._minKeyCount, newNode.Keys, 0, Tree._minKeyCount);
            Array.Clear(Keys, Tree._minKeyCount, Tree._minKeyCount);

            if (IsLeafNode)
            {
                Array.Copy(Values, Tree._minKeyCount, newNode.Values, 0, Tree._minKeyCount);
                Array.Clear(Values, Tree._minKeyCount, Tree._minKeyCount);

                newNode.Next = Next;
                Next = newNode;
            }
            else
            {
                Array.Copy(Children, Tree._minKeyCount, newNode.Children, 0, Tree._minKeyCount);
                for (var i = 0; i < Tree._minKeyCount; i++)
                {
                    newNode.Children[i].Parent = newNode;
                    newNode.Children[i].ChildIndex = i;
                }
                Array.Clear(Children, Tree._minKeyCount, Tree._minKeyCount);
            }

            newNode.KeyCount = Tree._minKeyCount;
            KeyCount -= Tree._minKeyCount;
            return newNode;
        }
    }

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly BPlusTreeDictionary<TKey, TValue> _dictionary;
        private readonly int _version;
        private BPlusTreeNode? _node;
        private KeyValuePair<TKey, TValue> _current;
        private int _index;

        internal Enumerator(BPlusTreeDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _version = dictionary._version;

            _node = _dictionary._firstLeaf;
            _index = -1;
        }

        public bool MoveNext()
        {
            Check.VersionEqual(_version, _dictionary._version);

            if (_node is null)
            {
                _current = default;
                return false;
            }

            if (++_index < _node.KeyCount)
                goto set_current;

            _node = _node.Next;
            if (_node is null)
            {
                _current = default;
                return false;
            }

            _index = 0; // move to the next node and reset index to 0

        set_current:
            _current = KeyValuePair.Create(_node.Keys[_index], _node.Values[_index]);
            return true;
        }

        public readonly KeyValuePair<TKey, TValue> Current => _current;
        readonly object IEnumerator.Current => Current;

        public void Reset()
        {
            Check.VersionEqual(_version, _dictionary._version);

            _node = _dictionary._firstLeaf;
            _index = -1;
        }

        public readonly void Dispose() { }
    }

    [DebuggerDisplay("Count = {Count}")]
    public sealed class KeyCollection : ReadOnlyItemCollection<TKey, KeyCollection.KeyEnumerator>
    {
        private readonly BPlusTreeDictionary<TKey, TValue> _dictionary;

        public KeyCollection(BPlusTreeDictionary<TKey, TValue> dictionary)
        {
            _dictionary = Check.NotNull(dictionary);
        }

        public override int Count => _dictionary.Count;
        public override KeyEnumerator GetEnumerator() => new(_dictionary);
        public override bool Contains(TKey item) => _dictionary.ContainsKey(item);

        public override void CopyTo(TKey[] array, int arrayIndex)
        {
            Check.CanCopyTo(array, arrayIndex, _dictionary.Count);

            foreach (var (key, _) in _dictionary)
                array[arrayIndex++] = key;
        }

        public struct KeyEnumerator : IEnumerator<TKey>
        {
            private readonly BPlusTreeDictionary<TKey, TValue> _dictionary;
            private readonly int _version;
            private Enumerator _e;

            internal KeyEnumerator(BPlusTreeDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _version = dictionary._version;
                _e = dictionary.GetEnumerator();
            }

            public readonly TKey Current => _e.Current.Key;
            readonly object IEnumerator.Current => Current;
            public bool MoveNext() => _e.MoveNext();
            public readonly void Dispose() => _e.Dispose();

            public void Reset()
            {
                Check.VersionEqual(_version, _dictionary._version);
                _e = _dictionary.GetEnumerator();
            }
        }
    }

    [DebuggerDisplay("Count = {Count}")]
    public sealed class ValueCollection : ReadOnlyItemCollection<TValue, ValueCollection.ValueEnumerator>
    {
        private readonly BPlusTreeDictionary<TKey, TValue> _dictionary;

        public ValueCollection(BPlusTreeDictionary<TKey, TValue> dictionary)
        {
            _dictionary = Check.NotNull(dictionary);
        }

        public override int Count => _dictionary.Count;
        public override ValueEnumerator GetEnumerator() => new(_dictionary);
        public override bool Contains(TValue item) => _dictionary.ContainsValue(item);

        public override void CopyTo(TValue[] array, int arrayIndex)
        {
            Check.CanCopyTo(array, arrayIndex, _dictionary.Count);

            foreach (var (_, value) in _dictionary)
                array[arrayIndex++] = value;
        }

        public struct ValueEnumerator : IEnumerator<TValue>
        {
            private readonly BPlusTreeDictionary<TKey, TValue> _dictionary;
            private readonly int _version;
            private Enumerator _e;

            internal ValueEnumerator(BPlusTreeDictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _version = dictionary._version;
                _e = dictionary.GetEnumerator();
            }

            public readonly TValue Current => _e.Current.Value;
            readonly object? IEnumerator.Current => Current;
            public bool MoveNext() => _e.MoveNext();
            public readonly void Dispose() => _e.Dispose();

            public void Reset()
            {
                Check.VersionEqual(_version, _dictionary._version);
                _e = _dictionary.GetEnumerator();
            }
        }
    }
}