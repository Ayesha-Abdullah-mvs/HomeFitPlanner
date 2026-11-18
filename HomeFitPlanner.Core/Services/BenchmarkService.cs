using System;
using System.Diagnostics;
using HomeFitPlanner.Core.Algorithms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.DataStructures.HashTables;
using HomeFitPlanner.Core.DataStructures.Trees;

namespace HomeFitPlanner.Core.Services;

public class AlgorithmBenchmarkItem
{
    public string AlgorithmName { get; set; } = string.Empty;
    public double ElapsedMicroseconds { get; set; }
    public long OperationsCount { get; set; }
    public string TheoreticalTimeComplexity { get; set; } = string.Empty;
    public string TheoreticalSpaceComplexity { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

public class BenchmarkComparisonGroup
{
    public string CategoryName { get; set; } = string.Empty;
    public int InputSize { get; set; }
    public CustomArray<AlgorithmBenchmarkItem> Items { get; set; } = new();
}

/// <summary>
/// Powers the Screen 9 Performance Lab by executing empirical timing comparisons
/// and contrasting empirical performance with theoretical Big-O complexity.
/// </summary>
public class BenchmarkService
{
    public BenchmarkComparisonGroup RunSortingComparison(int n)
    {
        var group = new BenchmarkComparisonGroup
        {
            CategoryName = "Sorting Algorithms",
            InputSize = n
        };

        int[] original = GenerateRandomIntArray(n);

        // 1. Bubble Sort
        if (n <= 5000)
        {
            int[] arr = (int[])original.Clone();
            var sw = Stopwatch.StartNew();
            var m = SortingAlgorithms.BubbleSort(arr);
            sw.Stop();
            group.Items.Add(new AlgorithmBenchmarkItem
            {
                AlgorithmName = "Bubble Sort",
                ElapsedMicroseconds = Math.Round(sw.Elapsed.TotalMicroseconds, 1),
                OperationsCount = m.Comparisons,
                TheoreticalTimeComplexity = "O(N²)",
                TheoreticalSpaceComplexity = "O(1)",
                Details = $"{m.Comparisons:N0} comps, {m.Swaps:N0} swaps"
            });
        }
        else
        {
            group.Items.Add(new AlgorithmBenchmarkItem
            {
                AlgorithmName = "Bubble Sort",
                ElapsedMicroseconds = 0,
                OperationsCount = 0,
                TheoreticalTimeComplexity = "O(N²)",
                TheoreticalSpaceComplexity = "O(1)",
                Details = "Skipped (N too large for O(N²))"
            });
        }

        // 2. Insertion Sort
        if (n <= 5000)
        {
            int[] arr = (int[])original.Clone();
            var sw = Stopwatch.StartNew();
            var m = SortingAlgorithms.InsertionSort(arr);
            sw.Stop();
            group.Items.Add(new AlgorithmBenchmarkItem
            {
                AlgorithmName = "Insertion Sort",
                ElapsedMicroseconds = Math.Round(sw.Elapsed.TotalMicroseconds, 1),
                OperationsCount = m.Comparisons,
                TheoreticalTimeComplexity = "O(N²)",
                TheoreticalSpaceComplexity = "O(1)",
                Details = $"{m.Comparisons:N0} comps, {m.Swaps:N0} shifts"
            });
        }

        // 3. Selection Sort
        if (n <= 5000)
        {
            int[] arr = (int[])original.Clone();
            var sw = Stopwatch.StartNew();
            var m = SortingAlgorithms.SelectionSort(arr);
            sw.Stop();
            group.Items.Add(new AlgorithmBenchmarkItem
            {
                AlgorithmName = "Selection Sort",
                ElapsedMicroseconds = Math.Round(sw.Elapsed.TotalMicroseconds, 1),
                OperationsCount = m.Comparisons,
                TheoreticalTimeComplexity = "O(N²)",
                TheoreticalSpaceComplexity = "O(1)",
                Details = $"{m.Comparisons:N0} comps, {m.Swaps:N0} swaps"
            });
        }

        // 4. Merge Sort
        {
            int[] arr = (int[])original.Clone();
            var sw = Stopwatch.StartNew();
            var m = SortingAlgorithms.MergeSort(arr);
            sw.Stop();
            group.Items.Add(new AlgorithmBenchmarkItem
            {
                AlgorithmName = "Merge Sort",
                ElapsedMicroseconds = Math.Round(sw.Elapsed.TotalMicroseconds, 1),
                OperationsCount = m.Comparisons,
                TheoreticalTimeComplexity = "O(N log N)",
                TheoreticalSpaceComplexity = "O(N)",
                Details = $"{m.Comparisons:N0} comparisons"
            });
        }

        // 5. Quick Sort
        {
            int[] arr = (int[])original.Clone();
            var sw = Stopwatch.StartNew();
            var m = SortingAlgorithms.QuickSort(arr);
            sw.Stop();
            group.Items.Add(new AlgorithmBenchmarkItem
            {
                AlgorithmName = "Quick Sort",
                ElapsedMicroseconds = Math.Round(sw.Elapsed.TotalMicroseconds, 1),
                OperationsCount = m.Comparisons,
                TheoreticalTimeComplexity = "O(N log N)",
                TheoreticalSpaceComplexity = "O(log N)",
                Details = $"{m.Comparisons:N0} comparisons"
            });
        }

        return group;
    }

    public BenchmarkComparisonGroup RunSearchComparison(int n)
    {
        var group = new BenchmarkComparisonGroup
        {
            CategoryName = "Linear vs Binary Search",
            InputSize = n
        };

        int[] array = new int[n];
        for (int i = 0; i < n; i++) array[i] = i * 2; // Sorted even numbers

        int target = (n - 1) * 2; // Target near the end (worst-case for linear)

        // Linear Search
        var sw1 = Stopwatch.StartNew();
        var m1 = SearchingAlgorithms.LinearSearch(array, target);
        sw1.Stop();
        group.Items.Add(new AlgorithmBenchmarkItem
        {
            AlgorithmName = "Linear Search",
            ElapsedMicroseconds = Math.Round(sw1.Elapsed.TotalMicroseconds, 2),
            OperationsCount = m1.Comparisons,
            TheoreticalTimeComplexity = "O(N)",
            TheoreticalSpaceComplexity = "O(1)",
            Details = $"Inspected {m1.Comparisons:N0} elements sequentially"
        });

        // Binary Search
        var sw2 = Stopwatch.StartNew();
        var m2 = SearchingAlgorithms.BinarySearch(array, target);
        sw2.Stop();
        group.Items.Add(new AlgorithmBenchmarkItem
        {
            AlgorithmName = "Binary Search",
            ElapsedMicroseconds = Math.Round(sw2.Elapsed.TotalMicroseconds, 2),
            OperationsCount = m2.Comparisons,
            TheoreticalTimeComplexity = "O(log N)",
            TheoreticalSpaceComplexity = "O(1)",
            Details = $"Inspected only {m2.Comparisons:N0} midpoints logarithmically"
        });

        return group;
    }

    public BenchmarkComparisonGroup RunTreesComparison(int n)
    {
        var group = new BenchmarkComparisonGroup
        {
            CategoryName = "Plain BST vs Self-Balancing AVL Tree",
            InputSize = n
        };

        // Insert sequential items (1 to n) which causes BST to degenerate to O(N) linked list!
        int[] sortedData = new int[n];
        for (int i = 0; i < n; i++) sortedData[i] = i + 1;

        // Plain BST
        var bst = new BinarySearchTree<int, string>();
        var swBstInsert = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
        {
            bst.Insert(sortedData[i], $"Item {sortedData[i]}");
        }
        swBstInsert.Stop();
        int bstHeight = bst.GetHeight();

        var swBstSearch = Stopwatch.StartNew();
        bst.Search(n); // search last item
        swBstSearch.Stop();

        group.Items.Add(new AlgorithmBenchmarkItem
        {
            AlgorithmName = "Plain BST (Unbalanced)",
            ElapsedMicroseconds = Math.Round(swBstInsert.Elapsed.TotalMicroseconds, 1),
            OperationsCount = bstHeight,
            TheoreticalTimeComplexity = "O(N) degraded",
            TheoreticalSpaceComplexity = "O(N)",
            Details = $"Tree Height: {bstHeight} (Degenerated to linked list!) | Search: {swBstSearch.Elapsed.TotalMicroseconds:F1}μs"
        });

        // AVL Tree
        var avl = new AvlTree<int, string>();
        var swAvlInsert = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
        {
            avl.Insert(sortedData[i], $"Item {sortedData[i]}");
        }
        swAvlInsert.Stop();
        int avlHeight = avl.GetHeight();

        var swAvlSearch = Stopwatch.StartNew();
        avl.Search(n);
        swAvlSearch.Stop();

        group.Items.Add(new AlgorithmBenchmarkItem
        {
            AlgorithmName = "AVL Tree (Self-Balancing)",
            ElapsedMicroseconds = Math.Round(swAvlInsert.Elapsed.TotalMicroseconds, 1),
            OperationsCount = avlHeight,
            TheoreticalTimeComplexity = "O(log N)",
            TheoreticalSpaceComplexity = "O(N)",
            Details = $"Tree Height: {avlHeight} (Strictly balanced with rotations!) | Search: {swAvlSearch.Elapsed.TotalMicroseconds:F1}μs"
        });

        return group;
    }

    public BenchmarkComparisonGroup RunHashTableComparison(int n)
    {
        var group = new BenchmarkComparisonGroup
        {
            CategoryName = "Hash Tables: Chaining vs Open Addressing",
            InputSize = n
        };

        // Separate Chaining
        var chaining = new ChainingHashTable<string, int>(n / 2);
        var swChaining = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
        {
            chaining.Insert($"exercise_key_{i}", i);
        }
        swChaining.Stop();

        group.Items.Add(new AlgorithmBenchmarkItem
        {
            AlgorithmName = "Separate Chaining",
            ElapsedMicroseconds = Math.Round(swChaining.Elapsed.TotalMicroseconds, 1),
            OperationsCount = chaining.TotalCollisions,
            TheoreticalTimeComplexity = "O(1) avg",
            TheoreticalSpaceComplexity = "O(N + Buckets)",
            Details = $"{chaining.TotalCollisions:N0} collisions resolved in linked buckets (Load: {chaining.LoadFactor:F2})"
        });

        // Open Addressing (Linear Probing)
        var openAddr = new OpenAddressingHashTable<string, int>(n * 2, ProbingStrategy.Linear);
        var swOpen = Stopwatch.StartNew();
        for (int i = 0; i < n; i++)
        {
            openAddr.Insert($"exercise_key_{i}", i);
        }
        swOpen.Stop();

        group.Items.Add(new AlgorithmBenchmarkItem
        {
            AlgorithmName = "Open Addressing (Linear Probing)",
            ElapsedMicroseconds = Math.Round(swOpen.Elapsed.TotalMicroseconds, 1),
            OperationsCount = openAddr.TotalCollisions,
            TheoreticalTimeComplexity = "O(1) avg",
            TheoreticalSpaceComplexity = "O(Capacity)",
            Details = $"{openAddr.TotalCollisions:N0} probe collisions resolved in-place (Load: {openAddr.LoadFactor:F2})"
        });

        return group;
    }

    public BenchmarkComparisonGroup RunMultiplicationComparison(int digits)
    {
        var group = new BenchmarkComparisonGroup
        {
            CategoryName = "Large Integer Multiplication",
            InputSize = digits
        };

        var rng = new Random(42);
        string num1 = GenerateLargeNumberString(digits, rng);
        string num2 = GenerateLargeNumberString(digits, rng);

        // Grade-School Multiplication O(N^2)
        var sw1 = Stopwatch.StartNew();
        string res1 = MathAlgorithms.MultiplyGradeSchool(num1, num2);
        sw1.Stop();

        group.Items.Add(new AlgorithmBenchmarkItem
        {
            AlgorithmName = "Grade-School Multiplication",
            ElapsedMicroseconds = Math.Round(sw1.Elapsed.TotalMicroseconds, 1),
            OperationsCount = (long)digits * digits,
            TheoreticalTimeComplexity = "O(N²)",
            TheoreticalSpaceComplexity = "O(N)",
            Details = $"{digits}x{digits} digit grid calculation"
        });

        // Karatsuba Divide-and-Conquer O(N^1.585)
        var sw2 = Stopwatch.StartNew();
        string res2 = MathAlgorithms.MultiplyKaratsuba(num1, num2);
        sw2.Stop();

        group.Items.Add(new AlgorithmBenchmarkItem
        {
            AlgorithmName = "Karatsuba Divide & Conquer",
            ElapsedMicroseconds = Math.Round(sw2.Elapsed.TotalMicroseconds, 1),
            OperationsCount = (long)Math.Pow(digits, 1.585),
            TheoreticalTimeComplexity = "O(N^1.585)",
            TheoreticalSpaceComplexity = "O(N log N)",
            Details = $"3 sub-multiplications per recursion"
        });

        return group;
    }

    private static int[] GenerateRandomIntArray(int size)
    {
        var rng = new Random(12345);
        var arr = new int[size];
        for (int i = 0; i < size; i++)
        {
            arr[i] = rng.Next(1, 1000000);
        }
        return arr;
    }

    private static string GenerateLargeNumberString(int digits, Random rng)
    {
        char[] chars = new char[digits];
        chars[0] = (char)('1' + rng.Next(9));
        for (int i = 1; i < digits; i++)
        {
            chars[i] = (char)('0' + rng.Next(10));
        }
        return new string(chars);
    }
}

