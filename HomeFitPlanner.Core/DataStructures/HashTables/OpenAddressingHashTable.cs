using System;
using System.Collections.Generic;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.DataStructures.HashTables;

public enum ProbingStrategy
{
    Linear,
    Quadratic
}

/// <summary>
/// Hand-written Hash Table using Open Addressing (Linear or Quadratic Probing).
/// Employs tombstone markers for deleted slots and tracks probing collision stats.
/// </summary>
public class OpenAddressingHashTable<TKey, TValue> where TKey : notnull
{
    private struct Entry
    {
        public TKey Key;
        public TValue Value;
        public bool IsOccupied;
        public bool IsDeleted;
    }

    private Entry[] _table;
    private int _count;
    private readonly ProbingStrategy _strategy;
    private const double MaxLoadFactor = 0.65;
    private const int DefaultCapacity = 17;

    public int Count => _count;
    public int TableSize => _table.Length;
    public long TotalCollisions { get; private set; }
    public double LoadFactor => (double)_count / _table.Length;

    public OpenAddressingHashTable(int capacity = DefaultCapacity, ProbingStrategy strategy = ProbingStrategy.Linear)
    {
        int cap = capacity <= 0 ? DefaultCapacity : capacity;
        _table = new Entry[cap];
        _strategy = strategy;
        _count = 0;
        TotalCollisions = 0;
    }

    private int ComputeHash(TKey key)
    {
        if (key is string s)
        {
            unchecked
            {
                int hash = 5381;
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
        if ((double)(_count + 1) / _table.Length > MaxLoadFactor)
        {
            Resize(_table.Length * 2 + 1);
        }

        int hash = ComputeHash(key);
        int firstTombstone = -1;

        for (int i = 0; i < _table.Length; i++)
        {
            int probeOffset = _strategy == ProbingStrategy.Linear ? i : i * i;
            int idx = (hash + probeOffset) % _table.Length;
            if (idx < 0) idx += _table.Length;

            if (_table[idx].IsOccupied)
            {
                if (EqualityComparer<TKey>.Default.Equals(_table[idx].Key, key))
                {
                    _table[idx].Value = value; // Update existing
                    return;
                }
                TotalCollisions++;
            }
            else
            {
                if (_table[idx].IsDeleted)
                {
                    if (firstTombstone == -1) firstTombstone = idx;
                }
                else
                {
                    // Empty virgin slot
                    int targetIdx = firstTombstone != -1 ? firstTombstone : idx;
                    _table[targetIdx] = new Entry { Key = key, Value = value, IsOccupied = true, IsDeleted = false };
                    _count++;
                    return;
                }
            }
        }

        if (firstTombstone != -1)
        {
            _table[firstTombstone] = new Entry { Key = key, Value = value, IsOccupied = true, IsDeleted = false };
            _count++;
            return;
        }

        // Table full, resize and retry
        Resize(_table.Length * 2 + 1);
        Insert(key, value);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int hash = ComputeHash(key);

        for (int i = 0; i < _table.Length; i++)
        {
            int probeOffset = _strategy == ProbingStrategy.Linear ? i : i * i;
            int idx = (hash + probeOffset) % _table.Length;
            if (idx < 0) idx += _table.Length;

            if (_table[idx].IsOccupied)
            {
                if (EqualityComparer<TKey>.Default.Equals(_table[idx].Key, key))
                {
                    value = _table[idx].Value;
                    return true;
                }
            }
            else if (!_table[idx].IsDeleted)
            {
                // Slot never occupied; element does not exist
                break;
            }
        }

        value = default!;
        return false;
    }

    public bool ContainsKey(TKey key) => TryGetValue(key, out _);

    public bool Remove(TKey key)
    {
        int hash = ComputeHash(key);

        for (int i = 0; i < _table.Length; i++)
        {
            int probeOffset = _strategy == ProbingStrategy.Linear ? i : i * i;
            int idx = (hash + probeOffset) % _table.Length;
            if (idx < 0) idx += _table.Length;

            if (_table[idx].IsOccupied)
            {
                if (EqualityComparer<TKey>.Default.Equals(_table[idx].Key, key))
                {
                    _table[idx].IsOccupied = false;
                    _table[idx].IsDeleted = true;
                    _table[idx].Key = default!;
                    _table[idx].Value = default!;
                    _count--;
                    return true;
                }
            }
            else if (!_table[idx].IsDeleted)
            {
                break;
            }
        }

        return false;
    }

    public CustomArray<TValue> GetAllValues()
    {
        var list = new CustomArray<TValue>(_count);
        for (int i = 0; i < _table.Length; i++)
        {
            if (_table[i].IsOccupied)
            {
                list.Add(_table[i].Value);
            }
        }
        return list;
    }

    private void Resize(int newCapacity)
    {
        var oldTable = _table;
        _table = new Entry[newCapacity];
        _count = 0;

        for (int i = 0; i < oldTable.Length; i++)
        {
            if (oldTable[i].IsOccupied)
            {
                Insert(oldTable[i].Key, oldTable[i].Value);
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _table.Length; i++)
        {
            _table[i] = default;
        }
        _count = 0;
        TotalCollisions = 0;
    }
}

