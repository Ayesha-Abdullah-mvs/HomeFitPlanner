using System;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.Models;

public class UserProfile
{
    public string Name { get; set; } = "Athlete";
    public int Age { get; set; } = 32;
    public GenderType Gender { get; set; } = GenderType.Female;
    public double HeightCm { get; set; } = 160.0;
    public double WeightKg { get; set; } = 70.0;

    // Current Activity (Sets starting level)
    public int CurrentActivityDays { get; set; } = 2;
    public string CurrentActivityKind { get; set; } = "walking";
    public FitnessLevel StartingLevel { get; set; } = FitnessLevel.Beginner;

    // Target Setup
    public GoalType Goal { get; set; } = GoalType.LoseWeight;
    public int WorkoutDaysPerWeek { get; set; } = 3;
    public int MinutesPerSession { get; set; } = 30;

    // Equipment and Limitations
    public bool HasNoEquipment { get; set; } = false;
    public bool HasResistanceBands { get; set; } = false;
    public bool HasDumbbells { get; set; } = false;
    public bool HasMat { get; set; } = false;
    public bool HasChairOrBench { get; set; } = true;

    public bool HasKneeLimitation { get; set; } = true;
    public bool HasBackLimitation { get; set; } = false;
    public bool HasShoulderLimitation { get; set; } = false;
    public bool HasWristLimitation { get; set; } = false;

    // Progress Metrics
    public int CurrentStreak { get; set; } = 3;
    public int BestStreak { get; set; } = 5;
    public int TotalWorkoutsCompleted { get; set; } = 12;
    public int TotalMinutesExercised { get; set; } = 360;

    // Track avoided exercises (if skipped/removed in 2 sessions)
    public CustomArray<string> AvoidedExerciseIds { get; set; } = new();

    public double CalculateBmi()
    {
        if (HeightCm <= 0) return 0;
        double heightM = HeightCm / 100.0;
        return Math.Round(WeightKg / (heightM * heightM), 1);
    }

    public string GetBmiCategory()
    {
        double bmi = CalculateBmi();
        if (bmi < 18.5) return "Underweight";
        if (bmi < 25.0) return "Normal";
        if (bmi < 30.0) return "Overweight";
        return "Obese";
    }

    /// <summary>
    /// Mifflin-St Jeor formula for BMR and TDEE based on activity days.
    /// </summary>
    public int CalculateDailyCalories()
    {
        if (WeightKg <= 0 || HeightCm <= 0 || Age <= 0) return 2000;

        // BMR: 10 * weight(kg) + 6.25 * height(cm) - 5 * age(y) + s
        // s = +5 for male, -161 for female, -78 for prefer not to say
        double s = Gender switch
        {
            GenderType.Male => 5,
            GenderType.Female => -161,
            _ => -78
        };

        double bmr = (10.0 * WeightKg) + (6.25 * HeightCm) - (5.0 * Age) + s;

        // Activity multiplier
        double multiplier = CurrentActivityDays switch
        {
            0 => 1.2,       // Sedentary
            1 or 2 => 1.35, // Light activity
            3 or 4 => 1.55, // Moderate activity
            _ => 1.725      // Very active
        };

        double tdee = bmr * multiplier;

        // Adjust slightly according to goal
        return Goal switch
        {
            GoalType.LoseWeight => (int)Math.Round(tdee - 350), // Mild calorie deficit
            GoalType.BuildMuscle => (int)Math.Round(tdee + 250), // Mild calorie surplus
            _ => (int)Math.Round(tdee)
        };
    }

    public void DeriveFitnessLevel()
    {
        string kind = CurrentActivityKind.ToLowerInvariant();
        if (CurrentActivityDays >= 5)
        {
            StartingLevel = FitnessLevel.Advanced;
        }
        else if (CurrentActivityDays >= 3 && (kind.Contains("strength") || kind.Contains("cardio")))
        {
            StartingLevel = FitnessLevel.Intermediate;
        }
        else
        {
            StartingLevel = FitnessLevel.Beginner;
        }
    }
}

