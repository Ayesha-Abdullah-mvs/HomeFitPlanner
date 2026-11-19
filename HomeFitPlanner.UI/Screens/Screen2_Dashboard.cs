using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen2_Dashboard : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly DataManager _dataManager;

    private Label _lblWelcomeGreeting = null!;
    private Label _lblTodayFocus = null!;
    private Label _lblTodayDetails = null!;
    private Button _btnStartWorkout = null!;

    private ProgressBar _pbWeeklyProgress = null!;
    private Label _lblWeeklyStats = null!;
    private Label _lblStreak = null!;

    private Label _lblBmiMetric = null!;
    private Label _lblGoalMetric = null!;
    private Label _lblDailyTip = null!;

    private readonly string[] _dailyTips = new[]
    {
        "Hydration Tip: Drink a glass of water 20-30 minutes before exercising to optimize muscle contractility and prevent cramps.",
        "Posture Reminder: Keep your shoulders back and ribcage down during all standing exercises to protect your spinal alignment.",
        "Recovery Science: Gentle 15-minute walks on rest days promote blood circulation and reduce delayed-onset muscle soreness.",
        "Form Over Speed: Smooth, controlled movements recruit more muscle fibers and prevent unnecessary joint stress.",
        "Rest & Sleep: Muscle tissue rebuilds during deep sleep. Aim for 7 to 8 hours of quality rest every night."
    };

    public Screen2_Dashboard(Action<string> navigateAction)
    {
        _navigateAction = navigateAction;
        _dataManager = DataManager.Instance;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UITheme.Background;
        AutoScroll = true;

        // Header Greeting
        var headerPanel = new Panel { Location = new Point(24, 20), Size = new Size(950, 60) };
        _lblWelcomeGreeting = new Label
        {
            Text = "Good day! Ready to move?",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.TextMain,
            AutoSize = true,
            Location = new Point(0, 0)
        };
        var lblSub = new Label
        {
            Text = "Here is your fitness overview for today. Stay consistent and take each movement step-by-step.",
            Font = UITheme.BodyFont,
            ForeColor = UITheme.TextMuted,
            AutoSize = true,
            Location = new Point(0, 32)
        };
        headerPanel.Controls.Add(_lblWelcomeGreeting);
        headerPanel.Controls.Add(lblSub);
        Controls.Add(headerPanel);

        // 1. Hero Card: Today's Workout / Rest Day
        var pnlHero = UITheme.CreateCard(24, 90, 600, 220);
        pnlHero.BackColor = Color.White;

        var lblTodayTitle = new Label { Text = "TODAY'S WORKOUT", Font = UITheme.SmallFont, ForeColor = UITheme.Primary, Location = new Point(20, 16), AutoSize = true };
        _lblTodayFocus = new Label { Text = "Full-Body Strength (Knee-Friendly)", Font = UITheme.HeaderFont, ForeColor = UITheme.TextMain, Location = new Point(20, 38), Size = new Size(560, 36) };
        _lblTodayDetails = new Label { Text = "5 exercises | Approx. 30 minutes | Target: Tone and strengthen with joint protection.", Font = UITheme.BodyFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 78), Size = new Size(560, 40) };

        _btnStartWorkout = UITheme.CreatePrimaryButton("Start Workout Now", 20, 140, 180, 48);
        _btnStartWorkout.Font = UITheme.TitleFont;
        _btnStartWorkout.Click += (s, e) => _navigateAction("Today's Workout");

        var btnViewPlan = UITheme.CreateSecondaryButton("View Weekly Grid", 215, 140, 150, 48);
        btnViewPlan.Click += (s, e) => _navigateAction("Weekly Plan");

        pnlHero.Controls.AddRange(new Control[] { lblTodayTitle, _lblTodayFocus, _lblTodayDetails, _btnStartWorkout, btnViewPlan });
        Controls.Add(pnlHero);

        // 2. Metrics & Streak Card
        var pnlMetrics = UITheme.CreateCard(640, 90, 334, 220);
        var lblStreakTitle = new Label { Text = "CURRENT STREAK", Font = UITheme.SmallFont, ForeColor = UITheme.Warning, Location = new Point(16, 16), AutoSize = true };
        _lblStreak = new Label { Text = "🔥 3 Planned Sessions Done", Font = UITheme.TitleFont, ForeColor = UITheme.TextMain, Location = new Point(16, 38), AutoSize = true };

        var lblWeekProgressTitle = new Label { Text = "Weekly Completion:", Font = UITheme.BodyFont, ForeColor = UITheme.TextMuted, Location = new Point(16, 80), AutoSize = true };
        _pbWeeklyProgress = new ProgressBar { Location = new Point(16, 104), Size = new Size(300, 16), Maximum = 100, Value = 66 };
        _lblWeeklyStats = new Label { Text = "2 of 3 sessions completed this week (66%)", Font = UITheme.SmallFont, ForeColor = UITheme.TextMain, Location = new Point(16, 126), AutoSize = true };

        var pnlMini = new Panel { Location = new Point(16, 150), Size = new Size(300, 50) };
        _lblBmiMetric = new Label { Text = "BMI: 27.3", Font = UITheme.SubtitleFont, ForeColor = UITheme.Primary, Location = new Point(0, 10), AutoSize = true };
        _lblGoalMetric = new Label { Text = "Goal: Lose Weight", Font = UITheme.SubtitleFont, ForeColor = UITheme.Secondary, Location = new Point(140, 10), AutoSize = true };
        pnlMini.Controls.Add(_lblBmiMetric);
        pnlMini.Controls.Add(_lblGoalMetric);

        pnlMetrics.Controls.AddRange(new Control[] { lblStreakTitle, _lblStreak, lblWeekProgressTitle, _pbWeeklyProgress, _lblWeeklyStats, pnlMini });
        Controls.Add(pnlMetrics);

        // 3. Daily Fitness Tip Card
        var pnlTip = UITheme.CreateCard(24, 325, 950, 95);
        pnlTip.BackColor = Color.FromArgb(240, 249, 255); // Sky light
        var lblTipHeading = new Label { Text = "💡 Daily Coach Insight", Font = UITheme.SubtitleFont, ForeColor = Color.FromArgb(2, 132, 199), Location = new Point(20, 14), AutoSize = true };
        _lblDailyTip = new Label
        {
            Text = _dailyTips[0],
            Font = UITheme.BodyFont,
            ForeColor = UITheme.TextMain,
            Location = new Point(20, 38),
            Size = new Size(820, 45)
        };
        var btnNextTip = UITheme.CreateSecondaryButton("Next Tip", 850, 28, 80, 34);
        btnNextTip.Click += (s, e) =>
        {
            var rnd = new Random();
            _lblDailyTip.Text = _dailyTips[rnd.Next(_dailyTips.Length)];
        };

        pnlTip.Controls.AddRange(new Control[] { lblTipHeading, _lblDailyTip, btnNextTip });
        Controls.Add(pnlTip);

        // 4. Quick Action Shortcuts Grid
        var pnlActions = UITheme.CreateCard(24, 435, 950, 160);
        var lblQuick = new Label { Text = "Quick Navigation", Font = UITheme.SubtitleFont, ForeColor = UITheme.TextMain, Location = new Point(20, 12), AutoSize = true };

        var btnLib = UITheme.CreateSecondaryButton("📚 Exercise Library", 20, 45, 210, 45);
        btnLib.Click += (s, e) => _navigateAction("Exercise Library");

        var btnSub = UITheme.CreateSecondaryButton("🔄 Substitutes & Level-Up", 250, 45, 210, 45);
        btnSub.Click += (s, e) => _navigateAction("Substitutes and Level-Up");

        var btnProg = UITheme.CreateSecondaryButton("📈 Progress Tracking", 480, 45, 210, 45);
        btnProg.Click += (s, e) => _navigateAction("Progress");

        var btnLab = UITheme.CreateSecondaryButton("⚡ Performance Lab", 710, 45, 210, 45);
        btnLab.Click += (s, e) => _navigateAction("Performance Lab");

        var btnEditProf = UITheme.CreateSecondaryButton("⚙️ Edit Profile / Settings", 20, 100, 210, 45);
        btnEditProf.Click += (s, e) => _navigateAction("Welcome & Profile");

        var btnSaved = UITheme.CreateSecondaryButton("📂 Saved Plans (BST/AVL)", 250, 100, 210, 45);
        btnSaved.Click += (s, e) => _navigateAction("Saved Plans");

        pnlActions.Controls.AddRange(new Control[] { lblQuick, btnLib, btnSub, btnProg, btnLab, btnEditProf, btnSaved });
        Controls.Add(pnlActions);
    }

    public void RefreshData()
    {
        var p = _dataManager.CurrentProfile;
        _lblWelcomeGreeting.Text = $"Welcome back, {p.Name}!";
        _lblStreak.Text = $"🔥 {p.CurrentStreak} Planned Sessions Completed (Best: {p.BestStreak})";

        double bmi = p.CalculateBmi();
        _lblBmiMetric.Text = $"BMI: {bmi:F1} ({p.GetBmiCategory()})";
        _lblGoalMetric.Text = $"Goal: {p.Goal}";

        // Check today's scheduled day
        int todayDayOfWeek = ((int)DateTime.Now.DayOfWeek + 6) % 7; // Monday = 0
        var plan = _dataManager.CurrentPlan;

        if (plan == null || plan.Days.Count == 0)
        {
            var gen = new PlanGeneratorService();
            plan = gen.GeneratePlan(p);
            _dataManager.CurrentPlan = plan;
        }

        var todayDay = plan.Days[todayDayOfWeek];
        if (todayDay.IsRestDay)
        {
            _lblTodayFocus.Text = "Today is a Rest Day";
            _lblTodayDetails.Text = "Rest is essential for muscular adaptation. Enjoy an optional 15-20 minute light walk or gentle stretching.";
            _btnStartWorkout.Text = "Optional Light Walk";
        }
        else
        {
            _lblTodayFocus.Text = $"{todayDay.DayName}: {todayDay.FocusDescription}";
            _lblTodayDetails.Text = $"{todayDay.Exercises.Count} exercises | Approx. {todayDay.TotalMinutes} minutes | Focus: {todayDay.FocusType}";
            _btnStartWorkout.Text = "Start Today's Workout";
        }

        int planned = plan.WorkoutDaysCount;
        int completed = Math.Min(planned, p.TotalWorkoutsCompleted % Math.Max(1, planned));
        _pbWeeklyProgress.Maximum = planned;
        _pbWeeklyProgress.Value = completed;
        _lblWeeklyStats.Text = $"{completed} of {planned} workout days finished this week";
    }
}

