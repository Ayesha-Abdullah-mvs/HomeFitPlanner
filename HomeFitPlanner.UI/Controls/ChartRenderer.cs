using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.UI.Controls;

public enum ChartType
{
    Bar,
    Line
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Subtitle { get; set; } = string.Empty;
    public Color CustomColor { get; set; } = Color.Empty;

    public ChartDataPoint(string label, double value, string subtitle = "")
    {
        Label = label;
        Value = value;
        Subtitle = subtitle;
    }

    public ChartDataPoint(string label, double value, Color customColor, string subtitle = "")
    {
        Label = label;
        Value = value;
        CustomColor = customColor;
        Subtitle = subtitle;
    }
}

/// <summary>
/// Custom GDI+ chart control rendering bar and line charts with gridlines,
/// axis labels, and high-contrast metric callouts.
/// </summary>
public class ChartRenderer : Control
{
    public ChartType ChartType { get; set; } = ChartType.Bar;
    public string ChartTitle { get; set; } = string.Empty;
    public string YAxisLabel { get; set; } = string.Empty;
    public Color PrimaryColor { get; set; } = Color.FromArgb(79, 70, 229);
    public CustomArray<ChartDataPoint> Points { get; set; } = new();

    public ChartRenderer()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
    }

    public void SetData(CustomArray<ChartDataPoint> points, string title = "", string yLabel = "", ChartType type = ChartType.Bar)
    {
        Points = points;
        ChartTitle = title;
        YAxisLabel = yLabel;
        ChartType = type;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Title
        using var titleFont = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(15, 23, 42));
        g.DrawString(ChartTitle, titleFont, textBrush, 15, 12);

        if (Points == null || Points.Count == 0)
        {
            using var font = new Font("Segoe UI", 9.5f);
            using var brush = new SolidBrush(Color.Gray);
            g.DrawString("No data available to plot.", font, brush, 15, 45);
            return;
        }

        // Determine drawing area
        int leftMargin = 55;
        int rightMargin = 25;
        int topMargin = 45;
        int bottomMargin = 40;

        int plotWidth = Width - leftMargin - rightMargin;
        int plotHeight = Height - topMargin - bottomMargin;

        if (plotWidth <= 20 || plotHeight <= 20) return;

        // Find max value
        double maxVal = 0;
        for (int i = 0; i < Points.Count; i++)
        {
            if (Points[i].Value > maxVal) maxVal = Points[i].Value;
        }
        if (maxVal <= 0) maxVal = 10;
        maxVal = Math.Ceiling(maxVal * 1.15); // Add headroom

        // Draw horizontal grid lines & Y-axis labels
        using var gridPen = new Pen(Color.FromArgb(241, 245, 249), 1f);
        using var axisPen = new Pen(Color.FromArgb(203, 213, 225), 1.5f);
        using var labelFont = new Font("Segoe UI", 8);
        using var labelBrush = new SolidBrush(Color.FromArgb(100, 116, 139));

        int gridSteps = 4;
        for (int i = 0; i <= gridSteps; i++)
        {
            double stepVal = (maxVal / gridSteps) * i;
            float y = topMargin + plotHeight - (float)((stepVal / maxVal) * plotHeight);

            g.DrawLine(gridPen, leftMargin, y, leftMargin + plotWidth, y);
            g.DrawString($"{stepVal:F0}", labelFont, labelBrush, 8, y - 6);
        }

        // Draw Axes
        g.DrawLine(axisPen, leftMargin, topMargin, leftMargin, topMargin + plotHeight);
        g.DrawLine(axisPen, leftMargin, topMargin + plotHeight, leftMargin + plotWidth, topMargin + plotHeight);

        // Render Data
        if (ChartType == ChartType.Bar)
        {
            float barWidth = Math.Max(12, (float)plotWidth / Points.Count - 16);
            float groupWidth = (float)plotWidth / Points.Count;

            for (int i = 0; i < Points.Count; i++)
            {
                var pt = Points[i];
                float barH = (float)((pt.Value / maxVal) * plotHeight);
                float x = leftMargin + i * groupWidth + (groupWidth - barWidth) / 2;
                float y = topMargin + plotHeight - barH;

                Color bColor = pt.CustomColor != Color.Empty ? pt.CustomColor : PrimaryColor;
                using var barBrush = new SolidBrush(bColor);
                g.FillRectangle(barBrush, x, y, barWidth, barH);

                // Value above bar
                string valStr = pt.Value >= 1000 ? $"{pt.Value:N0}" : $"{pt.Value:F1}";
                g.DrawString(valStr, labelFont, labelBrush, x, y - 14);

                // Label below bar
                string shortLabel = pt.Label.Length > 9 ? pt.Label.Substring(0, 7) + ".." : pt.Label;
                g.DrawString(shortLabel, labelFont, labelBrush, x - 4, topMargin + plotHeight + 6);
            }
        }
        else // Line Chart
        {
            var pts = new PointF[Points.Count];
            float stepX = (float)plotWidth / Math.Max(1, Points.Count - 1);

            for (int i = 0; i < Points.Count; i++)
            {
                var pt = Points[i];
                float barH = (float)((pt.Value / maxVal) * plotHeight);
                float x = leftMargin + i * stepX;
                float y = topMargin + plotHeight - barH;
                pts[i] = new PointF(x, y);

                // Label below point
                g.DrawString(pt.Label, labelFont, labelBrush, x - 12, topMargin + plotHeight + 6);
            }

            if (pts.Length > 1)
            {
                using var linePen = new Pen(PrimaryColor, 2.5f);
                g.DrawLines(linePen, pts);
            }

            // Draw point dots
            for (int i = 0; i < pts.Length; i++)
            {
                var p = pts[i];
                var dotRect = new RectangleF(p.X - 4, p.Y - 4, 8, 8);
                using var dotBrush = new SolidBrush(PrimaryColor);
                using var whitePen = new Pen(Color.White, 2f);
                g.FillEllipse(dotBrush, dotRect);
                g.DrawEllipse(whitePen, dotRect);

                // Value above point
                g.DrawString($"{Points[i].Value:F1}", labelFont, labelBrush, p.X - 10, p.Y - 16);
            }
        }
    }
}

