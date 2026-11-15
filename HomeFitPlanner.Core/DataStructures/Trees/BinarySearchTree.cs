using System;
using System.Collections.Generic;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.DataStructures.Trees;

public class BstNode<TKey, TValue> where TKey : IComparable<TKey>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
    public BstNode<TKey, TValue>? Left { get; set; }
    public BstNode<TKey, TValue>? Right { get; set; }

    public BstNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

/// <summary>
/// Hand-written Binary Search Tree (BST) for storing and retrieving workout plans by duration.
/// Supports standard traversals, range search [min, max], and height calculation.
/// </summary>
public class BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
{
    public BstNode<TKey, TValue>? Root { get; private set; }
    public int Count { get; private set; }

    public BinarySearchTree()
    {
        Root = null;
        Count = 0;
    }

    public void Insert(TKey key, TValue value)
    {
        Root = InsertRec(Root, key, value);
        Count++;
    }

    private BstNode<TKey, TValue> InsertRec(BstNode<TKey, TValue>? node, TKey key, TValue value)
    {
        if (node == null)
        {
            return new BstNode<TKey, TValue>(key, value);
        }

        int cmp = key.CompareTo(node.Key);
        if (cmp < 0)
        {
            node.Left = InsertRec(node.Left, key, value);
        }
        else
        {
            // If duplicate or greater, place in right subtree
            node.Right = InsertRec(node.Right, key, value);
        }
        return node;
    }

    public bool Contains(TKey key)
    {
        return FindNode(Root, key) != null;
    }

    public CustomArray<TValue> Search(TKey key)
    {
        var result = new CustomArray<TValue>();
        SearchAllRec(Root, key, result);
        return result;
    }

    private void SearchAllRec(BstNode<TKey, TValue>? node, TKey key, CustomArray<TValue> result)
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

    private BstNode<TKey, TValue>? FindNode(BstNode<TKey, TValue>? node, TKey key)
    {
        if (node == null) return null;

        int cmp = key.CompareTo(node.Key);
        if (cmp == 0) return node;
        return cmp < 0 ? FindNode(node.Left, key) : FindNode(node.Right, key);
    }

    public bool Delete(TKey key, Func<TValue, bool>? predicate = null)
    {
        int initialCount = Count;
        Root = DeleteRec(Root, key, predicate);
        return Count < initialCount;
    }

    private BstNode<TKey, TValue>? DeleteRec(BstNode<TKey, TValue>? node, TKey key, Func<TValue, bool>? predicate)
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
                // Continue searching right for duplicate keys
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
            // Delete successor from right subtree
            node.Right = DeleteRec(node.Right, successor.Key, v => EqualityComparer<TValue>.Default.Equals(v, successor.Value));
            Count++; // Adjustment because recursive call decremented Count
        }
        return node;
    }

    private BstNode<TKey, TValue> FindMin(BstNode<TKey, TValue> node)
    {
        var current = node;
        while (current.Left != null)
        {
            current = current.Left;
        }
        return current;
    }

    public CustomArray<TValue> InOrderTraversal()
    {
        var list = new CustomArray<TValue>();
        InOrderRec(Root, list);
        return list;
    }

    private void InOrderRec(BstNode<TKey, TValue>? node, CustomArray<TValue> list)
    {
        if (node == null) return;
        InOrderRec(node.Left, list);
        list.Add(node.Value);
        InOrderRec(node.Right, list);
    }

    public CustomArray<TValue> PreOrderTraversal()
    {
        var list = new CustomArray<TValue>();
        PreOrderRec(Root, list);
        return list;
    }

    private void PreOrderRec(BstNode<TKey, TValue>? node, CustomArray<TValue> list)
    {
        if (node == null) return;
        list.Add(node.Value);
        PreOrderRec(node.Left, list);
        PreOrderRec(node.Right, list);
    }

    public CustomArray<TValue> PostOrderTraversal()
    {
        var list = new CustomArray<TValue>();
        PostOrderRec(Root, list);
        return list;
    }

    private void PostOrderRec(BstNode<TKey, TValue>? node, CustomArray<TValue> list)
    {
        if (node == null) return;
        PostOrderRec(node.Left, list);
        PostOrderRec(node.Right, list);
        list.Add(node.Value);
    }

    /// <summary>
    /// Returns all values whose keys fall in range [minKey, maxKey] in sorted order.
    /// </summary>
    public CustomArray<TValue> GetRange(TKey minKey, TKey maxKey)
    {
        var list = new CustomArray<TValue>();
        RangeRec(Root, minKey, maxKey, list);
        return list;
    }

    private void RangeRec(BstNode<TKey, TValue>? node, TKey min, TKey max, CustomArray<TValue> list)
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

    public int GetHeight() => CalculateHeight(Root);

    private int CalculateHeight(BstNode<TKey, TValue>? node)
    {
        if (node == null) return 0;
        int left = CalculateHeight(node.Left);
        int right = CalculateHeight(node.Right);
        return 1 + (left > right ? left : right);
    }

    public void Clear()
    {
        Root = null;
        Count = 0;
    }
}

