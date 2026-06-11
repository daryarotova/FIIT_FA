using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
            return (null, null);

        int comparison = Comparer.Compare(key, root.Key);
        if (comparison < 0)
        {
            var (leftSub, rightSub) = Split(root.Left, key);
            root.Left = rightSub;
            if (rightSub != null)
                rightSub.Parent = root;
            return (leftSub, root);
        }
        else
        {
            var (leftSub, rightSub) = Split(root.Right, key);
            root.Right = leftSub;
            if (leftSub != null)
                leftSub.Parent = root;
            return (root, rightSub);
        }
    }

    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null)
            return right;
        if (right == null)
            return left;

        if (left.Priority > right.Priority)
        {
            left.Right = Merge(left.Right, right);
            if (left.Right != null)
                left.Right.Parent = left;
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);
            if (right.Left != null)
                right.Left.Parent = right;
            return right;
        }
    }

    public override void Add(TKey key, TValue value)
    {
        var existingNode = FindNode(key);
        if (existingNode != null)
        {
            existingNode.Value = value;
            return;
        }

        var (left, right) = Split(Root, key);
        var newNode = CreateNode(key, value);
        var merged = Merge(Merge(left, newNode), right);
        Root = merged;
        if (Root != null)
            Root.Parent = null;

        Count++;
        OnNodeAdded(newNode);
    }

    public override bool Remove(TKey key)
    {
        var node = FindNode(key);
        if (node == null)
            return false;

        var mergedChildren = Merge(node.Left, node.Right);
        Transplant(node, mergedChildren);
        Count--;
        OnNodeRemoved(node.Parent, mergedChildren);
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new TreapNode<TKey, TValue>(key, value);
    }

    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {

    }

    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
        
    }
}