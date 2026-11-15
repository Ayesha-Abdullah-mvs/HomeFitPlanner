using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.DataStructures.LinkedLists;
using HomeFitPlanner.Core.DataStructures.Stacks;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen4_TodayWorkout : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly DataManager _dataManager;

    // Hand-written Doubly Linked List representing the chain of session exercises
    private DoublyLinkedList<WorkoutExercise> _exerciseChain = new();
    private DoublyNode<WorkoutExercise>? _currentNode;

    // Hand-written stack for removing and restoring (undoing) removed exercises
    private CustomStack<WorkoutExercise> _removedStack = new();

    // UI elements
    private Label _lblPhaseBadge = null!;
    private Label _lblExerciseName = null!;
    private Label _lblWorkload = null!;
    private Label _lblTips = null!;
    private ProgressBar _pbSessionProgress = null!;
    private Label _lblProgressCounter = null!;

    private Label _lblTimerDisplay = null!;
    private Button _btnTimerStartPause = null!;
    private System.Windows.Forms.Timer _exerciseTimer = null!;
    private int _secondsRemaining = 0;
    private bool _isTimerRunning = false;
    private bool _isRestTimer = false;

    private Button _btnPrevious = null!;
    private Button _btnNext = null!;
    private Button _btnSkip = null!;
    private Button _btnRemove = null!;
    private Button _btnUndoRemove = null!;

    private int _completedExercisesCount = 0;
    private int _totalSessionMinutes = 0;

    public Screen4_TodayWorkout(Action<string> navigateAction)
    {
        _navigateAction = navigateAction;
        _dataManager = DataManager.Instance;
        InitializeComponent();
        SetupTimer();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UITheme.Background;
        AutoScroll = true;

        var headerPanel = new Panel { Location = new Point(24, 16), Size = new Size(950, 50) };
        var lblTitle = new Label { Text = "Today's Guided Workout Session", Font = UITheme.HeaderFont, ForeColor = UITheme.TextMain, Location = new Point(0, 8), AutoSize = true };
        headerPanel.Controls.Add(lblTitle);
        Controls.Add(headerPanel);

        // Progress bar card
        var pnlProgress = UITheme.CreateCard(24, 70, 950, 45);
        _pbSessionProgress = new ProgressBar { Location = new Point(16, 12), Size = new Size(720, 20), Maximum = 100, Value = 0 };
        _lblProgressCounter = new Label { Text = "Exercise 1 of 5", Location = new Point(750, 12), AutoSize = true, Font = UITheme.SubtitleFont, ForeColor = UITheme.TextMain };
        pnlProgress.Controls.AddRange(new Control[] { _pbSessionProgress, _lblProgressCounter });
        Controls.Add(pnlProgress);

        // Main Exercise Stage Card
        var pnlStage = UITheme.CreateCard(24, 125, 950, 360);

        _lblPhaseBadge = new Label
        {
            Text = "MAIN EXERCISE",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = UITheme.Primary,
            Location = new Point(24, 20),
            Padding = new Padding(8, 4, 8, 4),
            AutoSize = true
        };

        _lblExerciseName = new Label
        {
            Text = "Wall Push-up",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = UITheme.TextMain,
            Location = new Point(24, 52),
            Size = new Size(580, 45)
        };

        _lblWorkload = new Label
        {
            Text = "3 sets x 10 reps (Rest: 45s between sets)",
            Font = UITheme.TitleFont,
            ForeColor = UITheme.Secondary,
            Location = new Point(24, 102),
            Size = new Size(580, 30)
        };

        _lblTips = new Label
        {
            Text = "Instructions: Stand facing a wall about 2 feet away. Place palms flat on wall at shoulder height. Lower chest with controlled tempo, then push back up. Keep core braced.",
            Font = UITheme.BodyFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(24, 140),
            Size = new Size(580, 130)
        };

        // Live Timer Box
        var pnlTimer = new Panel
        {
            Location = new Point(640, 30),
            Size = new Size(280, 190),
            BackColor = Color.FromArgb(241, 245, 249),
            BorderStyle = BorderStyle.FixedSingle
        };
        var lblTimerHeader = new Label { Text = "EXERCISE & REST TIMER", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 15), AutoSize = true };
        _lblTimerDisplay = new Label
        {
            Text = "00:45",
            Font = new Font("Segoe UI", 36, FontStyle.Bold),
            ForeColor = UITheme.Primary,
            Location = new Point(20, 45),
            AutoSize = true
        };
        _btnTimerStartPause = UITheme.CreatePrimaryButton("Start Timer", 20, 125, 120, 40);
        _btnTimerStartPause.Click += BtnTimerStartPause_Click;

        var btnResetTimer = UITheme.CreateSecondaryButton("Reset", 150, 125, 90, 40);
        btnResetTimer.Click += (s, e) => ResetTimer();

        pnlTimer.Controls.AddRange(new Control[] { lblTimerHeader, _lblTimerDisplay, _btnTimerStartPause, btnResetTimer });

        // Navigation Controls Bar (Using hand-written Doubly Linked List & Stacks)
        int btnY = 280;
        _btnPrevious = UITheme.CreateSecondaryButton("⏮ Previous", 24, btnY, 130, 44);
        _btnPrevious.Click += (s, e) => MovePrevious();

        _btnNext = UITheme.CreatePrimaryButton("Next ⏭", 165, btnY, 140, 44);
        _btnNext.Click += (s, e) => MoveNext();

        _btnSkip = UITheme.CreateSecondaryButton("⏭ Skip", 315, btnY, 110, 44);
        _btnSkip.Click += (s, e) => SkipCurrent();

        _btnRemove = UITheme.CreateSecondaryButton("⚠️ Hurts / Remove", 435, btnY, 160, 44);
        _btnRemove.ForeColor = UITheme.Danger;
        _btnRemove.Click += (s, e) => RemoveHurtingExercise();

        _btnUndoRemove = UITheme.CreateSecondaryButton("↩ Undo Remove", 605, btnY, 150, 44);
        _btnUndoRemove.Click += (s, e) => UndoRemoveExercise();
        _btnUndoRemove.Enabled = false;

        pnlStage.Controls.AddRange(new Control[] {
            _lblPhaseBadge, _lblExerciseName, _lblWorkload, _lblTips,
            pnlTimer, _btnPrevious, _btnNext, _btnSkip, _btnRemove, _btnUndoRemove
        });
        Controls.Add(pnlStage);
    }

    private void SetupTimer()
    {
        _exerciseTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _exerciseTimer.Tick += ExerciseTimer_Tick;
    }

    public void LoadTodayWorkout()
    {
        _exerciseChain.Clear();
        _removedStack.Clear();
        _completedExercisesCount = 0;

        var plan = _dataManager.CurrentPlan;
        if (plan == null)
        {
            var gen = new PlanGeneratorService();
            plan = gen.GeneratePlan(_dataManager.CurrentProfile);
            _dataManager.CurrentPlan = plan;
        }

        int todayDayOfWeek = ((int)DateTime.Now.DayOfWeek + 6) % 7;
        var day = plan.Days[todayDayOfWeek];

        if (day.IsRestDay || day.Exercises.Count == 0)
        {
            // Pick first non-rest day for demonstration
            for (int i = 0; i < plan.Days.Count; i++)
            {
                if (!plan.Days[i].IsRestDay)
                {
                    day = plan.Days[i];
                    break;
                }
            }
        }

        _totalSessionMinutes = day.TotalMinutes;

        // Build Doubly Linked List from day exercises
        for (int i = 0; i < day.Exercises.Count; i++)
        {
            _exerciseChain.AddLast(day.Exercises[i].Clone());
        }

        _currentNode = _exerciseChain.Head;
        DisplayCurrentExercise();
    }

    private void DisplayCurrentExercise()
    {
        if (_currentNode == null)
        {
            FinishWorkoutSession();
            return;
        }

        var ex = _currentNode.Value;
        _lblExerciseName.Text = ex.ExerciseName;
        _lblPhaseBadge.Text = ex.Phase.ToUpperInvariant();
        _lblPhaseBadge.BackColor = ex.Phase == "Warm-up" ? UITheme.Warning : (ex.Phase == "Cool-down" ? UITheme.Secondary : UITheme.Primary);

        _lblWorkload.Text = $"{ex.GetWorkloadString()} | Rest: {ex.RestSeconds}s";

        var baseEx = _dataManager.FindExerciseById(ex.ExerciseId) ?? _dataManager.FindExerciseByName(ex.ExerciseName);
        string instructionsText = "";
        if (baseEx != null)
        {
            instructionsText = "Instructions:\n";
            if (baseEx.Instructions != null && baseEx.Instructions.Length > 0)
            {
                for (int i = 0; i < baseEx.Instructions.Length; i++)
                {
                    instructionsText += $"{i + 1}. {baseEx.Instructions[i]}\n";
                }
            }
            if (!string.IsNullOrEmpty(baseEx.Tips))
            {
                instructionsText += $"\n💡 Form Cue: {baseEx.Tips}";
            }
        }
        else
        {
            instructionsText = string.IsNullOrEmpty(ex.HowToSummary) ? "Perform with steady breathing and full range of motion." : ex.HowToSummary;
        }
        _lblTips.Text = instructionsText;

        _secondsRemaining = ex.IsTimed ? ex.Seconds : ex.RestSeconds;
        _isRestTimer = !ex.IsTimed;
        UpdateTimerDisplay();

        // Update progress
        int idx = GetCurrentIndex();
        int total = _exerciseChain.Count;
        _pbSessionProgress.Maximum = Math.Max(1, total);
        _pbSessionProgress.Value = Math.Min(total, idx + 1);
        _lblProgressCounter.Text = $"Exercise {idx + 1} of {total}";

        _btnPrevious.Enabled = (_currentNode.Prev != null);
        _btnUndoRemove.Enabled = (!_removedStack.IsEmpty);
    }

    private int GetCurrentIndex()
    {
        int count = 0;
        var temp = _exerciseChain.Head;
        while (temp != null && temp != _currentNode)
        {
            count++;
            temp = temp.Next;
        }
        return count;
    }

    private void MoveNext()
    {
        if (_currentNode != null)
        {
            _currentNode.Value.IsCompleted = true;
            _completedExercisesCount++;
        }

        if (_currentNode?.Next != null)
        {
            _currentNode = _currentNode.Next;
            DisplayCurrentExercise();
        }
        else
        {
            FinishWorkoutSession();
        }
    }

    private void MovePrevious()
    {
        if (_currentNode?.Prev != null)
        {
            _currentNode = _currentNode.Prev;
            DisplayCurrentExercise();
        }
    }

    private void SkipCurrent()
    {
        if (_currentNode != null)
        {
            _currentNode.Value.WasSkipped = true;
        }

        if (_currentNode?.Next != null)
        {
            _currentNode = _currentNode.Next;
            DisplayCurrentExercise();
        }
        else
        {
            FinishWorkoutSession();
        }
    }

    private void RemoveHurtingExercise()
    {
        if (_currentNode == null) return;

        var hurting = _currentNode.Value;
        hurting.WasRemoved = true;
        _removedStack.Push(hurting);

        var nextNode = _currentNode.Next ?? _currentNode.Prev;
        _exerciseChain.RemoveNode(_currentNode);
        _currentNode = nextNode;

        _btnUndoRemove.Enabled = true;

        if (_currentNode != null)
        {
            DisplayCurrentExercise();
        }
        else
        {
            FinishWorkoutSession();
        }

        MessageBox.Show($"'{hurting.ExerciseName}' removed from session. Remember: Never push through sharp joint pain!", "Safety First", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void UndoRemoveExercise()
    {
        if (_removedStack.IsEmpty) return;

        var restored = _removedStack.Pop();
        if (_currentNode != null)
        {
            var newNode = _exerciseChain.InsertAfter(_currentNode, restored);
            _currentNode = newNode;
        }
        else
        {
            _currentNode = _exerciseChain.AddLast(restored);
        }

        _btnUndoRemove.Enabled = !_removedStack.IsEmpty;
        DisplayCurrentExercise();
        MessageBox.Show($"Restored '{restored.ExerciseName}' back into the session chain.", "Exercise Restored", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnTimerStartPause_Click(object? sender, EventArgs e)
    {
        if (_isTimerRunning)
        {
            _exerciseTimer.Stop();
            _isTimerRunning = false;
            _btnTimerStartPause.Text = "Resume";
        }
        else
        {
            _exerciseTimer.Start();
            _isTimerRunning = true;
            _btnTimerStartPause.Text = "Pause";
        }
    }

    private void ResetTimer()
    {
        _exerciseTimer.Stop();
        _isTimerRunning = false;
        _btnTimerStartPause.Text = "Start Timer";
        if (_currentNode != null)
        {
            _secondsRemaining = _currentNode.Value.IsTimed ? _currentNode.Value.Seconds : _currentNode.Value.RestSeconds;
        }
        UpdateTimerDisplay();
    }

    private void ExerciseTimer_Tick(object? sender, EventArgs e)
    {
        if (_secondsRemaining > 0)
        {
            _secondsRemaining--;
            UpdateTimerDisplay();
        }
        else
        {
            _exerciseTimer.Stop();
            _isTimerRunning = false;
            _btnTimerStartPause.Text = "Start Timer";
            System.Media.SystemSounds.Beep.Play();

            if (_isRestTimer)
            {
                MessageBox.Show("Rest time is up! Ready for next set.", "Time's Up", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        int mins = _secondsRemaining / 60;
        int secs = _secondsRemaining % 60;
        _lblTimerDisplay.Text = $"{mins:D2}:{secs:D2}";
    }

    private void FinishWorkoutSession()
    {
        _exerciseTimer.Stop();

        // Build workload expression: e.g. (10 * 3) + (12 * 3) + (10 * 3)
        string expression = "";
        var temp = _exerciseChain.Head;
        while (temp != null)
        {
            if (temp.Value.IsCompleted)
            {
                string part = temp.Value.GetWorkloadExpressionPart();
                expression = string.IsNullOrEmpty(expression) ? part : $"{expression} + {part}";
            }
            temp = temp.Next;
        }
        if (string.IsNullOrEmpty(expression)) expression = "(10 * 3)";

        double calculatedVolume = ExpressionEvaluator.Evaluate(expression);

        // Feedback Rating Dialog
        var ratingForm = new Form
        {
            Text = "Workout Complete! Rate Your Session",
            Size = new Size(500, 320),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblCongrats = new Label
        {
            Text = "🎉 Session Completed!",
            Font = UITheme.HeaderFont,
            ForeColor = UITheme.Primary,
            Location = new Point(24, 20),
            AutoSize = true
        };

        var lblStats = new Label
        {
            Text = $"You trained for ~{_totalSessionMinutes} minutes and completed {_completedExercisesCount} exercises.\n" +
                   $"Calculated Total Volume: {expression} = {calculatedVolume:F0} reps/work units.",
            Font = UITheme.BodyFont,
            ForeColor = UITheme.TextMain,
            Location = new Point(24, 60),
            Size = new Size(440, 48)
        };

        var lblPrompt = new Label
        {
            Text = "How did today's session feel?",
            Font = UITheme.SubtitleFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(24, 125),
            AutoSize = true
        };

        var btnEasy = UITheme.CreateSecondaryButton("Too Easy 🟢", 24, 160, 130, 45);
        var btnRight = UITheme.CreatePrimaryButton("Just Right 🎯", 168, 160, 130, 45);
        var btnHard = UITheme.CreateSecondaryButton("Too Hard 🔴", 312, 160, 130, 45);

        btnEasy.Click += (s, e) => SaveSessionAndClose(ratingForm, SessionRating.TooEasy, expression, calculatedVolume);
        btnRight.Click += (s, e) => SaveSessionAndClose(ratingForm, SessionRating.JustRight, expression, calculatedVolume);
        btnHard.Click += (s, e) => SaveSessionAndClose(ratingForm, SessionRating.TooHard, expression, calculatedVolume);

        ratingForm.Controls.AddRange(new Control[] { lblCongrats, lblStats, lblPrompt, btnEasy, btnRight, btnHard });
        ratingForm.ShowDialog(this);
    }

    private void SaveSessionAndClose(Form form, SessionRating rating, string expression, double volume)
    {
        var log = new WorkoutSessionLog
        {
            DayName = DateTime.Now.DayOfWeek.ToString(),
            PlanName = _dataManager.CurrentPlan?.PlanName ?? "Custom Plan",
            MinutesTrained = _totalSessionMinutes,
            ExercisesCompleted = _completedExercisesCount,
            TotalExercises = _exerciseChain.Count,
            Rating = rating,
            WorkloadExpression = expression,
            CalculatedWorkload = volume
        };

        _dataManager.SessionLogs.Add(log);

        // Update profile streaks
        var p = _dataManager.CurrentProfile;
        p.TotalWorkoutsCompleted++;
        p.TotalMinutesExercised += _totalSessionMinutes;
        p.CurrentStreak++;
        if (p.CurrentStreak > p.BestStreak) p.BestStreak = p.CurrentStreak;

        form.Close();
        _navigateAction("Progress");
    }
}

