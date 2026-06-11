using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public enum TraversalStrategy
{
    InOrder,
    PreOrder,
    PostOrder,
    InOrderReverse,
    PreOrderReverse,
    PostOrderReverse
}

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null)
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default;
    public int Count { get; protected set; }
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>();
            var iterator = new TreeIterator(Root, TraversalStrategy.InOrder, GetNodeHeight);
            while (iterator.MoveNext())
                keys.Add(iterator.Current.Key);
            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>();
            var iterator = new TreeIterator(Root, TraversalStrategy.InOrder, GetNodeHeight);
            while (iterator.MoveNext())
                values.Add(iterator.Current.Value);
            return values;
        }
    }

    public virtual void Add(TKey key, TValue value)
    {
        var newNode = CreateNode(key, value);
        if (Root == null)
        {
            Root = newNode;
            Count++;
            OnNodeAdded(newNode);
            return;
        }

        var current = Root;
        TNode? parent = null;
        int comparison = 0;

        while (current != null)
        {
            parent = current;
            comparison = Comparer.Compare(key, current.Key);
            if (comparison == 0)
            {
                current.Value = value;
                return;
            }
            current = comparison < 0 ? current.Left : current.Right;
        }

        newNode.Parent = parent;
        if (comparison < 0)
            parent!.Left = newNode;
        else
            parent!.Right = newNode;

        Count++;
        OnNodeAdded(newNode);
    }

    public virtual bool Remove(TKey key)
    {
        var node = FindNode(key);
        if (node == null) return false;

        RemoveNode(node);
        Count--;
        return true;
    }

    protected virtual void RemoveNode(TNode node)
    {
        TNode? parent = node.Parent;
        TNode? child = null;

        if (node.Left == null)
        {
            child = node.Right;
            Transplant(node, node.Right);
        }
        else if (node.Right == null)
        {
            child = node.Left;
            Transplant(node, node.Left);
        }
        else
        {
            var successor = node.Right;
            while (successor.Left != null)
                successor = successor.Left;

            if (successor.Parent != node)
            {
                Transplant(successor, successor.Right);
                successor.Right = node.Right;
                if (successor.Right != null)
                    successor.Right.Parent = successor;
            }

            Transplant(node, successor);
            successor.Left = node.Left;
            if (successor.Left != null)
                successor.Left.Parent = successor;

            child = successor;
        }

        OnNodeRemoved(parent, child);
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;

    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out var val) ? val! : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    #region Hooks
    
    protected virtual void OnNodeAdded(TNode newNode) { }
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }

    #endregion

    #region Helpers

    protected abstract TNode CreateNode(TKey key, TValue value);

    protected TNode? FindNode(TKey key)
    {
        var current = Root;
        while (current != null)
        {
            var comparison = Comparer.Compare(key, current.Key);
            if (comparison == 0) return current;
            current = comparison < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected void RotateLeft(TNode x)
    {
        if (x?.Right == null) return;

        var y = x.Right;
        x.Right = y.Left;
        if (y.Left != null) y.Left.Parent = x;

        y.Parent = x.Parent;
        if (x.Parent == null)
            Root = y;
        else if (x.IsLeftChild)
            x.Parent.Left = y;
        else
            x.Parent.Right = y;

        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        if (y?.Left == null) return;

        var x = y.Left;
        y.Left = x.Right;
        if (x.Right != null) x.Right.Parent = y;

        x.Parent = y.Parent;
        if (y.Parent == null)
            Root = x;
        else if (y.IsLeftChild)
            y.Parent.Left = x;
        else
            y.Parent.Right = x;

        x.Right = y;
        y.Parent = x;
    }

    protected void RotateBigLeft(TNode x)
    {
        RotateRight(x.Right!);
        RotateLeft(x);
    }

    protected void RotateBigRight(TNode y)
    {
        RotateLeft(y.Left!);
        RotateRight(y);
    }

    protected void RotateDoubleLeft(TNode x)
    {
        RotateLeft(x);
        RotateLeft(x);
    }

    protected void RotateDoubleRight(TNode y)
    {
        RotateRight(y);
        RotateRight(y);
    }

    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
            Root = v;
        else if (u.IsLeftChild)
            u.Parent.Left = v;
        else
            u.Parent.Right = v;

        if (v != null) v.Parent = u.Parent;
    }

    protected virtual int GetNodeHeight(TNode node) => 0;

    #endregion

    #region Traversals & Iterators

    public IEnumerable<TreeEntry<TKey, TValue>> InOrder() =>
        new TreeIterator(Root, TraversalStrategy.InOrder, GetNodeHeight);

    public IEnumerable<TreeEntry<TKey, TValue>> PreOrder() =>
        new TreeIterator(Root, TraversalStrategy.PreOrder, GetNodeHeight);

    public IEnumerable<TreeEntry<TKey, TValue>> PostOrder() =>
        new TreeIterator(Root, TraversalStrategy.PostOrder, GetNodeHeight);

    public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse() =>
        new TreeIterator(Root, TraversalStrategy.InOrderReverse, GetNodeHeight);

    public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse() =>
        new TreeIterator(Root, TraversalStrategy.PreOrderReverse, GetNodeHeight);

    public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse() =>
        new TreeIterator(Root, TraversalStrategy.PostOrderReverse, GetNodeHeight);

    private struct TreeIterator : IEnumerable<TreeEntry<TKey, TValue>>, IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TNode? _root;
        private readonly TraversalStrategy _strategy;
        private readonly Func<TNode, int> _heightProvider;
        private Stack<(TNode Node, bool Visited)> _stack;
        private TreeEntry<TKey, TValue> _currentEntry;
        private bool _hasCurrent;

        public TreeIterator(TNode? root, TraversalStrategy strategy, Func<TNode, int> heightProvider)
        {
            _root = root;
            _strategy = strategy;
            _heightProvider = heightProvider;
            _stack = new Stack<(TNode, bool)>();
            _currentEntry = default!;
            _hasCurrent = false;
            Reset();
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        public TreeEntry<TKey, TValue> Current => _hasCurrent ? _currentEntry : throw new InvalidOperationException();
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            while (_stack.Count > 0)
            {
                var (node, visited) = _stack.Pop();
                if (node == null) continue;

                switch (_strategy)
                {
                    case TraversalStrategy.PreOrder:
                        if (!visited)
                        {
                            _stack.Push((node.Right!, false));
                            _stack.Push((node.Left!, false));
                            _currentEntry = CreateEntry(node);
                            _hasCurrent = true;
                            return true;
                        }
                        break;

                    case TraversalStrategy.InOrder:
                        if (!visited)
                        {
                            _stack.Push((node.Right!, false));
                            _stack.Push((node, true));
                            _stack.Push((node.Left!, false));
                        }
                        else
                        {
                            _currentEntry = CreateEntry(node);
                            _hasCurrent = true;
                            return true;
                        }
                        break;

                    case TraversalStrategy.PostOrder:
                        if (!visited)
                        {
                            _stack.Push((node, true));
                            _stack.Push((node.Right!, false));
                            _stack.Push((node.Left!, false));
                        }
                        else
                        {
                            _currentEntry = CreateEntry(node);
                            _hasCurrent = true;
                            return true;
                        }
                        break;

                    case TraversalStrategy.PreOrderReverse:
                        if (!visited)
                        {
                            _stack.Push((node.Left!, false));
                            _stack.Push((node.Right!, false));
                            _currentEntry = CreateEntry(node);
                            _hasCurrent = true;
                            return true;
                        }
                        break;

                    case TraversalStrategy.InOrderReverse:
                        if (!visited)
                        {
                            _stack.Push((node.Left!, false));
                            _stack.Push((node, true));
                            _stack.Push((node.Right!, false));
                        }
                        else
                        {
                            _currentEntry = CreateEntry(node);
                            _hasCurrent = true;
                            return true;
                        }
                        break;

                    case TraversalStrategy.PostOrderReverse:
                        if (!visited)
                        {
                            _stack.Push((node, true));
                            _stack.Push((node.Left!, false));
                            _stack.Push((node.Right!, false));
                        }
                        else
                        {
                            _currentEntry = CreateEntry(node);
                            _hasCurrent = true;
                            return true;
                        }
                        break;
                }
            }

            _hasCurrent = false;
            return false;
        }

        private TreeEntry<TKey, TValue> CreateEntry(TNode node)
        {
            return new TreeEntry<TKey, TValue>(node.Key, node.Value, _heightProvider(node));
        }

        public void Reset()
        {
            _stack.Clear();
            _hasCurrent = false;
            if (_root == null) return;

            _stack.Push((_root, false));
        }

        public void Dispose() => _stack.Clear();
    }

    #endregion

    #region ICollection / IDictionary Implementation

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new EnumeratorWrapper(Root, GetNodeHeight);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class EnumeratorWrapper : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly TreeIterator _iterator;

        public EnumeratorWrapper(TNode? root, Func<TNode, int> heightProvider)
        {
            _iterator = new TreeIterator(root, TraversalStrategy.InOrder, heightProvider);
        }

        public KeyValuePair<TKey, TValue> Current =>
            new(_iterator.Current.Key, _iterator.Current.Value);

        object IEnumerator.Current => Current;

        public bool MoveNext() => _iterator.MoveNext();
        public void Reset() => _iterator.Reset();
        public void Dispose() => _iterator.Dispose();
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count) throw new ArgumentException("Insufficient space");

        var iterator = new TreeIterator(Root, TraversalStrategy.InOrder, GetNodeHeight);
        int index = arrayIndex;
        while (iterator.MoveNext())
        {
            var entry = iterator.Current;
            array[index++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }

    #endregion
}