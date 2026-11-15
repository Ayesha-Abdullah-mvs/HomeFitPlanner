using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HomeFitPlanner.Core.Services;
using HomeFitPlanner.UI.Controls;
using HomeFitPlanner.UI.Screens;

namespace HomeFitPlanner.UI;

public partial class MainForm : Form
{
    private Panel _pnlSidebar = null!;
    private Panel _pnlTopBar = null!;
    private Panel _pnlContentContainer = null!;

    private Label _lblTopTitle = null!;
    private Label _lblSidebarUsername = null!;
    private Label _lblSidebarAvatar = null!;

    private Button[] _navButtons = null!;
    private string[] _screenNames = null!;

    // The 9 Screen instances
    private Screen1_WelcomeProfile _screen1 = null!;
    private Screen2_Dashboard _screen2 = null!;
    private Screen3_WeeklyPlan _screen3 = null!;
    private Screen4_TodayWorkout _screen4 = null!;
    private Screen5_ExerciseLibrary _screen5 = null!;
    private Screen6_SubstitutesLevelUp _screen6 = null!;
    private Screen7_Progress _screen7 = null!;
    private Screen8_SavedPlans _screen8 = null!;
    private Screen9_PerformanceLab _screen9 = null!;

    public MainForm()
    {
        InitializeComponent();
        InitializeScreens();
        NavigateTo("Dashboard");
    }

    private void InitializeComponent()
    {
        Text = "HomeFit Planner - Personalized Home Workout Planning System";
        Size = new Size(1280, 780);
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UITheme.Background;
        Font = UITheme.BodyFont;

        // 1. Sidebar Panel (Left)
        _pnlSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 240,
            BackColor = UITheme.SidebarBg
        };

        // App Logo / Branding at Top of Sidebar
        var pnlBranding = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Color.FromArgb(10, 15, 30) };
        var lblLogo = new Label
        {
            Text = "💪 HomeFit Planner",
            Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(14, 18),
            AutoSize = true
        };
        var lblSlogan = new Label
        {
            Text = "Personalized Fitness System",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(16, 48),
            AutoSize = true
        };
        pnlBranding.Controls.Add(lblLogo);
        pnlBranding.Controls.Add(lblSlogan);
        _pnlSidebar.Controls.Add(pnlBranding);

        // USER ACCOUNT PILL AT BOTTOM OF SIDEBAR (As requested)
        var pnlUserAccount = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 72,
            BackColor = Color.FromArgb(10, 15, 30),
            Cursor = Cursors.Hand
        };

        var pnlUserInner = new Panel
        {
            Location = new Point(12, 10),
            Size = new Size(216, 52),
            BackColor = Color.FromArgb(18, 24, 40)
        };
        pnlUserInner.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var p = new Pen(Color.FromArgb(35, 45, 65), 1);
            e.Graphics.DrawRectangle(p, 0, 0, pnlUserInner.Width - 1, pnlUserInner.Height - 1);
        };

        _lblSidebarAvatar = new Label
        {
            Text = "👤",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(79, 70, 229),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 10),
            Size = new Size(32, 32)
        };

        _lblSidebarUsername = new Label
        {
            Text = "User Profile",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(48, 8),
            Size = new Size(140, 18)
        };

        var lblSidebarLevel = new Label
        {
            Text = "• Active Athlete",
            Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(52, 211, 153),
            Location = new Point(48, 28),
            Size = new Size(140, 16)
        };

        var lblGear = new Label
        {
            Text = "⚙️",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(188, 16),
            Size = new Size(20, 20)
        };

        pnlUserInner.Controls.AddRange(new Control[] { _lblSidebarAvatar, _lblSidebarUsername, lblSidebarLevel, lblGear });
        pnlUserAccount.Controls.Add(pnlUserInner);

        Action openProfile = () => NavigateTo("Welcome & Profile");
        pnlUserAccount.Click += (s, e) => openProfile();
        pnlUserInner.Click += (s, e) => openProfile();
        _lblSidebarAvatar.Click += (s, e) => openProfile();
        _lblSidebarUsername.Click += (s, e) => openProfile();
        lblSidebarLevel.Click += (s, e) => openProfile();

        _pnlSidebar.Controls.Add(pnlUserAccount);

        // Navigation Menu in Middle
        _screenNames = new[]
        {
            "Dashboard",
            "Weekly Plan",
            "Today's Workout",
            "Exercise Library",
            "Substitutes and Level-Up",
            "Progress",
            "Saved Plans",
            "Performance Lab",
            "Welcome & Profile"
        };

        string[] buttonLabels = new[]
        {
            "🏠 Dashboard",
            "📅 Weekly Plan",
            "🏋️ Today's Workout",
            "📚 Exercise Library & Form",
            "🔄 Substitutes [M2 Preview]",
            "📈 Progress Analytics",
            "📂 Saved Plans [M2 Preview]",
            "⚡ Performance Lab",
            "👤 Edit Profile & Setup"
        };

        _navButtons = new Button[buttonLabels.Length];
        var pnlNav = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = false,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0)
        };

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            string screen = _screenNames[i];
            var btn = new Button
            {
                Text = buttonLabels[i],
                Size = new Size(240, 42),
                FlatStyle = FlatStyle.Flat,
                Font = UITheme.SubtitleFont,
                ForeColor = UITheme.SidebarText,
                BackColor = UITheme.SidebarBg,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Click += (s, e) => NavigateTo(screen);
            _navButtons[i] = btn;
            pnlNav.Controls.Add(btn);
        }

        _pnlSidebar.Controls.Add(pnlNav);
        Controls.Add(_pnlSidebar);

        // 2. Top Bar (Header)
        _pnlTopBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.FromArgb(15, 23, 42)
        };
        _pnlTopBar.Paint += (s, e) =>
        {
            using var p = new Pen(Color.FromArgb(30, 41, 59), 1);
            e.Graphics.DrawLine(p, 0, _pnlTopBar.Height - 1, _pnlTopBar.Width, _pnlTopBar.Height - 1);
        };

        // TOP BAR LEFT: SCREEN TITLE
        _lblTopTitle = new Label
        {
            Text = "Dashboard Overview",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 16),
            AutoSize = true
        };
        _pnlTopBar.Controls.Add(_lblTopTitle);

        // Right Header Badges & Actions
        var lblStreakTop = new Label
        {
            Text = "🔥 3 Day Streak",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(253, 230, 138),
            BackColor = Color.FromArgb(40, 30, 15),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(120, 30),
            Location = new Point(885, 13),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        var btnStartWorkoutTop = UITheme.CreatePrimaryButton("▶ Start Workout", 1020, 11, 140, 34);
        btnStartWorkoutTop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnStartWorkoutTop.Click += (s, e) => NavigateTo("Today's Workout");

        // TOP BAR CORNER: ONLY AA INITIALS VISIBLE IN FORM OF A CIRCLE (As requested)
        var btnProjectDetails = new Button
        {
            Text = "AA",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(10, 15, 26),
            BackColor = Color.FromArgb(56, 189, 248),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(38, 38),
            Location = new Point(1180, 9),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        btnProjectDetails.FlatAppearance.BorderSize = 0;
        using var ellipsePath = new GraphicsPath();
        ellipsePath.AddEllipse(0, 0, 38, 38);
        btnProjectDetails.Region = new Region(ellipsePath);

        btnProjectDetails.Click += (s, e) =>
        {
            using var detailsForm = new ProjectDetailsForm();
            detailsForm.ShowDialog(this);
        };

        _pnlTopBar.Controls.AddRange(new Control[] { lblStreakTop, btnStartWorkoutTop, btnProjectDetails });
        Controls.Add(_pnlTopBar);

        // 3. Content Container Panel
        _pnlContentContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UITheme.Background
        };
        Controls.Add(_pnlContentContainer);

        // Dock ordering: controls added in reverse order of precedence
        _pnlContentContainer.BringToFront();
    }

    private void InitializeScreens()
    {
        _screen1 = new Screen1_WelcomeProfile(NavigateTo);
        _screen2 = new Screen2_Dashboard(NavigateTo);
        _screen3 = new Screen3_WeeklyPlan(NavigateTo);
        _screen4 = new Screen4_TodayWorkout(NavigateTo);
        _screen5 = new Screen5_ExerciseLibrary(NavigateTo);
        _screen6 = new Screen6_SubstitutesLevelUp(NavigateTo);
        _screen7 = new Screen7_Progress(NavigateTo);
        _screen8 = new Screen8_SavedPlans(NavigateTo);
        _screen9 = new Screen9_PerformanceLab(NavigateTo);
    }

    public void NavigateTo(string screenName)
    {
        UserControl targetScreen = screenName switch
        {
            "Welcome & Profile" => _screen1,
            "Dashboard" => _screen2,
            "Weekly Plan" => _screen3,
            "Today's Workout" => _screen4,
            "Exercise Library" => _screen5,
            "Substitutes and Level-Up" => _screen6,
            "Progress" => _screen7,
            "Saved Plans" => _screen8,
            "Performance Lab" => _screen9,
            _ => _screen2
        };

        _lblTopTitle.Text = screenName switch
        {
            "Welcome & Profile" => "Athlete Profile & Medical Limitations Setup",
            "Dashboard" => "Dashboard Overview & Adaptive Health Metrics",
            "Weekly Plan" => "Weekly Schedule & Periodized Workout Plan",
            "Today's Workout" => "Guided Workout Session & Real-Time Player",
            "Exercise Library" => "Exercise Library & Form Instruction Guide",
            "Substitutes and Level-Up" => "Progression Graph & BFS Exercise Substitution",
            "Progress" => "Progress Tracker & Streak Analytics",
            "Saved Plans" => "Saved Plans Storage (Binary Search Tree & AVL Tree)",
            "Performance Lab" => "Big-O Performance Benchmarking Lab",
            _ => screenName
        };

        // Update active user profile details in sidebar
        var currentProfile = DataManager.Instance.CurrentProfile;
        if (currentProfile != null && !string.IsNullOrWhiteSpace(currentProfile.Name))
        {
            _lblSidebarUsername.Text = currentProfile.Name;
            _lblSidebarAvatar.Text = currentProfile.Name.Substring(0, 1).ToUpperInvariant();
        }

        // Update active sidebar button styles
        for (int i = 0; i < _screenNames.Length; i++)
        {
            if (_screenNames[i] == screenName)
            {
                _navButtons[i].BackColor = UITheme.SidebarActive;
                _navButtons[i].ForeColor = Color.White;
            }
            else
            {
                _navButtons[i].BackColor = UITheme.SidebarBg;
                _navButtons[i].ForeColor = UITheme.SidebarText;
            }
        }

        // Swap control in container
        _pnlContentContainer.Controls.Clear();
        targetScreen.Dock = DockStyle.Fill;
        _pnlContentContainer.Controls.Add(targetScreen);

        // Refresh screen state
        switch (screenName)
        {
            case "Dashboard":
                _screen2.RefreshData();
                break;
            case "Weekly Plan":
                _screen3.RefreshData();
                break;
            case "Today's Workout":
                _screen4.LoadTodayWorkout();
                break;
            case "Exercise Library":
                _screen5.RefreshData();
                break;
            case "Substitutes and Level-Up":
                _screen6.RefreshData();
                break;
            case "Progress":
                _screen7.RefreshData();
                break;
            case "Saved Plans":
                _screen8.RefreshData();
                break;
            case "Performance Lab":
                _screen9.RefreshData();
                break;
        }
    }
}
