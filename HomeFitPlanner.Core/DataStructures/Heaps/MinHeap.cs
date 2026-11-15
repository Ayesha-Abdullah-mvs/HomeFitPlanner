using System;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.DataStructures.Heaps;

/// <summary>
/// Hand-written binary Min-Heap using an array buffer.
/// Elements with the lowest key/value bubble to the root.
/// </summary>
public class MinHeap<T>
{
    private T[] _items;
    private int _count;
    private readonly IComparer<T> _comparer;
    private const int DefaultCapacity = 16;

    public MinHeap(IComparer<T>? comparer = null, int capacity = DefaultCapacity)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        _items = new T[capacity <= 0 ? DefaultCapacity : capacity];
        _count = 0;
    }

    public int Count => _count;
    public bool IsEmpty => _count == 0;

    public void Insert(T item)
    {
        EnsureCapacity();
        _items[_count] = item;
        SiftUp(_count);
        _count++;
    }

    public T ExtractMin()
    {
        if (_count == 0)
            throw new InvalidOperationException("Heap is empty.");

        T min = _items[0];
        _count--;
        if (_count > 0)
        {
            _items[0] = _items[_count];
            _items[_count] = default!;
            SiftDown(0);
        }
        else
        {
            _items[0] = default!;
        }
        return min;
    }

    public T Peek()
    {
        if (_count == 0)
            throw new InvalidOperationException("Heap is empty.");
        return _items[0];
    }

    public void Clear()
    {
        for (int i = 0; i < _count; i++)
        {
            _items[i] = default!;
        }
        _count = 0;
    }

    private void SiftUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (_comparer.Compare(_items[index], _items[parent]) < 0)
            {
                Swap(index, parent);
                index = parent;
            }
            else
            {
                break;
            }
        }
    }

    private void SiftDown(int index)
    {
        while (true)
        {
            int left = 2 * index + 1;
            int right = 2 * index + 2;
            int smallest = index;

            if (left < _count && _comparer.Compare(_items[left], _items[smallest]) < 0)
            {
                smallest = left;
            }
            if (right < _count && _comparer.Compare(_items[right], _items[smallest]) < 0)
            {
                smallest = right;
            }

            if (smallest != index)
            {
                Swap(index, smallest);
                index = smallest;
            }
            else
            {
                break;
            }
        }
    }

    private void Swap(int a, int b)
    {
        T temp = _items[a];
        _items[a] = _items[b];
        _items[b] = temp;
    }

    private void EnsureCapacity()
    {
        if (_count == _items.Length)
        {
            int newCapacity = _items.Length * 2;
            var newArray = new T[newCapacity];
            for (int i = 0; i < _count; i++)
            {
                newArray[i] = _items[i];
            }
            _items = newArray;
        }
    }
}

