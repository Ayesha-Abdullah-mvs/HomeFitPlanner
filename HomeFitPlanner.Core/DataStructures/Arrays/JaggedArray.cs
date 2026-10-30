using System;

namespace HomeFitPlanner.Core.DataStructures.Arrays;

/// <summary>
/// Hand-written jagged array representation for weekly workout schedules,
/// where each day (row) can contain a varying number of exercises (columns).
/// </summary>
public class JaggedArray<T>
{
    private T[][] _rows;

    public JaggedArray(int rowCount)
    {
        if (rowCount <= 0) throw new ArgumentOutOfRangeException(nameof(rowCount));
        _rows = new T[rowCount][];
        for (int i = 0; i < rowCount; i++)
        {
            _rows[i] = Array.Empty<T>();
        }
    }

    public int RowCount => _rows.Length;

    public int GetColumnCount(int row)
    {
        ValidateRow(row);
        return _rows[row].Length;
    }

    public T this[int row, int col]
    {
        get
        {
            ValidateRow(row);
            ValidateCol(row, col);
            return _rows[row][col];
        }
        set
        {
            ValidateRow(row);
            ValidateCol(row, col);
            _rows[row][col] = value;
        }
    }

    public void SetRow(int row, T[] items)
    {
        ValidateRow(row);
        if (items == null)
        {
            _rows[row] = Array.Empty<T>();
            return;
        }

        _rows[row] = new T[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            _rows[row][i] = items[i];
        }
    }

    public T[] GetRow(int row)
    {
        ValidateRow(row);
        var copy = new T[_rows[row].Length];
        for (int i = 0; i < _rows[row].Length; i++)
        {
            copy[i] = _rows[row][i];
        }
        return copy;
    }

    public void AddToRow(int row, T item)
    {
        ValidateRow(row);
        int currentLen = _rows[row].Length;
        var newRow = new T[currentLen + 1];
        for (int i = 0; i < currentLen; i++)
        {
            newRow[i] = _rows[row][i];
        }
        newRow[currentLen] = item;
        _rows[row] = newRow;
    }

    public void RemoveFromRow(int row, int col)
    {
        ValidateRow(row);
        ValidateCol(row, col);
        int currentLen = _rows[row].Length;
        var newRow = new T[currentLen - 1];
        int writeIdx = 0;
        for (int i = 0; i < currentLen; i++)
        {
            if (i == col) continue;
            newRow[writeIdx++] = _rows[row][i];
        }
        _rows[row] = newRow;
    }

    public T[][] ToRawJaggedArray()
    {
        var result = new T[_rows.Length][];
        for (int i = 0; i < _rows.Length; i++)
        {
            result[i] = GetRow(i);
        }
        return result;
    }

    private void ValidateRow(int row)
    {
        if (row < 0 || row >= _rows.Length)
            throw new ArgumentOutOfRangeException(nameof(row), $"Row {row} is outside bounds [0, {_rows.Length - 1}].");
    }

    private void ValidateCol(int row, int col)
    {
        if (col < 0 || col >= _rows[row].Length)
            throw new ArgumentOutOfRangeException(nameof(col), $"Column {col} is outside bounds [0, {_rows[row].Length - 1}].");
    }
}

