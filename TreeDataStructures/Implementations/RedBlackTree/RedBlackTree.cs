using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new RbNode<TKey, TValue>(key, value) { Color = RbColor.Red };
    }

    private static bool IsRed(RbNode<TKey, TValue>? node)
        => node != null && node.Color == RbColor.Red;

    private static bool IsBlack(RbNode<TKey, TValue>? node)
        => node == null || node.Color == RbColor.Black;

    private void FixInsert(RbNode<TKey, TValue> node)
    {
        while (node.Parent != null && node.Parent.Color == RbColor.Red)
        {
            if (node.Parent == node.Parent.Parent?.Left)
            {
                var uncle = node.Parent.Parent.Right;
                if (IsRed(uncle))
                {
                    node.Parent.Color = RbColor.Black;
                    if (uncle != null) uncle.Color = RbColor.Black;
                    node.Parent.Parent.Color = RbColor.Red;
                    node = node.Parent.Parent;
                }
                else
                {
                    if (node == node.Parent.Right)
                    {
                        node = node.Parent;
                        RotateLeft(node);
                    }
                    node.Parent!.Color = RbColor.Black;
                    node.Parent.Parent!.Color = RbColor.Red;
                    RotateRight(node.Parent.Parent);
                }
            }
            else
            {
                var uncle = node.Parent.Parent?.Left;
                if (IsRed(uncle))
                {
                    node.Parent.Color = RbColor.Black;
                    if (uncle != null) uncle.Color = RbColor.Black;
                    node.Parent.Parent!.Color = RbColor.Red;
                    node = node.Parent.Parent;
                }
                else
                {
                    if (node == node.Parent.Left)
                    {
                        node = node.Parent;
                        RotateRight(node);
                    }
                    node.Parent!.Color = RbColor.Black;
                    node.Parent.Parent!.Color = RbColor.Red;
                    RotateLeft(node.Parent.Parent);
                }
            }
        }
        Root!.Color = RbColor.Black;
    }

    private void FixDelete(RbNode<TKey, TValue>? replacement, RbNode<TKey, TValue>? parent)
    {
        while (IsBlack(replacement) && replacement != Root)
        {
            if (replacement == parent?.Left)
            {
                var sibling = parent!.Right;
                if (IsRed(sibling))
                {
                    sibling!.Color = RbColor.Black;
                    parent.Color = RbColor.Red;
                    RotateLeft(parent);
                    sibling = parent.Right;
                }
                if (IsBlack(sibling?.Left) && IsBlack(sibling?.Right))
                {
                    if (sibling != null) sibling.Color = RbColor.Red;
                    replacement = parent;
                    parent = replacement.Parent;
                }
                else
                {
                    if (IsBlack(sibling?.Right))
                    {
                        if (sibling?.Left != null) sibling.Left.Color = RbColor.Black;
                        if (sibling != null) sibling.Color = RbColor.Red;
                        RotateRight(sibling!);
                        sibling = parent.Right;
                    }
                    if (sibling != null)
                    {
                        sibling.Color = parent.Color;
                        if (sibling.Right != null) sibling.Right.Color = RbColor.Black;
                    }
                    parent.Color = RbColor.Black;
                    RotateLeft(parent);
                    replacement = Root;
                }
            }
            else
            {
                var sibling = parent?.Left;
                if (IsRed(sibling))
                {
                    sibling!.Color = RbColor.Black;
                    parent!.Color = RbColor.Red;
                    RotateRight(parent);
                    sibling = parent?.Left;
                }
                if (IsBlack(sibling?.Left) && IsBlack(sibling?.Right))
                {
                    if (sibling != null) sibling.Color = RbColor.Red;
                    replacement = parent;
                    parent = replacement?.Parent;
                }
                else
                {
                    if (IsBlack(sibling?.Left))
                    {
                        if (sibling?.Right != null) sibling.Right.Color = RbColor.Black;
                        if (sibling != null) sibling.Color = RbColor.Red;
                        RotateLeft(sibling!);
                        sibling = parent?.Left;
                    }
                    if (sibling != null)
                    {
                        sibling.Color = parent!.Color;
                        if (sibling.Left != null) sibling.Left.Color = RbColor.Black;
                    }
                    if (parent != null) parent.Color = RbColor.Black;
                    RotateRight(parent!);
                    replacement = Root;
                }
            }
        }
        if (replacement != null)
            replacement.Color = RbColor.Black;
    }

    protected override void RemoveNode(RbNode<TKey, TValue> node)
    {
        RbNode<TKey, TValue>? replacement;
        RbNode<TKey, TValue>? child;
        RbNode<TKey, TValue>? parent;
        RbColor originalColor = node.Color;

        if (node.Left == null)
        {
            replacement = node.Right;
            parent = node.Parent;
            child = node.Right;
            Transplant(node, node.Right);
        }
        else if (node.Right == null)
        {
            replacement = node.Left;
            parent = node.Parent;
            child = node.Left;
            Transplant(node, node.Left);
        }
        else
        {
            var successor = node.Right;
            while (successor.Left != null)
                successor = successor.Left;

            originalColor = successor.Color;
            child = successor.Right;
            parent = successor.Parent;

            if (successor.Parent == node)
            {
                parent = successor;
                if (child != null)
                    child.Parent = successor;
            }
            else
            {
                Transplant(successor, successor.Right);
                successor.Right = node.Right;
                if (successor.Right != null)
                    successor.Right.Parent = successor;
                parent = successor.Parent;
            }

            Transplant(node, successor);
            successor.Left = node.Left;
            if (successor.Left != null)
                successor.Left.Parent = successor;
            successor.Color = node.Color;
            replacement = successor;
        }

        if (originalColor == RbColor.Black)
            FixDelete(child, parent);

        OnNodeRemoved(parent, child);
    }

    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        FixInsert(newNode);
    }

    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        
    }
}