using System;
using System.Collections;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.LinkedLists;

public class SinglyNode<T>
{
    public T Value { get; set; }
    public SinglyNode<T>? Next { get; set; }

    public SinglyNode(T value, SinglyNode<T>? next = null)
    {
        Value = value;
        Next = next;
    }
}

/// <summary>
/// Hand-written singly linked list with O(1) head insertion, traversal, and node removal.
/// </summary>
public class SinglyLinkedList<T> : IEnumerable<T>
{
    public SinglyNode<T>? Head { get; private set; }
    public SinglyNode<T>? Tail { get; private set; }
    public int Count { get; private set; }

    public SinglyLinkedList()
    {
        Head = null;
        Tail = null;
        Count = 0;
    }

    public void AddFirst(T item)
    {
        var node = new SinglyNode<T>(item, Head);
        Head = node;
        if (Tail == null)
        {
            Tail = node;
        }
        Count++;
    }

    public void AddLast(T item)
    {
        var node = new SinglyNode<T>(item);
        if (Tail == null)
        {
            Head = node;
            Tail = node;
        }
        else
        {
            Tail.Next = node;
            Tail = node;
        }
        Count++;
    }

    public bool Remove(T item)
    {
        if (Head == null) return false;

        var comparer = EqualityComparer<T>.Default;
        if (comparer.Equals(Head.Value, item))
        {
            Head = Head.Next;
            if (Head == null) Tail = null;
            Count--;
            return true;
        }

        var current = Head;
        while (current.Next != null)
        {
            if (comparer.Equals(current.Next.Value, item))
            {
                if (current.Next == Tail)
                {
                    Tail = current;
                }
                current.Next = current.Next.Next;
                Count--;
                return true;
            }
            current = current.Next;
        }

        return false;
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

