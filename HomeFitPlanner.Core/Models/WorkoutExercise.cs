namespace HomeFitPlanner.Core.Models;

public class WorkoutExercise
{
    public string ExerciseId { get; set; } = string.Empty;
    public string ExerciseName { get; set; } = string.Empty;
    public ExerciseType Type { get; set; } = ExerciseType.Strength;
    public MuscleGroup MuscleGroup { get; set; } = MuscleGroup.FullBody;
    public int Sets { get; set; } = 3;
    public int Reps { get; set; } = 10;
    public int Seconds { get; set; } = 30;
    public bool IsTimed { get; set; } = false;
    public int RestSeconds { get; set; } = 45;
    public int EstimatedMinutes { get; set; } = 4;
    public string Phase { get; set; } = "Main"; // Warm-up, Main, Cool-down
    public bool IsCompleted { get; set; } = false;
    public bool WasSkipped { get; set; } = false;
    public bool WasRemoved { get; set; } = false;
    public string HowToSummary { get; set; } = string.Empty;

    public string GetWorkloadString()
    {
        return IsTimed ? $"{Sets} x {Seconds}s" : $"{Sets} x {Reps} reps";
    }

    public string GetWorkloadExpressionPart()
    {
        int count = IsTimed ? Seconds : Reps;
        return $"({Sets} * {count})";
    }

    public WorkoutExercise Clone()
    {
        return new WorkoutExercise
        {
            ExerciseId = ExerciseId,
            ExerciseName = ExerciseName,
            Type = Type,
            MuscleGroup = MuscleGroup,
            Sets = Sets,
            Reps = Reps,
            Seconds = Seconds,
            IsTimed = IsTimed,
            RestSeconds = RestSeconds,
            EstimatedMinutes = EstimatedMinutes,
            Phase = Phase,
            IsCompleted = IsCompleted,
            WasSkipped = WasSkipped,
            WasRemoved = WasRemoved,
            HowToSummary = HowToSummary
        };
    }

    public override string ToString() => $"{ExerciseName} ({GetWorkloadString()})";
}

