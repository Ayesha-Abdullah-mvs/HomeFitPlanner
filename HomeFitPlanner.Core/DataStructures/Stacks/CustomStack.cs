using System;
using System.Collections;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.Stacks;

/// <summary>
/// Hand-written generic stack implementing LIFO order with dynamic resizing.
/// </summary>
public class CustomStack<T> : IEnumerable<T>
{
    private T[] _items;
    private int _top;
    private const int DefaultCapacity = 8;

    public CustomStack(int capacity = DefaultCapacity)
    {
        if (capacity <= 0) capacity = DefaultCapacity;
        _items = new T[capacity];
        _top = 0;
    }

    public int Count => _top;

    public bool IsEmpty => _top == 0;

    public void Push(T item)
    {
        if (_top == _items.Length)
        {
            Resize(_items.Length * 2);
        }
        _items[_top++] = item;
    }

    public T Pop()
    {
        if (_top == 0)
            throw new InvalidOperationException("Stack is empty.");

        T item = _items[--_top];
        _items[_top] = default!;
        return item;
    }

    public T Peek()
    {
        if (_top == 0)
            throw new InvalidOperationException("Stack is empty.");

        return _items[_top - 1];
    }

    public void Clear()
    {
        for (int i = 0; i < _top; i++)
        {
            _items[i] = default!;
        }
        _top = 0;
    }

    public T[] ToArray()
    {
        var result = new T[_top];
        for (int i = 0; i < _top; i++)
        {
            result[i] = _items[_top - 1 - i]; // Top to bottom
        }
        return result;
    }

    private void Resize(int newCapacity)
    {
        var newArray = new T[newCapacity];
        for (int i = 0; i < _top; i++)
        {
            newArray[i] = _items[i];
        }
        _items = newArray;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = _top - 1; i >= 0; i--)
        {
            yield return _items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

