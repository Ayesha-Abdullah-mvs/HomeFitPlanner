using System;
using System.Drawing;
using System.Windows.Forms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;

namespace HomeFitPlanner.UI.Screens;

public class Screen9_PerformanceLab : UserControl
{
    private readonly Action<string> _navigateAction;
    private readonly BenchmarkService _benchmarkService = new();

    private ComboBox _cmbComparisonType = null!;
    private ComboBox _cmbInputSize = null!;
    private Button _btnRunBenchmark = null!;

    private ListView _lvResults = null!;
    private ChartRenderer _chartBenchmarks = null!;
    private Label _lblAnalysisSummary = null!;

    public Screen9_PerformanceLab(Action<string> navigateAction)
    {
        _navigateAction = navigateAction;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UITheme.Background;
        AutoScroll = true;

        var headerPanel = new Panel { Location = new Point(24, 16), Size = new Size(950, 45) };
        var lblTitle = new Label { Text = "Algorithm Performance Lab & Big-O Benchmarking", Font = UITheme.HeaderFont, ForeColor = UITheme.TextMain, Location = new Point(0, 4), AutoSize = true };
        headerPanel.Controls.Add(lblTitle);
        Controls.Add(headerPanel);

        // Control Panel
        var pnlControls = UITheme.CreateCard(24, 68, 950, 70);

        var lblComp = new Label { Text = "Comparison Suite:", Location = new Point(16, 12), AutoSize = true, Font = UITheme.SmallFont };
        _cmbComparisonType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(16, 32), Size = new Size(340, 24), Font = UITheme.SubtitleFont };
        _cmbComparisonType.Items.AddRange(new object[] {
            "1. Sorting Algorithms (Bubble, Insert, Select, Merge, Quick)",
            "2. Searching Methods (Linear Search vs Binary Search)",
            "3. Large Multiplication (Grade-School vs Karatsuba Divide & Conquer)"
        });
        _cmbComparisonType.SelectedIndex = 0;
        _cmbComparisonType.SelectedIndexChanged += (s, e) => UpdateInputSizes();

        var lblSize = new Label { Text = "Input Size (N):", Location = new Point(380, 12), AutoSize = true, Font = UITheme.SmallFont };
        _cmbInputSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(380, 32), Size = new Size(130, 24), Font = UITheme.SubtitleFont };
        UpdateInputSizes();

        _btnRunBenchmark = UITheme.CreatePrimaryButton("⚡ Run Empirical Benchmark", 540, 26, 230, 36);
        _btnRunBenchmark.Click += (s, e) => ExecuteSelectedBenchmark();

        pnlControls.Controls.AddRange(new Control[] { lblComp, _cmbComparisonType, lblSize, _cmbInputSize, _btnRunBenchmark });
        Controls.Add(pnlControls);

        // Benchmark Results Table
        var pnlTableCard = UITheme.CreateCard(24, 148, 950, 200);
        var lblTblTitle = new Label { Text = "EMPIRICAL TIMING & THEORETICAL COMPLEXITY ANALYSIS", Font = UITheme.SmallFont, ForeColor = UITheme.Primary, Location = new Point(14, 10), AutoSize = true };

        _lvResults = new ListView
        {
            Location = new Point(14, 30),
            Size = new Size(920, 155),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = UITheme.BodyFont
        };
        _lvResults.Columns.Add("Algorithm / Structure", 230);
        _lvResults.Columns.Add("Empirical Time (μs)", 150);
        _lvResults.Columns.Add("Operations / Metric", 170);
        _lvResults.Columns.Add("Time Complexity (Big-O)", 170);
        _lvResults.Columns.Add("Space Complexity", 160);

        pnlTableCard.Controls.AddRange(new Control[] { lblTblTitle, _lvResults });
        Controls.Add(pnlTableCard);

        // GDI+ Runtime Comparison Bar Chart
        var pnlChartCard = UITheme.CreateCard(24, 358, 950, 225);
        _chartBenchmarks = new ChartRenderer { Location = new Point(0, 0), Dock = DockStyle.Fill };
        pnlChartCard.Controls.Add(_chartBenchmarks);
        Controls.Add(pnlChartCard);

        _lblAnalysisSummary = new Label
        {
            Text = "Select an algorithm suite and input size N, then click 'Run Empirical Benchmark' to measure wall-clock microseconds.",
            Font = UITheme.SmallFont,
            ForeColor = UITheme.TextMuted,
            Location = new Point(24, 590),
            Size = new Size(950, 24)
        };
        Controls.Add(_lblAnalysisSummary);
    }

    private void UpdateInputSizes()
    {
        _cmbInputSize.Items.Clear();
        int suite = _cmbComparisonType.SelectedIndex;
        if (suite == 2) // Large numbers (Karatsuba)
        {
            _cmbInputSize.Items.AddRange(new object[] { "16 digits", "32 digits", "64 digits", "128 digits", "256 digits" });
            _cmbInputSize.SelectedIndex = 2; // 64 digits
        }
        else
        {
            _cmbInputSize.Items.AddRange(new object[] { "100", "500", "1,000", "2,500", "5,000", "10,000" });
            _cmbInputSize.SelectedIndex = 2; // 1,000
        }
    }

    public void RefreshData()
    {
        ExecuteSelectedBenchmark();
    }

    private void ExecuteSelectedBenchmark()
    {
        int suite = _cmbComparisonType.SelectedIndex;
        string sizeStr = _cmbInputSize.SelectedItem?.ToString()?.Replace(",", "").Replace(" digits", "") ?? "1000";
        int n = int.Parse(sizeStr);

        BenchmarkComparisonGroup group = suite switch
        {
            0 => _benchmarkService.RunSortingComparison(n),
            1 => _benchmarkService.RunSearchComparison(n),
            _ => _benchmarkService.RunMultiplicationComparison(n)
        };

        // Populate List View
        _lvResults.Items.Clear();
        var chartPoints = new CustomArray<ChartDataPoint>();

        for (int i = 0; i < group.Items.Count; i++)
        {
            var item = group.Items[i];
            var lvi = new ListViewItem(item.AlgorithmName);
            lvi.SubItems.Add($"{item.ElapsedMicroseconds:N1} μs");
            lvi.SubItems.Add(item.Details);
            lvi.SubItems.Add(item.TheoreticalTimeComplexity);
            lvi.SubItems.Add(item.TheoreticalSpaceComplexity);
            _lvResults.Items.Add(lvi);

            Color color = item.TheoreticalTimeComplexity.Contains("N²")
                ? Color.FromArgb(245, 158, 11)   // Amber for O(N^2)
                : (item.TheoreticalTimeComplexity.Contains("N log N")
                    ? Color.FromArgb(16, 185, 129) // Emerald for O(N log N)
                    : (item.TheoreticalTimeComplexity.Contains("log N")
                        ? Color.FromArgb(129, 140, 248) // Indigo for O(log N)
                        : (item.TheoreticalTimeComplexity.Contains("1")
                            ? Color.FromArgb(192, 132, 252) // Purple for O(1)
                            : Color.FromArgb(56, 189, 248)))); // Sky for O(N)

            chartPoints.Add(new ChartDataPoint(item.AlgorithmName, item.ElapsedMicroseconds, color));
        }

        // Render Bar Chart
        _chartBenchmarks.SetData(chartPoints, $"{group.CategoryName} Execution Time (N = {group.InputSize:N0})", "Microseconds (μs)", ChartType.Bar);

        _lblAnalysisSummary.Text = $"Benchmark complete for {group.CategoryName} with N = {group.InputSize:N0}. " +
            $"Notice the stark contrast between polynomial O(N²) growth and logarithmic / divide-and-conquer efficiency!";
    }
}

