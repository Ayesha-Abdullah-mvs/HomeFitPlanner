using System;
using System.Collections;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.Queues;

/// <summary>
/// Hand-written double-ended queue (Deque) supporting O(1) insertions
/// and deletions from both front and back ends.
/// </summary>
public class CustomDeque<T> : IEnumerable<T>
{
    private T[] _items;
    private int _head;
    private int _tail;
    private int _count;
    private const int DefaultCapacity = 8;

    public CustomDeque(int capacity = DefaultCapacity)
    {
        if (capacity <= 0) capacity = DefaultCapacity;
        _items = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;

    public void AddBack(T item)
    {
        EnsureCapacity();
        _items[_tail] = item;
        _tail = (_tail + 1) % _items.Length;
        _count++;
    }

    public void AddFront(T item)
    {
        EnsureCapacity();
        _head = (_head - 1 + _items.Length) % _items.Length;
        _items[_head] = item;
        _count++;
    }

    public T RemoveFront()
    {
        if (_count == 0)
            throw new InvalidOperationException("Deque is empty.");

        T item = _items[_head];
        _items[_head] = default!;
        _head = (_head + 1) % _items.Length;
        _count--;
        return item;
    }

    public T RemoveBack()
    {
        if (_count == 0)
            throw new InvalidOperationException("Deque is empty.");

        _tail = (_tail - 1 + _items.Length) % _items.Length;
        T item = _items[_tail];
        _items[_tail] = default!;
        _count--;
        return item;
    }

    public T PeekFront()
    {
        if (_count == 0)
            throw new InvalidOperationException("Deque is empty.");
        return _items[_head];
    }

    public T PeekBack()
    {
        if (_count == 0)
            throw new InvalidOperationException("Deque is empty.");
        int idx = (_tail - 1 + _items.Length) % _items.Length;
        return _items[idx];
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

    private void EnsureCapacity()
    {
        if (_count == _items.Length)
        {
            int newCapacity = _items.Length * 2;
            var newItems = new T[newCapacity];
            for (int i = 0; i < _count; i++)
            {
                newItems[i] = _items[(_head + i) % _items.Length];
            }
            _items = newItems;
            _head = 0;
            _tail = _count;
        }
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

