using System;
using System.Collections;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.Arrays;

/// <summary>
/// Hand-written generic dynamic array structure for constant-time indexed access
/// and dynamic resizing without relying on System.Collections.Generic.List.
/// </summary>
public class CustomArray<T> : IEnumerable<T>
{
    private T[] _items;
    private int _count;
    private const int DefaultCapacity = 4;

    public CustomArray(int capacity = DefaultCapacity)
    {
        if (capacity < 0) capacity = DefaultCapacity;
        _items = new T[capacity];
        _count = 0;
    }

    public CustomArray(IEnumerable<T> collection)
    {
        _items = new T[DefaultCapacity];
        _count = 0;
        if (collection != null)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }
    }

    public int Count => _count;

    public int Capacity => _items.Length;

    public T this[int index]
    {
        get
        {
            ValidateIndex(index);
            return _items[index];
        }
        set
        {
            ValidateIndex(index);
            _items[index] = value;
        }
    }

    public void Add(T item)
    {
        EnsureCapacity(_count + 1);
        _items[_count++] = item;
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > _count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

        EnsureCapacity(_count + 1);

        for (int i = _count; i > index; i--)
        {
            _items[i] = _items[i - 1];
        }

        _items[index] = item;
        _count++;
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        ValidateIndex(index);

        for (int i = index; i < _count - 1; i++)
        {
            _items[i] = _items[i + 1];
        }

        _items[_count - 1] = default!;
        _count--;
    }

    public int IndexOf(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < _count; i++)
        {
            if (comparer.Equals(_items[i], item))
                return i;
        }
        return -1;
    }

    public bool Contains(T item) => IndexOf(item) >= 0;

    public void Clear()
    {
        for (int i = 0; i < _count; i++)
        {
            _items[i] = default!;
        }
        _count = 0;
    }

    public T[] ToArray()
    {
        var result = new T[_count];
        for (int i = 0; i < _count; i++)
        {
            result[i] = _items[i];
        }
        return result;
    }

    public CustomArray<T> Clone()
    {
        var clone = new CustomArray<T>(_count);
        for (int i = 0; i < _count; i++)
        {
            clone.Add(_items[i]);
        }
        return clone;
    }

    private void EnsureCapacity(int min)
    {
        if (_items.Length < min)
        {
            int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
            if (newCapacity < min) newCapacity = min;
            
            var newItems = new T[newCapacity];
            for (int i = 0; i < _count; i++)
            {
                newItems[i] = _items[i];
            }
            _items = newItems;
        }
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of bounds [0, {_count - 1}].");
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            yield return _items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

