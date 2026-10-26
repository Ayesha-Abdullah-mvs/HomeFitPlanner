using System;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.Models;

public class WorkoutPlan
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string PlanName { get; set; } = "Weekly Plan";
    public GoalType Goal { get; set; } = GoalType.LoseWeight;
    public FitnessLevel FitnessLevel { get; set; } = FitnessLevel.Beginner;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public CustomArray<DayPlan> Days { get; set; } = new();
    public int TargetSessionMinutes { get; set; } = 30;
    public int WorkoutDaysCount { get; set; } = 3;

    public int TotalWeeklyMinutes
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < Days.Count; i++)
            {
                sum += Days[i].TotalMinutes;
            }
            return sum;
        }
    }

    /// <summary>
    /// Builds the hand-written JaggedArray representation for the 7 days.
    /// Each row fits the exact count of exercises scheduled on that day.
    /// </summary>
    public JaggedArray<string> BuildJaggedExerciseNames()
    {
        var jagged = new JaggedArray<string>(Days.Count > 0 ? Days.Count : 7);
        for (int d = 0; d < Days.Count; d++)
        {
            var day = Days[d];
            if (day.IsRestDay || day.Exercises.Count == 0)
            {
                jagged.SetRow(d, new[] { "Rest / Light Walk" });
            }
            else
            {
                var names = new string[day.Exercises.Count];
                for (int e = 0; e < day.Exercises.Count; e++)
                {
                    names[e] = day.Exercises[e].ExerciseName;
                }
                jagged.SetRow(d, names);
            }
        }
        return jagged;
    }

    public WorkoutPlan Clone()
    {
        var clone = new WorkoutPlan
        {
            Id = Id,
            PlanName = PlanName,
            Goal = Goal,
            FitnessLevel = FitnessLevel,
            CreatedDate = CreatedDate,
            TargetSessionMinutes = TargetSessionMinutes,
            WorkoutDaysCount = WorkoutDaysCount,
            Days = new CustomArray<DayPlan>()
        };

        for (int i = 0; i < Days.Count; i++)
        {
            clone.Days.Add(Days[i].Clone());
        }

        return clone;
    }
}

