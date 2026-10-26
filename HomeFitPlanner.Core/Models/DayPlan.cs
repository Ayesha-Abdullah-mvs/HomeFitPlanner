using System;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.Models;

public class DayPlan
{
    public int DayIndex { get; set; } // 0 = Mon, 6 = Sun
    public string DayName { get; set; } = "Monday";
    public DayFocusType FocusType { get; set; } = DayFocusType.Strength;
    public string FocusDescription { get; set; } = string.Empty;
    public CustomArray<WorkoutExercise> Exercises { get; set; } = new();
    public int TotalMinutes { get; set; } = 30;
    public bool IsRestDay { get; set; } = false;
    public bool IsCompleted { get; set; } = false;

    public void RecalculateTotalMinutes()
    {
        if (IsRestDay)
        {
            TotalMinutes = 0;
            return;
        }

        int mins = 0;
        for (int i = 0; i < Exercises.Count; i++)
        {
            mins += Exercises[i].EstimatedMinutes;
        }
        TotalMinutes = mins;
    }

    public DayPlan Clone()
    {
        var clone = new DayPlan
        {
            DayIndex = DayIndex,
            DayName = DayName,
            FocusType = FocusType,
            FocusDescription = FocusDescription,
            TotalMinutes = TotalMinutes,
            IsRestDay = IsRestDay,
            IsCompleted = IsCompleted,
            Exercises = new CustomArray<WorkoutExercise>()
        };

        for (int i = 0; i < Exercises.Count; i++)
        {
            clone.Exercises.Add(Exercises[i].Clone());
        }

        return clone;
    }
}

