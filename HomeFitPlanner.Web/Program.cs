using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HomeFitPlanner.Core.Algorithms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.DataStructures.Stacks;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

var dataManager = DataManager.Instance;
var planGenerator = new PlanGeneratorService(dataManager);
var planAdapter = new PlanAdaptationService(dataManager);
var benchmarkService = new BenchmarkService();
var undoRedoManager = new UndoRedoManager<WorkoutPlan>();
var authService = AuthService.Instance;

// 0. DSA Authentication & User Registry APIs
app.MapGet("/api/auth/current", () =>
{
    var user = authService.CurrentUser;
    if (user == null) return Results.Ok(new { isAuthenticated = false });
    return Results.Ok(new
    {
        isAuthenticated = true,
        id = user.Id,
        name = user.Name,
        email = user.Email,
        username = user.Username,
        createdAt = user.CreatedAt,
        profile = new
        {
            name = user.Profile.Name,
            startingLevel = user.Profile.StartingLevel.ToString(),
            goal = user.Profile.Goal.ToString()
        }
    });
});

app.MapPost("/api/auth/login", (LoginRequestDto dto) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var (success, message, user) = authService.LogIn(dto.Identifier ?? "", dto.Password ?? "");
    sw.Stop();

    if (!success || user == null)
    {
        return Results.BadRequest(new { success = false, message });
    }

    var stats = authService.GetDsaAuthStats(dto.Identifier);

    return Results.Ok(new
    {
        success = true,
        message,
        lookupElapsedMicroseconds = sw.Elapsed.TotalMicroseconds,
        user = new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            username = user.Username,
            goal = user.Profile.Goal.ToString(),
            level = user.Profile.StartingLevel.ToString()
        },
        dsaMetrics = stats
    });
});

app.MapPost("/api/auth/signup", (SignUpRequestDto dto) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var (success, message, user) = authService.SignUp(
        dto.Name ?? "",
        dto.Email ?? "",
        dto.Username ?? "",
        dto.Password ?? "",
        dto.Goal ?? "LoseWeight",
        dto.Level ?? "Beginner",
        dto.Age ?? 22,
        dto.Gender ?? "Female",
        dto.WeightKg ?? 65,
        dto.HeightCm ?? 162
    );
    sw.Stop();

    if (!success || user == null)
    {
        return Results.BadRequest(new { success = false, message });
    }

    var stats = authService.GetDsaAuthStats(user.Username);

    return Results.Ok(new
    {
        success = true,
        message,
        registrationElapsedMicroseconds = sw.Elapsed.TotalMicroseconds,
        user = new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            username = user.Username,
            goal = user.Profile.Goal.ToString(),
            level = user.Profile.StartingLevel.ToString()
        },
        dsaMetrics = stats
    });
});

app.MapGet("/api/auth/check-username", (string? username) =>
{
    var result = authService.CheckUsernameTrie(username ?? "");
    return Results.Ok(result);
});

app.MapGet("/api/auth/stats", (string? query) =>
{
    var stats = authService.GetDsaAuthStats(query);
    return Results.Ok(stats);
});

app.MapPost("/api/auth/logout", () =>
{
    authService.LogOut();
    return Results.Ok(new { success = true, message = "Logged out successfully" });
});

// 1. Profile APIs
app.MapGet("/api/profile", () =>
{
    var p = dataManager.CurrentProfile;
    return Results.Ok(new
    {
        name = p.Name,
        age = p.Age,
        gender = p.Gender.ToString(),
        heightCm = p.HeightCm,
        weightKg = p.WeightKg,
        currentActivityDays = p.CurrentActivityDays,
        currentActivityKind = p.CurrentActivityKind,
        startingLevel = p.StartingLevel.ToString(),
        goal = p.Goal.ToString(),
        workoutDaysPerWeek = p.WorkoutDaysPerWeek,
        minutesPerSession = p.MinutesPerSession,
        hasChairOrBench = p.HasChairOrBench,
        hasMat = p.HasMat,
        hasResistanceBands = p.HasResistanceBands,
        hasDumbbells = p.HasDumbbells,
        hasKneeLimitation = p.HasKneeLimitation,
        hasBackLimitation = p.HasBackLimitation,
        hasShoulderLimitation = p.HasShoulderLimitation,
        hasWristLimitation = p.HasWristLimitation,
        bmi = p.CalculateBmi(),
        bmiCategory = p.GetBmiCategory(),
        dailyCalories = p.CalculateDailyCalories(),
        currentStreak = p.CurrentStreak,
        bestStreak = p.BestStreak,
        totalWorkoutsCompleted = p.TotalWorkoutsCompleted,
        totalMinutesExercised = p.TotalMinutesExercised
    });
});

app.MapPost("/api/profile", (UserProfileUpdateDto dto) =>
{
    var p = dataManager.CurrentProfile;
    if (!string.IsNullOrEmpty(dto.Name)) p.Name = dto.Name;
    if (dto.Age.HasValue) p.Age = dto.Age.Value;
    if (!string.IsNullOrEmpty(dto.Gender) && Enum.TryParse<GenderType>(dto.Gender, true, out var g)) p.Gender = g;
    if (dto.HeightCm.HasValue) p.HeightCm = dto.HeightCm.Value;
    if (dto.WeightKg.HasValue) p.WeightKg = dto.WeightKg.Value;
    if (dto.CurrentActivityDays.HasValue) p.CurrentActivityDays = dto.CurrentActivityDays.Value;
    if (!string.IsNullOrEmpty(dto.CurrentActivityKind)) p.CurrentActivityKind = dto.CurrentActivityKind;
    if (!string.IsNullOrEmpty(dto.Goal) && Enum.TryParse<GoalType>(dto.Goal, true, out var goal)) p.Goal = goal;
    if (dto.WorkoutDaysPerWeek.HasValue) p.WorkoutDaysPerWeek = dto.WorkoutDaysPerWeek.Value;
    if (dto.MinutesPerSession.HasValue) p.MinutesPerSession = dto.MinutesPerSession.Value;

    if (dto.HasChairOrBench.HasValue) p.HasChairOrBench = dto.HasChairOrBench.Value;
    if (dto.HasMat.HasValue) p.HasMat = dto.HasMat.Value;
    if (dto.HasResistanceBands.HasValue) p.HasResistanceBands = dto.HasResistanceBands.Value;
    if (dto.HasDumbbells.HasValue) p.HasDumbbells = dto.HasDumbbells.Value;

    if (dto.HasKneeLimitation.HasValue) p.HasKneeLimitation = dto.HasKneeLimitation.Value;
    if (dto.HasBackLimitation.HasValue) p.HasBackLimitation = dto.HasBackLimitation.Value;
    if (dto.HasShoulderLimitation.HasValue) p.HasShoulderLimitation = dto.HasShoulderLimitation.Value;
    if (dto.HasWristLimitation.HasValue) p.HasWristLimitation = dto.HasWristLimitation.Value;

    p.DeriveFitnessLevel();

    // Regenerate plan automatically with updated parameters
    var newPlan = planGenerator.GeneratePlan(p);
    dataManager.CurrentPlan = newPlan;
    dataManager.SavePlan(newPlan);

    return Results.Ok(new { message = "Profile updated and new plan generated successfully!", bmi = p.CalculateBmi(), calories = p.CalculateDailyCalories() });
});

// 2. Weekly Plan APIs
app.MapGet("/api/plan", () =>
{
    var plan = dataManager.CurrentPlan;
    if (plan == null)
    {
        plan = planGenerator.GeneratePlan(dataManager.CurrentProfile);
        dataManager.CurrentPlan = plan;
        dataManager.SavePlan(plan);
    }

    return Results.Ok(SerializePlan(plan));
});

app.MapPost("/api/plan/regenerate", () =>
{
    var current = dataManager.CurrentPlan;
    if (current != null) undoRedoManager.RecordState(current.Clone());

    var plan = planGenerator.GeneratePlan(dataManager.CurrentProfile);
    dataManager.CurrentPlan = plan;
    return Results.Ok(SerializePlan(plan));
});

app.MapPost("/api/plan/swap", (SwapRequestDto dto) =>
{
    var plan = dataManager.CurrentPlan;
    if (plan == null || dto.DayIndex < 0 || dto.DayIndex >= plan.Days.Count)
        return Results.BadRequest(new { error = "Invalid plan or day index." });

    var day = plan.Days[dto.DayIndex];
    if (dto.ExerciseIndex < 0 || dto.ExerciseIndex >= day.Exercises.Count)
        return Results.BadRequest(new { error = "Invalid exercise index." });

    var currentEx = day.Exercises[dto.ExerciseIndex];
    var candidate = dataManager.FindExerciseByName(dto.NewExerciseName);
    if (candidate == null)
        return Results.BadRequest(new { error = "Target exercise not found." });

    undoRedoManager.RecordState(plan.Clone());

    currentEx.ExerciseId = candidate.Id;
    currentEx.ExerciseName = candidate.Name;
    currentEx.Type = candidate.Type;
    currentEx.MuscleGroup = candidate.MuscleGroup;
    currentEx.HowToSummary = candidate.Tips;
    day.RecalculateTotalMinutes();

    return Results.Ok(SerializePlan(plan));
});

app.MapPost("/api/plan/remove", (RemoveRequestDto dto) =>
{
    var plan = dataManager.CurrentPlan;
    if (plan == null || dto.DayIndex < 0 || dto.DayIndex >= plan.Days.Count)
        return Results.BadRequest(new { error = "Invalid day index." });

    var day = plan.Days[dto.DayIndex];
    if (dto.ExerciseIndex < 0 || dto.ExerciseIndex >= day.Exercises.Count)
        return Results.BadRequest(new { error = "Invalid exercise index." });

    undoRedoManager.RecordState(plan.Clone());
    day.Exercises.RemoveAt(dto.ExerciseIndex);
    if (day.Exercises.Count == 0)
    {
        day.IsRestDay = true;
    }
    day.RecalculateTotalMinutes();

    return Results.Ok(SerializePlan(plan));
});

app.MapPost("/api/plan/add", (AddExerciseRequestDto dto) =>
{
    var plan = dataManager.CurrentPlan;
    if (plan == null || dto.DayIndex < 0 || dto.DayIndex >= plan.Days.Count)
        return Results.BadRequest(new { error = "Invalid day index." });

    var ex = dataManager.FindExerciseByName(dto.ExerciseName);
    if (ex == null) return Results.BadRequest(new { error = "Exercise not found." });

    undoRedoManager.RecordState(plan.Clone());
    var day = plan.Days[dto.DayIndex];
    int insertIndex = day.Exercises.Count > 1 ? day.Exercises.Count - 1 : day.Exercises.Count;
    day.Exercises.Insert(insertIndex, new WorkoutExercise
    {
        ExerciseId = ex.Id,
        ExerciseName = ex.Name,
        Type = ex.Type,
        MuscleGroup = ex.MuscleGroup,
        Sets = ex.DefaultSets,
        Reps = ex.DefaultReps,
        Seconds = ex.DefaultSeconds,
        IsTimed = ex.IsTimed,
        Phase = "Main",
        EstimatedMinutes = ex.TimeMinutes,
        HowToSummary = ex.Tips
    });
    day.IsRestDay = false;
    day.RecalculateTotalMinutes();

    return Results.Ok(SerializePlan(plan));
});

app.MapPost("/api/plan/undo", () =>
{
    if (!undoRedoManager.CanUndo) return Results.BadRequest(new { error = "No undo states available." });
    var prev = undoRedoManager.Undo(dataManager.CurrentPlan!);
    if (prev != null) dataManager.CurrentPlan = prev;
    return Results.Ok(SerializePlan(dataManager.CurrentPlan!));
});

app.MapPost("/api/plan/redo", () =>
{
    if (!undoRedoManager.CanRedo) return Results.BadRequest(new { error = "No redo states available." });
    var next = undoRedoManager.Redo(dataManager.CurrentPlan!);
    if (next != null) dataManager.CurrentPlan = next;
    return Results.Ok(SerializePlan(dataManager.CurrentPlan!));
});

app.MapPost("/api/plan/adapt", () =>
{
    var plan = dataManager.CurrentPlan;
    if (plan == null) return Results.BadRequest(new { error = "No plan to adapt." });

    var outcome = planAdapter.AdaptForNextWeek(plan, dataManager.SessionLogs, dataManager.CurrentProfile);
    undoRedoManager.RecordState(plan.Clone());
    dataManager.CurrentPlan = outcome.AdjustedPlan;

    return Results.Ok(new
    {
        message = outcome.SummaryMessage,
        adjustments = outcome.AdjustmentsMade.ToArray(),
        plan = SerializePlan(outcome.AdjustedPlan)
    });
});

app.MapPost("/api/plan/save", (SavePlanDto dto) =>
{
    var plan = dataManager.CurrentPlan;
    if (plan == null) return Results.BadRequest(new { error = "No current plan." });

    var clone = plan.Clone();
    if (!string.IsNullOrEmpty(dto.Name)) clone.PlanName = dto.Name;
    dataManager.SavePlan(clone);

    return Results.Ok(new { message = $"Plan '{clone.PlanName}' successfully saved into BST and AVL tree!" });
});

// 3. Exercise Library APIs
app.MapGet("/api/exercises", (string? search, string? muscle, string? equip, string? type, string? sort) =>
{
    var list = new CustomArray<Exercise>();
    for (int i = 0; i < dataManager.Exercises.Count; i++)
    {
        var ex = dataManager.Exercises[i];
        if (!string.IsNullOrEmpty(search) && !StringSanitizer.ManualContains(ex.Name, search, true)) continue;
        if (!string.IsNullOrEmpty(muscle) && muscle != "All" && ex.MuscleGroup.ToString() != muscle) continue;
        if (!string.IsNullOrEmpty(equip) && equip != "All" && ex.Equipment.ToString() != equip) continue;
        if (!string.IsNullOrEmpty(type) && type != "All" && ex.Type.ToString() != type) continue;
        list.Add(ex);
    }

    var arr = list.ToArray();
    if (sort == "diff_asc") SortingAlgorithms.MergeSort(arr, Comparer<Exercise>.Create((a, b) => a.Difficulty.CompareTo(b.Difficulty)));
    else if (sort == "diff_desc") SortingAlgorithms.HeapSort(arr, Comparer<Exercise>.Create((a, b) => b.Difficulty.CompareTo(a.Difficulty)));
    else if (sort == "time_asc") SortingAlgorithms.InsertionSort(arr, Comparer<Exercise>.Create((a, b) => a.TimeMinutes.CompareTo(b.TimeMinutes)));
    else SortingAlgorithms.QuickSort(arr, Comparer<Exercise>.Create((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)));

    return Results.Ok(arr.Select(e => new
    {
        id = e.Id,
        name = e.Name,
        muscleGroup = e.MuscleGroup.ToString(),
        difficulty = e.Difficulty,
        equipment = e.Equipment.ToString(),
        type = e.Type.ToString(),
        timeMinutes = e.TimeMinutes,
        workload = e.GetDefaultWorkDescription(),
        stressesKnees = e.StressesKnees,
        stressesBack = e.StressesBack,
        stressesShoulders = e.StressesShoulders,
        stressesWrists = e.StressesWrists,
        isHighImpact = e.IsHighImpact,
        instructions = e.Instructions,
        tips = e.Tips,
        easier = !string.IsNullOrEmpty(e.EasierVariationId) ? dataManager.FindExerciseById(e.EasierVariationId)?.Name : null,
        harder = !string.IsNullOrEmpty(e.HarderVariationId) ? dataManager.FindExerciseById(e.HarderVariationId)?.Name : null
    }));
});

// 4. Graph & Progression APIs
app.MapGet("/api/graph/substitutes", (string exerciseName) =>
{
    var p = dataManager.CurrentProfile;
    var baseEx = dataManager.FindExerciseByName(exerciseName);
    if (baseEx == null) return Results.NotFound(new { error = "Exercise not found." });

    var substitutes = GraphAlgorithms.BreadthFirstSearch(dataManager.ExerciseProgressionGraph, exerciseName, candName =>
    {
        var cand = dataManager.FindExerciseByName(candName);
        if (cand == null) return false;
        if (cand.MuscleGroup != baseEx.MuscleGroup && cand.MuscleGroup != MuscleGroup.FullBody) return false;
        if (p.HasKneeLimitation && cand.StressesKnees) return false;
        if (p.HasBackLimitation && cand.StressesBack) return false;
        if (p.HasShoulderLimitation && cand.StressesShoulders) return false;
        if (p.HasWristLimitation && cand.StressesWrists) return false;
        if (p.Age >= 50 && cand.IsHighImpact) return false;
        return true;
    });

    return Results.Ok(new
    {
        source = exerciseName,
        muscle = baseEx.MuscleGroup.ToString(),
        substitutes = substitutes.ToArray().Select(s =>
        {
            var ex = dataManager.FindExerciseByName(s);
            return new
            {
                id = ex?.Id ?? "",
                name = s,
                equipment = ex?.Equipment.ToString() ?? "None",
                difficulty = ex?.Difficulty ?? 1,
                muscle = ex?.MuscleGroup.ToString() ?? "FullBody",
                type = ex?.Type.ToString() ?? "Strength",
                instructions = ex?.Tips ?? "Perform with steady breathing and full range of motion.",
                stressesKnees = ex?.StressesKnees ?? false,
                isHighImpact = ex?.IsHighImpact ?? false
            };
        })
    });
});

app.MapGet("/api/graph/levelup", (string start, string target) =>
{
    var result = GraphAlgorithms.Dijkstra(dataManager.ExerciseProgressionGraph, start, target);
    return Results.Ok(new
    {
        start,
        target,
        reachable = result.IsReachable,
        totalDifficultyJump = result.TotalDistance,
        path = result.Path.ToArray(),
        steps = result.StepDescriptions.ToArray()
    });
});

app.MapGet("/api/graph/data", () =>
{
    var vertices = dataManager.ExerciseProgressionGraph.GetAllVertices().ToArray();
    var edges = new System.Collections.Generic.List<object>();

    for (int i = 0; i < vertices.Length; i++)
    {
        var vEdges = dataManager.ExerciseProgressionGraph.GetEdgesByIndex(i);
        for (int j = 0; j < vEdges.Count; j++)
        {
            int tgtIdx = dataManager.ExerciseProgressionGraph.IndexOfVertex(vEdges[j].Target);
            if (tgtIdx > i)
            {
                edges.Add(new
                {
                    from = vertices[i],
                    to = vEdges[j].Target,
                    weight = vEdges[j].Weight,
                    label = vEdges[j].Label
                });
            }
        }
    }

    return Results.Ok(new
    {
        nodes = vertices.Select(v =>
        {
            var ex = dataManager.FindExerciseByName(v);
            return new
            {
                id = v,
                label = v,
                muscle = ex?.MuscleGroup.ToString() ?? "FullBody",
                difficulty = ex?.Difficulty ?? 1,
                equipment = ex?.Equipment.ToString() ?? "None",
                instructions = ex?.Tips ?? "",
                easier = !string.IsNullOrEmpty(ex?.EasierVariationId) ? dataManager.FindExerciseById(ex.EasierVariationId)?.Name : null,
                harder = !string.IsNullOrEmpty(ex?.HarderVariationId) ? dataManager.FindExerciseById(ex.HarderVariationId)?.Name : null
            };
        }),
        edges
    });
});

// 5. Trees & Saved Plans APIs
app.MapGet("/api/plans/saved", (int? exactDuration, int? minDuration, int? maxDuration) =>
{
    CustomArray<WorkoutPlan> plans;
    if (exactDuration.HasValue)
    {
        plans = dataManager.SearchPlansByDuration(exactDuration.Value);
    }
    else if (minDuration.HasValue && maxDuration.HasValue)
    {
        plans = dataManager.SearchPlansByDurationRange(minDuration.Value, maxDuration.Value);
    }
    else
    {
        plans = dataManager.SavedPlansList;
    }

    return Results.Ok(new
    {
        bstHeight = dataManager.SavedPlansBst.GetHeight(),
        avlHeight = dataManager.SavedPlansAvl.GetHeight(),
        bstCount = dataManager.SavedPlansBst.Count,
        avlCount = dataManager.SavedPlansAvl.Count,
        bstRoot = SerializeBstNode(dataManager.SavedPlansBst.Root),
        avlRoot = SerializeAvlNode(dataManager.SavedPlansAvl.Root),
        plans = plans.ToArray().Select(p => new
        {
            id = p.Id,
            name = p.PlanName,
            goal = p.Goal.ToString(),
            workoutDays = p.WorkoutDaysCount,
            duration = p.TargetSessionMinutes,
            created = p.CreatedDate.ToString("yyyy-MM-dd")
        })
    });
});

app.MapPost("/api/plans/insert-custom", (QuickInsertPlanDto dto) =>
{
    int duration = dto.Duration > 0 ? dto.Duration : 30;
    string name = !string.IsNullOrWhiteSpace(dto.Name) ? dto.Name : $"Custom Routine ({duration}m)";
    Enum.TryParse<GoalType>(dto.Goal, true, out var goal);

    var plan = new WorkoutPlan
    {
        Id = Guid.NewGuid().ToString("N"),
        PlanName = name,
        Goal = goal,
        FitnessLevel = FitnessLevel.Intermediate,
        TargetSessionMinutes = duration,
        WorkoutDaysCount = dto.WorkoutDays > 0 ? dto.WorkoutDays : 3
    };

    dataManager.SavePlan(plan);

    return Results.Ok(new
    {
        message = $"Inserted '{plan.PlanName}' into BST and AVL trees at Key = {duration}m!",
        bstHeight = dataManager.SavedPlansBst.GetHeight(),
        avlHeight = dataManager.SavedPlansAvl.GetHeight(),
        planId = plan.Id
    });
});

app.MapDelete("/api/plans/delete/{id}", (string id) =>
{
    bool deleted = dataManager.DeleteSavedPlan(id);
    if (deleted)
    {
        return Results.Ok(new { message = "Plan removed from trees and saved list." });
    }
    return Results.NotFound(new { error = "Plan not found." });
});

app.MapPost("/api/plans/load/{id}", (string id) =>
{
    for (int i = 0; i < dataManager.SavedPlansList.Count; i++)
    {
        if (dataManager.SavedPlansList[i].Id == id)
        {
            dataManager.CurrentPlan = dataManager.SavedPlansList[i].Clone();
            return Results.Ok(new { message = $"Plan '{dataManager.CurrentPlan.PlanName}' loaded successfully!" });
        }
    }
    return Results.NotFound(new { error = "Plan not found." });
});

// 6. Workout Session & Progress APIs
app.MapPost("/api/session/finish", (FinishSessionDto dto) =>
{
    double calculatedWorkload = ExpressionEvaluator.Evaluate(dto.Expression ?? "(10 * 3)");
    var rating = Enum.TryParse<SessionRating>(dto.Rating, true, out var r) ? r : SessionRating.JustRight;

    var log = new WorkoutSessionLog
    {
        DayName = DateTime.Now.DayOfWeek.ToString(),
        PlanName = dataManager.CurrentPlan?.PlanName ?? "Custom Routine",
        MinutesTrained = dto.MinutesTrained,
        ExercisesCompleted = dto.ExercisesCompleted,
        TotalExercises = dto.TotalExercises,
        Rating = rating,
        WorkloadExpression = dto.Expression ?? "(10 * 3)",
        CalculatedWorkload = calculatedWorkload
    };
    dataManager.SessionLogs.Add(log);

    var p = dataManager.CurrentProfile;
    p.TotalWorkoutsCompleted++;
    p.TotalMinutesExercised += dto.MinutesTrained;
    p.CurrentStreak++;
    if (p.CurrentStreak > p.BestStreak) p.BestStreak = p.CurrentStreak;

    return Results.Ok(new { message = "Workout session recorded successfully!", volume = calculatedWorkload });
});

app.MapGet("/api/progress", () =>
{
    var p = dataManager.CurrentProfile;
    return Results.Ok(new
    {
        totalWorkouts = p.TotalWorkoutsCompleted,
        totalMinutes = p.TotalMinutesExercised,
        currentStreak = p.CurrentStreak,
        bestStreak = p.BestStreak,
        weights = dataManager.WeightLogs.ToArray().Select(w => new { date = w.Date.ToString("MM/dd"), weight = w.WeightKg, bmi = w.Bmi }),
        history = dataManager.SessionLogs.ToArray().Reverse().Select(s => new
        {
            date = s.Date.ToString("yyyy-MM-dd"),
            day = s.DayName,
            plan = s.PlanName,
            minutes = s.MinutesTrained,
            exercises = $"{s.ExercisesCompleted}/{s.TotalExercises}",
            rating = s.Rating.ToString(),
            expression = $"{s.WorkloadExpression} = {s.CalculatedWorkload:F0}"
        })
    });
});

app.MapPost("/api/progress/weight", (WeightLogDto dto) =>
{
    var p = dataManager.CurrentProfile;
    p.WeightKg = dto.WeightKg;
    double bmi = p.CalculateBmi();

    dataManager.WeightLogs.Add(new WeightLogEntry
    {
        Date = DateTime.Now,
        WeightKg = dto.WeightKg,
        Bmi = bmi,
        Note = dto.Note ?? ""
    });

    return Results.Ok(new { message = "Weight logged successfully!", bmi });
});

// 7. Performance Lab APIs
app.MapPost("/api/benchmarks/run", (BenchmarkRequestDto dto) =>
{
    BenchmarkComparisonGroup group = dto.ComparisonType switch
    {
        0 => benchmarkService.RunSortingComparison(dto.InputSize),
        1 => benchmarkService.RunSearchComparison(dto.InputSize),
        2 => benchmarkService.RunTreesComparison(dto.InputSize),
        3 => benchmarkService.RunHashTableComparison(dto.InputSize),
        _ => benchmarkService.RunMultiplicationComparison(dto.InputSize)
    };

    return Results.Ok(new
    {
        category = group.CategoryName,
        inputSize = group.InputSize,
        results = group.Items.ToArray().Select(item => new
        {
            algorithm = item.AlgorithmName,
            microseconds = item.ElapsedMicroseconds,
            operations = item.OperationsCount,
            timeComplexity = item.TheoreticalTimeComplexity,
            spaceComplexity = item.TheoreticalSpaceComplexity,
            details = item.Details
        })
    });
});

app.MapFallbackToFile("index.html");

app.Run();

object SerializePlan(WorkoutPlan plan)
{
    return new
    {
        id = plan.Id,
        name = plan.PlanName,
        goal = plan.Goal.ToString(),
        level = plan.FitnessLevel.ToString(),
        targetMinutes = plan.TargetSessionMinutes,
        totalWeeklyMinutes = plan.TotalWeeklyMinutes,
        workoutDaysCount = plan.WorkoutDaysCount,
        days = plan.Days.ToArray().Select(d => new
        {
            dayIndex = d.DayIndex,
            dayName = d.DayName,
            focusType = d.FocusType.ToString(),
            focusDescription = d.FocusDescription,
            isRestDay = d.IsRestDay,
            totalMinutes = d.TotalMinutes,
            exercises = d.Exercises.ToArray().Select(e => new
            {
                id = e.ExerciseId,
                name = e.ExerciseName,
                phase = e.Phase,
                sets = e.Sets,
                reps = e.Reps,
                seconds = e.Seconds,
                isTimed = e.IsTimed,
                restSeconds = e.RestSeconds,
                estimatedMinutes = e.EstimatedMinutes,
                workload = e.GetWorkloadString(),
                instructions = e.HowToSummary
            })
        })
    };
}

object? SerializeBstNode(HomeFitPlanner.Core.DataStructures.Trees.BstNode<int, WorkoutPlan>? node)
{
    if (node == null) return null;
    return new
    {
        key = node.Key,
        plan = new
        {
            id = node.Value.Id,
            name = node.Value.PlanName,
            duration = node.Value.TargetSessionMinutes,
            goal = node.Value.Goal.ToString(),
            workoutDays = node.Value.WorkoutDaysCount,
            weeklyMinutes = node.Value.TotalWeeklyMinutes
        },
        left = SerializeBstNode(node.Left),
        right = SerializeBstNode(node.Right)
    };
}

object? SerializeAvlNode(HomeFitPlanner.Core.DataStructures.Trees.AvlNode<int, WorkoutPlan>? node)
{
    if (node == null) return null;
    int hl = node.Left?.Height ?? 0;
    int hr = node.Right?.Height ?? 0;
    int bf = hl - hr;
    return new
    {
        key = node.Key,
        height = node.Height,
        balanceFactor = bf,
        plan = new
        {
            id = node.Value.Id,
            name = node.Value.PlanName,
            duration = node.Value.TargetSessionMinutes,
            goal = node.Value.Goal.ToString(),
            workoutDays = node.Value.WorkoutDaysCount,
            weeklyMinutes = node.Value.TotalWeeklyMinutes
        },
        left = SerializeAvlNode(node.Left),
        right = SerializeAvlNode(node.Right)
    };
}

// DTO helper records
record UserProfileUpdateDto(string? Name, int? Age, string? Gender, double? HeightCm, double? WeightKg, int? CurrentActivityDays, string? CurrentActivityKind, string? Goal, int? WorkoutDaysPerWeek, int? MinutesPerSession, bool? HasChairOrBench, bool? HasMat, bool? HasResistanceBands, bool? HasDumbbells, bool? HasKneeLimitation, bool? HasBackLimitation, bool? HasShoulderLimitation, bool? HasWristLimitation);
record LoginRequestDto(string? Identifier, string? Password);
record SignUpRequestDto(string? Name, string? Email, string? Username, string? Password, string? Goal, string? Level, int? Age, string? Gender, double? WeightKg, double? HeightCm);
record QuickInsertPlanDto(string? Name, int Duration, string? Goal, int WorkoutDays);
record SwapRequestDto(int DayIndex, int ExerciseIndex, string NewExerciseName);
record RemoveRequestDto(int DayIndex, int ExerciseIndex);
record AddExerciseRequestDto(int DayIndex, string ExerciseName);
record SavePlanDto(string? Name);
record FinishSessionDto(int MinutesTrained, int ExercisesCompleted, int TotalExercises, string? Rating, string? Expression);
record WeightLogDto(double WeightKg, string? Note);
record BenchmarkRequestDto(int ComparisonType, int InputSize);
