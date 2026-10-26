namespace HomeFitPlanner.Core.Models;

public enum GoalType
{
    BuildMuscle,
    LoseWeight,
    StayFit,
    BoostCardio
}

public enum FitnessLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum MuscleGroup
{
    Chest,
    Back,
    Legs,
    Glutes,
    Shoulders,
    Arms,
    Core,
    FullBody
}

public enum EquipmentType
{
    None,
    ResistanceBands,
    Dumbbells,
    Mat,
    ChairOrBench
}

public enum LimitationType
{
    None,
    Knees,
    Back,
    Shoulders,
    Wrists
}

public enum ExerciseType
{
    WarmUp,
    Strength,
    Cardio,
    Stretch
}

public enum DayFocusType
{
    Strength,
    Cardio,
    Mixed,
    Rest
}

public enum SessionRating
{
    TooEasy,
    JustRight,
    TooHard
}

public enum GenderType
{
    Female,
    Male,
    PreferNotToSay
}

public enum PlanChangeAction
{
    Swap,
    Add,
    Remove,
    ModifyReps
}

