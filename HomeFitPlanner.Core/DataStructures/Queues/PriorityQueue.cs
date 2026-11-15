using System;
using System.Collections.Generic;
using HomeFitPlanner.Core.DataStructures.Heaps;

namespace HomeFitPlanner.Core.DataStructures.Queues;

/// <summary>
/// Hand-written Heap-based Priority Queue.
/// Higher priority items are dequeued first.
/// Used to select muscle groups needing recovery/priority focus.
/// </summary>
public class PriorityQueue<T>
{
    private class PriorityNode : IComparable<PriorityNode>
    {
        public T Item { get; }
        public double Priority { get; }

        public PriorityNode(T item, double priority)
        {
            Item = item;
            Priority = priority;
        }

        public int CompareTo(PriorityNode? other)
        {
            if (other == null) return 1;
            return Priority.CompareTo(other.Priority);
        }
    }

    private readonly MaxHeap<PriorityNode> _maxHeap;

    public PriorityQueue()
    {
        _maxHeap = new MaxHeap<PriorityNode>();
    }

    public int Count => _maxHeap.Count;
    public bool IsEmpty => _maxHeap.IsEmpty;

    public void Enqueue(T item, double priority)
    {
        _maxHeap.Insert(new PriorityNode(item, priority));
    }

    public T Dequeue()
    {
        var top = _maxHeap.ExtractMax();
        return top.Item;
    }

    public T Peek()
    {
        var top = _maxHeap.Peek();
        return top.Item;
    }

    public void Clear()
    {
        _maxHeap.Clear();
    }
}

