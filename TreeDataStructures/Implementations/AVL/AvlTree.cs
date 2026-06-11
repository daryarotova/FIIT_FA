using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    private int GetHeight(AvlNode<TKey, TValue>? node)
        => node?.Height ?? 0;

    private void UpdateHeight(AvlNode<TKey, TValue> node)
        => node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

    private int GetBalance(AvlNode<TKey, TValue>? node)
        => node == null ? 0 : GetHeight(node.Left) - GetHeight(node.Right);

    private AvlNode<TKey, TValue> Balance(AvlNode<TKey, TValue> node)
    {
        UpdateHeight(node);
        int balance = GetBalance(node);

        if (balance > 1)
        {
            if (GetBalance(node.Left) < 0)
            {
                RotateLeft(node.Left!);
                UpdateHeight(node.Left!);
            }
            RotateRight(node);
            UpdateHeight(node);
            return node.Parent!;
        }
        else if (balance < -1)
        {
            if (GetBalance(node.Right) > 0)
            {
                RotateRight(node.Right!);
                UpdateHeight(node.Right!);
            }
            RotateLeft(node);
            UpdateHeight(node);
            return node.Parent!;
        }

        return node;
    }

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        var current = newNode;
        while (current != null)
        {
            current = Balance(current);
            current = current.Parent;
        }
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        var current = parent ?? child;
        while (current != null)
        {
            current = Balance(current);
            current = current.Parent;
        }
    }
}