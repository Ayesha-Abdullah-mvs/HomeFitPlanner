using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HomeFitPlanner.Core.Models;

namespace HomeFitPlanner.UI.Controls;

public static class UITheme
{
    // Color Palette
    public static readonly Color Background = Color.FromArgb(248, 250, 252);         // Slate 50
    public static readonly Color SidebarBg = Color.FromArgb(15, 23, 42);             // Slate 900
    public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);          // Slate 800
    public static readonly Color SidebarActive = Color.FromArgb(79, 70, 229);        // Indigo 600
    public static readonly Color SidebarText = Color.FromArgb(226, 232, 240);        // Slate 200
    public static readonly Color CardBg = Color.White;
    public static readonly Color CardBorder = Color.FromArgb(226, 232, 240);         // Slate 200
    public static readonly Color TextMain = Color.FromArgb(15, 23, 42);              // Slate 900
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);          // Slate 500
    public static readonly Color Primary = Color.FromArgb(79, 70, 229);              // Indigo 600
    public static readonly Color PrimaryHover = Color.FromArgb(67, 56, 202);         // Indigo 700
    public static readonly Color Secondary = Color.FromArgb(13, 148, 136);           // Teal 600
    public static readonly Color Success = Color.FromArgb(16, 185, 129);             // Emerald 500
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);             // Amber 500
    public static readonly Color Danger = Color.FromArgb(239, 68, 68);               // Rose 500

    // Day Colors
    public static readonly Color StrengthDay = Color.FromArgb(59, 130, 246);         // Blue
    public static readonly Color CardioDay = Color.FromArgb(249, 115, 22);           // Orange
    public static readonly Color MixedDay = Color.FromArgb(168, 85, 247);            // Purple
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
            ForeColor = Color.White,
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
            BackColor = Color.FromArgb(241, 245, 249),
            ForeColor = TextMain,
            FlatStyle = FlatStyle.Flat,
            Font = SubtitleFont,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = CardBorder;
        return btn;
    }
}

