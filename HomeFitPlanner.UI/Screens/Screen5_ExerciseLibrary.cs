using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.Algorithms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen5_ExerciseLibrary : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly DataManager _dataManager;

    private TextBox _txtSearch = null!;
    private ComboBox _cmbFilterMuscle = null!;
    private ComboBox _cmbFilterEquip = null!;
    private ComboBox _cmbFilterType = null!;
    private ComboBox _cmbSort = null!;

    private ListView _lvExercises = null!;
    private Panel _pnlDetails = null!;

    private Label _lblDetailName = null!;
    private Label _lblDetailMeta = null!;
    private Label _lblDetailFlags = null!;
    private Label _lblDetailInstructions = null!;
    private Label _lblDetailVariations = null!;

    private Exercise? _selectedExercise;

    public Screen5_ExerciseLibrary(Action<string> navigateAction)
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
        var lblTitle = new Label { Text = "Exercise Library (Search, Filter & Sort)", Font = UITheme.HeaderFont, ForeColor = UITheme.TextMain, Location = new Point(0, 4), AutoSize = true };

        var btnAddCustom = UITheme.CreatePrimaryButton("➕ Add Custom Exercise", 740, 4, 190, 36);
        btnAddCustom.Click += (s, e) => AddNewExerciseDialog();

        headerPanel.Controls.Add(lblTitle);
        headerPanel.Controls.Add(btnAddCustom);
        Controls.Add(headerPanel);

        // Filter / Search Toolbar
        var pnlFilters = UITheme.CreateCard(24, 68, 950, 65);

        var lblSearch = new Label { Text = "Search Name:", Location = new Point(14, 10), AutoSize = true, Font = UITheme.SmallFont };
        _txtSearch = new TextBox { Location = new Point(14, 28), Size = new Size(180, 24) };
        _txtSearch.TextChanged += (s, e) => ApplyFiltersAndSort();

        var lblMuscle = new Label { Text = "Muscle Group:", Location = new Point(208, 10), AutoSize = true, Font = UITheme.SmallFont };
        _cmbFilterMuscle = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(208, 28), Size = new Size(130, 24) };
        _cmbFilterMuscle.Items.AddRange(new object[] { "All", "Chest", "Back", "Legs", "Glutes", "Shoulders", "Arms", "Core", "FullBody" });
        _cmbFilterMuscle.SelectedIndex = 0;
        _cmbFilterMuscle.SelectedIndexChanged += (s, e) => ApplyFiltersAndSort();

        var lblEquip = new Label { Text = "Equipment:", Location = new Point(352, 10), AutoSize = true, Font = UITheme.SmallFont };
        _cmbFilterEquip = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(352, 28), Size = new Size(130, 24) };
        _cmbFilterEquip.Items.AddRange(new object[] { "All", "None", "ResistanceBands", "Dumbbells", "Mat", "ChairOrBench" });
        _cmbFilterEquip.SelectedIndex = 0;
        _cmbFilterEquip.SelectedIndexChanged += (s, e) => ApplyFiltersAndSort();

        var lblType = new Label { Text = "Type:", Location = new Point(496, 10), AutoSize = true, Font = UITheme.SmallFont };
        _cmbFilterType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(496, 28), Size = new Size(110, 24) };
        _cmbFilterType.Items.AddRange(new object[] { "All", "WarmUp", "Strength", "Cardio", "Stretch" });
        _cmbFilterType.SelectedIndex = 0;
        _cmbFilterType.SelectedIndexChanged += (s, e) => ApplyFiltersAndSort();

        var lblSort = new Label { Text = "Sort By:", Location = new Point(620, 10), AutoSize = true, Font = UITheme.SmallFont };
        _cmbSort = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(620, 28), Size = new Size(190, 24) };
        _cmbSort.Items.AddRange(new object[] {
            "Name (A to Z) [QuickSort]",
            "Difficulty (1 to 5) [MergeSort]",
            "Difficulty (5 to 1) [BubbleSort]",
            "Duration (Lowest first) [InsertionSort]",
            "Duration (Highest first) [SelectionSort]"
        });
        _cmbSort.SelectedIndex = 0;
        _cmbSort.SelectedIndexChanged += (s, e) => ApplyFiltersAndSort();

        var btnClear = UITheme.CreateSecondaryButton("Reset", 830, 24, 90, 30);
        btnClear.Click += (s, e) => ResetFilters();

        pnlFilters.Controls.AddRange(new Control[] {
            lblSearch, _txtSearch, lblMuscle, _cmbFilterMuscle,
            lblEquip, _cmbFilterEquip, lblType, _cmbFilterType,
            lblSort, _cmbSort, btnClear
        });
        Controls.Add(pnlFilters);

        // List View of exercises
        _lvExercises = new ListView
        {
            Location = new Point(24, 142),
            Size = new Size(580, 440),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = UITheme.BodyFont
        };
        _lvExercises.Columns.Add("Name", 190);
        _lvExercises.Columns.Add("Muscle", 90);
        _lvExercises.Columns.Add("Diff", 45);
        _lvExercises.Columns.Add("Equipment", 110);
        _lvExercises.Columns.Add("Type", 75);
        _lvExercises.Columns.Add("Time", 50);

        _lvExercises.SelectedIndexChanged += LvExercises_SelectedIndexChanged;
        Controls.Add(_lvExercises);

        // Exercise Detail View Panel
        _pnlDetails = UITheme.CreateCard(616, 142, 358, 440);
        var lblDetTitle = new Label { Text = "EXERCISE DETAIL & SAFETY", Font = UITheme.SmallFont, ForeColor = UITheme.Primary, Location = new Point(14, 12), AutoSize = true };

        _lblDetailName = new Label { Text = "Select an exercise", Font = UITheme.TitleFont, ForeColor = UITheme.TextMain, Location = new Point(14, 32), Size = new Size(330, 28) };
        _lblDetailMeta = new Label { Text = "Muscle Group: - | Equipment: -", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, 62), Size = new Size(330, 20) };
        _lblDetailFlags = new Label { Text = "Safety Flags: None", Font = UITheme.SmallFont, ForeColor = UITheme.Danger, Location = new Point(14, 84), Size = new Size(330, 20) };

        var lblInstHeading = new Label { Text = "Form & Instructions:", Font = UITheme.SubtitleFont, ForeColor = UITheme.TextMain, Location = new Point(14, 110), AutoSize = true };
        _lblDetailInstructions = new Label
        {
            Text = "Select any exercise from the list to view how-to steps, joint safety flags, and easier/harder variations.",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(14, 132),
            Size = new Size(330, 160)
        };

        var lblVarHeading = new Label { Text = "Progression Path:", Font = UITheme.SubtitleFont, ForeColor = UITheme.TextMain, Location = new Point(14, 300), AutoSize = true };
        _lblDetailVariations = new Label
        {
            Text = "Easier: - | Harder: -",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.Secondary,
            Location = new Point(14, 324),
            Size = new Size(330, 36)
        };

        var btnDelete = UITheme.CreateSecondaryButton("🗑 Delete Exercise", 14, 385, 150, 36);
        btnDelete.Click += (s, e) => DeleteSelectedExercise();

        var btnViewGraph = UITheme.CreatePrimaryButton("View In Graph 🌐", 180, 385, 160, 36);
        btnViewGraph.Click += (s, e) =>
        {
            _navigateAction("Substitutes and Level-Up");
        };

        _pnlDetails.Controls.AddRange(new Control[] {
            lblDetTitle, _lblDetailName, _lblDetailMeta, _lblDetailFlags,
            lblInstHeading, _lblDetailInstructions, lblVarHeading, _lblDetailVariations,
            btnDelete, btnViewGraph
        });
        Controls.Add(_pnlDetails);
    }

    public void RefreshData()
    {
        ApplyFiltersAndSort();
    }

    private void ResetFilters()
    {
        _txtSearch.Text = "";
        _cmbFilterMuscle.SelectedIndex = 0;
        _cmbFilterEquip.SelectedIndex = 0;
        _cmbFilterType.SelectedIndex = 0;
        _cmbSort.SelectedIndex = 0;
        ApplyFiltersAndSort();
    }

    private void ApplyFiltersAndSort()
    {
        string search = StringSanitizer.ManualTrim(_txtSearch.Text);
        string muscle = _cmbFilterMuscle.SelectedItem?.ToString() ?? "All";
        string equip = _cmbFilterEquip.SelectedItem?.ToString() ?? "All";
        string type = _cmbFilterType.SelectedItem?.ToString() ?? "All";

        var filtered = new CustomArray<Exercise>();

        for (int i = 0; i < _dataManager.Exercises.Count; i++)
        {
            var ex = _dataManager.Exercises[i];

            if (!string.IsNullOrEmpty(search) && !StringSanitizer.ManualContains(ex.Name, search, ignoreCase: true))
                continue;

            if (muscle != "All" && ex.MuscleGroup.ToString() != muscle)
                continue;

            if (equip != "All" && ex.Equipment.ToString() != equip)
                continue;

            if (type != "All" && ex.Type.ToString() != type)
                continue;

            filtered.Add(ex);
        }

        // Apply hand-written sorting algorithm
        var arr = filtered.ToArray();
        int sortMode = _cmbSort.SelectedIndex;

        switch (sortMode)
        {
            case 0: // Name (A to Z) -> Quick Sort
                SortingAlgorithms.QuickSort(arr, Comparer<Exercise>.Create((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)));
                break;
            case 1: // Difficulty (1 to 5) -> Merge Sort
                SortingAlgorithms.MergeSort(arr, Comparer<Exercise>.Create((a, b) => a.Difficulty.CompareTo(b.Difficulty)));
                break;
            case 2: // Difficulty (5 to 1) -> Bubble Sort
                SortingAlgorithms.BubbleSort(arr, Comparer<Exercise>.Create((a, b) => b.Difficulty.CompareTo(a.Difficulty)));
                break;
            case 3: // Duration (Lowest first) -> Insertion Sort
                SortingAlgorithms.InsertionSort(arr, Comparer<Exercise>.Create((a, b) => a.TimeMinutes.CompareTo(b.TimeMinutes)));
                break;
            case 4: // Duration (Highest first) -> Selection Sort
                SortingAlgorithms.SelectionSort(arr, Comparer<Exercise>.Create((a, b) => b.TimeMinutes.CompareTo(a.TimeMinutes)));
                break;
        }

        _lvExercises.Items.Clear();
        for (int i = 0; i < arr.Length; i++)
        {
            var ex = arr[i];
            var lvi = new ListViewItem(ex.Name) { Tag = ex };
            lvi.SubItems.Add(ex.MuscleGroup.ToString());
            lvi.SubItems.Add(new string('★', ex.Difficulty));
            lvi.SubItems.Add(ex.Equipment.ToString());
            lvi.SubItems.Add(ex.Type.ToString());
            lvi.SubItems.Add($"{ex.TimeMinutes}m");
            _lvExercises.Items.Add(lvi);
        }

        if (_lvExercises.Items.Count > 0)
        {
            _lvExercises.Items[0].Selected = true;
        }
    }

    private void LvExercises_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_lvExercises.SelectedItems.Count == 0) return;

        var ex = (Exercise)_lvExercises.SelectedItems[0].Tag!;
        _selectedExercise = ex;

        _lblDetailName.Text = ex.Name;
        _lblDetailMeta.Text = $"Target: {ex.MuscleGroup} | Type: {ex.Type} | Equip: {ex.Equipment}";

        string flags = "";
        if (ex.StressesKnees) flags += "[Knees] ";
        if (ex.StressesBack) flags += "[Back] ";
        if (ex.StressesShoulders) flags += "[Shoulders] ";
        if (ex.StressesWrists) flags += "[Wrists] ";
        if (ex.IsHighImpact) flags += "[High Impact] ";
        _lblDetailFlags.Text = string.IsNullOrEmpty(flags) ? "Safety: Low joint impact / Gentle" : $"Safety Flags: {flags}";

        string steps = "";
        if (ex.Instructions != null)
        {
            for (int i = 0; i < ex.Instructions.Length; i++)
            {
                steps += $"{i + 1}. {ex.Instructions[i]}\n";
            }
        }
        if (!string.IsNullOrEmpty(ex.Tips))
        {
            steps += $"\nTip: {ex.Tips}";
        }
        _lblDetailInstructions.Text = steps;

        var easier = !string.IsNullOrEmpty(ex.EasierVariationId) ? _dataManager.FindExerciseById(ex.EasierVariationId)?.Name : "None (Entry level)";
        var harder = !string.IsNullOrEmpty(ex.HarderVariationId) ? _dataManager.FindExerciseById(ex.HarderVariationId)?.Name : "None (Peak move)";
        _lblDetailVariations.Text = $"Easier: {easier}\nHarder: {harder}";
    }

    private void AddNewExerciseDialog()
    {
        var form = new Form
        {
            Text = "Add Custom Exercise",
            Size = new Size(450, 420),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblN = new Label { Text = "Exercise Name:", Location = new Point(20, 15), AutoSize = true, Font = UITheme.BodyFont };
        var txtN = new TextBox { Location = new Point(20, 35), Size = new Size(390, 24) };

        var lblM = new Label { Text = "Muscle Group:", Location = new Point(20, 70), AutoSize = true, Font = UITheme.BodyFont };
        var cmbM = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(20, 90), Size = new Size(180, 24) };
        cmbM.Items.AddRange(Enum.GetNames(typeof(MuscleGroup)));
        cmbM.SelectedIndex = 0;

        var lblD = new Label { Text = "Difficulty (1-5):", Location = new Point(230, 70), AutoSize = true, Font = UITheme.BodyFont };
        var numD = new NumericUpDown { Minimum = 1, Maximum = 5, Value = 2, Location = new Point(230, 90), Size = new Size(80, 24) };

        var lblE = new Label { Text = "Equipment:", Location = new Point(20, 125), AutoSize = true, Font = UITheme.BodyFont };
        var cmbE = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(20, 145), Size = new Size(180, 24) };
        cmbE.Items.AddRange(Enum.GetNames(typeof(EquipmentType)));
        cmbE.SelectedIndex = 0;

        var lblT = new Label { Text = "Type:", Location = new Point(230, 125), AutoSize = true, Font = UITheme.BodyFont };
        var cmbT = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(230, 145), Size = new Size(180, 24) };
        cmbT.Items.AddRange(Enum.GetNames(typeof(ExerciseType)));
        cmbT.SelectedIndex = 1;

        var chkKnee = new CheckBox { Text = "Stresses Knees", Location = new Point(20, 185), AutoSize = true };
        var chkBack = new CheckBox { Text = "Stresses Back", Location = new Point(150, 185), AutoSize = true };
        var chkImpact = new CheckBox { Text = "High Impact", Location = new Point(270, 185), AutoSize = true };

        var lblInst = new Label { Text = "Instructions / Form Tips:", Location = new Point(20, 220), AutoSize = true, Font = UITheme.BodyFont };
        var txtInst = new TextBox { Location = new Point(20, 240), Size = new Size(390, 60), Multiline = true };

        var btnSave = UITheme.CreatePrimaryButton("Save to Library", 230, 320, 180, 40);
        btnSave.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtN.Text))
            {
                MessageBox.Show("Please enter an exercise name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customEx = new Exercise
            {
                Name = txtN.Text.Trim(),
                MuscleGroup = (MuscleGroup)cmbM.SelectedIndex,
                Difficulty = (int)numD.Value,
                Equipment = (EquipmentType)cmbE.SelectedIndex,
                Type = (ExerciseType)cmbT.SelectedIndex,
                StressesKnees = chkKnee.Checked,
                StressesBack = chkBack.Checked,
                IsHighImpact = chkImpact.Checked,
                Tips = txtInst.Text.Trim(),
                Instructions = new[] { txtInst.Text.Trim() }
            };

            _dataManager.AddCustomExercise(customEx);
            form.Close();
            ApplyFiltersAndSort();
            MessageBox.Show($"Custom exercise '{customEx.Name}' saved to library!", "Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        form.Controls.AddRange(new Control[] {
            lblN, txtN, lblM, cmbM, lblD, numD, lblE, cmbE, lblT, cmbT,
            chkKnee, chkBack, chkImpact, lblInst, txtInst, btnSave
        });

        form.ShowDialog(this);
    }

    private void DeleteSelectedExercise()
    {
        if (_selectedExercise == null) return;

        var res = MessageBox.Show($"Are you sure you want to delete '{_selectedExercise.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (res == DialogResult.Yes)
        {
            _dataManager.DeleteExercise(_selectedExercise.Id);
            _selectedExercise = null;
            ApplyFiltersAndSort();
        }
    }
}

