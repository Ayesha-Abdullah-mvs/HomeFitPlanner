using System;
using System.Collections.Generic;

namespace HomeFitPlanner.Core.Algorithms;

public class SortMetrics
{
    public long Comparisons { get; set; }
    public long Swaps { get; set; }
    public double ElapsedMicroseconds { get; set; }
}

/// <summary>
/// Hand-written sorting algorithms implemented from scratch.
/// Includes Bubble, Insertion, Selection, Merge, Quick, and Heap Sort.
/// </summary>
public static class SortingAlgorithms
{
    public static SortMetrics BubbleSort<T>(T[] array, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var metrics = new SortMetrics();
        int n = array.Length;

        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                metrics.Comparisons++;
                if (comparer.Compare(array[j], array[j + 1]) > 0)
                {
                    Swap(array, j, j + 1);
                    metrics.Swaps++;
                    swapped = true;
                }
            }
            if (!swapped) break;
        }

        return metrics;
    }

    public static SortMetrics InsertionSort<T>(T[] array, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var metrics = new SortMetrics();
        int n = array.Length;

        for (int i = 1; i < n; i++)
        {
            T key = array[i];
            int j = i - 1;

            while (j >= 0)
            {
                metrics.Comparisons++;
                if (comparer.Compare(array[j], key) > 0)
                {
                    array[j + 1] = array[j];
                    metrics.Swaps++;
                    j--;
                }
                else
                {
                    break;
                }
            }
            array[j + 1] = key;
        }

        return metrics;
    }

    public static SortMetrics SelectionSort<T>(T[] array, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var metrics = new SortMetrics();
        int n = array.Length;

        for (int i = 0; i < n - 1; i++)
        {
            int minIdx = i;
            for (int j = i + 1; j < n; j++)
            {
                metrics.Comparisons++;
                if (comparer.Compare(array[j], array[minIdx]) < 0)
                {
                    minIdx = j;
                }
            }

            if (minIdx != i)
            {
                Swap(array, i, minIdx);
                metrics.Swaps++;
            }
        }

        return metrics;
    }

    public static SortMetrics MergeSort<T>(T[] array, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var metrics = new SortMetrics();
        if (array.Length <= 1) return metrics;

        var temp = new T[array.Length];
        MergeSortInternal(array, temp, 0, array.Length - 1, comparer, metrics);
        return metrics;
    }

    private static void MergeSortInternal<T>(T[] array, T[] temp, int left, int right, IComparer<T> comparer, SortMetrics metrics)
    {
        if (left >= right) return;

        int mid = left + (right - left) / 2;
        MergeSortInternal(array, temp, left, mid, comparer, metrics);
        MergeSortInternal(array, temp, mid + 1, right, comparer, metrics);
        Merge(array, temp, left, mid, right, comparer, metrics);
    }

    private static void Merge<T>(T[] array, T[] temp, int left, int mid, int right, IComparer<T> comparer, SortMetrics metrics)
    {
        for (int k = left; k <= right; k++)
        {
            temp[k] = array[k];
        }

        int i = left;
        int j = mid + 1;
        int idx = left;

        while (i <= mid && j <= right)
        {
            metrics.Comparisons++;
            if (comparer.Compare(temp[i], temp[j]) <= 0)
            {
                array[idx++] = temp[i++];
            }
            else
            {
                array[idx++] = temp[j++];
                metrics.Swaps++;
            }
        }

        while (i <= mid)
        {
            array[idx++] = temp[i++];
        }

        while (j <= right)
        {
            array[idx++] = temp[j++];
        }
    }

    public static SortMetrics QuickSort<T>(T[] array, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var metrics = new SortMetrics();
        if (array.Length <= 1) return metrics;

        QuickSortInternal(array, 0, array.Length - 1, comparer, metrics);
        return metrics;
    }

    private static void QuickSortInternal<T>(T[] array, int low, int high, IComparer<T> comparer, SortMetrics metrics)
    {
        if (low < high)
        {
            int p = Partition(array, low, high, comparer, metrics);
            QuickSortInternal(array, low, p - 1, comparer, metrics);
            QuickSortInternal(array, p + 1, high, comparer, metrics);
        }
    }

    private static int Partition<T>(T[] array, int low, int high, IComparer<T> comparer, SortMetrics metrics)
    {
        // Median-of-three pivot selection to prevent worst-case on sorted data
        int mid = low + (high - low) / 2;
        if (comparer.Compare(array[low], array[mid]) > 0) Swap(array, low, mid);
        if (comparer.Compare(array[low], array[high]) > 0) Swap(array, low, high);
        if (comparer.Compare(array[mid], array[high]) > 0) Swap(array, mid, high);

        Swap(array, mid, high);
        T pivot = array[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            metrics.Comparisons++;
            if (comparer.Compare(array[j], pivot) <= 0)
            {
                i++;
                Swap(array, i, j);
                metrics.Swaps++;
            }
        }

        Swap(array, i + 1, high);
        metrics.Swaps++;
        return i + 1;
    }

    public static SortMetrics HeapSort<T>(T[] array, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var metrics = new SortMetrics();
        int n = array.Length;

        // Build max heap
        for (int i = n / 2 - 1; i >= 0; i--)
        {
            Heapify(array, n, i, comparer, metrics);
        }

        // Extract elements one by one
        for (int i = n - 1; i > 0; i--)
        {
            Swap(array, 0, i);
            metrics.Swaps++;
            Heapify(array, i, 0, comparer, metrics);
        }

        return metrics;
    }

    private static void Heapify<T>(T[] array, int n, int i, IComparer<T> comparer, SortMetrics metrics)
    {
        int largest = i;
        int left = 2 * i + 1;
        int right = 2 * i + 2;

        if (left < n)
        {
            metrics.Comparisons++;
            if (comparer.Compare(array[left], array[largest]) > 0)
            {
                largest = left;
            }
        }

        if (right < n)
        {
            metrics.Comparisons++;
            if (comparer.Compare(array[right], array[largest]) > 0)
            {
                largest = right;
            }
        }

        if (largest != i)
        {
            Swap(array, i, largest);
            metrics.Swaps++;
            Heapify(array, n, largest, comparer, metrics);
        }
    }

    private static void Swap<T>(T[] array, int a, int b)
    {
        T temp = array[a];
        array[a] = array[b];
        array[b] = temp;
    }
}

