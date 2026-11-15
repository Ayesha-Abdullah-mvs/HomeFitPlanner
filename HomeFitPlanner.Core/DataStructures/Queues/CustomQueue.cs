using System;
using System.Collections;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.Queues;

/// <summary>
/// Hand-written linear FIFO queue with dynamic resizing.
/// </summary>
public class CustomQueue<T> : IEnumerable<T>
{
    private T[] _items;
    private int _head;
    private int _tail;
    private int _count;
    private const int DefaultCapacity = 8;

    public CustomQueue(int capacity = DefaultCapacity)
    {
        if (capacity <= 0) capacity = DefaultCapacity;
        _items = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;

    public void Enqueue(T item)
    {
        if (_count == _items.Length)
        {
            Resize(_items.Length * 2);
        }

        _items[_tail] = item;
        _tail = (_tail + 1) % _items.Length;
        _count++;
    }

    public T Dequeue()
    {
        if (_count == 0)
            throw new InvalidOperationException("Queue is empty.");

        T item = _items[_head];
        _items[_head] = default!;
        _head = (_head + 1) % _items.Length;
        _count--;
        return item;
    }

    public T Peek()
    {
        if (_count == 0)
            throw new InvalidOperationException("Queue is empty.");

        return _items[_head];
    }

    public void Clear()
    {
        for (int i = 0; i < _items.Length; i++)
        {
            _items[i] = default!;
        }
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    private void Resize(int newCapacity)
    {
        var newArray = new T[newCapacity];
        for (int i = 0; i < _count; i++)
        {
            newArray[i] = _items[(_head + i) % _items.Length];
        }
        _items = newArray;
        _head = 0;
        _tail = _count;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            yield return _items[(_head + i) % _items.Length];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

