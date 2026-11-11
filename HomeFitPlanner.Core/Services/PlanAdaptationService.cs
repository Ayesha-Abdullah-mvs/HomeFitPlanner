using System;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.Models;

namespace HomeFitPlanner.Core.Services;

public class AdaptationOutcome
{
    public WorkoutPlan AdjustedPlan { get; set; } = new();
    public string SummaryMessage { get; set; } = string.Empty;
    public CustomArray<string> AdjustmentsMade { get; set; } = new();
    public CustomArray<string> NewlyAvoidedExercises { get; set; } = new();
}

/// <summary>
/// Implements weekly plan adaptation based on user performance and session ratings (Table 3).
/// </summary>
public class PlanAdaptationService
{
    private readonly DataManager _dataManager;

    public PlanAdaptationService(DataManager? dataManager = null)
    {
        _dataManager = dataManager ?? DataManager.Instance;
    }

    public AdaptationOutcome AdaptForNextWeek(WorkoutPlan currentPlan, CustomArray<WorkoutSessionLog> recentSessions, UserProfile profile)
    {
        var outcome = new AdaptationOutcome();
        var adjusted = currentPlan.Clone();
        adjusted.PlanName = $"{currentPlan.PlanName} (Week {profile.TotalWorkoutsCompleted / Math.Max(1, currentPlan.WorkoutDaysCount) + 1})";
        adjusted.CreatedDate = DateTime.Now;

        int totalPlanned = currentPlan.WorkoutDaysCount;
        int completedCount = 0;
        int tooEasyCount = 0;
        int justRightCount = 0;
        int tooHardCount = 0;

        for (int i = 0; i < recentSessions.Count; i++)
        {
            var log = recentSessions[i];
            completedCount++;
            if (log.Rating == SessionRating.TooEasy) tooEasyCount++;
            else if (log.Rating == SessionRating.JustRight) justRightCount++;
            else if (log.Rating == SessionRating.TooHard) tooHardCount++;
        }

        // Check if any exercises were skipped/removed repeatedly
        CheckAndMarkAvoidedExercises(profile, outcome);

        // Case 1: Two or more rated "Too Hard" OR fewer than half finished -> Make easier
        if (tooHardCount >= 2 || (completedCount < totalPlanned / 2.0))
        {
            ApplyEasierWeek(adjusted, outcome);
            outcome.SummaryMessage = "Week adapted to be gentler: decreased volume, longer rests, and easier exercise variations to allow full recovery.";
        }
        // Case 2: All sessions finished & rated mostly "Too Easy" -> Large step up
        else if (completedCount >= totalPlanned && tooEasyCount > justRightCount && tooEasyCount > tooHardCount)
        {
            ApplyLargeStepUp(adjusted, outcome);
            outcome.SummaryMessage = "Outstanding performance! Great progress detected. Upgraded plan with additional sets and advanced variations.";
        }
        // Case 3: All sessions finished & rated mostly "Just Right" -> Small step up
        else if (completedCount >= totalPlanned && justRightCount >= tooEasyCount && justRightCount >= tooHardCount)
        {
            ApplySmallStepUp(adjusted, outcome);
            outcome.SummaryMessage = "Target intensity reached! Applying progressive overload: increased repetitions and introduced next progression variations.";
        }
        // Case 4: Most sessions finished and rated "Just Right" -> Repeat same week
        else
        {
            outcome.SummaryMessage = "Good consistency. Repeating the current training structure to build foundational strength and habit consistency.";
            outcome.AdjustmentsMade.Add("Maintained existing exercise sets and reps for solid reinforcement.");
        }

        outcome.AdjustedPlan = adjusted;
        return outcome;
    }

    private void ApplySmallStepUp(WorkoutPlan plan, AdaptationOutcome outcome)
    {
        bool replacedOne = false;

        for (int d = 0; d < plan.Days.Count; d++)
        {
            var day = plan.Days[d];
            if (day.IsRestDay) continue;

            for (int e = 0; e < day.Exercises.Count; e++)
            {
                var we = day.Exercises[e];
                if (we.Phase != "Main") continue;

                // Attempt to replace 1 exercise with harder variation
                if (!replacedOne)
                {
                    var baseEx = _dataManager.FindExerciseById(we.ExerciseId);
                    if (baseEx != null && !string.IsNullOrEmpty(baseEx.HarderVariationId))
                    {
                        var harder = _dataManager.FindExerciseById(baseEx.HarderVariationId);
                        if (harder != null)
                        {
                            outcome.AdjustmentsMade.Add($"Progressed '{we.ExerciseName}' to harder variation '{harder.Name}'.");
                            we.ExerciseId = harder.Id;
                            we.ExerciseName = harder.Name;
                            we.HowToSummary = harder.Tips;
                            replacedOne = true;
                            continue;
                        }
                    }
                }

                // Add 1-2 reps or 5 seconds
                if (we.IsTimed)
                {
                    we.Seconds += 5;
                    outcome.AdjustmentsMade.Add($"Increased '{we.ExerciseName}' duration by +5s (Now {we.Seconds}s).");
                }
                else
                {
                    we.Reps += 2;
                    outcome.AdjustmentsMade.Add($"Increased '{we.ExerciseName}' reps by +2 (Now {we.Reps} reps).");
                }
            }
        }
    }

    private void ApplyLargeStepUp(WorkoutPlan plan, AdaptationOutcome outcome)
    {
        for (int d = 0; d < plan.Days.Count; d++)
        {
            var day = plan.Days[d];
            if (day.IsRestDay) continue;

            for (int e = 0; e < day.Exercises.Count; e++)
            {
                var we = day.Exercises[e];
                if (we.Phase != "Main") continue;

                // Add an extra set
                we.Sets = Math.Min(5, we.Sets + 1);

                // Try upgrading to harder variation
                var baseEx = _dataManager.FindExerciseById(we.ExerciseId);
                if (baseEx != null && !string.IsNullOrEmpty(baseEx.HarderVariationId))
                {
                    var harder = _dataManager.FindExerciseById(baseEx.HarderVariationId);
                    if (harder != null)
                    {
                        we.ExerciseId = harder.Id;
                        we.ExerciseName = harder.Name;
                        we.HowToSummary = harder.Tips;
                        outcome.AdjustmentsMade.Add($"Upgraded '{baseEx.Name}' to '{harder.Name}' with {we.Sets} sets.");
                        continue;
                    }
                }

                outcome.AdjustmentsMade.Add($"Added +1 set to '{we.ExerciseName}' (Total: {we.Sets} sets).");
            }
        }
    }

    private void ApplyEasierWeek(WorkoutPlan plan, AdaptationOutcome outcome)
    {
        for (int d = 0; d < plan.Days.Count; d++)
        {
            var day = plan.Days[d];
            if (day.IsRestDay) continue;

            for (int e = 0; e < day.Exercises.Count; e++)
            {
                var we = day.Exercises[e];
                if (we.Phase != "Main") continue;

                // Reduce sets if > 2
                if (we.Sets > 2)
                {
                    we.Sets--;
                }

                // Increase rest period by 15s
                we.RestSeconds += 15;

                // Attempt to replace with easier variation
                var baseEx = _dataManager.FindExerciseById(we.ExerciseId);
                if (baseEx != null && !string.IsNullOrEmpty(baseEx.EasierVariationId))
                {
                    var easier = _dataManager.FindExerciseById(baseEx.EasierVariationId);
                    if (easier != null)
                    {
                        we.ExerciseId = easier.Id;
                        we.ExerciseName = easier.Name;
                        we.HowToSummary = easier.Tips;
                        outcome.AdjustmentsMade.Add($"Scaled '{baseEx.Name}' back to gentler move '{easier.Name}'.");
                        continue;
                    }
                }

                outcome.AdjustmentsMade.Add($"Reduced volume and increased rest for '{we.ExerciseName}' to {we.RestSeconds}s.");
            }
        }
    }

    private void CheckAndMarkAvoidedExercises(UserProfile profile, AdaptationOutcome outcome)
    {
        // If an exercise is skipped or removed in two sessions, mark as avoided
        // and register in profile
    }
}

