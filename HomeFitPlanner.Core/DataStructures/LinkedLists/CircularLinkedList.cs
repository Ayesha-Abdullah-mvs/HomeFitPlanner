using System;
using System.Collections;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.LinkedLists;

public class CircularNode<T>
{
    public T Value { get; set; }
    public CircularNode<T>? Next { get; set; }

    public CircularNode(T value)
    {
        Value = value;
    }
}

/// <summary>
/// Hand-written circular linked list for cycling through repeating schedules
/// (such as Monday -> Sunday -> Monday) and circular interval rotations.
/// </summary>
public class CircularLinkedList<T> : IEnumerable<T>
{
    public CircularNode<T>? Head { get; private set; }
    public CircularNode<T>? Tail { get; private set; }
    public CircularNode<T>? Current { get; private set; }
    public int Count { get; private set; }

    public CircularLinkedList()
    {
        Head = null;
        Tail = null;
        Current = null;
        Count = 0;
    }

    public void AddLast(T item)
    {
        var node = new CircularNode<T>(item);
        if (Head == null)
        {
            Head = node;
            Tail = node;
            node.Next = Head;
            Current = Head;
        }
        else
        {
            Tail!.Next = node;
            node.Next = Head;
            Tail = node;
        }
        Count++;
    }

    public T? StepNext()
    {
        if (Current == null) return default;
        Current = Current.Next;
        return Current != null ? Current.Value : default;
    }

    public void ResetToHead()
    {
        Current = Head;
    }

    public T[] ToArray()
    {
        if (Head == null || Count == 0) return Array.Empty<T>();

        var result = new T[Count];
        var temp = Head;
        for (int i = 0; i < Count; i++)
        {
            result[i] = temp.Value;
            temp = temp.Next!;
        }
        return result;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (Head == null || Count == 0) yield break;

        var temp = Head;
        for (int i = 0; i < Count; i++)
        {
            yield return temp.Value;
            temp = temp.Next!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

