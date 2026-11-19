using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HomeFitPlanner.Core.Models;

namespace HomeFitPlanner.UI.Controls;

public static class UITheme
{
    // Color Palette - Cyber-Athletic Neon Volt & Electric Purple Dark Theme
    public static readonly Color Background = Color.FromArgb(8, 9, 12);               // Pitch Black
    public static readonly Color SidebarBg = Color.FromArgb(10, 11, 16);             // Sidebar Deep Dark
    public static readonly Color SidebarHover = Color.FromArgb(24, 26, 38);          // Dark Surface Hover
    public static readonly Color SidebarActive = Color.FromArgb(204, 255, 0);        // Neon Volt / Lime #CCFF00
    public static readonly Color SidebarText = Color.FromArgb(160, 165, 181);        // Muted Gray
    public static readonly Color CardBg = Color.FromArgb(16, 17, 24);                // Card Dark Slate
    public static readonly Color CardBorder = Color.FromArgb(34, 36, 52);            // Card Border
    public static readonly Color TextMain = Color.FromArgb(255, 255, 255);           // Pure White
    public static readonly Color TextMuted = Color.FromArgb(130, 136, 154);          // Slate Muted
    public static readonly Color Primary = Color.FromArgb(204, 255, 0);              // Neon Volt / Lime
    public static readonly Color PrimaryHover = Color.FromArgb(216, 255, 51);        // Neon Volt Hover
    public static readonly Color Secondary = Color.FromArgb(80, 43, 251);            // Electric Purple
    public static readonly Color Success = Color.FromArgb(204, 255, 0);              // Neon Volt Accent
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);             // Amber 500
    public static readonly Color Danger = Color.FromArgb(239, 68, 68);               // Rose 500

    // Day Colors
    public static readonly Color StrengthDay = Color.FromArgb(59, 130, 246);         // Blue
    public static readonly Color CardioDay = Color.FromArgb(249, 115, 22);           // Orange
    public static readonly Color MixedDay = Color.FromArgb(80, 43, 251);             // Electric Purple
    public static readonly Color RestDay = Color.FromArgb(100, 116, 139);            // Slate

    // Fonts
    public static readonly Font HeaderFont = new("Segoe UI", 16, FontStyle.Bold);
    public static readonly Font TitleFont = new("Segoe UI", 12, FontStyle.Bold);
    public static readonly Font SubtitleFont = new("Segoe UI", 10, FontStyle.Bold);
    public static readonly Font BodyFont = new("Segoe UI", 9.5f, FontStyle.Regular);
    public static readonly Font SmallFont = new("Segoe UI", 8.5f, FontStyle.Regular);
    public static readonly Font MetricFont = new("Segoe UI", 20, FontStyle.Bold);

    public static Color GetDayColor(DayFocusType type)
    {
        return type switch
        {
            DayFocusType.Strength => StrengthDay,
            DayFocusType.Cardio => CardioDay,
            DayFocusType.Mixed => MixedDay,
            _ => RestDay
        };
    }

    public static Panel CreateCard(int x, int y, int width, int height)
    {
        var panel = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = CardBg,
            BorderStyle = BorderStyle.FixedSingle
        };
        return panel;
    }

    public static Button CreatePrimaryButton(string text, int x, int y, int width = 140, int height = 36)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Primary,
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Font = SubtitleFont,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    public static Button CreateSecondaryButton(string text, int x, int y, int width = 120, int height = 36)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = Color.FromArgb(24, 26, 38),
            ForeColor = TextMain,
            FlatStyle = FlatStyle.Flat,
            Font = SubtitleFont,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = CardBorder;
        return btn;
    }
}

