using System;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.Models;

public class Exercise
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ExerciseType Type { get; set; } = ExerciseType.Strength;
    public MuscleGroup MuscleGroup { get; set; } = MuscleGroup.FullBody;
    public int Difficulty { get; set; } = 1; // 1 to 5
    public EquipmentType Equipment { get; set; } = EquipmentType.None;
    public int TimeMinutes { get; set; } = 4;
    public int DefaultSets { get; set; } = 3;
    public int DefaultReps { get; set; } = 10;
    public int DefaultSeconds { get; set; } = 30;
    public bool IsTimed { get; set; } = false;

    // Safety flags
    public bool StressesKnees { get; set; } = false;
    public bool StressesBack { get; set; } = false;
    public bool StressesShoulders { get; set; } = false;
    public bool StressesWrists { get; set; } = false;
    public bool IsHighImpact { get; set; } = false;

    // Benefit scores (0 to 100)
    public int MuscleBenefit { get; set; } = 50;
    public int WeightLossBenefit { get; set; } = 50;
    public int StayFitBenefit { get; set; } = 50;
    public int CardioBenefit { get; set; } = 50;

    // Guidance
    public string[] Instructions { get; set; } = Array.Empty<string>();
    public string Tips { get; set; } = string.Empty;

    // Related variations for graph progression
    public string? EasierVariationId { get; set; }
    public string? HarderVariationId { get; set; }
    public string[] SimilarExerciseIds { get; set; } = Array.Empty<string>();

    public string GetDefaultWorkDescription()
    {
        return IsTimed ? $"{DefaultSets} sets x {DefaultSeconds}s" : $"{DefaultSets} sets x {DefaultReps} reps";
    }

    public int GetBenefitScore(GoalType goal)
    {
        return goal switch
        {
            GoalType.BuildMuscle => MuscleBenefit,
            GoalType.LoseWeight => WeightLossBenefit,
            GoalType.StayFit => StayFitBenefit,
            GoalType.BoostCardio => CardioBenefit,
            _ => 50
        };
    }

    public override string ToString() => Name;
}

