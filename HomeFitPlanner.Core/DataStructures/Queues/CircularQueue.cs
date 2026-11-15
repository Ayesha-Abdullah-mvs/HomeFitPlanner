using System;
using System.Collections;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.Queues;

/// <summary>
/// Hand-written circular queue (ring buffer) using fixed capacity and modulo arithmetic.
/// Ideal for cyclical interval sets and bounded buffer streams.
/// </summary>
public class CircularQueue<T> : IEnumerable<T>
{
    private readonly T[] _items;
    private int _front;
    private int _rear;
    private int _count;
    private readonly int _capacity;

    public CircularQueue(int capacity = 16)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity;
        _items = new T[capacity];
        _front = 0;
        _rear = -1;
        _count = 0;
    }

    public int Count => _count;
    public int Capacity => _capacity;
    public bool IsEmpty => _count == 0;
    public bool IsFull => _count == _capacity;

    public bool Enqueue(T item)
    {
        if (IsFull) return false;

        _rear = (_rear + 1) % _capacity;
        _items[_rear] = item;
        _count++;
        return true;
    }

    public T Dequeue()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Circular queue is empty.");

        T item = _items[_front];
        _items[_front] = default!;
        _front = (_front + 1) % _capacity;
        _count--;
        return item;
    }

    public T Peek()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Circular queue is empty.");

        return _items[_front];
    }

    public void Clear()
    {
        for (int i = 0; i < _capacity; i++)
        {
            _items[i] = default!;
        }
        _front = 0;
        _rear = -1;
        _count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            yield return _items[(_front + i) % _capacity];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

