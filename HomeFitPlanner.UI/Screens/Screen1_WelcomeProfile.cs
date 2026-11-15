using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen1_WelcomeProfile : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly DataManager _dataManager;

    private CheckBox _chkSafety = null!;
    private TextBox _txtName = null!;
    private NumericUpDown _numAge = null!;
    private ComboBox _cmbGender = null!;

    private NumericUpDown _numHeight = null!;
    private NumericUpDown _numWeight = null!;
    private Label _lblBmiResult = null!;
    private Label _lblCaloriesResult = null!;

    private NumericUpDown _numActivityDays = null!;
    private ComboBox _cmbActivityKind = null!;
    private Label _lblStartingLevel = null!;

    private ComboBox _cmbGoal = null!;
    private NumericUpDown _numPlanDays = null!;
    private ComboBox _cmbMinutes = null!;

    private CheckBox _chkEquipBands = null!;
    private CheckBox _chkEquipDumbbells = null!;
    private CheckBox _chkEquipMat = null!;
    private CheckBox _chkEquipChair = null!;

    private CheckBox _chkLimitKnees = null!;
    private CheckBox _chkLimitBack = null!;
    private CheckBox _chkLimitShoulders = null!;
    private CheckBox _chkLimitWrists = null!;

    private Label _lblSummary = null!;
    private Button _btnConfirm = null!;

    public Screen1_WelcomeProfile(Action<string> navigateAction)
    {
        _navigateAction = navigateAction;
        _dataManager = DataManager.Instance;
        InitializeComponent();
        LoadExistingProfile();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UITheme.Background;
        AutoScroll = true;

        var headerPanel = new Panel { Location = new Point(24, 20), Size = new Size(950, 60) };
        var lblTitle = new Label
        {
            Text = "Welcome to HomeFit Planner",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextMain,
            AutoSize = true,
            Location = new Point(0, 0)
        };
        var lblSubtitle = new Label
        {
            Text = "A personalized, adaptive home workout planning system powered by custom data structures & algorithms.",
            Font = UITheme.BodyFont,
            ForeColor = UITheme.TextMuted,
            AutoSize = true,
            Location = new Point(0, 32)
        };
        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(lblSubtitle);
        Controls.Add(headerPanel);

        // Safety Notice Card
        var safetyCard = UITheme.CreateCard(24, 90, 950, 75);
        safetyCard.BackColor = Color.FromArgb(254, 242, 242);
        var lblSafetyNotice = new Label
        {
            Text = "SAFETY NOTICE: HomeFit Planner is a personal planning tool and not medical advice. " +
                   "Always consult a physician before beginning any new exercise routine. Stop immediately if you experience pain.",
            Font = UITheme.BodyFont,
            ForeColor = UITheme.Danger,
            Location = new Point(16, 12),
            Size = new Size(910, 32)
        };
        _chkSafety = new CheckBox
        {
            Text = "I understand that this is a planning tool and I will stop immediately if something hurts.",
            Font = UITheme.SubtitleFont,
            ForeColor = UITheme.TextMain,
            Checked = true,
            Location = new Point(16, 45),
            Size = new Size(700, 24)
        };
        safetyCard.Controls.Add(lblSafetyNotice);
        safetyCard.Controls.Add(_chkSafety);
        Controls.Add(safetyCard);

        // 5-Step Form Panel
        int yOffset = 180;

        // Step 1: About You
        var pnlStep1 = UITheme.CreateCard(24, yOffset, 460, 160);
        AddStepHeader(pnlStep1, "Step 1: About You", "Personal identifiers and age parameters");

        var lblName = new Label { Text = "Full Name:", Location = new Point(16, 45), AutoSize = true, Font = UITheme.BodyFont };
        _txtName = new TextBox { Text = "Athlete", Location = new Point(16, 68), Size = new Size(200, 24) };

        var lblAge = new Label { Text = "Age (18+):", Location = new Point(236, 45), AutoSize = true, Font = UITheme.BodyFont };
        _numAge = new NumericUpDown { Minimum = 18, Maximum = 99, Value = 32, Location = new Point(236, 68), Size = new Size(80, 24) };
        _numAge.ValueChanged += (s, e) => UpdateCalculations();

        var lblGender = new Label { Text = "Gender (for calories):", Location = new Point(16, 100), AutoSize = true, Font = UITheme.BodyFont };
        _cmbGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 122), Size = new Size(200, 24) };
        _cmbGender.Items.AddRange(new object[] { "Female", "Male", "Prefer not to say" });
        _cmbGender.SelectedIndex = 0;
        _cmbGender.SelectedIndexChanged += (s, e) => UpdateCalculations();

        pnlStep1.Controls.AddRange(new Control[] { lblName, _txtName, lblAge, _numAge, lblGender, _cmbGender });
        Controls.Add(pnlStep1);

        // Step 2: Your Body (BMI & Calories)
        var pnlStep2 = UITheme.CreateCard(500, yOffset, 474, 160);
        AddStepHeader(pnlStep2, "Step 2: Your Body Details", "Height, weight, and automated metabolic analysis");

        var lblHeight = new Label { Text = "Height (cm):", Location = new Point(16, 45), AutoSize = true, Font = UITheme.BodyFont };
        _numHeight = new NumericUpDown { Minimum = 120, Maximum = 230, Value = 160, Location = new Point(16, 68), Size = new Size(90, 24) };
        _numHeight.ValueChanged += (s, e) => UpdateCalculations();

        var lblWeight = new Label { Text = "Weight (kg):", Location = new Point(126, 45), AutoSize = true, Font = UITheme.BodyFont };
        _numWeight = new NumericUpDown { Minimum = 35, Maximum = 200, Value = 70, Location = new Point(126, 68), Size = new Size(90, 24) };
        _numWeight.ValueChanged += (s, e) => UpdateCalculations();

        _lblBmiResult = new Label
        {
            Text = "BMI: 27.3 (Overweight)",
            Font = UITheme.SubtitleFont,
            ForeColor = UITheme.Primary,
            Location = new Point(240, 52),
            AutoSize = true
        };

        _lblCaloriesResult = new Label
        {
            Text = "Est. Daily Need: 1,750 kcal/day",
            Font = UITheme.SubtitleFont,
            ForeColor = UITheme.Secondary,
            Location = new Point(240, 80),
            AutoSize = true
        };

        var lblRoughGuide = new Label
        {
            Text = "*BMI and calorie needs are rough guides and never used to judge.",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(16, 118),
            Size = new Size(440, 30)
        };

        pnlStep2.Controls.AddRange(new Control[] { lblHeight, _numHeight, lblWeight, _numWeight, _lblBmiResult, _lblCaloriesResult, lblRoughGuide });
        Controls.Add(pnlStep2);

        // Step 3: Current Activity
        yOffset += 175;
        var pnlStep3 = UITheme.CreateCard(24, yOffset, 460, 160);
        AddStepHeader(pnlStep3, "Step 3: Current Activity", "Baseline activity setting starting fitness level");

        var lblActDays = new Label { Text = "Days active per week:", Location = new Point(16, 45), AutoSize = true, Font = UITheme.BodyFont };
        _numActivityDays = new NumericUpDown { Minimum = 0, Maximum = 7, Value = 2, Location = new Point(16, 68), Size = new Size(80, 24) };
        _numActivityDays.ValueChanged += (s, e) => UpdateCalculations();

        var lblActKind = new Label { Text = "Primary kind of exercise:", Location = new Point(126, 45), AutoSize = true, Font = UITheme.BodyFont };
        _cmbActivityKind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(126, 68), Size = new Size(220, 24) };
        _cmbActivityKind.Items.AddRange(new object[] { "walking", "cardio", "strength", "nothing regular" });
        _cmbActivityKind.SelectedIndex = 0;
        _cmbActivityKind.SelectedIndexChanged += (s, e) => UpdateCalculations();

        _lblStartingLevel = new Label
        {
            Text = "Starting Level: Beginner (Gentle & clear start)",
            Font = UITheme.SubtitleFont,
            ForeColor = UITheme.Success,
            Location = new Point(16, 110),
            AutoSize = true
        };

        pnlStep3.Controls.AddRange(new Control[] { lblActDays, _numActivityDays, lblActKind, _cmbActivityKind, _lblStartingLevel });
        Controls.Add(pnlStep3);

        // Step 4: Your Goal
        var pnlStep4 = UITheme.CreateCard(500, yOffset, 474, 160);
        AddStepHeader(pnlStep4, "Step 4: Your Fitness Goal", "Sets the blend of strength, cardio and rest (Table 2)");

        var lblGoal = new Label { Text = "Select Goal:", Location = new Point(16, 45), AutoSize = true, Font = UITheme.BodyFont };
        _cmbGoal = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 68), Size = new Size(200, 24) };
        _cmbGoal.Items.AddRange(new object[] { "Lose Weight", "Build Muscle", "Stay Fit", "Boost Cardio" });
        _cmbGoal.SelectedIndex = 0;
        _cmbGoal.SelectedIndexChanged += (s, e) => UpdateCalculations();

        var lblGoalDesc = new Label
        {
            Text = "• Lose Weight: Mix of cardio intervals & joint-friendly full body strength\n" +
                   "• Build Muscle: Hypertrophy focus split by body area with progressive overload\n" +
                   "• Stay Fit: Balanced, sustainable full body conditioning & core health\n" +
                   "• Boost Cardio: Aerobic intervals and stamina endurance",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(16, 98),
            Size = new Size(440, 56)
        };

        pnlStep4.Controls.AddRange(new Control[] { lblGoal, _cmbGoal, lblGoalDesc });
        Controls.Add(pnlStep4);

        // Step 5: Setup & Limitations
        yOffset += 175;
        var pnlStep5 = UITheme.CreateCard(24, yOffset, 950, 180);
        AddStepHeader(pnlStep5, "Step 5: Setup, Equipment & Limitations", "Customize days, time, and physical safety boundaries");

        var lblPlanDays = new Label { Text = "Workout Days/Wk (2-6):", Location = new Point(16, 45), AutoSize = true, Font = UITheme.BodyFont };
        _numPlanDays = new NumericUpDown { Minimum = 2, Maximum = 6, Value = 3, Location = new Point(16, 68), Size = new Size(80, 24) };
        _numPlanDays.ValueChanged += (s, e) => UpdateCalculations();

        var lblMinutes = new Label { Text = "Minutes per session:", Location = new Point(180, 45), AutoSize = true, Font = UITheme.BodyFont };
        _cmbMinutes = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(180, 68), Size = new Size(100, 24) };
        _cmbMinutes.Items.AddRange(new object[] { "15", "20", "30", "45", "60" });
        _cmbMinutes.SelectedItem = "30";
        _cmbMinutes.SelectedIndexChanged += (s, e) => UpdateCalculations();

        // Equipment checkboxes
        var lblEquip = new Label { Text = "Available Equipment:", Location = new Point(350, 45), AutoSize = true, Font = UITheme.SubtitleFont };
        _chkEquipChair = new CheckBox { Text = "Chair / Bench", Location = new Point(350, 70), AutoSize = true, Checked = true };
        _chkEquipMat = new CheckBox { Text = "Exercise Mat", Location = new Point(350, 95), AutoSize = true, Checked = false };
        _chkEquipBands = new CheckBox { Text = "Resistance Bands", Location = new Point(480, 70), AutoSize = true, Checked = false };
        _chkEquipDumbbells = new CheckBox { Text = "Dumbbells", Location = new Point(480, 95), AutoSize = true, Checked = false };

        // Limitation checkboxes
        var lblLimit = new Label { Text = "Physical Limitations:", Location = new Point(650, 45), AutoSize = true, Font = UITheme.SubtitleFont };
        _chkLimitKnees = new CheckBox { Text = "Sore Knees", Location = new Point(650, 70), AutoSize = true, Checked = true };
        _chkLimitBack = new CheckBox { Text = "Weak Back", Location = new Point(650, 95), AutoSize = true, Checked = false };
        _chkLimitShoulders = new CheckBox { Text = "Shoulders", Location = new Point(780, 70), AutoSize = true, Checked = false };
        _chkLimitWrists = new CheckBox { Text = "Sensitive Wrists", Location = new Point(780, 95), AutoSize = true, Checked = false };

        _chkLimitKnees.CheckedChanged += (s, e) => UpdateCalculations();

        pnlStep5.Controls.AddRange(new Control[] {
            lblPlanDays, _numPlanDays, lblMinutes, _cmbMinutes,
            lblEquip, _chkEquipChair, _chkEquipMat, _chkEquipBands, _chkEquipDumbbells,
            lblLimit, _chkLimitKnees, _chkLimitBack, _chkLimitShoulders, _chkLimitWrists
        });
        Controls.Add(pnlStep5);

        // Summary Card & Action Button
        yOffset += 195;
        var pnlSummary = UITheme.CreateCard(24, yOffset, 950, 110);
        pnlSummary.BackColor = Color.FromArgb(240, 253, 250); // Light teal
        var lblSumTitle = new Label { Text = "Personalized Recommendation Summary", Font = UITheme.SubtitleFont, ForeColor = UITheme.Secondary, Location = new Point(16, 12), AutoSize = true };
        _lblSummary = new Label
        {
            Text = "Suggested Plan: 3 days a week, 30 minutes per session with joint-friendly weight loss focus. Knees protected.",
            Font = UITheme.BodyFont,
            ForeColor = UITheme.TextMain,
            Location = new Point(16, 38),
            Size = new Size(700, 48)
        };

        _btnConfirm = UITheme.CreatePrimaryButton("Confirm & Generate Plan", 720, 35, 200, 45);
        _btnConfirm.Click += BtnConfirm_Click;

        pnlSummary.Controls.AddRange(new Control[] { lblSumTitle, _lblSummary, _btnConfirm });
        Controls.Add(pnlSummary);
    }

    private void AddStepHeader(Panel pnl, string title, string subtitle)
    {
        var lblT = new Label { Text = title, Font = UITheme.SubtitleFont, ForeColor = UITheme.Primary, Location = new Point(16, 10), AutoSize = true };
        var lblS = new Label { Text = subtitle, Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(16, 28), AutoSize = true };
        pnl.Controls.Add(lblT);
        pnl.Controls.Add(lblS);
    }

    private void LoadExistingProfile()
    {
        var p = _dataManager.CurrentProfile;
        _txtName.Text = p.Name;
        _numAge.Value = Math.Clamp(p.Age, 18, 99);
        _cmbGender.SelectedIndex = p.Gender switch { GenderType.Female => 0, GenderType.Male => 1, _ => 2 };
        _numHeight.Value = (decimal)p.HeightCm;
        _numWeight.Value = (decimal)p.WeightKg;
        _numActivityDays.Value = p.CurrentActivityDays;
        _cmbActivityKind.SelectedItem = p.CurrentActivityKind;
        _cmbGoal.SelectedIndex = (int)p.Goal;
        _numPlanDays.Value = p.WorkoutDaysPerWeek;
        _cmbMinutes.SelectedItem = p.MinutesPerSession.ToString();
        _chkEquipChair.Checked = p.HasChairOrBench;
        _chkEquipMat.Checked = p.HasMat;
        _chkEquipBands.Checked = p.HasResistanceBands;
        _chkEquipDumbbells.Checked = p.HasDumbbells;
        _chkLimitKnees.Checked = p.HasKneeLimitation;
        _chkLimitBack.Checked = p.HasBackLimitation;
        _chkLimitShoulders.Checked = p.HasShoulderLimitation;
        _chkLimitWrists.Checked = p.HasWristLimitation;

        UpdateCalculations();
    }

    private void UpdateCalculations()
    {
        double h = (double)_numHeight.Value;
        double w = (double)_numWeight.Value;
        int age = (int)_numAge.Value;
        var gender = _cmbGender.SelectedIndex switch { 0 => GenderType.Female, 1 => GenderType.Male, _ => GenderType.PreferNotToSay };
        int actDays = (int)_numActivityDays.Value;
        string kind = _cmbActivityKind.SelectedItem?.ToString() ?? "walking";
        var goal = (GoalType)_cmbGoal.SelectedIndex;

        var tempProfile = new UserProfile
        {
            HeightCm = h,
            WeightKg = w,
            Age = age,
            Gender = gender,
            CurrentActivityDays = actDays,
            CurrentActivityKind = kind,
            Goal = goal
        };
        tempProfile.DeriveFitnessLevel();

        double bmi = tempProfile.CalculateBmi();
        string bmiCat = tempProfile.GetBmiCategory();
        int calories = tempProfile.CalculateDailyCalories();

        _lblBmiResult.Text = $"BMI: {bmi:F1} ({bmiCat})";
        _lblCaloriesResult.Text = $"Est. Daily Need: {calories:N0} kcal/day";
        _lblStartingLevel.Text = $"Starting Level: {tempProfile.StartingLevel} " +
            (tempProfile.StartingLevel == FitnessLevel.Beginner ? "(Gentle, foundation building)" : "(Progressive load)");

        string lims = _chkLimitKnees.Checked ? "Knees protected." : "No joint flags.";
        _lblSummary.Text = $"Suggested Plan: {_numPlanDays.Value} days/wk, {_cmbMinutes.SelectedItem}m per session with {goal} focus. {lims}";
    }

    private void BtnConfirm_Click(object? sender, EventArgs e)
    {
        if (!_chkSafety.Checked)
        {
            MessageBox.Show("Please confirm that you understand the safety notice to continue.", "Safety Confirmation Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var p = _dataManager.CurrentProfile;
        p.Name = _txtName.Text;
        p.Age = (int)_numAge.Value;
        p.Gender = _cmbGender.SelectedIndex switch { 0 => GenderType.Female, 1 => GenderType.Male, _ => GenderType.PreferNotToSay };
        p.HeightCm = (double)_numHeight.Value;
        p.WeightKg = (double)_numWeight.Value;
        p.CurrentActivityDays = (int)_numActivityDays.Value;
        p.CurrentActivityKind = _cmbActivityKind.SelectedItem?.ToString() ?? "walking";
        p.Goal = (GoalType)_cmbGoal.SelectedIndex;
        p.WorkoutDaysPerWeek = (int)_numPlanDays.Value;
        p.MinutesPerSession = int.Parse(_cmbMinutes.SelectedItem?.ToString() ?? "30");

        p.HasChairOrBench = _chkEquipChair.Checked;
        p.HasMat = _chkEquipMat.Checked;
        p.HasResistanceBands = _chkEquipBands.Checked;
        p.HasDumbbells = _chkEquipDumbbells.Checked;

        p.HasKneeLimitation = _chkLimitKnees.Checked;
        p.HasBackLimitation = _chkLimitBack.Checked;
        p.HasShoulderLimitation = _chkLimitShoulders.Checked;
        p.HasWristLimitation = _chkLimitWrists.Checked;

        p.DeriveFitnessLevel();

        // Generate the 7-day personalized weekly plan
        var generator = new PlanGeneratorService();
        var plan = generator.GeneratePlan(p, $"{p.Name}'s {p.Goal} Plan");
        _dataManager.CurrentPlan = plan;
        _dataManager.SavePlan(plan);

        MessageBox.Show($"Profile successfully updated! Generated a 7-day personal plan for {p.Name} with {p.WorkoutDaysPerWeek} workout days.", "Plan Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // Move to Dashboard
        _navigateAction("Dashboard");
    }
}

