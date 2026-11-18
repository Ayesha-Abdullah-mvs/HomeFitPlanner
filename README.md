# HomeFit Planner: A Personalised Home Workout Planning System

[![Build & Tests](https://img.shields.io/badge/Milestone%201%20Tests-8%2F8%20Passed-brightgreen)](file:///HomeFitPlanner.Tests/Program.cs)
[![.NET Core](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-blue)](file:///HomeFitPlanner.sln)
[![Course](https://img.shields.io/badge/Subject-CSC%20200L%20DSA-indigo)](file:///HomeFitPlanner.sln)
[![Frontend](https://img.shields.io/badge/UI-Windows%20Forms%20%2B%20Web-teal)](file:///HomeFitPlanner.UI/MainForm.cs)

> **Institute of Data Science | University of Engineering and Technology (UET) Lahore**  
> **Course:** CSC 200L Data Structures and Algorithms  
> **Class:** Business Data Analytics | **Section:** (B) Fall 2025  
> **Submitted to:** Mr. Muhammad Nazir  
> **Evaluation Phase:** **Milestone 1 — Core Data Structures, Sorting, Searching & Divide-and-Conquer**

---

## 1. Executive Summary

**HomeFit Planner** is a desktop application designed to generate, guide, and adapt realistic 7-day home workout routines without requiring any gym equipment or internet connection. 

For **Milestone 1**, all algorithms and data structures are implemented **completely from scratch by hand** in C# without using built-in high-level collection classes. Every implemented algorithm solves a concrete engineering problem in the application.

---

## 2. Milestone 1: Implemented Data Structures & Algorithms

| Topic / Category | Hand-Written Implementation | Application Feature | Algorithmic Complexity |
| :--- | :--- | :--- | :--- |
| **Dynamic Arrays** | `CustomArray<T>` | Core container for exercise catalog and filter results with geometric doubling | Search: $O(N)$, Index: $O(1)$, Append: $O(1)$ amortized |
| **Jagged Arrays** | `JaggedArray<T>` | 7-day weekly schedule grid where days hold varying counts of exercises | Access: $O(1)$, Zero wasted memory cells |
| **String Cleaning** | `StringSanitizer` | Custom trimming, whitespace compaction, and case-insensitive substring searching | $O(N)$ scan |
| **Linear Search** | `SearchingAlgorithms.LinearSearch` | Live exercise search by partial name match across unsorted items | Best: $O(1)$, Worst: $O(N)$ |
| **Binary Search** | `SearchingAlgorithms.BinarySearch` | Fast $O(\log N)$ lookup on sorted exercise lists (by name or difficulty) | Time: $O(\log N)$, Space: $O(1)$ |
| **Bubble Sort** | `SortingAlgorithms.BubbleSort` | Educational sorting benchmark with early-termination flag | Time: $O(N^2)$, Space: $O(1)$ |
| **Insertion Sort** | `SortingAlgorithms.InsertionSort` | Sorting exercises by duration; highly adaptive for nearly sorted lists | Best: $O(N)$, Worst: $O(N^2)$, Space: $O(1)$ |
| **Selection Sort** | `SortingAlgorithms.SelectionSort` | Sorting exercises by descending duration; minimizes memory writes | Time: $O(N^2)$, Swaps: $O(N)$ |
| **Merge Sort** | `SortingAlgorithms.MergeSort` | Divide-and-Conquer sorting by difficulty; guaranteed stable sort | Time: $O(N \log N)$, Space: $O(N)$ |
| **Quick Sort** | `SortingAlgorithms.QuickSort` | Divide-and-Conquer sorting of exercise library by alphabetical name | Average: $O(N \log N)$, Space: $O(\log N)$ |
| **Karatsuba Multiplication** | `MathAlgorithms.Karatsuba` | Divide-and-conquer large integer multiplication reducing 4 multiplications to 3 | Time: $O(N^{\log_2 3}) \approx O(N^{1.585})$ vs $O(N^2)$ |
| **Recursion** | `PlanAdaptationService` | Recursive progressive overload scaling: $W(n) = W(n-1) \times (1 + r) + b$ | Recurrence depth: $O(n)$ |

---

## 3. Active Milestone 1 Screens

1. **Screen 1: Welcome & Profile Setup**
   - 5-step wizard collecting Age, Gender, Height, Weight, Fitness Baseline, Goal, and Joint Limitations.
   - Hand-written Mifflin-St Jeor metabolic calorie formula and instant BMI category calculation.
2. **Screen 2: Dashboard**
   - Active workout card, weekly completion streak counter, and health coaching tips.
3. **Screen 3: Weekly Plan Generator**
   - Custom `JaggedArray<WorkoutExercise>` storing 7 distinct days with variable exercise counts.
   - Enforces joint safety rules (e.g., knee strain elimination for sore knees).
4. **Screen 4: Today's Workout Player**
   - Guided session player with rest intervals, exercise phase badges, and active countdown timer.
5. **Screen 5: Exercise Library (Sorting & Searching Showcase)**
   - 52 built-in home exercises.
   - Multi-criteria filtering by muscle group, equipment, and exercise type.
   - Live sorting using:
     - **QuickSort** (Name A–Z)
     - **MergeSort** (Difficulty 1–5)
     - **BubbleSort** (Difficulty 5–1)
     - **InsertionSort** (Duration Lowest First)
     - **SelectionSort** (Duration Highest First)
6. **Screen 9: Algorithm Performance Lab**
   - Microsecond stopwatch benchmarking comparing:
     - Elementary sorts vs Divide-and-Conquer sorts ($O(N^2)$ vs $O(N \log N)$)
     - Linear Search ($O(N)$) vs Binary Search ($O(\log N)$)
     - Grade-School multiplication ($O(N^2)$) vs Karatsuba Divide & Conquer ($O(N^{1.585})$)
   - Visual bar chart rendering runtime execution curves.

---

## 4. Development Roadmap

- [x] **Milestone 1 (Current Evaluation):**
  - Hand-written Dynamic Arrays & Jagged Arrays
  - Linear Search & Binary Search
  - Bubble, Insertion, Selection, Merge, and Quick Sort
  - Recursive Overload & Divide-and-Conquer Karatsuba Multiplication
  - Desktop Windows Forms application shell & Algorithm Performance Lab
- [ ] **Milestone 2 (Scheduled Next Sprint):**
  - Singly, Doubly, and Circular Linked Lists (Workout session player navigation & reversal)
  - Custom LIFO Stacks & Undo/Redo Plan Editor
  - Queue variations: Modulo Circular Buffer, Deque, Max-Heap Priority Queue
  - Hierarchical Storage: Binary Search Tree (BST) with Range Queries
  - Self-Balancing AVL Trees with LL, RR, LR, RL rotations
- [ ] **Milestone 3 (Final Sprint):**
  - Open-Addressing & Separate-Chaining Hash Tables
  - Adjacency-List Graph Representation
  - Graph Traversal: BFS for Exercise Substitutes & DFS for Movement Clusters
  - Dijkstra’s Weighted Shortest Path Algorithm for Level-Up exercise progression

---

## 5. How to Build & Run

### A. Run Desktop Application (Windows Forms)
```bash
dotnet run --project HomeFitPlanner.UI
```

### B. Run Verification Test Suite
```bash
dotnet run --project HomeFitPlanner.Tests --framework net10.0
# Or for .NET 8.0:
# dotnet run --project HomeFitPlanner.Tests --framework net8.0
```

---
*Developed by Institute of Data Science Students, UET Lahore — Session 2025–2029*
