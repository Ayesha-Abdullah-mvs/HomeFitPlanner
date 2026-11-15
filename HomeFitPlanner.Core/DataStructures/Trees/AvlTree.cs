using System;
using System.Collections.Generic;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.DataStructures.Trees;

public class AvlNode<TKey, TValue> where TKey : IComparable<TKey>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
    public int Height { get; set; }
    public AvlNode<TKey, TValue>? Left { get; set; }
    public AvlNode<TKey, TValue>? Right { get; set; }

    public AvlNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
        Height = 1;
    }
}

/// <summary>
/// Hand-written self-balancing AVL Tree maintaining O(log N) operations.
/// Supports 4 rotation cases (LL, RR, LR, RL) and balance factor calculation.
/// </summary>
public class AvlTree<TKey, TValue> where TKey : IComparable<TKey>
{
    public AvlNode<TKey, TValue>? Root { get; private set; }
    public int Count { get; private set; }

    public AvlTree()
    {
        Root = null;
        Count = 0;
    }

    public int GetHeight() => NodeHeight(Root);

    private int NodeHeight(AvlNode<TKey, TValue>? node) => node?.Height ?? 0;

    private int GetBalance(AvlNode<TKey, TValue>? node)
    {
        if (node == null) return 0;
        return NodeHeight(node.Left) - NodeHeight(node.Right);
    }

    private void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        int hl = NodeHeight(node.Left);
        int hr = NodeHeight(node.Right);
        node.Height = 1 + (hl > hr ? hl : hr);
    }

    private AvlNode<TKey, TValue> RotateRight(AvlNode<TKey, TValue> y)
    {
        var x = y.Left!;
        var t2 = x.Right;

        x.Right = y;
        y.Left = t2;

        UpdateHeight(y);
        UpdateHeight(x);

        return x;
    }

    private AvlNode<TKey, TValue> RotateLeft(AvlNode<TKey, TValue> x)
    {
        var y = x.Right!;
        var t2 = y.Left;

        y.Left = x;
        x.Right = t2;

        UpdateHeight(x);
        UpdateHeight(y);

        return y;
    }

    public void Insert(TKey key, TValue value)
    {
        Root = InsertRec(Root, key, value);
        Count++;
    }

    private AvlNode<TKey, TValue> InsertRec(AvlNode<TKey, TValue>? node, TKey key, TValue value)
    {
        if (node == null)
        {
            return new AvlNode<TKey, TValue>(key, value);
        }

        int cmp = key.CompareTo(node.Key);
        if (cmp < 0)
        {
            node.Left = InsertRec(node.Left, key, value);
        }
        else
        {
            node.Right = InsertRec(node.Right, key, value);
        }

        UpdateHeight(node);
        int balance = GetBalance(node);

        // Case 1: Left Left (LL)
        if (balance > 1 && key.CompareTo(node.Left!.Key) < 0)
        {
            return RotateRight(node);
        }

        // Case 2: Right Right (RR)
        if (balance < -1 && key.CompareTo(node.Right!.Key) >= 0)
        {
            return RotateLeft(node);
        }

        // Case 3: Left Right (LR)
        if (balance > 1 && key.CompareTo(node.Left!.Key) >= 0)
        {
            node.Left = RotateLeft(node.Left);
            return RotateRight(node);
        }

        // Case 4: Right Left (RL)
        if (balance < -1 && key.CompareTo(node.Right!.Key) < 0)
        {
            node.Right = RotateRight(node.Right);
            return RotateLeft(node);
        }

        return node;
    }

    public CustomArray<TValue> Search(TKey key)
    {
        var result = new CustomArray<TValue>();
        SearchAllRec(Root, key, result);
        return result;
    }

    private void SearchAllRec(AvlNode<TKey, TValue>? node, TKey key, CustomArray<TValue> result)
    {
        if (node == null) return;

        int cmp = key.CompareTo(node.Key);
        if (cmp == 0)
        {
            result.Add(node.Value);
        }

        if (cmp <= 0)
        {
            SearchAllRec(node.Left, key, result);
        }
        if (cmp >= 0)
        {
            SearchAllRec(node.Right, key, result);
        }
    }

    public CustomArray<TValue> InOrderTraversal()
    {
        var list = new CustomArray<TValue>();
        InOrderRec(Root, list);
        return list;
    }

    private void InOrderRec(AvlNode<TKey, TValue>? node, CustomArray<TValue> list)
    {
        if (node == null) return;
        InOrderRec(node.Left, list);
        list.Add(node.Value);
        InOrderRec(node.Right, list);
    }

    public CustomArray<TValue> GetRange(TKey minKey, TKey maxKey)
    {
        var list = new CustomArray<TValue>();
        RangeRec(Root, minKey, maxKey, list);
        return list;
    }

    private void RangeRec(AvlNode<TKey, TValue>? node, TKey min, TKey max, CustomArray<TValue> list)
    {
        if (node == null) return;

        if (node.Key.CompareTo(min) > 0)
        {
            RangeRec(node.Left, min, max, list);
        }

        if (node.Key.CompareTo(min) >= 0 && node.Key.CompareTo(max) <= 0)
        {
            list.Add(node.Value);
        }

        if (node.Key.CompareTo(max) < 0)
        {
            RangeRec(node.Right, min, max, list);
        }
    }

    public bool Delete(TKey key, Func<TValue, bool>? predicate = null)
    {
        int initialCount = Count;
        Root = DeleteRec(Root, key, predicate);
        return Count < initialCount;
    }

    private AvlNode<TKey, TValue>? DeleteRec(AvlNode<TKey, TValue>? node, TKey key, Func<TValue, bool>? predicate)
    {
        if (node == null) return null;

        int cmp = key.CompareTo(node.Key);
        if (cmp < 0)
        {
            node.Left = DeleteRec(node.Left, key, predicate);
        }
        else if (cmp > 0)
        {
            node.Right = DeleteRec(node.Right, key, predicate);
        }
        else
        {
            // Found matching key; if predicate provided, verify match
            if (predicate != null && !predicate(node.Value))
            {
                node.Right = DeleteRec(node.Right, key, predicate);
                return node;
            }

            Count--;
            // Case 1: Leaf or single child
            if (node.Left == null) return node.Right;
            if (node.Right == null) return node.Left;

            // Case 2: Two children -> find in-order successor (min in right subtree)
            var successor = FindMin(node.Right);
            node.Key = successor.Key;
            node.Value = successor.Value;
            node.Right = DeleteRec(node.Right, successor.Key, v => EqualityComparer<TValue>.Default.Equals(v, successor.Value));
            Count++; // Adjustment because recursive call decremented Count
        }

        if (node == null) return null;

        UpdateHeight(node);
        int balance = GetBalance(node);

        // Case 1: Left Left (LL)
        if (balance > 1 && GetBalance(node.Left) >= 0)
        {
            return RotateRight(node);
        }

        // Case 2: Left Right (LR)
        if (balance > 1 && GetBalance(node.Left) < 0)
        {
            node.Left = RotateLeft(node.Left!);
            return RotateRight(node);
        }

        // Case 3: Right Right (RR)
        if (balance < -1 && GetBalance(node.Right) <= 0)
        {
            return RotateLeft(node);
        }

        // Case 4: Right Left (RL)
        if (balance < -1 && GetBalance(node.Right) > 0)
        {
            node.Right = RotateRight(node.Right!);
            return RotateLeft(node);
        }

        return node;
    }

    private AvlNode<TKey, TValue> FindMin(AvlNode<TKey, TValue> node)
    {
        var current = node;
        while (current.Left != null)
        {
            current = current.Left;
        }
        return current;
    }

    public void Clear()
    {
        Root = null;
        Count = 0;
    }
}

