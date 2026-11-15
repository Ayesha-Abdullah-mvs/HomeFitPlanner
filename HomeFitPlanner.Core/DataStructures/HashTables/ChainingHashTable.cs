using System;
using System.Collections.Generic;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.DataStructures.HashTables;

/// <summary>
/// Hand-written Hash Table using Separate Chaining for collision resolution.
/// Tracks collision count and load factor for analytical comparison.
/// </summary>
public class ChainingHashTable<TKey, TValue> where TKey : notnull
{
    private class HashNode
    {
        public TKey Key { get; }
        public TValue Value { get; set; }
        public HashNode? Next { get; set; }

        public HashNode(TKey key, TValue value, HashNode? next = null)
        {
            Key = key;
            Value = value;
            Next = next;
        }
    }

    private HashNode?[] _buckets;
    private int _count;
    private const double MaxLoadFactor = 0.75;
    private const int DefaultCapacity = 17; // prime number

    public int Count => _count;
    public int BucketCount => _buckets.Length;
    public long TotalCollisions { get; private set; }
    public double LoadFactor => (double)_count / _buckets.Length;

    public ChainingHashTable(int capacity = DefaultCapacity)
    {
        int cap = capacity <= 0 ? DefaultCapacity : capacity;
        _buckets = new HashNode?[cap];
        _count = 0;
        TotalCollisions = 0;
    }

    private int GetBucketIndex(TKey key, int capacity)
    {
        int hash = ComputeHash(key);
        int index = hash % capacity;
        if (index < 0) index += capacity;
        return index;
    }

    private int ComputeHash(TKey key)
    {
        if (key is string s)
        {
            unchecked
            {
                int hash = 5381; // djb2 hash
                for (int i = 0; i < s.Length; i++)
                {
                    hash = ((hash << 5) + hash) ^ s[i];
                }
                return hash;
            }
        }
        return key.GetHashCode();
    }

    public void Insert(TKey key, TValue value)
    {
        if ((double)(_count + 1) / _buckets.Length > MaxLoadFactor)
        {
            Resize(_buckets.Length * 2 + 1);
        }

        int index = GetBucketIndex(key, _buckets.Length);
        var current = _buckets[index];

        if (current == null)
        {
            _buckets[index] = new HashNode(key, value);
            _count++;
            return;
        }

        // Collision occurred in this bucket
        TotalCollisions++;

        while (current != null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                current.Value = value; // update
                return;
            }
            if (current.Next == null) break;
            current = current.Next;
        }

        current!.Next = new HashNode(key, value);
        _count++;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = GetBucketIndex(key, _buckets.Length);
        var current = _buckets[index];

        while (current != null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                value = current.Value;
                return true;
            }
            current = current.Next;
        }

        value = default!;
        return false;
    }

    public bool ContainsKey(TKey key) => TryGetValue(key, out _);

    public bool Remove(TKey key)
    {
        int index = GetBucketIndex(key, _buckets.Length);
        var current = _buckets[index];
        HashNode? prev = null;

        while (current != null)
        {
            if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
            {
                if (prev == null)
                {
                    _buckets[index] = current.Next;
                }
                else
                {
                    prev.Next = current.Next;
                }
                _count--;
                return true;
            }
            prev = current;
            current = current.Next;
        }

        return false;
    }

    public CustomArray<TValue> GetAllValues()
    {
        var list = new CustomArray<TValue>(_count);
        for (int i = 0; i < _buckets.Length; i++)
        {
            var node = _buckets[i];
            while (node != null)
            {
                list.Add(node.Value);
                node = node.Next;
            }
        }
        return list;
    }

    private void Resize(int newCapacity)
    {
        var oldBuckets = _buckets;
        _buckets = new HashNode?[newCapacity];
        _count = 0;

        for (int i = 0; i < oldBuckets.Length; i++)
        {
            var node = oldBuckets[i];
            while (node != null)
            {
                Insert(node.Key, node.Value);
                node = node.Next;
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i] = null;
        }
        _count = 0;
        TotalCollisions = 0;
    }
}

