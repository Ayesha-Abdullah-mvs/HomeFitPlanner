using System;
using System.Collections;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.LinkedLists;

public class DoublyNode<T>
{
    public T Value { get; set; }
    public DoublyNode<T>? Prev { get; set; }
    public DoublyNode<T>? Next { get; set; }

    public DoublyNode(T value)
    {
        Value = value;
    }
}

/// <summary>
/// Hand-written doubly linked list for workout session navigation (Next, Previous,
/// O(1) exercise removal on discomfort, inserting rest breaks, and in-place reversal for cool-down).
/// </summary>
public class DoublyLinkedList<T> : IEnumerable<T>
{
    public DoublyNode<T>? Head { get; private set; }
    public DoublyNode<T>? Tail { get; private set; }
    public int Count { get; private set; }

    public DoublyLinkedList()
    {
        Head = null;
        Tail = null;
        Count = 0;
    }

    public DoublyNode<T> AddLast(T item)
    {
        var node = new DoublyNode<T>(item);
        if (Tail == null)
        {
            Head = node;
            Tail = node;
        }
        else
        {
            Tail.Next = node;
            node.Prev = Tail;
            Tail = node;
        }
        Count++;
        return node;
    }

    public DoublyNode<T> AddFirst(T item)
    {
        var node = new DoublyNode<T>(item);
        if (Head == null)
        {
            Head = node;
            Tail = node;
        }
        else
        {
            node.Next = Head;
            Head.Prev = node;
            Head = node;
        }
        Count++;
        return node;
    }

    public DoublyNode<T> InsertAfter(DoublyNode<T> targetNode, T item)
    {
        if (targetNode == null) throw new ArgumentNullException(nameof(targetNode));

        if (targetNode == Tail)
        {
            return AddLast(item);
        }

        var newNode = new DoublyNode<T>(item)
        {
            Prev = targetNode,
            Next = targetNode.Next
        };

        if (targetNode.Next != null)
        {
            targetNode.Next.Prev = newNode;
        }
        targetNode.Next = newNode;

        Count++;
        return newNode;
    }

    public DoublyNode<T> InsertBefore(DoublyNode<T> targetNode, T item)
    {
        if (targetNode == null) throw new ArgumentNullException(nameof(targetNode));

        if (targetNode == Head)
        {
            return AddFirst(item);
        }

        var newNode = new DoublyNode<T>(item)
        {
            Next = targetNode,
            Prev = targetNode.Prev
        };

        if (targetNode.Prev != null)
        {
            targetNode.Prev.Next = newNode;
        }
        targetNode.Prev = newNode;

        Count++;
        return newNode;
    }

    public void RemoveNode(DoublyNode<T> node)
    {
        if (node == null) return;

        if (node.Prev != null)
        {
            node.Prev.Next = node.Next;
        }
        else
        {
            Head = node.Next;
        }

        if (node.Next != null)
        {
            node.Next.Prev = node.Prev;
        }
        else
        {
            Tail = node.Prev;
        }

        node.Prev = null;
        node.Next = null;
        Count--;
    }

    public bool Remove(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        var current = Head;
        while (current != null)
        {
            if (comparer.Equals(current.Value, item))
            {
                RemoveNode(current);
                return true;
            }
            current = current.Next;
        }
        return false;
    }

    public void Reverse()
    {
        if (Head == null || Head == Tail) return;

        var current = Head;
        DoublyNode<T>? temp = null;

        while (current != null)
        {
            temp = current.Prev;
            current.Prev = current.Next;
            current.Next = temp;
            current = current.Prev;
        }

        if (temp != null)
        {
            Tail = Head;
            Head = temp.Prev;
        }
    }

    public void Clear()
    {
        Head = null;
        Tail = null;
        Count = 0;
    }

    public T[] ToArray()
    {
        var array = new T[Count];
        int idx = 0;
        var current = Head;
        while (current != null)
        {
            array[idx++] = current.Value;
            current = current.Next;
        }
        return array;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var current = Head;
        while (current != null)
        {
            yield return current.Value;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

