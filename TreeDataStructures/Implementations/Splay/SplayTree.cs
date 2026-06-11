using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    private void Splay(BstNode<TKey, TValue>? node)
    {
        if (node == null)
            return;

        while (node.Parent != null)
        {
            var ancestor = node.Parent;
            var grandAncestor = ancestor.Parent;

            if (grandAncestor == null)
            {
                if (node.IsLeftChild)
                    RotateRight(ancestor);
                else
                    RotateLeft(ancestor);
            }
            else if (node.IsLeftChild && ancestor.IsLeftChild)
            {
                RotateRight(grandAncestor);
                RotateRight(ancestor);
            }
            else if (node.IsRightChild && ancestor.IsRightChild)
            {
                RotateLeft(grandAncestor);
                RotateLeft(ancestor);
            }
            else if (node.IsRightChild && ancestor.IsLeftChild)
            {
                RotateLeft(ancestor);
                RotateRight(grandAncestor);
            }
            else
            {
                RotateRight(ancestor);
                RotateLeft(grandAncestor);
            }
        }

        Root = node;
        node.Parent = null;
    }

    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }

    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (parent != null)
            Splay(parent);
        else if (child != null)
            Splay(child);
    }

    public override bool ContainsKey(TKey key)
    {
        var node = FindNode(key);
        if (node != null)
        {
            Splay(node);
            return true;
        }
        return false;
    }

    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            Splay(node);
            return true;
        }

        if (Root != null)
        {
            var current = Root;
            while (current != null)
            {
                int comparison = Comparer.Compare(key, current.Key);
                if (comparison < 0)
                {
                    if (current.Left == null)
                        break;
                    current = current.Left;
                }
                else if (comparison > 0)
                {
                    if (current.Right == null)
                        break;
                    current = current.Right;
                }
                else
                {
                    break;
                }
            }
            Splay(current);
        }

        value = default;
        return false;
    }
}