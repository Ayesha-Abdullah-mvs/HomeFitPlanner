using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.Algorithms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.DataStructures.Stacks;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen3_WeeklyPlan : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly DataManager _dataManager;
    private readonly PlanGeneratorService _generator;
    private readonly UndoRedoManager<WorkoutPlan> _undoRedoManager = new();

    private FlowLayoutPanel _pnlDaysGrid = null!;
    private Panel _pnlDayDetail = null!;
    private Label _lblSelectedDayTitle = null!;
    private ListBox _lstDayExercises = null!;
    private Label _lblInstructionDetail = null!;
    private Button _btnUndo = null!;
    private Button _btnRedo = null!;
    private Label _lblJaggedArrayInfo = null!;

    private int _selectedDayIndex = 0;

    public Screen3_WeeklyPlan(Action<string> navigateAction)
    {
        _navigateAction = navigateAction;
        _dataManager = DataManager.Instance;
        _generator = new PlanGeneratorService(_dataManager);
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UITheme.Background;
        AutoScroll = true;

        // Top Command Bar
        var topBar = new Panel { Location = new Point(24, 16), Size = new Size(950, 50) };
        var lblTitle = new Label { Text = "7-Day Weekly Workout Plan", Font = UITheme.HeaderFont, ForeColor = UITheme.TextMain, Location = new Point(0, 8), AutoSize = true };

        var btnGen = UITheme.CreatePrimaryButton("⚡ Regenerate", 420, 8, 120, 36);
        btnGen.Click += (s, e) => RegeneratePlan();

        _btnUndo = UITheme.CreateSecondaryButton("⤺ Undo", 550, 8, 85, 36);
        _btnUndo.Click += (s, e) => UndoAction();

        _btnRedo = UITheme.CreateSecondaryButton("⤻ Redo", 645, 8, 85, 36);
        _btnRedo.Click += (s, e) => RedoAction();

        var btnSave = UITheme.CreateSecondaryButton("💾 Save Plan", 740, 8, 100, 36);
        btnSave.Click += (s, e) => SaveCurrentPlan();

        var btnAdapt = UITheme.CreatePrimaryButton("🔄 Adapt Plan", 850, 8, 100, 36);
        btnAdapt.Click += (s, e) => AdaptPlan();

        topBar.Controls.AddRange(new Control[] { lblTitle, btnGen, _btnUndo, _btnRedo, btnSave, btnAdapt });
        Controls.Add(topBar);

        // Legend panel
        var pnlLegend = new Panel { Location = new Point(24, 68), Size = new Size(950, 26) };
        AddLegendBadge(pnlLegend, 0, "Strength Focus", UITheme.StrengthDay);
        AddLegendBadge(pnlLegend, 130, "Cardio Focus", UITheme.CardioDay);
        AddLegendBadge(pnlLegend, 250, "Mixed Focus", UITheme.MixedDay);
        AddLegendBadge(pnlLegend, 360, "Rest / Recovery", UITheme.RestDay);
        Controls.Add(pnlLegend);

        // 7-Day Grid Cards
        _pnlDaysGrid = new FlowLayoutPanel
        {
            Location = new Point(24, 98),
            Size = new Size(950, 160),
            WrapContents = false,
            AutoScroll = true
        };
        Controls.Add(_pnlDaysGrid);

        // Day Detail and Modification Panel
        _pnlDayDetail = UITheme.CreateCard(24, 275, 950, 310);
        _lblSelectedDayTitle = new Label { Text = "Monday: Low-Impact Cardio & Core", Font = UITheme.TitleFont, ForeColor = UITheme.TextMain, Location = new Point(20, 15), AutoSize = true };

        _lstDayExercises = new ListBox
        {
            Location = new Point(20, 48),
            Size = new Size(460, 200),
            Font = UITheme.BodyFont,
            ItemHeight = 26
        };
        _lstDayExercises.SelectedIndexChanged += LstDayExercises_SelectedIndexChanged;

        // Dedicated Instructions Display Box for Every Exercise (As requested)
        var pnlInstructionBox = new Panel
        {
            Location = new Point(490, 48),
            Size = new Size(290, 200),
            BackColor = Color.FromArgb(248, 250, 252)
        };
        pnlInstructionBox.Paint += (s, e) =>
        {
            using var p = new Pen(Color.FromArgb(226, 232, 240), 1);
            e.Graphics.DrawRectangle(p, 0, 0, pnlInstructionBox.Width - 1, pnlInstructionBox.Height - 1);
        };
        var lblInstTitle = new Label
        {
            Text = "📋 Form Instructions & Cues",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = UITheme.Primary,
            Location = new Point(10, 8),
            Size = new Size(270, 20)
        };
        _lblInstructionDetail = new Label
        {
            Text = "Select an exercise from the list to view how-to steps and posture guidance.",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(10, 32),
            Size = new Size(270, 160)
        };
        pnlInstructionBox.Controls.Add(lblInstTitle);
        pnlInstructionBox.Controls.Add(_lblInstructionDetail);

        var btnSwap = UITheme.CreatePrimaryButton("🔄 Swap", 795, 48, 135, 36);
        btnSwap.Click += (s, e) => SwapSelectedExercise();

        var btnRemove = UITheme.CreateSecondaryButton("❌ Remove", 795, 90, 135, 36);
        btnRemove.Click += (s, e) => RemoveSelectedExercise();

        var btnAdd = UITheme.CreateSecondaryButton("➕ Add", 795, 132, 135, 36);
        btnAdd.Click += (s, e) => AddExerciseToDay();

        var btnStartDay = UITheme.CreatePrimaryButton("▶ Guide Session", 795, 185, 135, 45);
        btnStartDay.BackColor = UITheme.Success;
        btnStartDay.Click += (s, e) =>
        {
            _navigateAction("Today's Workout");
        };

        _lblJaggedArrayInfo = new Label
        {
            Text = "JaggedArray representation: 7 days allocated as variable-length rows [0..6].",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(20, 265),
            Size = new Size(800, 24)
        };

        _pnlDayDetail.Controls.AddRange(new Control[] {
            _lblSelectedDayTitle, _lstDayExercises, pnlInstructionBox,
            btnSwap, btnRemove, btnAdd, btnStartDay, _lblJaggedArrayInfo
        });
        Controls.Add(_pnlDayDetail);
    }

    private void AddLegendBadge(Panel pnl, int x, string text, Color color)
    {
        var box = new Panel { Location = new Point(x, 6), Size = new Size(14, 14), BackColor = color };
        var lbl = new Label { Text = text, Location = new Point(x + 18, 5), AutoSize = true, Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted };
        pnl.Controls.Add(box);
        pnl.Controls.Add(lbl);
    }

    public void RefreshData()
    {
        if (_dataManager.CurrentPlan == null)
        {
            RegeneratePlan();
            return;
        }

        RenderDaysGrid();
        RenderSelectedDay();
        UpdateUndoRedoButtons();
    }

    private void RegeneratePlan()
    {
        var plan = _dataManager.CurrentPlan;
        if (plan != null)
        {
            _undoRedoManager.RecordState(plan.Clone());
        }

        var newPlan = _generator.GeneratePlan(_dataManager.CurrentProfile);
        _dataManager.CurrentPlan = newPlan;
        RefreshData();
    }

    private void UndoAction()
    {
        if (_undoRedoManager.CanUndo && _dataManager.CurrentPlan != null)
        {
            var prev = _undoRedoManager.Undo(_dataManager.CurrentPlan);
            if (prev != null)
            {
                _dataManager.CurrentPlan = prev;
                RefreshData();
            }
        }
    }

    private void RedoAction()
    {
        if (_undoRedoManager.CanRedo && _dataManager.CurrentPlan != null)
        {
            var next = _undoRedoManager.Redo(_dataManager.CurrentPlan);
            if (next != null)
            {
                _dataManager.CurrentPlan = next;
                RefreshData();
            }
        }
    }

    private void UpdateUndoRedoButtons()
    {
        _btnUndo.Enabled = _undoRedoManager.CanUndo;
        _btnRedo.Enabled = _undoRedoManager.CanRedo;
    }

    private void RenderDaysGrid()
    {
        var plan = _dataManager.CurrentPlan;
        if (plan == null) return;

        _pnlDaysGrid.Controls.Clear();

        for (int i = 0; i < plan.Days.Count; i++)
        {
            int dayIdx = i;
            var day = plan.Days[i];

            var card = new Panel
            {
                Size = new Size(125, 140),
                BackColor = (dayIdx == _selectedDayIndex) ? Color.FromArgb(238, 242, 255) : Color.White,
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand
            };

            var colorBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 6,
                BackColor = day.IsRestDay ? UITheme.RestDay :
                            day.FocusType == DayFocusType.Strength ? UITheme.StrengthDay :
                            day.FocusType == DayFocusType.Cardio ? UITheme.CardioDay : UITheme.MixedDay
            };

            var lblName = new Label
            {
                Text = day.DayName,
                Font = UITheme.SubtitleFont,
                ForeColor = UITheme.TextMain,
                Location = new Point(8, 16),
                AutoSize = true
            };

            var lblFocus = new Label
            {
                Text = day.IsRestDay ? "Rest" : day.FocusDescription,
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(8, 40),
                Size = new Size(110, 40)
            };

            var lblDuration = new Label
            {
                Text = day.IsRestDay ? "Light Walk" : $"{day.TotalMinutes} min ({day.Exercises.Count} moves)",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = day.IsRestDay ? UITheme.RestDay : UITheme.Primary,
                Location = new Point(8, 90),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { colorBar, lblName, lblFocus, lblDuration });
            card.Click += (s, e) =>
            {
                _selectedDayIndex = dayIdx;
                RenderDaysGrid();
                RenderSelectedDay();
            };

            foreach (Control c in card.Controls)
            {
                c.Click += (s, e) =>
                {
                    _selectedDayIndex = dayIdx;
                    RenderDaysGrid();
                    RenderSelectedDay();
                };
            }

            _pnlDaysGrid.Controls.Add(card);
        }
    }

    private void RenderSelectedDay()
    {
        var plan = _dataManager.CurrentPlan;
        if (plan == null || _selectedDayIndex >= plan.Days.Count) return;

        var day = plan.Days[_selectedDayIndex];
        _lblSelectedDayTitle.Text = $"{day.DayName} Focus: {day.FocusDescription} ({day.TotalMinutes} min)";
        _lstDayExercises.Items.Clear();

        if (day.IsRestDay || day.Exercises.Count == 0)
        {
            _lstDayExercises.Items.Add("Rest Day - Optional gentle walk or light recovery stretching.");
            _lblInstructionDetail.Text = "Rest & Recovery: Allows muscular adaptation and prevents joint fatigue.";
            return;
        }

        for (int i = 0; i < day.Exercises.Count; i++)
        {
            var we = day.Exercises[i];
            var baseEx = _dataManager.FindExerciseById(we.ExerciseId) ?? _dataManager.FindExerciseByName(we.ExerciseName);
            string prefix = we.Phase == "Warm-up" ? "[Warm-up]" : (we.Phase == "Cool-down" ? "[Cool-down]" : "[Main]");
            string cue = !string.IsNullOrEmpty(baseEx?.Tips) ? $" • {baseEx.Tips}" : (!string.IsNullOrEmpty(we.HowToSummary) ? $" • {we.HowToSummary}" : "");
            _lstDayExercises.Items.Add($"{prefix} {we.ExerciseName} - {we.GetWorkloadString()}{cue}");
        }

        var jagged = plan.BuildJaggedExerciseNames();
        _lblJaggedArrayInfo.Text = $"Jagged Array row [{_selectedDayIndex}] has {jagged.GetColumnCount(_selectedDayIndex)} exercises scheduled.";

        if (_lstDayExercises.Items.Count > 0)
        {
            _lstDayExercises.SelectedIndex = 0;
        }
    }

    private void LstDayExercises_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var plan = _dataManager.CurrentPlan;
        if (plan == null || _selectedDayIndex >= plan.Days.Count) return;
        var day = plan.Days[_selectedDayIndex];

        int sel = _lstDayExercises.SelectedIndex;
        if (sel < 0 || sel >= day.Exercises.Count)
        {
            _lblInstructionDetail.Text = "Select an exercise from the list to view how-to steps and posture guidance.";
            return;
        }

        var we = day.Exercises[sel];
        var baseEx = _dataManager.FindExerciseById(we.ExerciseId) ?? _dataManager.FindExerciseByName(we.ExerciseName);
        if (baseEx != null)
        {
            string text = $"{baseEx.Name} ({we.GetWorkloadString()}):\n\n";
            if (baseEx.Instructions != null && baseEx.Instructions.Length > 0)
            {
                for (int i = 0; i < baseEx.Instructions.Length; i++)
                {
                    text += $"{i + 1}. {baseEx.Instructions[i]}\n";
                }
            }
            if (!string.IsNullOrEmpty(baseEx.Tips))
            {
                text += $"\n💡 Tip: {baseEx.Tips}";
            }
            _lblInstructionDetail.Text = text;
        }
        else
        {
            _lblInstructionDetail.Text = $"{we.ExerciseName}:\n{we.HowToSummary}";
        }
    }

    private void SwapSelectedExercise()
    {
        var plan = _dataManager.CurrentPlan;
        if (plan == null || _selectedDayIndex >= plan.Days.Count) return;
        var day = plan.Days[_selectedDayIndex];

        int sel = _lstDayExercises.SelectedIndex;
        if (sel < 0 || sel >= day.Exercises.Count)
        {
            MessageBox.Show("Please select an exercise in the list to swap.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var currentEx = day.Exercises[sel];
        var substitutes = GraphAlgorithms.BreadthFirstSearch(_dataManager.ExerciseProgressionGraph, currentEx.ExerciseName);

        if (substitutes.Count == 0)
        {
            MessageBox.Show($"No direct variation substitutes found for {currentEx.ExerciseName}.", "No Substitutes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var prompt = new Form
        {
            Text = $"Swap {currentEx.ExerciseName} (BFS Variation Graph)",
            Size = new Size(420, 260),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblPrompt = new Label { Text = "Select substitute exercise from graph:", Location = new Point(20, 15), AutoSize = true, Font = UITheme.BodyFont };
        var cmbSubs = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(20, 45), Size = new Size(360, 24) };
        for (int i = 0; i < substitutes.Count; i++) cmbSubs.Items.Add(substitutes[i]);
        cmbSubs.SelectedIndex = 0;

        var btnOk = UITheme.CreatePrimaryButton("Apply Swap", 240, 150, 140, 36);
        btnOk.Click += (s, e) =>
        {
            string chosen = cmbSubs.SelectedItem?.ToString() ?? "";
            var candidate = _dataManager.FindExerciseByName(chosen);
            if (candidate != null)
            {
                _undoRedoManager.RecordState(plan.Clone());
                currentEx.ExerciseId = candidate.Id;
                currentEx.ExerciseName = candidate.Name;
                currentEx.Type = candidate.Type;
                currentEx.MuscleGroup = candidate.MuscleGroup;
                currentEx.HowToSummary = candidate.Tips;
                day.RecalculateTotalMinutes();
                prompt.Close();
                RefreshData();
            }
        };

        prompt.Controls.AddRange(new Control[] { lblPrompt, cmbSubs, btnOk });
        prompt.ShowDialog(this);
    }

    private void RemoveSelectedExercise()
    {
        var plan = _dataManager.CurrentPlan;
        if (plan == null || _selectedDayIndex >= plan.Days.Count) return;
        var day = plan.Days[_selectedDayIndex];

        int sel = _lstDayExercises.SelectedIndex;
        if (sel < 0 || sel >= day.Exercises.Count) return;

        _undoRedoManager.RecordState(plan.Clone());
        day.Exercises.RemoveAt(sel);
        day.RecalculateTotalMinutes();
        RefreshData();
    }

    private void AddExerciseToDay()
    {
        var plan = _dataManager.CurrentPlan;
        if (plan == null || _selectedDayIndex >= plan.Days.Count) return;
        var day = plan.Days[_selectedDayIndex];

        var prompt = new Form
        {
            Text = "Add Exercise to Day",
            Size = new Size(420, 250),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblPrompt = new Label { Text = "Select exercise to insert into session:", Location = new Point(20, 15), AutoSize = true, Font = UITheme.BodyFont };
        var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(20, 45), Size = new Size(360, 24) };
        for (int i = 0; i < _dataManager.Exercises.Count; i++) cmb.Items.Add(_dataManager.Exercises[i].Name);
        cmb.SelectedIndex = 0;

        var btnAdd = UITheme.CreatePrimaryButton("Add to Plan", 240, 140, 140, 36);
        btnAdd.Click += (s, e) =>
        {
            string chosen = cmb.SelectedItem?.ToString() ?? "";
            var ex = _dataManager.FindExerciseByName(chosen);
            if (ex != null)
            {
                _undoRedoManager.RecordState(plan.Clone());
                day.Exercises.Insert(Math.Max(0, day.Exercises.Count - 1), new WorkoutExercise
                {
                    ExerciseId = ex.Id,
                    ExerciseName = ex.Name,
                    Type = ex.Type,
                    MuscleGroup = ex.MuscleGroup,
                    Sets = ex.DefaultSets,
                    Reps = ex.DefaultReps,
                    Seconds = ex.DefaultSeconds,
                    IsTimed = ex.IsTimed,
                    Phase = "Main",
                    EstimatedMinutes = ex.TimeMinutes,
                    HowToSummary = ex.Tips
                });
                day.RecalculateTotalMinutes();
                prompt.Close();
                RefreshData();
            }
        };

        prompt.Controls.AddRange(new Control[] { lblPrompt, cmb, btnAdd });
        prompt.ShowDialog(this);
    }

    private void SaveCurrentPlan()
    {
        if (_dataManager.CurrentPlan == null) return;
        _dataManager.SavePlan(_dataManager.CurrentPlan.Clone());
        MessageBox.Show($"Current plan '{_dataManager.CurrentPlan.PlanName}' successfully saved into Binary Search Tree and AVL Tree!", "Plan Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AdaptPlan()
    {
        if (_dataManager.CurrentPlan == null) return;
        var adapter = new PlanAdaptationService(_dataManager);
        var result = adapter.AdaptForNextWeek(_dataManager.CurrentPlan, _dataManager.SessionLogs, _dataManager.CurrentProfile);

        _undoRedoManager.RecordState(_dataManager.CurrentPlan.Clone());
        _dataManager.CurrentPlan = result.AdjustedPlan;
        RefreshData();

        string msg = $"{result.SummaryMessage}\n\nAdjustments made:\n";
        for (int i = 0; i < result.AdjustmentsMade.Count; i++)
        {
            msg += $"• {result.AdjustmentsMade[i]}\n";
        }
        MessageBox.Show(msg, "Adaptive Plan Progression", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
