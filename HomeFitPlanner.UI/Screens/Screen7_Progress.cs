using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen7_Progress : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly DataManager _dataManager;

    private Label _lblTotalWorkouts = null!;
    private Label _lblTotalMinutes = null!;
    private Label _lblCurrentStreak = null!;
    private Label _lblBestStreak = null!;

    private ChartRenderer _chartSessions = null!;
    private ChartRenderer _chartWeight = null!;

    private ListView _lvSessionHistory = null!;

    public Screen7_Progress(Action<string> navigateAction)
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

        var headerPanel = new Panel { Location = new Point(24, 16), Size = new Size(950, 45) };
        var lblTitle = new Label { Text = "Progress Tracking & Health Analytics", Font = UITheme.HeaderFont, ForeColor = UITheme.TextMain, Location = new Point(0, 4), AutoSize = true };

        var btnLogWeight = UITheme.CreatePrimaryButton("⚖️ Log Weekly Weight", 760, 4, 180, 36);
        btnLogWeight.Click += (s, e) => LogWeightDialog();

        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(btnLogWeight);
        Controls.Add(headerPanel);

        // 4 Summary Metric Cards
        int cardW = 226;
        int cardH = 90;

        var pnlM1 = UITheme.CreateCard(24, 68, cardW, cardH);
        var lblT1 = new Label { Text = "TOTAL WORKOUTS", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, 12), AutoSize = true };
        _lblTotalWorkouts = new Label { Text = "12 Completed", Font = UITheme.TitleFont, ForeColor = UITheme.Primary, Location = new Point(14, 38), AutoSize = true };
        pnlM1.Controls.AddRange(new Control[] { lblT1, _lblTotalWorkouts });

        var pnlM2 = UITheme.CreateCard(262, 68, cardW, cardH);
        var lblT2 = new Label { Text = "MINUTES TRAINED", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, 12), AutoSize = true };
        _lblTotalMinutes = new Label { Text = "360 Mins", Font = UITheme.TitleFont, ForeColor = UITheme.Secondary, Location = new Point(14, 38), AutoSize = true };
        pnlM2.Controls.AddRange(new Control[] { lblT2, _lblTotalMinutes });

        var pnlM3 = UITheme.CreateCard(500, 68, cardW, cardH);
        var lblT3 = new Label { Text = "CURRENT STREAK", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, 12), AutoSize = true };
        _lblCurrentStreak = new Label { Text = "🔥 3 In A Row", Font = UITheme.TitleFont, ForeColor = UITheme.Warning, Location = new Point(14, 38), AutoSize = true };
        pnlM3.Controls.AddRange(new Control[] { lblT3, _lblCurrentStreak });

        var pnlM4 = UITheme.CreateCard(738, 68, cardW, cardH);
        var lblT4 = new Label { Text = "ALL-TIME BEST", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, 12), AutoSize = true };
        _lblBestStreak = new Label { Text = "⭐ 5 Sessions", Font = UITheme.TitleFont, ForeColor = UITheme.Success, Location = new Point(14, 38), AutoSize = true };
        pnlM4.Controls.AddRange(new Control[] { lblT4, _lblBestStreak });

        Controls.AddRange(new Control[] { pnlM1, pnlM2, pnlM3, pnlM4 });

        // Charts Row
        var pnlChart1 = UITheme.CreateCard(24, 170, 464, 210);
        _chartSessions = new ChartRenderer { Location = new Point(0, 0), Dock = DockStyle.Fill };
        pnlChart1.Controls.Add(_chartSessions);
        Controls.Add(pnlChart1);

        var pnlChart2 = UITheme.CreateCard(500, 170, 474, 210);
        _chartWeight = new ChartRenderer { Location = new Point(0, 0), Dock = DockStyle.Fill };
        pnlChart2.Controls.Add(_chartWeight);
        Controls.Add(pnlChart2);

        // Session History Table Card
        var pnlHistory = UITheme.CreateCard(24, 395, 950, 190);
        var lblHistTitle = new Label { Text = "COMPLETED SESSION HISTORY & RATINGS", Font = UITheme.SmallFont, ForeColor = UITheme.Primary, Location = new Point(14, 10), AutoSize = true };

        _lvSessionHistory = new ListView
        {
            Location = new Point(14, 32),
            Size = new Size(920, 145),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = UITheme.BodyFont
        };
        _lvSessionHistory.Columns.Add("Date", 110);
        _lvSessionHistory.Columns.Add("Day", 90);
        _lvSessionHistory.Columns.Add("Plan Name", 180);
        _lvSessionHistory.Columns.Add("Minutes", 80);
        _lvSessionHistory.Columns.Add("Moves", 70);
        _lvSessionHistory.Columns.Add("Rating", 100);
        _lvSessionHistory.Columns.Add("Workload Formula (Evaluated Volume)", 250);

        pnlHistory.Controls.AddRange(new Control[] { lblHistTitle, _lvSessionHistory });
        Controls.Add(pnlHistory);
    }

    public void RefreshData()
    {
        var p = _dataManager.CurrentProfile;
        _lblTotalWorkouts.Text = $"{p.TotalWorkoutsCompleted} Completed";
        _lblTotalMinutes.Text = $"{p.TotalMinutesExercised} Mins";
        _lblCurrentStreak.Text = $"🔥 {p.CurrentStreak} In A Row";
        _lblBestStreak.Text = $"⭐ {p.BestStreak} Sessions";

        // Sessions per week bar chart
        var sessionPoints = new CustomArray<ChartDataPoint>();
        sessionPoints.Add(new ChartDataPoint("Week 1", 3));
        sessionPoints.Add(new ChartDataPoint("Week 2", 3));
        sessionPoints.Add(new ChartDataPoint("Week 3", 4));
        sessionPoints.Add(new ChartDataPoint("Week 4", 3));
        sessionPoints.Add(new ChartDataPoint("This Wk", Math.Min(3, p.CurrentStreak)));
        _chartSessions.SetData(sessionPoints, "Workouts Completed Per Week", "Sessions", ChartType.Bar);

        // Body weight line chart
        var weightPoints = new CustomArray<ChartDataPoint>();
        for (int i = 0; i < _dataManager.WeightLogs.Count; i++)
        {
            var w = _dataManager.WeightLogs[i];
            weightPoints.Add(new ChartDataPoint(w.Date.ToString("MM/dd"), w.WeightKg));
        }
        _chartWeight.PrimaryColor = UITheme.Secondary;
        _chartWeight.SetData(weightPoints, "Body Weight Trend Over Time (kg)", "kg", ChartType.Line);

        // Session History
        _lvSessionHistory.Items.Clear();
        for (int i = _dataManager.SessionLogs.Count - 1; i >= 0; i--)
        {
            var s = _dataManager.SessionLogs[i];
            var lvi = new ListViewItem(s.Date.ToString("yyyy-MM-dd"));
            lvi.SubItems.Add(s.DayName);
            lvi.SubItems.Add(s.PlanName);
            lvi.SubItems.Add($"{s.MinutesTrained}m");
            lvi.SubItems.Add($"{s.ExercisesCompleted}/{s.TotalExercises}");
            lvi.SubItems.Add(s.Rating.ToString());
            lvi.SubItems.Add($"{s.WorkloadExpression} = {s.CalculatedWorkload:F0}");
            _lvSessionHistory.Items.Add(lvi);
        }
    }

    private void LogWeightDialog()
    {
        var form = new Form
        {
            Text = "Log Weekly Body Weight",
            Size = new Size(360, 240),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblW = new Label { Text = "Weight (kg):", Location = new Point(24, 20), AutoSize = true, Font = UITheme.BodyFont };
        var numW = new NumericUpDown { Minimum = 30, Maximum = 250, DecimalPlaces = 1, Value = (decimal)_dataManager.CurrentProfile.WeightKg, Location = new Point(24, 45), Size = new Size(120, 24) };

        var lblN = new Label { Text = "Notes (optional):", Location = new Point(24, 80), AutoSize = true, Font = UITheme.BodyFont };
        var txtN = new TextBox { Location = new Point(24, 105), Size = new Size(290, 24) };

        var btnSave = UITheme.CreatePrimaryButton("Save Entry", 160, 150, 150, 36);
        btnSave.Click += (s, e) =>
        {
            double newWeight = (double)numW.Value;
            _dataManager.CurrentProfile.WeightKg = newWeight;
            double newBmi = _dataManager.CurrentProfile.CalculateBmi();

            _dataManager.WeightLogs.Add(new WeightLogEntry
            {
                Date = DateTime.Now,
                WeightKg = newWeight,
                Bmi = newBmi,
                Note = txtN.Text.Trim()
            });

            form.Close();
            RefreshData();
            MessageBox.Show($"Logged {newWeight:F1} kg! Updated BMI: {newBmi:F1}.", "Weight Recorded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        form.Controls.AddRange(new Control[] { lblW, numW, lblN, txtN, btnSave });
        form.ShowDialog(this);
    }
}

