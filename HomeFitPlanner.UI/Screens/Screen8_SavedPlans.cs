using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen8_SavedPlans : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly DataManager _dataManager;

    private ListView _lvSavedPlans = null!;
    private NumericUpDown _numExactDuration = null!;
    private NumericUpDown _numMinDuration = null!;
    private NumericUpDown _numMaxDuration = null!;

    private TreeRenderer _treeRenderer = null!;
    private RadioButton _rbPlainBst = null!;
    private RadioButton _rbAvlTree = null!;

    public Screen8_SavedPlans(Action<string> navigateAction)
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
        var lblTitle = new Label { Text = "Saved Plans: BST & AVL Tree Storage [Milestone 2 Preview]", Font = UITheme.HeaderFont, ForeColor = UITheme.TextMain, Location = new Point(0, 0), AutoSize = true };
        var lblNotice = new Label { Text = "📋 Frontend UI layout complete. Backend Search Trees (BST & Self-Balancing AVL) scheduled for Milestone 2 per syllabus.", Font = UITheme.SmallFont, ForeColor = UITheme.Primary, Location = new Point(2, 26), AutoSize = true };
        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(lblNotice);
        Controls.Add(headerPanel);

        // Search & Filter Card (Duration Search via BST)
        var pnlSearch = UITheme.CreateCard(24, 68, 950, 65);

        var lblExact = new Label { Text = "Exact Duration (min):", Location = new Point(14, 10), AutoSize = true, Font = UITheme.SmallFont };
        _numExactDuration = new NumericUpDown { Minimum = 10, Maximum = 90, Value = 30, Location = new Point(14, 28), Size = new Size(70, 24) };

        var btnSearchExact = UITheme.CreatePrimaryButton("BST Search", 95, 24, 110, 30);
        btnSearchExact.Click += (s, e) => SearchExactDuration();

        var lblRange = new Label { Text = "Range Query [Min - Max Mins]:", Location = new Point(230, 10), AutoSize = true, Font = UITheme.SmallFont };
        _numMinDuration = new NumericUpDown { Minimum = 10, Maximum = 90, Value = 20, Location = new Point(230, 28), Size = new Size(60, 24) };
        _numMaxDuration = new NumericUpDown { Minimum = 10, Maximum = 90, Value = 45, Location = new Point(300, 28), Size = new Size(60, 24) };

        var btnSearchRange = UITheme.CreateSecondaryButton("BST Range Search", 375, 24, 140, 30);
        btnSearchRange.Click += (s, e) => SearchDurationRange();

        var btnReset = UITheme.CreateSecondaryButton("Show All", 525, 24, 90, 30);
        btnReset.Click += (s, e) => RefreshData();

        // Radio buttons for Tree Visualization Mode
        _rbPlainBst = new RadioButton { Text = "Plain BST", Location = new Point(660, 26), AutoSize = true, Checked = true, Font = UITheme.SubtitleFont };
        _rbAvlTree = new RadioButton { Text = "AVL Balanced Tree", Location = new Point(760, 26), AutoSize = true, Font = UITheme.SubtitleFont };
        _rbPlainBst.CheckedChanged += (s, e) => UpdateTreeVisualization();
        _rbAvlTree.CheckedChanged += (s, e) => UpdateTreeVisualization();

        pnlSearch.Controls.AddRange(new Control[] {
            lblExact, _numExactDuration, btnSearchExact,
            lblRange, _numMinDuration, _numMaxDuration, btnSearchRange, btnReset,
            _rbPlainBst, _rbAvlTree
        });
        Controls.Add(pnlSearch);

        // Saved Plans List View
        _lvSavedPlans = new ListView
        {
            Location = new Point(24, 142),
            Size = new Size(420, 380),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = UITheme.BodyFont
        };
        _lvSavedPlans.Columns.Add("Plan Name", 170);
        _lvSavedPlans.Columns.Add("Goal", 95);
        _lvSavedPlans.Columns.Add("Days", 55);
        _lvSavedPlans.Columns.Add("Mins", 65);
        Controls.Add(_lvSavedPlans);

        // Actions Below List
        var btnLoad = UITheme.CreatePrimaryButton("📂 Load as Current Plan", 24, 532, 200, 42);
        btnLoad.Click += (s, e) => LoadSelectedPlan();

        var btnDelete = UITheme.CreateSecondaryButton("🗑 Delete Plan", 235, 532, 120, 42);
        btnDelete.Click += (s, e) => DeleteSelectedPlan();

        Controls.Add(btnLoad);
        Controls.Add(btnDelete);

        // Right Card: GDI+ Tree Structure Visualization
        var pnlTree = UITheme.CreateCard(456, 142, 518, 432);
        _treeRenderer = new TreeRenderer { Location = new Point(0, 0), Dock = DockStyle.Fill };
        pnlTree.Controls.Add(_treeRenderer);
        Controls.Add(pnlTree);
    }

    public void RefreshData()
    {
        EnsureSamplePlansExist();
        PopulateListView(_dataManager.SavedPlansList);
        UpdateTreeVisualization();
    }

    private void EnsureSamplePlansExist()
    {
        if (_dataManager.SavedPlansList.Count < 3)
        {
            var gen = new PlanGeneratorService(_dataManager);
            var p = _dataManager.CurrentProfile;

            // Seed several plans of different durations for BST/AVL demonstration
            int[] durations = { 15, 20, 30, 45, 60 };
            foreach (var d in durations)
            {
                var cloneProfile = new UserProfile
                {
                    Name = p.Name,
                    Goal = p.Goal,
                    StartingLevel = p.StartingLevel,
                    WorkoutDaysPerWeek = 3,
                    MinutesPerSession = d,
                    HasChairOrBench = true,
                    HasKneeLimitation = true
                };
                var plan = gen.GeneratePlan(cloneProfile, $"{d}m Quick Routine");
                _dataManager.SavePlan(plan);
            }
        }
    }

    private void PopulateListView(CustomArray<WorkoutPlan> plans)
    {
        _lvSavedPlans.Items.Clear();
        for (int i = 0; i < plans.Count; i++)
        {
            var p = plans[i];
            var lvi = new ListViewItem(p.PlanName) { Tag = p };
            lvi.SubItems.Add(p.Goal.ToString());
            lvi.SubItems.Add($"{p.WorkoutDaysCount}d");
            lvi.SubItems.Add($"{p.TargetSessionMinutes}m");
            _lvSavedPlans.Items.Add(lvi);
        }

        if (_lvSavedPlans.Items.Count > 0)
        {
            _lvSavedPlans.Items[0].Selected = true;
        }
    }

    private void SearchExactDuration()
    {
        int exact = (int)_numExactDuration.Value;
        var results = _dataManager.SearchPlansByDuration(exact);
        PopulateListView(results);
    }

    private void SearchDurationRange()
    {
        int min = (int)_numMinDuration.Value;
        int max = (int)_numMaxDuration.Value;
        if (min > max)
        {
            int t = min; min = max; max = t;
        }
        var results = _dataManager.SearchPlansByDurationRange(min, max);
        PopulateListView(results);
    }

    private void UpdateTreeVisualization()
    {
        if (_rbAvlTree.Checked)
        {
            _treeRenderer.SetAvl(_dataManager.SavedPlansAvl);
        }
        else
        {
            _treeRenderer.SetBst(_dataManager.SavedPlansBst);
        }
    }

    private void LoadSelectedPlan()
    {
        if (_lvSavedPlans.SelectedItems.Count == 0) return;

        var plan = (WorkoutPlan)_lvSavedPlans.SelectedItems[0].Tag!;
        _dataManager.CurrentPlan = plan.Clone();

        MessageBox.Show($"Loaded plan '{plan.PlanName}' as your active weekly schedule!", "Plan Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        _navigateAction("Weekly Plan");
    }

    private void DeleteSelectedPlan()
    {
        if (_lvSavedPlans.SelectedItems.Count == 0) return;

        var plan = (WorkoutPlan)_lvSavedPlans.SelectedItems[0].Tag!;
        _dataManager.DeleteSavedPlan(plan.Id);
        RefreshData();
    }
}

