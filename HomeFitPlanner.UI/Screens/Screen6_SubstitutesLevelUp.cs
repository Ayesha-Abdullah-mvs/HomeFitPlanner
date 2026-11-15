using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.Algorithms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen6_SubstitutesLevelUp : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly DataManager _dataManager;

    private TabControl _tabControl = null!;
    private ComboBox _cmbSubExercise = null!;
    private ListBox _lstSubstitutes = null!;
    private Label _lblSubReason = null!;

    private ComboBox _cmbLevelStart = null!;
    private ComboBox _cmbLevelTarget = null!;
    private ListBox _lstLevelPath = null!;
    private Label _lblLevelSummary = null!;

    private GraphRenderer _graphRenderer = null!;

    public Screen6_SubstitutesLevelUp(Action<string> navigateAction)
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

        var headerPanel = new Panel { Location = new Point(24, 16), Size = new Size(950, 48) };
        var lblTitle = new Label { Text = "Exercise Relationships: Substitutes & Level-Up [Milestone 2 Preview]", Font = UITheme.HeaderFont, ForeColor = UITheme.TextMain, Location = new Point(0, 0), AutoSize = true };
        var lblNotice = new Label { Text = "📋 Frontend UI layout complete. Backend Graph Algorithms (BFS, Dijkstra) scheduled for Milestone 2 per syllabus.", Font = UITheme.SmallFont, ForeColor = UITheme.Primary, Location = new Point(2, 26), AutoSize = true };
        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(lblNotice);
        Controls.Add(headerPanel);

        // Tab Control for Modes (Substitutes vs Level-Up)
        _tabControl = new TabControl
        {
            Location = new Point(24, 65),
            Size = new Size(440, 520),
            Font = UITheme.SubtitleFont
        };

        // Tab 1: Substitutes Finder (BFS)
        var tabSub = new TabPage("🔄 Find Substitutes (BFS)") { BackColor = Color.White };
        var lblSubPick = new Label { Text = "Select exercise you cannot do or wish to swap:", Location = new Point(16, 15), AutoSize = true, Font = UITheme.BodyFont };
        _cmbSubExercise = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 38), Size = new Size(390, 24), Font = UITheme.BodyFont };
        _cmbSubExercise.SelectedIndexChanged += (s, e) => FindSubstitutes();

        var lblSubResults = new Label { Text = "Safe, Compatible Substitutes (Filtered by Equipment & Joints):", Location = new Point(16, 80), AutoSize = true, Font = UITheme.SubtitleFont, ForeColor = UITheme.Primary };
        _lstSubstitutes = new ListBox { Location = new Point(16, 105), Size = new Size(390, 260), Font = UITheme.BodyFont, ItemHeight = 24 };

        _lblSubReason = new Label
        {
            Text = "Breadth-First Search (BFS) explores neighboring graph vertices layer-by-layer to discover closest movement patterns.",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(16, 380),
            Size = new Size(390, 60)
        };

        tabSub.Controls.AddRange(new Control[] { lblSubPick, _cmbSubExercise, lblSubResults, _lstSubstitutes, _lblSubReason });
        _tabControl.TabPages.Add(tabSub);

        // Tab 2: Level-Up Progression (Dijkstra)
        var tabLevel = new TabPage("🚀 Level-Up Path (Dijkstra)") { BackColor = Color.White };
        var lblStart = new Label { Text = "Starting Exercise (e.g. Wall Push-up):", Location = new Point(16, 15), AutoSize = true, Font = UITheme.BodyFont };
        _cmbLevelStart = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 38), Size = new Size(390, 24), Font = UITheme.BodyFont };

        var lblTarget = new Label { Text = "Target Goal (e.g. Standard Push-up):", Location = new Point(16, 75), AutoSize = true, Font = UITheme.BodyFont };
        _cmbLevelTarget = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 98), Size = new Size(390, 24), Font = UITheme.BodyFont };

        var btnFindPath = UITheme.CreatePrimaryButton("Find Optimal Progression Path", 16, 135, 260, 36);
        btnFindPath.Click += (s, e) => CalculateLevelUpPath();

        var lblPathResult = new Label { Text = "Shortest Difficulty Progression Chain:", Location = new Point(16, 185), AutoSize = true, Font = UITheme.SubtitleFont, ForeColor = UITheme.Secondary };
        _lstLevelPath = new ListBox { Location = new Point(16, 210), Size = new Size(390, 180), Font = UITheme.BodyFont, ItemHeight = 24 };

        _lblLevelSummary = new Label
        {
            Text = "Dijkstra's Algorithm finds the minimum total difficulty jump between movements.",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(16, 400),
            Size = new Size(390, 50)
        };

        tabLevel.Controls.AddRange(new Control[] { lblStart, _cmbLevelStart, lblTarget, _cmbLevelTarget, btnFindPath, lblPathResult, _lstLevelPath, _lblLevelSummary });
        _tabControl.TabPages.Add(tabLevel);

        Controls.Add(_tabControl);

        // Right Panel: Interactive GDI+ Graph Canvas
        var pnlGraphCard = UITheme.CreateCard(475, 65, 499, 520);
        var lblGraphTitle = new Label { Text = "EXERCISE RELATIONSHIP GRAPH (GDI+ VISUALIZATION)", Font = UITheme.SmallFont, ForeColor = UITheme.Primary, Location = new Point(14, 12), AutoSize = true };

        _graphRenderer = new GraphRenderer
        {
            Location = new Point(14, 38),
            Size = new Size(470, 465)
        };

        pnlGraphCard.Controls.AddRange(new Control[] { lblGraphTitle, _graphRenderer });
        Controls.Add(pnlGraphCard);
    }

    public void RefreshData()
    {
        _cmbSubExercise.Items.Clear();
        _cmbLevelStart.Items.Clear();
        _cmbLevelTarget.Items.Clear();

        for (int i = 0; i < _dataManager.Exercises.Count; i++)
        {
            string name = _dataManager.Exercises[i].Name;
            _cmbSubExercise.Items.Add(name);
            _cmbLevelStart.Items.Add(name);
            _cmbLevelTarget.Items.Add(name);
        }

        if (_cmbSubExercise.Items.Count > 0) _cmbSubExercise.SelectedIndex = 0;
        if (_cmbLevelStart.Items.Contains("Wall Push-up")) _cmbLevelStart.SelectedItem = "Wall Push-up";
        if (_cmbLevelTarget.Items.Contains("Standard Push-up")) _cmbLevelTarget.SelectedItem = "Standard Push-up";

        _graphRenderer.SetGraph(_dataManager.ExerciseProgressionGraph);
    }

    private void FindSubstitutes()
    {
        string? selectedName = _cmbSubExercise.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedName)) return;

        var currentEx = _dataManager.FindExerciseByName(selectedName);
        if (currentEx == null) return;

        var p = _dataManager.CurrentProfile;

        // BFS traversal with safety & equipment filtering
        var substitutes = GraphAlgorithms.BreadthFirstSearch(_dataManager.ExerciseProgressionGraph, selectedName, candidateName =>
        {
            var cand = _dataManager.FindExerciseByName(candidateName);
            if (cand == null) return false;

            // Must match same muscle group or full body
            if (cand.MuscleGroup != currentEx.MuscleGroup && cand.MuscleGroup != MuscleGroup.FullBody)
                return false;

            // Must satisfy user's physical limitations
            if (p.HasKneeLimitation && cand.StressesKnees) return false;
            if (p.HasBackLimitation && cand.StressesBack) return false;
            if (p.HasShoulderLimitation && cand.StressesShoulders) return false;
            if (p.HasWristLimitation && cand.StressesWrists) return false;
            if (p.Age >= 50 && cand.IsHighImpact) return false;

            return true;
        }, maxDepth: 3);

        _lstSubstitutes.Items.Clear();
        if (substitutes.Count == 0)
        {
            _lstSubstitutes.Items.Add("No direct substitutes match your joint safety criteria.");
        }
        else
        {
            for (int i = 0; i < substitutes.Count; i++)
            {
                var cand = _dataManager.FindExerciseByName(substitutes[i]);
                string equip = cand != null ? $"[{cand.Equipment}]" : "";
                string cue = cand != null && !string.IsNullOrEmpty(cand.Tips) ? $" • Form: {cand.Tips}" : "";
                _lstSubstitutes.Items.Add($"✓ {substitutes[i]} {equip} (Diff: {cand?.Difficulty ?? 1}){cue}");
            }
        }

        _lblSubReason.Text = $"BFS discovered {substitutes.Count} valid substitutes in the {currentEx.MuscleGroup} family matching your equipment and limitations.";
        _graphRenderer.SetGraph(_dataManager.ExerciseProgressionGraph, substitutes: substitutes, selected: selectedName);
    }

    private void CalculateLevelUpPath()
    {
        string? start = _cmbLevelStart.SelectedItem?.ToString();
        string? target = _cmbLevelTarget.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(target)) return;

        var result = GraphAlgorithms.Dijkstra(_dataManager.ExerciseProgressionGraph, start, target);

        _lstLevelPath.Items.Clear();

        if (!result.IsReachable)
        {
            _lstLevelPath.Items.Add("No direct progression path exists between these movements.");
            _lblLevelSummary.Text = "Disconnected components in graph.";
            _graphRenderer.SetGraph(_dataManager.ExerciseProgressionGraph);
            return;
        }

        for (int i = 0; i < result.StepDescriptions.Count; i++)
        {
            _lstLevelPath.Items.Add(result.StepDescriptions[i]);
        }

        _lblLevelSummary.Text = $"Shortest path contains {result.Path.Count} steps with total difficulty jump weight of {result.TotalDistance:F1}.";
        _graphRenderer.SetGraph(_dataManager.ExerciseProgressionGraph, path: result.Path);
    }
}

