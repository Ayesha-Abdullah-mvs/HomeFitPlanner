using System;

namespace HomeFitPlanner.Core.Models;

public class WorkoutSessionLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime Date { get; set; } = DateTime.Now;
    public string DayName { get; set; } = "Monday";
    public string PlanName { get; set; } = "Weekly Plan";
    public int MinutesTrained { get; set; } = 30;
    public int ExercisesCompleted { get; set; } = 5;
    public int TotalExercises { get; set; } = 5;
    public SessionRating Rating { get; set; } = SessionRating.JustRight;
    public string WorkloadExpression { get; set; } = "(10 * 3) + (12 * 3)";
    public double CalculatedWorkload { get; set; } = 66.0;
    public string Notes { get; set; } = string.Empty;
}

public class WeightLogEntry
{
    public DateTime Date { get; set; } = DateTime.Now;
    public double WeightKg { get; set; } = 70.0;
    public double Bmi { get; set; } = 27.3;
    public string Note { get; set; } = string.Empty;
}

