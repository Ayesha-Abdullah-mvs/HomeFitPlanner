using System;
using HomeFitPlanner.Core.Algorithms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.DataStructures.Graphs;
using HomeFitPlanner.Core.DataStructures.HashTables;
using HomeFitPlanner.Core.DataStructures.Heaps;
using HomeFitPlanner.Core.DataStructures.LinkedLists;
using HomeFitPlanner.Core.DataStructures.Queues;
using HomeFitPlanner.Core.DataStructures.Stacks;
using HomeFitPlanner.Core.DataStructures.Trees;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;

namespace HomeFitPlanner.Tests;

public class Program
{
    private static int _passed = 0;
    private static int _failed = 0;

    public static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("================================================================================");
        Console.WriteLine("   HomeFit Planner: Milestone 1 Algorithm Verification Test Suite               ");
        Console.WriteLine("   CSC 200L Data Structures & Algorithms | Institute of Data Science, UET Lahore");
        Console.WriteLine("================================================================================");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("--- [MILESTONE 1 VERIFIED CORE MODULES] ---");
        Console.ResetColor();
        RunTest("1. Custom Dynamic Array (Doubling Strategy)", TestCustomArray);
        RunTest("2. Jagged Array (7-Day Variable Schedule Grid)", TestJaggedArray);
        RunTest("3. String Sanitizer (Manual Text Cleaning & Tokenizer)", TestStringSanitizer);
        RunTest("4. Searching Algorithms (Linear & Binary Search)", TestSearchingAlgorithms);
        RunTest("5. Sorting Algorithms (Bubble, Insertion, Selection, Merge, Quick)", TestSortingAlgorithms);
        RunTest("6. Large Integer Multiplication (Grade-School vs Karatsuba)", TestMultiplicationAlgorithms);
        RunTest("7. Section 9 Worked Example: Plan Generation with Jagged Arrays", TestWorkedExamplePlanGeneration);
        RunTest("8. Section 6.6 Plan Adaptation Rules (Recursive Overload)", TestPlanAdaptation);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine();
        Console.WriteLine("--- [MILESTONE 2 PREVIEW (PENDING SYLLABUS TOPICS)] ---");
        Console.ResetColor();
        RunSkipped("9. Singly & Doubly Linked Lists (Session Player Navigation)");
        RunSkipped("10. Custom LIFO Stacks & Undo/Redo Manager");
        RunSkipped("11. Queue Structures (Linear, Circular Buffer, Priority Queue)");
        RunSkipped("12. Binary Search Tree (BST) & In-Order Range Queries");
        RunSkipped("13. Self-Balancing AVL Tree (Rotations & Height Balance)");
        RunSkipped("14. Hash Tables (Separate Chaining vs Open Addressing)");
        RunSkipped("15. Graph Algorithms (BFS Substitutes, DFS & Dijkstra Path)");
        RunSkipped("16. Prefix Tree (Trie) & User Authentication");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("================================================================================");
        if (_failed == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"   ALL {_passed} MILESTONE 1 TEST SUITES PASSED SUCCESSFULLY! (0 Failures)");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"   TEST RUN COMPLETED: {_passed} Passed, {_failed} FAILED");
        }
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("================================================================================");
        Console.ResetColor();
    }

    private static void RunSkipped(string testName)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"Running: {testName.PadRight(65)} ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("[PENDING SPRINT 2]");
        Console.ResetColor();
    }

    private static void RunTest(string testName, Action testAction)
    {
        try
        {
            Console.Write($"Running: {testName.PadRight(65)} ");
            testAction();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[PASS]");
            Console.ResetColor();
            _passed++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[FAIL]");
            Console.WriteLine($"  Error: {ex.Message}");
            Console.ResetColor();
            _failed++;
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition) throw new Exception($"Assertion failed: {message}");
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!object.Equals(expected, actual))
            throw new Exception($"Assertion failed for '{message}'. Expected: {expected}, Actual: {actual}");
    }

    private static void TestCustomArray()
    {
        var arr = new CustomArray<int>();
        for (int i = 0; i < 10; i++) arr.Add(i * 10);

        AssertEqual(10, arr.Count, "Array count after 10 adds");
        AssertEqual(0, arr[0], "First element");
        AssertEqual(90, arr[9], "Last element");

        arr.Insert(2, 999);
        AssertEqual(11, arr.Count, "Count after insert");
        AssertEqual(999, arr[2], "Inserted element at index 2");
        AssertEqual(20, arr[3], "Shifted element at index 3");

        arr.RemoveAt(2);
        AssertEqual(10, arr.Count, "Count after RemoveAt");
        AssertEqual(20, arr[2], "Element at index 2 restored");

        AssertTrue(arr.Contains(50), "Contains element 50");
        AssertTrue(!arr.Contains(999), "Does not contain removed element");
    }

    private static void TestJaggedArray()
    {
        var jagged = new JaggedArray<string>(7);
        AssertEqual(7, jagged.RowCount, "Jagged array has 7 rows for days of week");

        jagged.SetRow(0, new[] { "March in Place", "Chair Squat", "Wall Push-up", "Hamstring Stretch" });
        jagged.SetRow(1, new[] { "Rest" });

        AssertEqual(4, jagged.GetColumnCount(0), "Monday has 4 exercises");
        AssertEqual(1, jagged.GetColumnCount(1), "Tuesday has 1 entry");
        AssertEqual("Chair Squat", jagged[0, 1], "Monday 2nd exercise");
        AssertEqual("Rest", jagged[1, 0], "Tuesday rest");

        jagged.AddToRow(1, "Optional Light Walk");
        AssertEqual(2, jagged.GetColumnCount(1), "Tuesday now has 2 entries");
    }

    private static void TestStringSanitizer()
    {
        string raw = "   Wall  Push-up (Beginner)   ";
        string trimmed = StringSanitizer.ManualTrim(raw);
        AssertEqual("Wall  Push-up (Beginner)", trimmed, "Manual trim");

        string lower = StringSanitizer.ManualToLower(trimmed);
        AssertEqual("wall  push-up (beginner)", lower, "Manual to-lower");

        string cleaned = StringSanitizer.CleanAlphaNumericAndSpaces(raw);
        AssertEqual("Wall Push-up (Beginner)", cleaned, "Clean alphanumeric and normalized spaces");

        string[] tokens = StringSanitizer.ManualSplit("Chest,Arms,Core", ',');
        AssertEqual(3, tokens.Length, "Manual split count");
        AssertEqual("Chest", tokens[0], "Token 0");
        AssertEqual("Core", tokens[2], "Token 2");
    }

    private static void TestSinglyLinkedList()
    {
        var list = new SinglyLinkedList<string>();
        list.AddLast("Warm-up");
        list.AddLast("Main");
        list.AddLast("Cool-down");
        list.AddFirst("Pre-check");

        AssertEqual(4, list.Count, "Singly linked list count");
        AssertEqual("Pre-check", list.Head!.Value, "Head value");
        AssertEqual("Cool-down", list.Tail!.Value, "Tail value");

        bool removed = list.Remove("Main");
        AssertTrue(removed, "Removed middle element");
        AssertEqual(3, list.Count, "Count after remove");
    }

    private static void TestDoublyLinkedList()
    {
        var dll = new DoublyLinkedList<string>();
        var n1 = dll.AddLast("March in place");
        var n2 = dll.AddLast("Wall Push-up");
        var n3 = dll.AddLast("Plank");
        var n4 = dll.AddLast("Hamstring Stretch");

        AssertEqual(4, dll.Count, "Doubly linked list count");
        AssertEqual(n2, n1.Next, "Forward navigation");
        AssertEqual(n1, n2.Prev, "Backward navigation");

        // O(1) removal of node during workout
        dll.RemoveNode(n3);
        AssertEqual(3, dll.Count, "Count after node removal");
        AssertEqual(n4, n2.Next, "Link repaired after removal");
        AssertEqual(n2, n4.Prev, "Back-link repaired after removal");

        // In-place reversal for cool-down
        dll.Reverse();
        AssertEqual("Hamstring Stretch", dll.Head!.Value, "Reversed head");
        AssertEqual("March in place", dll.Tail!.Value, "Reversed tail");
    }

    private static void TestCircularLinkedList()
    {
        var cll = new CircularLinkedList<string>();
        cll.AddLast("Monday");
        cll.AddLast("Tuesday");
        cll.AddLast("Wednesday");

        AssertEqual(3, cll.Count, "Circular list count");
        AssertEqual("Monday", cll.Current!.Value, "Current starts at head");

        var d2 = cll.StepNext();
        AssertEqual("Tuesday", d2, "Next day Tuesday");
        var d3 = cll.StepNext();
        AssertEqual("Wednesday", d3, "Next day Wednesday");
        var d4 = cll.StepNext();
        AssertEqual("Monday", d4, "Wraps around to Monday in circle!");
    }

    private static void TestStackAndUndoRedo()
    {
        var stack = new CustomStack<int>();
        stack.Push(10);
        stack.Push(20);
        stack.Push(30);

        AssertEqual(30, stack.Peek(), "Stack peek");
        AssertEqual(30, stack.Pop(), "Stack pop 1");
        AssertEqual(20, stack.Pop(), "Stack pop 2");
        AssertEqual(1, stack.Count, "Stack count");

        // Test Undo/Redo Manager
        var undoManager = new UndoRedoManager<string>();
        string state = "State 1";
        undoManager.RecordState(state);

        state = "State 2";
        undoManager.RecordState(state);

        state = "State 3";

        AssertTrue(undoManager.CanUndo, "Can undo");
        state = undoManager.Undo(state)!;
        AssertEqual("State 2", state, "Undo to State 2");

        state = undoManager.Undo(state)!;
        AssertEqual("State 1", state, "Undo to State 1");

        AssertTrue(undoManager.CanRedo, "Can redo");
        state = undoManager.Redo(state)!;
        AssertEqual("State 2", state, "Redo back to State 2");
    }

    private static void TestExpressionEvaluator()
    {
        // proposal: evaluation of a workload expression such as (12 x 3) + (10 x 4)
        double r1 = ExpressionEvaluator.Evaluate("(12 * 3) + (10 * 4)");
        AssertEqual(76.0, r1, "Workload (12 * 3) + (10 * 4) = 76");

        double r2 = ExpressionEvaluator.Evaluate("(12 x 3) + (10 x 4) + (15 x 3)");
        AssertEqual(121.0, r2, "Workload with 'x' operator");

        double r3 = ExpressionEvaluator.Evaluate("100 + (25 * 4) / 2");
        AssertEqual(150.0, r3, "Operator precedence");
    }

    private static void TestQueues()
    {
        // Linear queue
        var q = new CustomQueue<int>();
        q.Enqueue(1);
        q.Enqueue(2);
        q.Enqueue(3);
        AssertEqual(1, q.Dequeue(), "FIFO queue dequeue");

        // Circular queue
        var cq = new CircularQueue<string>(3);
        AssertTrue(cq.Enqueue("A"), "Enqueue A");
        AssertTrue(cq.Enqueue("B"), "Enqueue B");
        AssertTrue(cq.Enqueue("C"), "Enqueue C");
        AssertTrue(cq.IsFull, "Circular queue is full");
        AssertEqual("A", cq.Dequeue(), "Dequeue A");
        AssertTrue(cq.Enqueue("D"), "Enqueue D after wrap-around");
        AssertEqual("B", cq.Dequeue(), "Dequeue B");

        // Deque
        var deque = new CustomDeque<string>();
        deque.AddFront("Front1");
        deque.AddBack("Back1");
        deque.AddFront("Front2");
        AssertEqual("Front2", deque.RemoveFront(), "Deque remove front");
        AssertEqual("Back1", deque.RemoveBack(), "Deque remove back");

        // Priority Queue
        var pq = new PriorityQueue<string>();
        pq.Enqueue("Chest", 50);
        pq.Enqueue("Legs", 90);
        pq.Enqueue("Core", 30);
        AssertEqual("Legs", pq.Dequeue(), "Highest priority 90 dequeued first");
        AssertEqual("Chest", pq.Dequeue(), "Second highest 50 dequeued next");
    }

    private static void TestBinarySearchTree()
    {
        var bst = new BinarySearchTree<int, string>();
        bst.Insert(30, "Plan 30m");
        bst.Insert(20, "Plan 20m");
        bst.Insert(45, "Plan 45m");
        bst.Insert(15, "Plan 15m");
        bst.Insert(60, "Plan 60m");

        AssertEqual(5, bst.Count, "BST node count");

        // In-order traversal must be sorted
        var inOrder = bst.InOrderTraversal();
        AssertEqual("Plan 15m", inOrder[0], "First in-order");
        AssertEqual("Plan 60m", inOrder[4], "Last in-order");

        // Range query: plans between 20 and 45 minutes
        var range = bst.GetRange(20, 45);
        AssertEqual(3, range.Count, "Range [20, 45] returns 3 plans");
        AssertEqual("Plan 20m", range[0], "Range item 1");
        AssertEqual("Plan 30m", range[1], "Range item 2");
        AssertEqual("Plan 45m", range[2], "Range item 3");
    }

    private static void TestAvlTree()
    {
        var avl = new AvlTree<int, string>();
        // Insert strictly increasing values (10, 20, 30, 40, 50, 60, 70)
        // A plain BST would degenerate into height 7
        // An AVL tree must rebalance via rotations to height <= 4!
        for (int i = 1; i <= 7; i++)
        {
            avl.Insert(i * 10, $"Plan {i * 10}m");
        }

        AssertEqual(7, avl.Count, "AVL node count");
        AssertTrue(avl.GetHeight() <= 4, $"AVL height is balanced ({avl.GetHeight()} <= 4)");

        var inOrder = avl.InOrderTraversal();
        AssertEqual("Plan 10m", inOrder[0], "AVL in-order first");
        AssertEqual("Plan 70m", inOrder[6], "AVL in-order last");
    }

    private static void TestHashTables()
    {
        // Chaining
        var chaining = new ChainingHashTable<string, string>();
        chaining.Insert("Wall Push-up", "Chest/Arms");
        chaining.Insert("Chair Squat", "Legs");
        chaining.Insert("Plank", "Core");

        AssertTrue(chaining.TryGetValue("Plank", out string? v1), "Chaining find Plank");
        AssertEqual("Core", v1, "Chaining Plank muscle");
        AssertTrue(!chaining.ContainsKey("NonExistent"), "Chaining key not found");

        // Open Addressing
        var openAddr = new OpenAddressingHashTable<string, string>();
        openAddr.Insert("Wall Push-up", "Chest/Arms");
        openAddr.Insert("Chair Squat", "Legs");
        openAddr.Insert("Plank", "Core");

        AssertTrue(openAddr.TryGetValue("Chair Squat", out string? v2), "Open addressing find Chair Squat");
        AssertEqual("Legs", v2, "Open addressing muscle");

        openAddr.Remove("Chair Squat");
        AssertTrue(!openAddr.ContainsKey("Chair Squat"), "Tombstone deletion verified");
    }

    private static void TestSortingAlgorithms()
    {
        int[] original = { 45, 12, 85, 32, 89, 39, 69, 44, 42, 1, 99 };
        int[] sorted = (int[])original.Clone();
        Array.Sort(sorted);

        // Bubble Sort
        int[] a1 = (int[])original.Clone();
        SortingAlgorithms.BubbleSort(a1);
        AssertArraysEqual(sorted, a1, "Bubble Sort");

        // Insertion Sort
        int[] a2 = (int[])original.Clone();
        SortingAlgorithms.InsertionSort(a2);
        AssertArraysEqual(sorted, a2, "Insertion Sort");

        // Selection Sort
        int[] a3 = (int[])original.Clone();
        SortingAlgorithms.SelectionSort(a3);
        AssertArraysEqual(sorted, a3, "Selection Sort");

        // Merge Sort
        int[] a4 = (int[])original.Clone();
        SortingAlgorithms.MergeSort(a4);
        AssertArraysEqual(sorted, a4, "Merge Sort");

        // Quick Sort
        int[] a5 = (int[])original.Clone();
        SortingAlgorithms.QuickSort(a5);
        AssertArraysEqual(sorted, a5, "Quick Sort");

        // Heap Sort
        int[] a6 = (int[])original.Clone();
        SortingAlgorithms.HeapSort(a6);
        AssertArraysEqual(sorted, a6, "Heap Sort");
    }

    private static void AssertArraysEqual(int[] expected, int[] actual, string algorithmName)
    {
        AssertEqual(expected.Length, actual.Length, $"{algorithmName} length");
        for (int i = 0; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
                throw new Exception($"{algorithmName} failed at index {i}. Expected: {expected[i]}, Actual: {actual[i]}");
        }
    }

    private static void TestSearchingAlgorithms()
    {
        int[] arr = { 10, 20, 30, 40, 50, 60, 70, 80 };

        var m1 = SearchingAlgorithms.LinearSearch(arr, 50);
        AssertEqual(4, m1.FoundIndex, "Linear search found index");

        var m2 = SearchingAlgorithms.BinarySearch(arr, 70);
        AssertEqual(6, m2.FoundIndex, "Binary search found index");

        var m3 = SearchingAlgorithms.BinarySearch(arr, 999);
        AssertEqual(-1, m3.FoundIndex, "Binary search not found");
    }

    private static void TestMultiplicationAlgorithms()
    {
        string n1 = "123456789";
        string n2 = "987654321";

        string gs = MathAlgorithms.MultiplyGradeSchool(n1, n2);
        string karatsuba = MathAlgorithms.MultiplyKaratsuba(n1, n2);

        AssertEqual("121932631112635269", gs, "Grade-school multiplication product");
        AssertEqual(gs, karatsuba, "Karatsuba matches Grade-school product exactly");
    }

    private static void TestGraphAlgorithms()
    {
        var dm = DataManager.Instance;
        var graph = dm.ExerciseProgressionGraph;

        // BFS: Find substitute for "Standard Push-up"
        var substitutes = GraphAlgorithms.BreadthFirstSearch(graph, "Standard Push-up");
        AssertTrue(substitutes.Count > 0, "BFS found substitutes for Standard Push-up");
        AssertTrue(substitutes.Contains("Knee Push-up") || substitutes.Contains("Diamond Push-up") || substitutes.Contains("Incline Push-up"),
            "Substitutes include push-up family variations");

        // Dijkstra: Level-Up path from "Wall Push-up" to "Standard Push-up"
        var dijkstra = GraphAlgorithms.Dijkstra(graph, "Wall Push-up", "Standard Push-up");
        AssertTrue(dijkstra.IsReachable, "Path is reachable");
        AssertEqual("Wall Push-up", dijkstra.Path[0], "Path starts at Wall Push-up");
        AssertEqual("Standard Push-up", dijkstra.Path[dijkstra.Path.Count - 1], "Path ends at Standard Push-up");
        AssertTrue(dijkstra.StepDescriptions.Count > 0, "Dijkstra generated step descriptions");

        // Connected Components
        var components = GraphAlgorithms.FindConnectedComponents(graph);
        AssertTrue(components.Count > 0, "Exercise families connected components found");
    }

    private static void TestWorkedExamplePlanGeneration()
    {
        // Proposal Section 9 Worked Example:
        // 32-year-old woman, 160 cm, 70 kg, walks 2 days a week, goal: lose weight, 3 days/wk, 30 min, chair only, sore knees
        var profile = new UserProfile
        {
            Name = "Athlete",
            Age = 32,
            Gender = GenderType.Female,
            HeightCm = 160.0,
            WeightKg = 70.0,
            CurrentActivityDays = 2,
            CurrentActivityKind = "walking",
            Goal = GoalType.LoseWeight,
            WorkoutDaysPerWeek = 3,
            MinutesPerSession = 30,
            HasChairOrBench = true,
            HasKneeLimitation = true
        };
        profile.DeriveFitnessLevel();

        AssertEqual(FitnessLevel.Beginner, profile.StartingLevel, "Derived starting level is Beginner");
        AssertEqual(27.3, profile.CalculateBmi(), "Calculated BMI ~ 27.3");
        AssertEqual("Overweight", profile.GetBmiCategory(), "BMI Category Overweight");

        var generator = new PlanGeneratorService();
        var plan = generator.GeneratePlan(profile, "Personalized Workout Plan");

        AssertEqual(7, plan.Days.Count, "Plan has 7 days");
        AssertEqual(3, plan.WorkoutDaysCount, "Plan has 3 active workout days");

        // Monday (0), Wednesday (2), Friday (4) are active; Tue, Thu, Sat, Sun are rest
        AssertTrue(!plan.Days[0].IsRestDay, "Monday is an active workout day");
        AssertTrue(plan.Days[1].IsRestDay, "Tuesday is a rest day");
        AssertTrue(!plan.Days[2].IsRestDay, "Wednesday is an active workout day");
        AssertTrue(plan.Days[3].IsRestDay, "Thursday is a rest day");
        AssertTrue(!plan.Days[4].IsRestDay, "Friday is an active workout day");
        AssertTrue(plan.Days[5].IsRestDay, "Saturday is a rest day");
        AssertTrue(plan.Days[6].IsRestDay, "Sunday is a rest day");

        // Verify knee limitation: No bodyweight squats or jumping jacks
        for (int d = 0; d < 7; d++)
        {
            var day = plan.Days[d];
            for (int e = 0; e < day.Exercises.Count; e++)
            {
                var name = day.Exercises[e].ExerciseName;
                AssertTrue(!name.Contains("Jumping Jacks", StringComparison.OrdinalIgnoreCase), $"No jumping jacks in day {d} due to knee flag");
                AssertTrue(!name.Equals("Bodyweight Squat", StringComparison.OrdinalIgnoreCase), $"No bodyweight squat in day {d} due to knee flag");
                AssertTrue(!name.Contains("Jump Squat", StringComparison.OrdinalIgnoreCase), $"No jump squat in day {d} due to knee flag");
            }
        }

        // Verify warmup and cooldown are included
        var mon = plan.Days[0];
        AssertEqual("Warm-up", mon.Exercises[0].Phase, "First exercise is Warm-up");
        AssertEqual("Cool-down", mon.Exercises[mon.Exercises.Count - 1].Phase, "Last exercise is Cool-down");
    }

    private static void TestPlanAdaptation()
    {
        var dm = DataManager.Instance;
        var profile = dm.CurrentProfile;
        var generator = new PlanGeneratorService();
        var basePlan = generator.GeneratePlan(profile);

        var adapter = new PlanAdaptationService();

        // 1. Simulate All Sessions Just Right -> Small step up
        var justRightLogs = new CustomArray<WorkoutSessionLog>();
        for (int i = 0; i < basePlan.WorkoutDaysCount; i++)
        {
            justRightLogs.Add(new WorkoutSessionLog { Rating = SessionRating.JustRight });
        }
        var outcome1 = adapter.AdaptForNextWeek(basePlan, justRightLogs, profile);
        AssertTrue(outcome1.AdjustmentsMade.Count > 0, "Adjustments made for 'just right' week");
        AssertTrue(outcome1.SummaryMessage.Contains("progressive overload", StringComparison.OrdinalIgnoreCase) ||
                   outcome1.SummaryMessage.Contains("intensity", StringComparison.OrdinalIgnoreCase), "Positive adaptation message");

        // 2. Simulate 2 Sessions Too Hard -> Easier week
        var hardLogs = new CustomArray<WorkoutSessionLog>();
        hardLogs.Add(new WorkoutSessionLog { Rating = SessionRating.TooHard });
        hardLogs.Add(new WorkoutSessionLog { Rating = SessionRating.TooHard });
        var outcome2 = adapter.AdaptForNextWeek(basePlan, hardLogs, profile);
        AssertTrue(outcome2.SummaryMessage.Contains("gentler", StringComparison.OrdinalIgnoreCase) ||
                   outcome2.SummaryMessage.Contains("recovery", StringComparison.OrdinalIgnoreCase), "Easier week message for hard ratings");
    }

    private static void TestAuthenticationDsa()
    {
        // 1. Test Hand-written Prefix Tree (Trie)
        var trie = new Trie();
        trie.Insert("athlete1");
        trie.Insert("athlete2");
        trie.Insert("athlete3");
        trie.Insert("athlete4");

        AssertEqual(4, trie.WordCount, "Trie contains 4 words");
        AssertTrue(trie.Contains("athlete1", out int d1), "Trie finds 'athlete1'");
        AssertEqual(8, d1, "Trie traversed depth 8 for 'athlete1' (O(L) complexity)");

        AssertTrue(trie.StartsWith("ath", out int d2), "Trie prefix matching for 'ath'");
        AssertEqual(3, d2, "Trie traversed depth 3 for prefix 'ath'");

        AssertTrue(!trie.Contains("unknown_user", out _), "Trie correctly returns false for absent word");

        // 2. Test AuthService integration
        var auth = AuthService.Instance;
        var (loginSuccess, _, user) = auth.LogIn("demo1@homefit.edu", "password123");
        AssertTrue(loginSuccess, "Default demo login succeeds via O(1) Chaining Hash Table");
        AssertEqual("Athlete 1", user!.Name, "User name matches Athlete 1");

        var (failLogin, _, _) = auth.LogIn("demo1@homefit.edu", "wrongpassword");
        AssertTrue(!failLogin, "Login fails with incorrect password");

        // 3. Test New User Sign Up
        string testEmail = $"new_athlete_{Guid.NewGuid():N}@homefit.edu";
        string testUsername = $"athlete_{Guid.NewGuid():N}".Substring(0, 10);
        var (signUpSuccess, _, newUser) = auth.SignUp("New Athlete", testEmail, testUsername, "password123", "BuildMuscle", "Intermediate");
        AssertTrue(signUpSuccess, "Sign up succeeds and inserts into Hash Table + Trie");

        var (secondSignUp, _, _) = auth.SignUp("Duplicate User", testEmail, testUsername, "password123", "BuildMuscle", "Intermediate");
        AssertTrue(!secondSignUp, "Duplicate username rejected by Trie collision check");
    }
}
