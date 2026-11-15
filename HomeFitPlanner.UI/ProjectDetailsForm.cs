using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HomeFitPlanner.UI;

public class ProjectDetailsForm : Form
{
    public ProjectDetailsForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Project & Developer Details";
        Size = new Size(460, 410);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.FromArgb(11, 15, 25);
        ShowInTaskbar = false;
        DoubleBuffered = true;

        // Custom Paint for sleek border
        Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(40, 55, 80), 2);
            e.Graphics.DrawRectangle(pen, 1, 1, ClientSize.Width - 2, ClientSize.Height - 2);
        };

        // Close button (X) in top-right
        var btnClose = new Button
        {
            Text = "✕",
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            ForeColor = Color.FromArgb(148, 163, 184),
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(36, 36),
            Location = new Point(ClientSize.Width - 46, 12),
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
        btnClose.Click += (s, e) => Close();
        Controls.Add(btnClose);

        // Department Card: Institute of Data Science, UET Lahore
        var pnlDept = new Panel
        {
            Size = new Size(ClientSize.Width - 60, 68),
            Location = new Point(30, 50),
            BackColor = Color.FromArgb(15, 23, 42)
        };
        pnlDept.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var p = new Pen(Color.FromArgb(30, 41, 59), 1.5f);
            e.Graphics.DrawRectangle(p, 0, 0, pnlDept.Width - 1, pnlDept.Height - 1);
        };
        var lblDept = new Label
        {
            Text = "Institute of Data Science\nUniversity of Engineering and Technology (UET)\nLahore, Pakistan",
            Font = new Font("Segoe UI", 8.8f, FontStyle.Regular),
            ForeColor = Color.FromArgb(203, 213, 225),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        pnlDept.Controls.Add(lblDept);
        Controls.Add(pnlDept);

        // Academic Project Metadata Box & Prominent Team Members Box
        var pnlMeta = new Panel
        {
            Size = new Size(ClientSize.Width - 60, 185),
            Location = new Point(30, 132),
            BackColor = Color.FromArgb(18, 22, 34)
        };
        pnlMeta.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var p = new Pen(Color.FromArgb(35, 45, 65), 1.5f);
            e.Graphics.DrawRectangle(p, 0, 0, pnlMeta.Width - 1, pnlMeta.Height - 1);
        };

        var lblCourseTitle = new Label
        {
            Text = "HomeFit Planner • DSA Final Project\nSupervisor: Mr. Muhammad Nazir",
            Font = new Font("Segoe UI", 7.8f, FontStyle.Regular),
            ForeColor = Color.FromArgb(148, 163, 184),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(5, 8),
            Size = new Size(pnlMeta.Width - 10, 28)
        };

        var lblTeamHeader = new Label
        {
            Text = "PROJECT INFORMATION",
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(56, 189, 248),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(5, 38),
            Size = new Size(pnlMeta.Width - 10, 18)
        };

        var lblTeamMembers = new Label
        {
            Text = "Class: Business Data Analytics (Section B)\n" +
                   "Course: CSC 200L Data Structures & Algorithms\n" +
                   "Submitted to: Mr. Muhammad Nazir\n" +
                   "Institute of Data Science, UET Lahore",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(5, 60),
            Size = new Size(pnlMeta.Width - 10, 115)
        };

        pnlMeta.Controls.AddRange(new Control[] { lblCourseTitle, lblTeamHeader, lblTeamMembers });
        Controls.Add(pnlMeta);

        // Single Action Button: GitHub Repository Only (as requested)
        var btnGithub = new Button
        {
            Text = "🐙   GitHub Repository",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(30, 41, 59),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(ClientSize.Width - 60, 42),
            Location = new Point(30, 335),
            Cursor = Cursors.Hand
        };
        btnGithub.FlatAppearance.BorderColor = Color.FromArgb(51, 65, 85);
        btnGithub.Click += (s, e) => OpenUrl("https://github.com");
        Controls.Add(btnGithub);

        // Draggable Form on mouse down
        MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        };
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch { }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
}
