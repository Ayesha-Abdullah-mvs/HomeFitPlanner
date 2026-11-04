using System;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.Algorithms;

public class SearchMetrics
{
    public int FoundIndex { get; set; } = -1;
    public long Comparisons { get; set; }
    public double ElapsedMicroseconds { get; set; }
}

/// <summary>
/// Hand-written linear and binary search algorithms.
/// </summary>
public static class SearchingAlgorithms
{
    public static SearchMetrics LinearSearch<T>(T[] array, T key, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var metrics = new SearchMetrics();

        for (int i = 0; i < array.Length; i++)
        {
            metrics.Comparisons++;
            if (comparer.Compare(array[i], key) == 0)
            {
                metrics.FoundIndex = i;
                return metrics;
            }
        }

        return metrics;
    }

    public static SearchMetrics BinarySearch<T>(T[] array, T key, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var metrics = new SearchMetrics();

        int low = 0;
        int high = array.Length - 1;

        while (low <= high)
        {
            metrics.Comparisons++;
            int mid = low + (high - low) / 2;
            int cmp = comparer.Compare(array[mid], key);

            if (cmp == 0)
            {
                metrics.FoundIndex = mid;
                return metrics;
            }
            if (cmp < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return metrics;
    }
}

