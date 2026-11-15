using System;
using System.IO;
using System.Text.Json;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.DataStructures.Graphs;
using HomeFitPlanner.Core.DataStructures.HashTables;
using HomeFitPlanner.Core.DataStructures.Trees;
using HomeFitPlanner.Core.Models;

namespace HomeFitPlanner.Core.Services;

/// <summary>
/// Central repository and data management service.
/// Stores and retrieves exercises, user profiles, saved workout plans, and workout logs.
/// Maintains the BST and AVL tree of saved plans and the exercise progression graph.
/// </summary>
public class DataManager
{
    private static DataManager? _instance;
    public static DataManager Instance => _instance ??= new DataManager();

    public CustomArray<Exercise> Exercises { get; private set; } = new();
    public ChainingHashTable<string, Exercise> ExerciseNameTableChaining { get; private set; } = new();
    public OpenAddressingHashTable<string, Exercise> ExerciseNameTableOpenAddressing { get; private set; } = new();
    public Graph<string> ExerciseProgressionGraph { get; private set; } = new();

    // Trees for saved plans (Indexed by plan duration in minutes)
    public BinarySearchTree<int, WorkoutPlan> SavedPlansBst { get; private set; } = new();
    public AvlTree<int, WorkoutPlan> SavedPlansAvl { get; private set; } = new();
    public CustomArray<WorkoutPlan> SavedPlansList { get; private set; } = new();

    // Workout session logs and body weight log entries
    public CustomArray<WorkoutSessionLog> SessionLogs { get; private set; } = new();
    public CustomArray<WeightLogEntry> WeightLogs { get; private set; } = new();

    public UserProfile CurrentProfile { get; set; } = new();
    public WorkoutPlan? CurrentPlan { get; set; }

    public string DataDirectory { get; }

    public DataManager()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        DataDirectory = Path.Combine(baseDir, "Data");
        if (!Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
        }

        InitializeExercises();
        BuildExerciseLookupTables();
        BuildExerciseGraph();
        InitializeSampleUserData();
    }

    private void InitializeExercises()
    {
        Exercises.Clear();

        // 1. Warm-up exercises
        AddEx("ex_march", "March in Place", ExerciseType.WarmUp, MuscleGroup.FullBody, 1, EquipmentType.None, 3, 1, 0, 180, true,
            false, false, false, false, false, 20, 70, 80, 80,
            new[] { "Stand tall with feet hip-width apart.", "Lift your knees toward chest alternately.", "Swing arms rhythmically with each step." },
            "Keep your core lightly engaged and land softly on the balls of your feet.", null, "ex_high_knees", new[] { "ex_step_touch", "ex_butt_kicks_low" });

        AddEx("ex_step_touch", "Step-Touch Intervals", ExerciseType.WarmUp, MuscleGroup.FullBody, 1, EquipmentType.None, 3, 1, 0, 180, true,
            false, false, false, false, false, 20, 80, 85, 80,
            new[] { "Step right foot to the right, bring left foot to touch.", "Step left foot to the left, bring right foot to touch.", "Add gentle arm reaches or chest pulls." },
            "A safe, rhythmic low-impact cardio move that warms up hips and calves.", null, "ex_shadow_box", new[] { "ex_march" });

        AddEx("ex_arm_circles", "Arm Circles", ExerciseType.WarmUp, MuscleGroup.Shoulders, 1, EquipmentType.None, 2, 2, 20, 0, false,
            false, false, false, false, false, 30, 40, 75, 40,
            new[] { "Extend arms straight out to sides at shoulder height.", "Make small controlled circles forward for 10 reps.", "Reverse direction for 10 reps." },
            "Keep shoulders relaxed away from your ears.", null, null, new[] { "ex_torso_twist" });

        AddEx("ex_torso_twist", "Torso Twists", ExerciseType.WarmUp, MuscleGroup.Core, 1, EquipmentType.None, 2, 2, 15, 0, false,
            false, false, false, false, false, 30, 50, 75, 40,
            new[] { "Stand with feet shoulder-width apart, elbows bent.", "Gently rotate torso side to side from the ribs.", "Allow back heel to pivot naturally." },
            "Do not force deep spinal rotation; keep movement fluid and comfortable.", null, null, new[] { "ex_arm_circles" });

        // 2. Chest & Arms (Push family)
        AddEx("ex_wall_pushup", "Wall Push-up", ExerciseType.Strength, MuscleGroup.Chest, 1, EquipmentType.None, 4, 3, 10, 0, false,
            false, false, false, false, false, 55, 60, 75, 40,
            new[] { "Face a sturdy wall about two feet away.", "Place hands flat against the wall at shoulder height.", "Lower chest toward wall, then press back." },
            "Keep your body in a straight plank line from heels to head.", null, "ex_incline_pushup", new[] { "ex_knee_pushup" });

        AddEx("ex_incline_pushup", "Incline Push-up", ExerciseType.Strength, MuscleGroup.Chest, 2, EquipmentType.ChairOrBench, 4, 3, 10, 0, false,
            false, false, false, false, false, 65, 65, 80, 45,
            new[] { "Place hands on the seat of a sturdy chair or bench.", "Step feet back into an elevated plank.", "Lower chest to chair seat and push back up." },
            "Make sure the chair is wedged against a wall so it cannot slide.", "ex_wall_pushup", "ex_knee_pushup", new[] { "ex_pushup" });

        AddEx("ex_knee_pushup", "Knee Push-up", ExerciseType.Strength, MuscleGroup.Chest, 2, EquipmentType.None, 4, 3, 10, 0, false,
            false, false, false, false, false, 75, 70, 85, 50,
            new[] { "Rest on knees and hands on the floor.", "Keep torso and hips in one straight diagonal line.", "Lower chest to ground and push back up." },
            "Keep hips tucked forward rather than sticking your glutes up.", "ex_incline_pushup", "ex_pushup", new[] { "ex_wall_pushup" });

        AddEx("ex_pushup", "Standard Push-up", ExerciseType.Strength, MuscleGroup.Chest, 3, EquipmentType.None, 5, 3, 12, 0, false,
            false, false, false, true, false, 85, 75, 90, 55,
            new[] { "Begin in high plank with hands slightly wider than shoulders.", "Lower chest until elbows hit 90 degrees.", "Drive through palms to return to top." },
            "Flags wrists: if wrists ache, place fists on the floor.", "ex_knee_pushup", "ex_diamond_pushup", new[] { "ex_decline_pushup" });

        AddEx("ex_diamond_pushup", "Diamond Push-up", ExerciseType.Strength, MuscleGroup.Arms, 4, EquipmentType.None, 5, 3, 8, 0, false,
            false, false, false, true, false, 90, 70, 85, 50,
            new[] { "Form a diamond shape with index fingers and thumbs on floor.", "Keep elbows tucked close to ribcage.", "Lower chest toward diamond and press up." },
            "Focuses intensely on triceps and inner chest.", "ex_pushup", null, new[] { "ex_chair_dips" });

        AddEx("ex_decline_pushup", "Decline Push-up", ExerciseType.Strength, MuscleGroup.Chest, 4, EquipmentType.ChairOrBench, 5, 3, 10, 0, false,
            false, false, true, true, false, 90, 75, 85, 50,
            new[] { "Place feet elevated on a chair or bench, hands on the floor.", "Keep core braced tight.", "Lower chest toward floor and push back up." },
            "Intense upper chest builder with elevated feet.", "ex_pushup", null, new[] { "ex_diamond_pushup" });

        AddEx("ex_chair_dips", "Chair Dips", ExerciseType.Strength, MuscleGroup.Arms, 2, EquipmentType.ChairOrBench, 4, 3, 10, 0, false,
            false, false, true, true, false, 75, 65, 80, 45,
            new[] { "Sit on chair edge, hands gripping seat next to hips.", "Slide hips off seat with knees bent at 90 degrees.", "Lower hips by bending elbows, then press back up." },
            "Flags shoulders and wrists: keep shoulders pressed down away from ears.", null, null, new[] { "ex_diamond_pushup", "ex_band_bicep_curl" });

        AddEx("ex_db_curl", "Dumbbell Bicep Curl", ExerciseType.Strength, MuscleGroup.Arms, 2, EquipmentType.Dumbbells, 4, 3, 12, 0, false,
            false, false, false, false, false, 80, 55, 75, 35,
            new[] { "Stand holding dumbbells at sides, palms facing forward.", "Curl weights up toward shoulders while keeping elbows pinned to ribs.", "Lower with control." },
            "Do not swing your torso; control both lifting and lowering.", null, null, new[] { "ex_band_bicep_curl" });

        AddEx("ex_band_bicep_curl", "Resistance Band Bicep Curl", ExerciseType.Strength, MuscleGroup.Arms, 2, EquipmentType.ResistanceBands, 4, 3, 12, 0, false,
            false, false, false, false, false, 75, 55, 75, 35,
            new[] { "Step on the center of the band, grip handles.", "Curl handles toward shoulders keeping elbows steady.", "Lower under continuous band tension." },
            "Constant tension through the entire range of motion.", null, null, new[] { "ex_db_curl" });

        // 3. Back & Pull family
        AddEx("ex_doorframe_row", "Doorframe Row", ExerciseType.Strength, MuscleGroup.Back, 1, EquipmentType.None, 4, 3, 10, 0, false,
            false, false, false, false, false, 60, 60, 75, 40,
            new[] { "Stand facing an open doorframe, grip sides at chest height.", "Lean body back with arms straight.", "Pull chest toward doorframe by squeezing shoulder blades." },
            "Low impact, requires zero equipment, easy on back and knees.", null, "ex_band_row", new[] { "ex_db_row" });

        AddEx("ex_band_row", "Resistance Band Row", ExerciseType.Strength, MuscleGroup.Back, 2, EquipmentType.ResistanceBands, 4, 3, 12, 0, false,
            false, false, false, false, false, 78, 65, 80, 45,
            new[] { "Sit with legs extended, loop band around feet.", "Pull handles toward ribcage, squeezing upper back.", "Return slowly to start." },
            "Maintain an upright posture without slumping forward.", "ex_doorframe_row", "ex_db_row", new[] { "ex_bird_dog" });

        AddEx("ex_db_row", "Dumbbell Bent-Over Row", ExerciseType.Strength, MuscleGroup.Back, 2, EquipmentType.Dumbbells, 4, 3, 10, 0, false,
            false, true, false, false, false, 85, 70, 80, 45,
            new[] { "Hinge forward at hips with flat back, knees slightly bent.", "Pull dumbbells up toward hips, driving elbows back.", "Lower with control." },
            "Flags back: keep spine completely neutral and avoid rounding.", "ex_band_row", null, new[] { "ex_doorframe_row" });

        // 4. Legs & Glutes (Squat / Lower body family)
        AddEx("ex_chair_squat", "Chair Squat", ExerciseType.Strength, MuscleGroup.Legs, 1, EquipmentType.ChairOrBench, 4, 3, 10, 0, false,
            false, false, false, false, false, 65, 75, 85, 55,
            new[] { "Stand in front of a chair with feet shoulder-width apart.", "Sit hips back gently onto the chair seat.", "Stand back up by driving through your heels." },
            "Safe supported squat; protects knees and reinforces hip hinge.", null, "ex_bw_squat", new[] { "ex_glute_bridge", "ex_wall_sit" });

        AddEx("ex_bw_squat", "Bodyweight Squat", ExerciseType.Strength, MuscleGroup.Legs, 2, EquipmentType.None, 4, 3, 12, 0, false,
            true, false, false, false, false, 80, 80, 85, 60,
            new[] { "Stand tall with feet slightly wider than shoulders.", "Push hips back and bend knees down to parallel.", "Press through heels to return to standing." },
            "Flags knees: ensure knees track in line with toes, not caving inward.", "ex_chair_squat", "ex_band_squat", new[] { "ex_jump_squat", "ex_wall_sit" });

        AddEx("ex_band_squat", "Resistance Band Squat", ExerciseType.Strength, MuscleGroup.Legs, 2, EquipmentType.ResistanceBands, 4, 3, 12, 0, false,
            true, false, false, false, false, 85, 82, 85, 60,
            new[] { "Step on the center of the band, hold handles at shoulder level.", "Perform a squat against the band resistance.", "Exhale and drive up to standing." },
            "Flags knees: band adds variable ascending resistance at the top.", "ex_bw_squat", "ex_db_squat", new[] { "ex_chair_squat" });

        AddEx("ex_db_squat", "Dumbbell Goblet Squat", ExerciseType.Strength, MuscleGroup.Legs, 3, EquipmentType.Dumbbells, 5, 3, 10, 0, false,
            true, true, false, false, false, 90, 85, 85, 65,
            new[] { "Hold one dumbbell vertically at chest height.", "Squat deep while keeping chest proud and spine long.", "Drive out of the hole through feet." },
            "Flags knees and back: keep weight balanced over mid-foot.", "ex_bw_squat", null, new[] { "ex_band_squat" });

        AddEx("ex_jump_squat", "Jump Squat", ExerciseType.Cardio, MuscleGroup.Legs, 3, EquipmentType.None, 4, 3, 10, 0, false,
            true, false, false, false, true, 80, 95, 85, 95,
            new[] { "Perform a standard squat.", "Explosively jump straight up at the top.", "Land softly through toes and immediately absorb into next squat." },
            "Flags knees and high impact: strictly avoid if knees hurt or aged 50+.", "ex_bw_squat", null, new[] { "ex_bw_squat" });

        AddEx("ex_wall_sit", "Wall Sit", ExerciseType.Strength, MuscleGroup.Legs, 2, EquipmentType.None, 3, 3, 0, 30, true,
            false, false, false, false, false, 70, 75, 80, 55,
            new[] { "Slide back down wall until thighs are parallel to ground.", "Keep knees bent at 90 degrees directly over ankles.", "Hold position statically." },
            "Excellent isometric quad strengthener with minimal knee joint movement.", "ex_chair_squat", null, new[] { "ex_chair_squat" });

        AddEx("ex_calf_raise", "Calf Raises", ExerciseType.Strength, MuscleGroup.Legs, 1, EquipmentType.None, 3, 3, 15, 0, false,
            false, false, false, false, false, 55, 60, 75, 45,
            new[] { "Stand tall near a wall for balance.", "Raise heels as high as possible onto the balls of your feet.", "Pause for a second and lower slowly." },
            "Low impact, strengthens ankles and lower legs.", null, null, new[] { "ex_march" });

        AddEx("ex_glute_bridge", "Glute Bridge", ExerciseType.Strength, MuscleGroup.Glutes, 1, EquipmentType.Mat, 4, 3, 12, 0, false,
            false, false, false, false, false, 75, 75, 85, 45,
            new[] { "Lie on your back on a mat, knees bent, feet flat on floor.", "Squeeze glutes and lift hips until body forms a straight line from knees to shoulders.", "Lower hips gently." },
            "Low impact, knee friendly, and restores dormant glutes.", null, "ex_single_glute_bridge", new[] { "ex_bird_dog" });

        AddEx("ex_single_glute_bridge", "Single-Leg Glute Bridge", ExerciseType.Strength, MuscleGroup.Glutes, 3, EquipmentType.Mat, 4, 3, 10, 0, false,
            false, false, false, false, false, 85, 75, 85, 50,
            new[] { "From bridge position, lift one leg straight in the air.", "Press through the grounded heel to raise hips.", "Maintain level pelvis." },
            "Advanced unilateral glute and hamstring challenge.", "ex_glute_bridge", null, new[] { "ex_glute_bridge" });

        AddEx("ex_rev_lunge", "Reverse Lunge", ExerciseType.Strength, MuscleGroup.Legs, 2, EquipmentType.None, 4, 3, 10, 0, false,
            true, false, false, false, false, 78, 80, 85, 60,
            new[] { "Step backward with one leg and lower back knee toward floor.", "Keep front knee stacked directly above front ankle.", "Push off front foot to return." },
            "Flags knees: stepping backward puts far less shear force on the knee than forward lunges.", "ex_chair_squat", "ex_walk_lunge", new[] { "ex_bw_squat" });

        AddEx("ex_walk_lunge", "Walking Lunges", ExerciseType.Strength, MuscleGroup.Legs, 3, EquipmentType.None, 4, 3, 12, 0, false,
            true, false, false, false, false, 82, 85, 85, 70,
            new[] { "Step forward into a lunge.", "Rise up and step the back leg forward into the next lunge.", "Continue alternating forward." },
            "Flags knees: requires balance and decelerating momentum.", "ex_rev_lunge", null, new[] { "ex_rev_lunge" });

        // 5. Core & Abdominals
        AddEx("ex_knee_plank", "Knee Plank", ExerciseType.Strength, MuscleGroup.Core, 1, EquipmentType.Mat, 3, 3, 0, 25, true,
            false, false, true, true, false, 60, 65, 80, 45,
            new[] { "Support yourself on forearms and knees on a mat.", "Form a straight line from head to knees.", "Brace abdominals like bracing for a punch." },
            "Gentle on lower back, beginner-friendly core foundation.", null, "ex_plank", new[] { "ex_bird_dog", "ex_dead_bug" });

        AddEx("ex_plank", "Forearm Plank", ExerciseType.Strength, MuscleGroup.Core, 3, EquipmentType.Mat, 3, 3, 0, 40, true,
            false, false, true, true, false, 80, 80, 85, 60,
            new[] { "Support body on forearms and toes.", "Keep spine completely rigid and level.", "Hold without letting hips sag or pike." },
            "Flags shoulders and wrists: if shoulders ache, perform on knees.", "ex_knee_plank", "ex_side_plank", new[] { "ex_bird_dog" });

        AddEx("ex_side_plank", "Side Plank", ExerciseType.Strength, MuscleGroup.Core, 3, EquipmentType.Mat, 3, 3, 0, 25, true,
            false, false, true, true, false, 80, 75, 80, 50,
            new[] { "Lie on side, prop up on forearm with elbow directly below shoulder.", "Lift hips off ground until body forms a diagonal line.", "Hold steadily." },
            "Flags shoulders: targets obliques and lateral core stability.", "ex_plank", null, new[] { "ex_plank" });

        AddEx("ex_bird_dog", "Bird Dog", ExerciseType.Strength, MuscleGroup.Core, 1, EquipmentType.Mat, 3, 3, 10, 0, false,
            false, false, false, false, false, 65, 65, 85, 45,
            new[] { "Start on all fours with hands under shoulders, knees under hips.", "Reach right arm forward and left leg backward simultaneously.", "Hold 2 seconds, return and switch sides." },
            "Zero joint impact, widely recommended for back pain and spinal health.", null, "ex_plank", new[] { "ex_dead_bug", "ex_glute_bridge" });

        AddEx("ex_dead_bug", "Dead Bug", ExerciseType.Strength, MuscleGroup.Core, 1, EquipmentType.Mat, 3, 3, 10, 0, false,
            false, false, false, false, false, 65, 65, 85, 45,
            new[] { "Lie on back with arms pointing to ceiling, hips and knees at 90 degrees.", "Lower opposite arm and leg toward floor while pressing lower back into mat.", "Return and alternate." },
            "Back-friendly core exercise with no spinal flexion.", null, "ex_plank", new[] { "ex_bird_dog" });

        AddEx("ex_standing_knee_lift", "Standing Knee Lifts", ExerciseType.Cardio, MuscleGroup.Core, 1, EquipmentType.None, 3, 3, 0, 40, true,
            false, false, false, false, false, 45, 80, 85, 85,
            new[] { "Stand tall, pull elbows down as you lift your right knee toward chest.", "Lower and repeat with left knee in an active cadence.", "Breathe steadily." },
            "Low-impact cardio and standing abdominal movement.", null, "ex_slow_mtn_climber", new[] { "ex_march", "ex_step_touch" });

        AddEx("ex_russian_twist", "Russian Twists", ExerciseType.Strength, MuscleGroup.Core, 2, EquipmentType.Mat, 3, 3, 16, 0, false,
            false, true, false, false, false, 75, 75, 80, 55,
            new[] { "Sit on floor with knees bent, lean back slightly.", "Clasp hands and rotate torso to touch floor on right, then left.", "Keep motion controlled." },
            "Flags back: keep chest open and avoid hunching shoulders.", "ex_dead_bug", null, new[] { "ex_side_plank" });

        // 6. Cardio & Conditioning
        AddEx("ex_jumping_jacks", "Jumping Jacks", ExerciseType.Cardio, MuscleGroup.FullBody, 2, EquipmentType.None, 3, 3, 0, 45, true,
            true, false, false, false, true, 40, 90, 85, 95,
            new[] { "Start with feet together and arms at sides.", "Jump feet outward while sweeping arms overhead.", "Jump back to starting position." },
            "Flags knees and high impact: substitute with step-touch intervals if joints hurt.", "ex_step_touch", null, new[] { "ex_high_knees", "ex_butt_kicks" });

        AddEx("ex_high_knees", "High Knees", ExerciseType.Cardio, MuscleGroup.FullBody, 3, EquipmentType.None, 3, 3, 0, 30, true,
            true, false, false, false, true, 50, 95, 85, 95,
            new[] { "Jog in place while driving knees up to hip level.", "Pump arms vigorously.", "Land softly on balls of feet." },
            "High impact cardio: avoid if knee limitation or age 50+.", "ex_march", null, new[] { "ex_jumping_jacks" });

        AddEx("ex_slow_mtn_climber", "Slow Mountain Climbers", ExerciseType.Cardio, MuscleGroup.Core, 2, EquipmentType.Mat, 3, 3, 16, 0, false,
            false, false, true, true, false, 65, 85, 85, 80,
            new[] { "Begin in high plank position.", "Slowly drive right knee toward chest, pause, and return.", "Alternate smoothly with left leg." },
            "Controlled core and cardio exercise without explosive impact.", "ex_knee_plank", "ex_mtn_climber", new[] { "ex_standing_knee_lift" });

        AddEx("ex_mtn_climber", "Mountain Climbers", ExerciseType.Cardio, MuscleGroup.Core, 3, EquipmentType.Mat, 3, 3, 0, 30, true,
            false, false, true, true, true, 70, 90, 85, 90,
            new[] { "From plank, rapidly run knees into chest alternating legs.", "Keep hips level and hands firmly planted.", "Maintain fast pace." },
            "Flags wrists and high impact: fast full-body metabolic burner.", "ex_slow_mtn_climber", null, new[] { "ex_slow_mtn_climber" });

        AddEx("ex_butt_kicks_low", "Low-Impact Butt Kicks", ExerciseType.Cardio, MuscleGroup.Legs, 1, EquipmentType.None, 3, 3, 0, 45, true,
            false, false, false, false, false, 30, 80, 85, 75,
            new[] { "Step side to side, bringing one heel at a time toward glutes.", "Gently pull elbows back with each step.", "Keep continuous cadence." },
            "Gentle on knee joints while activating hamstrings.", null, "ex_butt_kicks", new[] { "ex_step_touch" });

        AddEx("ex_butt_kicks", "Butt Kicks (Running)", ExerciseType.Cardio, MuscleGroup.Legs, 2, EquipmentType.None, 3, 3, 0, 30, true,
            true, false, false, false, true, 40, 85, 80, 90,
            new[] { "Jog in place, kicking heels up to touch glutes on every stride.", "Maintain tall upright posture.", "Land on forefoot." },
            "Flags knees and high impact: running cadence drill.", "ex_butt_kicks_low", null, new[] { "ex_high_knees" });

        AddEx("ex_shadow_box", "Shadow Boxing", ExerciseType.Cardio, MuscleGroup.FullBody, 2, EquipmentType.None, 4, 3, 0, 60, true,
            false, false, false, false, false, 50, 88, 85, 90,
            new[] { "Adopt a boxer stance, hands guarding chin.", "Throw light jabs, crosses and hooks into the air.", "Keep feet bouncing lightly and hips rotating." },
            "Fun, low impact cardio that builds shoulder stamina and core rotation.", null, null, new[] { "ex_step_touch" });

        // 7. Shoulders
        AddEx("ex_db_overhead_press", "Dumbbell Overhead Press", ExerciseType.Strength, MuscleGroup.Shoulders, 3, EquipmentType.Dumbbells, 4, 3, 10, 0, false,
            false, false, true, false, false, 85, 60, 75, 40,
            new[] { "Hold dumbbells at shoulder height with palms forward.", "Press weights overhead until arms are extended.", "Lower slowly back to ears." },
            "Flags shoulders: avoid flaring elbows wide or arching lower back.", "ex_band_lat_raise", null, new[] { "ex_pushup" });

        AddEx("ex_band_lat_raise", "Resistance Band Lateral Raise", ExerciseType.Strength, MuscleGroup.Shoulders, 2, EquipmentType.ResistanceBands, 4, 3, 12, 0, false,
            false, false, true, false, false, 75, 60, 75, 40,
            new[] { "Step on band center, hold handles at sides.", "Raise arms out to sides until parallel to floor.", "Lower with control." },
            "Flags shoulders: lead with elbows and keep hands slightly below elbows.", null, "ex_db_overhead_press", new[] { "ex_arm_circles" });

        // 8. Stretching & Cool-down
        AddEx("ex_hamstring_stretch", "Standing Hamstring Stretch", ExerciseType.Stretch, MuscleGroup.Legs, 1, EquipmentType.None, 3, 2, 0, 30, true,
            false, false, false, false, false, 15, 20, 85, 20,
            new[] { "Extend one leg forward with heel down and toes pointing up.", "Hinge forward at hips keeping back flat.", "Feel gentle stretch along back of thigh." },
            "Crucial cool-down stretch for hamstrings and lower back.", null, null, new[] { "ex_quad_stretch" });

        AddEx("ex_quad_stretch", "Standing Quad Stretch", ExerciseType.Stretch, MuscleGroup.Legs, 1, EquipmentType.None, 3, 2, 0, 30, true,
            false, false, false, false, false, 15, 20, 85, 20,
            new[] { "Stand on one leg, hold wall for balance.", "Bend other knee and hold ankle, gently drawing heel toward glute.", "Keep knees side by side." },
            "Relieves tension in front of thighs and hip flexors.", null, null, new[] { "ex_hamstring_stretch" });

        AddEx("ex_chest_opener", "Chest Opener Stretch", ExerciseType.Stretch, MuscleGroup.Chest, 1, EquipmentType.None, 2, 2, 0, 30, true,
            false, false, false, false, false, 20, 20, 85, 20,
            new[] { "Interlace fingers behind your back or place hands on hips.", "Gently draw shoulder blades together and lift chest upward.", "Breathe deeply." },
            "Reverses slouching posture and opens tight pectoral muscles.", null, null, new[] { "ex_childs_pose" });

        AddEx("ex_childs_pose", "Child's Pose", ExerciseType.Stretch, MuscleGroup.Back, 1, EquipmentType.Mat, 3, 2, 0, 45, true,
            false, false, false, false, false, 20, 20, 90, 20,
            new[] { "Kneel on mat, sit hips back onto heels.", "Reach arms forward along mat and rest forehead on the floor.", "Take long calming breaths into lower back." },
            "Deep restorative stretch for spine, lats and hips.", null, null, new[] { "ex_cat_cow" });

        AddEx("ex_cat_cow", "Cat-Cow Stretch", ExerciseType.Stretch, MuscleGroup.Back, 1, EquipmentType.Mat, 3, 2, 10, 0, false,
            false, false, false, false, false, 20, 20, 90, 20,
            new[] { "On hands and knees, inhale and arch back, looking up (Cow).", "Exhale and round spine up toward ceiling, tucking chin (Cat).", "Flow smoothly." },
            "Restores segmental mobility to entire spinal column.", null, null, new[] { "ex_childs_pose" });

        AddEx("ex_shoulder_stretch", "Cross-Body Shoulder Stretch", ExerciseType.Stretch, MuscleGroup.Shoulders, 1, EquipmentType.None, 2, 2, 0, 30, true,
            false, false, false, false, false, 15, 20, 85, 20,
            new[] { "Bring right arm across chest.", "Use left hand to gently pull right arm closer to body.", "Hold and switch sides." },
            "Soothes posterior shoulder muscles after upper body training.", null, null, new[] { "ex_tricep_stretch" });

        AddEx("ex_tricep_stretch", "Overhead Triceps Stretch", ExerciseType.Stretch, MuscleGroup.Arms, 1, EquipmentType.None, 2, 2, 0, 30, true,
            false, false, false, false, false, 15, 20, 85, 20,
            new[] { "Reach right hand up and bend elbow behind neck.", "Gently pull right elbow with left hand.", "Hold and switch." },
            "Relieves triceps and lat tension.", null, null, new[] { "ex_shoulder_stretch" });

        AddEx("ex_side_reach", "Standing Side Reach", ExerciseType.Stretch, MuscleGroup.Core, 1, EquipmentType.None, 2, 2, 0, 30, true,
            false, false, false, false, false, 15, 20, 85, 20,
            new[] { "Stand tall, reach right arm up and arc over to the left.", "Feel lateral stretch along ribs and waist.", "Repeat on opposite side." },
            "Opens intercostals and improves breathing mechanics.", null, null, new[] { "ex_torso_twist" });
    }

    private void AddEx(string id, string name, ExerciseType type, MuscleGroup muscle, int diff,
        EquipmentType equip, int timeMin, int sets, int reps, int secs, bool timed,
        bool knees, bool back, bool shoulders, bool wrists, bool highImpact,
        int muscleScr, int weightLossScr, int stayFitScr, int cardioScr,
        string[] instructions, string tips, string? easier, string? harder, string[] similar)
    {
        var ex = new Exercise
        {
            Id = id,
            Name = name,
            Type = type,
            MuscleGroup = muscle,
            Difficulty = diff,
            Equipment = equip,
            TimeMinutes = timeMin,
            DefaultSets = sets,
            DefaultReps = reps,
            DefaultSeconds = secs,
            IsTimed = timed,
            StressesKnees = knees,
            StressesBack = back,
            StressesShoulders = shoulders,
            StressesWrists = wrists,
            IsHighImpact = highImpact,
            MuscleBenefit = muscleScr,
            WeightLossBenefit = weightLossScr,
            StayFitBenefit = stayFitScr,
            CardioBenefit = cardioScr,
            Instructions = instructions,
            Tips = tips,
            EasierVariationId = easier,
            HarderVariationId = harder,
            SimilarExerciseIds = similar
        };

        Exercises.Add(ex);
    }

    private void BuildExerciseLookupTables()
    {
        ExerciseNameTableChaining.Clear();
        ExerciseNameTableOpenAddressing.Clear();

        for (int i = 0; i < Exercises.Count; i++)
        {
            var ex = Exercises[i];
            ExerciseNameTableChaining.Insert(ex.Name, ex);
            ExerciseNameTableOpenAddressing.Insert(ex.Name, ex);
        }
    }

    private void BuildExerciseGraph()
    {
        ExerciseProgressionGraph.Clear();

        // Add all exercise names as vertices
        for (int i = 0; i < Exercises.Count; i++)
        {
            ExerciseProgressionGraph.AddVertex(Exercises[i].Name);
        }

        // Add edges based on progression hierarchy
        for (int i = 0; i < Exercises.Count; i++)
        {
            var ex = Exercises[i];

            if (!string.IsNullOrEmpty(ex.HarderVariationId))
            {
                var harder = FindExerciseById(ex.HarderVariationId);
                if (harder != null)
                {
                    double weight = Math.Max(1.0, harder.Difficulty - ex.Difficulty);
                    ExerciseProgressionGraph.AddEdge(ex.Name, harder.Name, weight, bidirectional: true, label: "Harder Variation");
                }
            }

            if (!string.IsNullOrEmpty(ex.EasierVariationId))
            {
                var easier = FindExerciseById(ex.EasierVariationId);
                if (easier != null)
                {
                    double weight = Math.Max(1.0, ex.Difficulty - easier.Difficulty);
                    ExerciseProgressionGraph.AddEdge(ex.Name, easier.Name, weight, bidirectional: true, label: "Easier Variation");
                }
            }

            if (ex.SimilarExerciseIds != null)
            {
                for (int s = 0; s < ex.SimilarExerciseIds.Length; s++)
                {
                    var sim = FindExerciseById(ex.SimilarExerciseIds[s]);
                    if (sim != null)
                    {
                        double weight = 1.5 + Math.Abs(ex.Difficulty - sim.Difficulty) * 0.5;
                        ExerciseProgressionGraph.AddEdge(ex.Name, sim.Name, weight, bidirectional: true, label: "Similar Alternative");
                    }
                }
            }
        }
    }

    public Exercise? FindExerciseById(string id)
    {
        for (int i = 0; i < Exercises.Count; i++)
        {
            if (Exercises[i].Id == id) return Exercises[i];
        }
        return null;
    }

    public Exercise? FindExerciseByName(string name)
    {
        if (ExerciseNameTableChaining.TryGetValue(name, out var ex))
        {
            return ex;
        }

        // Fallback linear scan
        for (int i = 0; i < Exercises.Count; i++)
        {
            if (string.Equals(Exercises[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return Exercises[i];
        }
        return null;
    }

    public void AddCustomExercise(Exercise exercise)
    {
        if (string.IsNullOrEmpty(exercise.Id))
        {
            exercise.Id = "cust_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
        Exercises.Add(exercise);
        ExerciseNameTableChaining.Insert(exercise.Name, exercise);
        ExerciseNameTableOpenAddressing.Insert(exercise.Name, exercise);
        ExerciseProgressionGraph.AddVertex(exercise.Name);
    }

    public bool DeleteExercise(string id)
    {
        for (int i = 0; i < Exercises.Count; i++)
        {
            if (Exercises[i].Id == id)
            {
                string name = Exercises[i].Name;
                Exercises.RemoveAt(i);
                ExerciseNameTableChaining.Remove(name);
                ExerciseNameTableOpenAddressing.Remove(name);
                return true;
            }
        }
        return false;
    }

    public void SavePlan(WorkoutPlan plan)
    {
        SavedPlansList.Add(plan);
        // Duration is key for BST and AVL tree storage
        SavedPlansBst.Insert(plan.TargetSessionMinutes, plan);
        SavedPlansAvl.Insert(plan.TargetSessionMinutes, plan);
    }

    public bool DeleteSavedPlan(string planId)
    {
        for (int i = 0; i < SavedPlansList.Count; i++)
        {
            if (SavedPlansList[i].Id == planId)
            {
                var plan = SavedPlansList[i];
                SavedPlansList.RemoveAt(i);
                SavedPlansBst.Delete(plan.TargetSessionMinutes, p => p.Id == planId);
                SavedPlansAvl.Delete(plan.TargetSessionMinutes, p => p.Id == planId);
                return true;
            }
        }
        return false;
    }

    public CustomArray<WorkoutPlan> SearchPlansByDuration(int exactMinutes)
    {
        return SavedPlansBst.Search(exactMinutes);
    }

    public CustomArray<WorkoutPlan> SearchPlansByDurationRange(int minMinutes, int maxMinutes)
    {
        return SavedPlansBst.GetRange(minMinutes, maxMinutes);
    }

    private void InitializeSampleUserData()
    {
        // Default worked example from Section 9 of the proposal:
        // 32-year-old woman, 160 cm, 70 kg, walks 2 days a week, goal: lose weight, 3 days/wk, 30 min, chair only, sore knees
        CurrentProfile = new UserProfile
        {
            Name = "Athlete",
            Age = 32,
            Gender = GenderType.Female,
            HeightCm = 160.0,
            WeightKg = 70.0,
            CurrentActivityDays = 2,
            CurrentActivityKind = "walking",
            StartingLevel = FitnessLevel.Beginner,
            Goal = GoalType.LoseWeight,
            WorkoutDaysPerWeek = 3,
            MinutesPerSession = 30,
            HasChairOrBench = true,
            HasKneeLimitation = true,
            CurrentStreak = 3,
            BestStreak = 5,
            TotalWorkoutsCompleted = 8,
            TotalMinutesExercised = 240
        };

        // Populate sample weight history for progress charts
        var now = DateTime.Now;
        WeightLogs.Add(new WeightLogEntry { Date = now.AddDays(-28), WeightKg = 72.0, Bmi = 28.1, Note = "Starting week 1" });
        WeightLogs.Add(new WeightLogEntry { Date = now.AddDays(-21), WeightKg = 71.4, Bmi = 27.9, Note = "Week 2 check-in" });
        WeightLogs.Add(new WeightLogEntry { Date = now.AddDays(-14), WeightKg = 70.8, Bmi = 27.7, Note = "Week 3 check-in" });
        WeightLogs.Add(new WeightLogEntry { Date = now.AddDays(-7), WeightKg = 70.3, Bmi = 27.5, Note = "Week 4 check-in" });
        WeightLogs.Add(new WeightLogEntry { Date = now, WeightKg = 70.0, Bmi = 27.3, Note = "Current week" });

        // Populate sample session history
        SessionLogs.Add(new WorkoutSessionLog
        {
            Date = now.AddDays(-5),
            DayName = "Monday",
            PlanName = "Weight Loss Kickstarter",
            MinutesTrained = 30,
            ExercisesCompleted = 5,
            TotalExercises = 5,
            Rating = SessionRating.JustRight,
            WorkloadExpression = "(10 * 3) + (10 * 3) + (12 * 3) + (10 * 3)",
            CalculatedWorkload = 96
        });
        SessionLogs.Add(new WorkoutSessionLog
        {
            Date = now.AddDays(-3),
            DayName = "Wednesday",
            PlanName = "Weight Loss Kickstarter",
            MinutesTrained = 30,
            ExercisesCompleted = 5,
            TotalExercises = 5,
            Rating = SessionRating.JustRight,
            WorkloadExpression = "(10 * 3) + (10 * 3) + (12 * 3) + (25 * 3)",
            CalculatedWorkload = 141
        });
        SessionLogs.Add(new WorkoutSessionLog
        {
            Date = now.AddDays(-1),
            DayName = "Friday",
            PlanName = "Weight Loss Kickstarter",
            MinutesTrained = 30,
            ExercisesCompleted = 5,
            TotalExercises = 5,
            Rating = SessionRating.JustRight,
            WorkloadExpression = "(10 * 3) + (10 * 3) + (10 * 3) + (12 * 3)",
            CalculatedWorkload = 96
        });

        // Seed diverse workout plans across durations for BST & AVL Tree visualization
        if (SavedPlansList.Count == 0)
        {
            var p30 = new WorkoutPlan
            {
                Id = Guid.NewGuid().ToString("N"),
                PlanName = "LoseWeight Foundation (30m)",
                Goal = GoalType.LoseWeight,
                FitnessLevel = FitnessLevel.Beginner,
                TargetSessionMinutes = 30,
                WorkoutDaysCount = 3
            };
            SavePlan(p30);

            var p20 = new WorkoutPlan
            {
                Id = Guid.NewGuid().ToString("N"),
                PlanName = "Quick HIIT Burner (20m)",
                Goal = GoalType.BoostCardio,
                FitnessLevel = FitnessLevel.Intermediate,
                TargetSessionMinutes = 20,
                WorkoutDaysCount = 4
            };
            SavePlan(p20);

            var p45 = new WorkoutPlan
            {
                Id = Guid.NewGuid().ToString("N"),
                PlanName = "Total Body Hypertrophy (45m)",
                Goal = GoalType.BuildMuscle,
                FitnessLevel = FitnessLevel.Intermediate,
                TargetSessionMinutes = 45,
                WorkoutDaysCount = 4
            };
            SavePlan(p45);

            var p15 = new WorkoutPlan
            {
                Id = Guid.NewGuid().ToString("N"),
                PlanName = "Express Core & Mobility (15m)",
                Goal = GoalType.StayFit,
                FitnessLevel = FitnessLevel.Beginner,
                TargetSessionMinutes = 15,
                WorkoutDaysCount = 5
            };
            SavePlan(p15);

            var p60 = new WorkoutPlan
            {
                Id = Guid.NewGuid().ToString("N"),
                PlanName = "Endurance & Conditioning (60m)",
                Goal = GoalType.BoostCardio,
                FitnessLevel = FitnessLevel.Advanced,
                TargetSessionMinutes = 60,
                WorkoutDaysCount = 3
            };
            SavePlan(p60);
        }
    }
}

