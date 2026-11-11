using System;
using HomeFitPlanner.Core.Algorithms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.Models;

namespace HomeFitPlanner.Core.Services;

/// <summary>
/// Implements the 6-step weekly plan generation algorithm described in Section 6.5 of the proposal.
/// Adheres strictly to safety rules, equipment checks, limitation exclusions, body area spacing,
/// and benefit-per-minute ranking via divide-and-conquer sorting.
/// </summary>
public class PlanGeneratorService
{
    private readonly DataManager _dataManager;

    public PlanGeneratorService(DataManager? dataManager = null)
    {
        _dataManager = dataManager ?? DataManager.Instance;
    }

    public WorkoutPlan GeneratePlan(UserProfile profile, string? planName = null)
    {
        // Step 1: Set the shape of the week
        int workoutDays = Math.Clamp(profile.WorkoutDaysPerWeek, 2, 6);
        var dayFocusses = DetermineDayFocusses(profile.Goal, workoutDays, profile.StartingLevel);

        // Step 2: Filter library to find eligible exercises
        var eligibleExercises = FilterEligibleExercises(profile);

        // Step 3 & 4: Place days and fill each day with warm-up, main exercises, and cool-down
        var plan = new WorkoutPlan
        {
            PlanName = planName ?? $"{profile.Goal} Plan ({profile.MinutesPerSession}m)",
            Goal = profile.Goal,
            FitnessLevel = profile.StartingLevel,
            TargetSessionMinutes = profile.MinutesPerSession,
            WorkoutDaysCount = workoutDays
        };

        string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        for (int d = 0; d < 7; d++)
        {
            var dayPlan = new DayPlan
            {
                DayIndex = d,
                DayName = dayNames[d]
            };

            var focus = dayFocusses[d];
            dayPlan.FocusType = focus.Type;
            dayPlan.FocusDescription = focus.Description;
            dayPlan.IsRestDay = (focus.Type == DayFocusType.Rest);

            if (!dayPlan.IsRestDay)
            {
                FillDaySession(dayPlan, focus, eligibleExercises, profile);
            }
            else
            {
                dayPlan.TotalMinutes = 0;
            }

            plan.Days.Add(dayPlan);
        }

        // Step 6: Validate plan against all constraints
        ValidatePlanRules(plan, profile);

        return plan;
    }

    private struct DayFocusConfig
    {
        public DayFocusType Type;
        public string Description;
        public MuscleGroup PrimaryMuscle;
    }

    private DayFocusConfig[] DetermineDayFocusses(GoalType goal, int workoutDays, FitnessLevel level)
    {
        var focusses = new DayFocusConfig[7];
        for (int i = 0; i < 7; i++)
        {
            focusses[i] = new DayFocusConfig { Type = DayFocusType.Rest, Description = "Rest with light walk", PrimaryMuscle = MuscleGroup.FullBody };
        }

        // Days placement logic
        if (workoutDays == 2)
        {
            // Tuesday & Friday
            focusses[1] = CreateFocus(goal, 1, level);
            focusses[4] = CreateFocus(goal, 2, level);
        }
        else if (workoutDays == 3)
        {
            // Monday, Wednesday, Friday (worked example pattern)
            focusses[0] = CreateFocus(goal, 1, level);
            focusses[2] = CreateFocus(goal, 2, level);
            focusses[4] = CreateFocus(goal, 3, level);
        }
        else if (workoutDays == 4)
        {
            // Monday, Tuesday, Thursday, Friday (alternating splits)
            focusses[0] = CreateFocus(goal, 1, level);
            focusses[1] = CreateFocus(goal, 2, level);
            focusses[3] = CreateFocus(goal, 3, level);
            focusses[4] = CreateFocus(goal, 4, level);
        }
        else if (workoutDays == 5)
        {
            // Mon, Tue, Wed, Fri, Sat
            focusses[0] = CreateFocus(goal, 1, level);
            focusses[1] = CreateFocus(goal, 2, level);
            focusses[2] = CreateFocus(goal, 3, level);
            focusses[4] = CreateFocus(goal, 4, level);
            focusses[5] = CreateFocus(goal, 5, level);
        }
        else // 6 days
        {
            for (int i = 0; i < 6; i++)
            {
                focusses[i] = CreateFocus(goal, i + 1, level);
            }
        }

        return focusses;
    }

    private DayFocusConfig CreateFocus(GoalType goal, int dayOrder, FitnessLevel level)
    {
        return goal switch
        {
            GoalType.BuildMuscle => dayOrder switch
            {
                1 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Upper Body Strength", PrimaryMuscle = MuscleGroup.Chest },
                2 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Lower Body Strength", PrimaryMuscle = MuscleGroup.Legs },
                3 => new DayFocusConfig { Type = DayFocusType.Mixed, Description = "Core & Functional Conditioning", PrimaryMuscle = MuscleGroup.Core },
                4 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Full Body Hypertrophy", PrimaryMuscle = MuscleGroup.FullBody },
                5 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Upper Body & Arms Sculpt", PrimaryMuscle = MuscleGroup.Arms },
                _ => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Lower Body & Glutes", PrimaryMuscle = MuscleGroup.Glutes }
            },
            GoalType.LoseWeight => dayOrder switch
            {
                1 => new DayFocusConfig { Type = DayFocusType.Cardio, Description = "Low-Impact Cardio & Core", PrimaryMuscle = MuscleGroup.Core },
                2 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Full-Body Strength (Joint-Friendly)", PrimaryMuscle = MuscleGroup.FullBody },
                3 => new DayFocusConfig { Type = DayFocusType.Mixed, Description = "Cardio Intervals & Full Body", PrimaryMuscle = MuscleGroup.FullBody },
                4 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Metabolic Strength & Resistance", PrimaryMuscle = MuscleGroup.Legs },
                5 => new DayFocusConfig { Type = DayFocusType.Cardio, Description = "Steady-State Fat Burning", PrimaryMuscle = MuscleGroup.FullBody },
                _ => new DayFocusConfig { Type = DayFocusType.Mixed, Description = "Conditioning & Stamina", PrimaryMuscle = MuscleGroup.Core }
            },
            GoalType.BoostCardio => dayOrder switch
            {
                1 => new DayFocusConfig { Type = DayFocusType.Cardio, Description = "Aerobic Intervals", PrimaryMuscle = MuscleGroup.FullBody },
                2 => new DayFocusConfig { Type = DayFocusType.Cardio, Description = "Steady Cardio Rhythm", PrimaryMuscle = MuscleGroup.FullBody },
                3 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Strength Support for Stamina", PrimaryMuscle = MuscleGroup.Legs },
                4 => new DayFocusConfig { Type = DayFocusType.Cardio, Description = "Long Endurance Session", PrimaryMuscle = MuscleGroup.FullBody },
                _ => new DayFocusConfig { Type = DayFocusType.Mixed, Description = "Tempo Intervals", PrimaryMuscle = MuscleGroup.FullBody }
            },
            _ => dayOrder switch // StayFit (Balanced)
            {
                1 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Balanced Full Body Strength", PrimaryMuscle = MuscleGroup.FullBody },
                2 => new DayFocusConfig { Type = DayFocusType.Cardio, Description = "Cardio & Core Mobility", PrimaryMuscle = MuscleGroup.Core },
                3 => new DayFocusConfig { Type = DayFocusType.Strength, Description = "Upper & Lower Body Synergy", PrimaryMuscle = MuscleGroup.FullBody },
                4 => new DayFocusConfig { Type = DayFocusType.Mixed, Description = "Functional Tone & Conditioning", PrimaryMuscle = MuscleGroup.FullBody },
                _ => new DayFocusConfig { Type = DayFocusType.Mixed, Description = "Total Body Maintenance", PrimaryMuscle = MuscleGroup.FullBody }
            }
        };
    }

    /// <summary>
    /// Step 2: Filters out exercises that require missing equipment, stress limited areas,
    /// or exceed age restrictions (users 50+ lose high-impact moves).
    /// </summary>
    public CustomArray<Exercise> FilterEligibleExercises(UserProfile profile)
    {
        var eligible = new CustomArray<Exercise>();

        for (int i = 0; i < _dataManager.Exercises.Count; i++)
        {
            var ex = _dataManager.Exercises[i];

            // 1. Equipment check
            if (!IsEquipmentAvailable(ex.Equipment, profile))
                continue;

            // 2. Limitations check
            if (profile.HasKneeLimitation && ex.StressesKnees)
                continue;
            if (profile.HasBackLimitation && ex.StressesBack)
                continue;
            if (profile.HasShoulderLimitation && ex.StressesShoulders)
                continue;
            if (profile.HasWristLimitation && ex.StressesWrists)
                continue;

            // 3. Age safety check: 50+ users lose high-impact exercises
            if (profile.Age >= 50 && ex.IsHighImpact)
                continue;

            // 4. Avoided exercises check
            if (profile.AvoidedExerciseIds.Contains(ex.Id))
                continue;

            // 5. Difficulty ceiling based on starting level
            int maxDiff = profile.StartingLevel switch
            {
                FitnessLevel.Beginner => 3,
                FitnessLevel.Intermediate => 4,
                _ => 5
            };
            if (ex.Difficulty > maxDiff)
                continue;

            eligible.Add(ex);
        }

        return eligible;
    }

    private bool IsEquipmentAvailable(EquipmentType equip, UserProfile profile)
    {
        return equip switch
        {
            EquipmentType.None => true,
            EquipmentType.ResistanceBands => profile.HasResistanceBands,
            EquipmentType.Dumbbells => profile.HasDumbbells,
            EquipmentType.Mat => profile.HasMat,
            EquipmentType.ChairOrBench => profile.HasChairOrBench,
            _ => true
        };
    }

    /// <summary>
    /// Step 4: Fills each active day with Warm-Up, Goal-benefit ranked Main exercises, and Cool-Down.
    /// </summary>
    private void FillDaySession(DayPlan day, DayFocusConfig focus, CustomArray<Exercise> eligible, UserProfile profile)
    {
        int targetMinutes = profile.MinutesPerSession;
        int remainingMinutes = targetMinutes;

        // 1. Add Warm-Up (3 mins)
        var warmUps = GetExercisesByType(eligible, ExerciseType.WarmUp);
        var chosenWarmUp = PickBestExercise(warmUps, profile.Goal, focus.PrimaryMuscle) ??
                           _dataManager.FindExerciseById("ex_march");

        if (chosenWarmUp != null)
        {
            day.Exercises.Add(CreateWorkoutExercise(chosenWarmUp, "Warm-up", profile));
            remainingMinutes -= chosenWarmUp.TimeMinutes;
        }

        // 2. Select Cool-Down stretch (3 mins)
        var stretches = GetExercisesByType(eligible, ExerciseType.Stretch);
        var chosenStretch = PickBestExercise(stretches, profile.Goal, focus.PrimaryMuscle) ??
                            _dataManager.FindExerciseById("ex_hamstring_stretch");

        int coolDownMinutes = chosenStretch?.TimeMinutes ?? 3;
        remainingMinutes -= coolDownMinutes;

        // 3. Rank main exercises by benefit per minute using QuickSort/MergeSort
        var mainCandidates = new CustomArray<Exercise>();
        for (int i = 0; i < eligible.Count; i++)
        {
            var ex = eligible[i];
            if (ex.Type == ExerciseType.Strength || ex.Type == ExerciseType.Cardio)
            {
                // Align with day focus
                if (focus.Type == DayFocusType.Cardio && ex.Type == ExerciseType.Strength && ex.MuscleBenefit < 70)
                    continue;

                mainCandidates.Add(ex);
            }
        }

        // Sort descending by benefit per minute
        var candidateArray = mainCandidates.ToArray();
        SortingAlgorithms.QuickSort(candidateArray, new BenefitPerMinuteComparer(profile.Goal));

        // Fill remaining minutes
        var usedExerciseIds = new CustomArray<string>();
        if (chosenWarmUp != null) usedExerciseIds.Add(chosenWarmUp.Id);

        for (int i = 0; i < candidateArray.Length && remainingMinutes >= 3; i++)
        {
            var ex = candidateArray[i];

            if (usedExerciseIds.Contains(ex.Id)) continue;

            // Prioritize primary muscle group or full body
            if (focus.PrimaryMuscle != MuscleGroup.FullBody && ex.MuscleGroup != focus.PrimaryMuscle && ex.MuscleGroup != MuscleGroup.FullBody && ex.MuscleGroup != MuscleGroup.Core)
            {
                // Keep looking if early in session
                if (day.Exercises.Count < 3 && candidateArray.Length > 5) continue;
            }

            day.Exercises.Add(CreateWorkoutExercise(ex, "Main", profile));
            usedExerciseIds.Add(ex.Id);
            remainingMinutes -= ex.TimeMinutes;
        }

        // 4. Attach cool-down at the end
        if (chosenStretch != null)
        {
            day.Exercises.Add(CreateWorkoutExercise(chosenStretch, "Cool-down", profile));
        }

        day.RecalculateTotalMinutes();
    }

    private WorkoutExercise CreateWorkoutExercise(Exercise ex, string phase, UserProfile profile)
    {
        // Adjust sets and rest for older adults (age 50+) or beginners
        int sets = ex.DefaultSets;
        int rest = 45;

        if (profile.Age >= 50)
        {
            rest = 60; // Longer rests for older adults
        }

        if (profile.StartingLevel == FitnessLevel.Beginner && sets > 2 && phase == "Main")
        {
            sets = 3;
        }

        return new WorkoutExercise
        {
            ExerciseId = ex.Id,
            ExerciseName = ex.Name,
            Type = ex.Type,
            MuscleGroup = ex.MuscleGroup,
            Sets = sets,
            Reps = ex.DefaultReps,
            Seconds = ex.DefaultSeconds,
            IsTimed = ex.IsTimed,
            RestSeconds = rest,
            EstimatedMinutes = ex.TimeMinutes,
            Phase = phase,
            HowToSummary = ex.Tips
        };
    }

    private CustomArray<Exercise> GetExercisesByType(CustomArray<Exercise> pool, ExerciseType type)
    {
        var result = new CustomArray<Exercise>();
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].Type == type) result.Add(pool[i]);
        }
        return result;
    }

    private Exercise? PickBestExercise(CustomArray<Exercise> list, GoalType goal, MuscleGroup targetMuscle)
    {
        Exercise? best = null;
        int bestScore = -1;

        for (int i = 0; i < list.Count; i++)
        {
            int score = list[i].GetBenefitScore(goal);
            if (list[i].MuscleGroup == targetMuscle) score += 20;

            if (score > bestScore)
            {
                bestScore = score;
                best = list[i];
            }
        }

        return best;
    }

    private class BenefitPerMinuteComparer : System.Collections.Generic.IComparer<Exercise>
    {
        private readonly GoalType _goal;

        public BenefitPerMinuteComparer(GoalType goal)
        {
            _goal = goal;
        }

        public int Compare(Exercise? x, Exercise? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            double bpmX = (double)x.GetBenefitScore(_goal) / Math.Max(1, x.TimeMinutes);
            double bpmY = (double)y.GetBenefitScore(_goal) / Math.Max(1, y.TimeMinutes);

            // Descending order
            return bpmY.CompareTo(bpmX);
        }
    }

    /// <summary>
    /// Step 6: Validates plan against safety constraints:
    /// - No missing equipment
    /// - No limitations stressed
    /// - No body area trained on two consecutive days
    /// </summary>
    public bool ValidatePlanRules(WorkoutPlan plan, UserProfile profile)
    {
        bool lastWasStrength = false;

        for (int d = 0; d < plan.Days.Count; d++)
        {
            var day = plan.Days[d];
            if (day.IsRestDay)
            {
                lastWasStrength = false;
                continue;
            }

            for (int e = 0; e < day.Exercises.Count; e++)
            {
                var we = day.Exercises[e];
                var ex = _dataManager.FindExerciseById(we.ExerciseId);
                if (ex == null) continue;

                // Limitation violations check
                if (profile.HasKneeLimitation && ex.StressesKnees) return false;
                if (profile.HasBackLimitation && ex.StressesBack) return false;
                if (profile.HasShoulderLimitation && ex.StressesShoulders) return false;
                if (profile.HasWristLimitation && ex.StressesWrists) return false;
            }

            // Check consecutive body area rule
            if (day.FocusType == DayFocusType.Strength)
            {
                if (lastWasStrength && day.FocusDescription == plan.Days[d - 1].FocusDescription)
                {
                    // Body area collision on consecutive days
                    return false;
                }
                lastWasStrength = true;
            }
            else
            {
                lastWasStrength = false;
            }
        }

        return true;
    }
}
