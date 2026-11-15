using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.DataStructures.Graphs;
using HomeFitPlanner.Core.Models;
using HomeFitPlanner.Core.Services;

namespace HomeFitPlanner.UI.Controls;

/// <summary>
/// Custom GDI+ canvas for rendering the exercise relationship graph,
/// highlighting BFS substitute clusters and Dijkstra Level-Up shortest paths.
/// </summary>
public class GraphRenderer : Control
{
    private Graph<string>? _graph;
    private CustomArray<string>? _highlightedPath;
    private CustomArray<string>? _highlightedSubstitutes;
    private string? _selectedVertex;

    public GraphRenderer()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(248, 250, 252);
    }

    public void SetGraph(Graph<string> graph, CustomArray<string>? path = null, CustomArray<string>? substitutes = null, string? selected = null)
    {
        _graph = graph;
        _highlightedPath = path;
        _highlightedSubstitutes = substitutes;
        _selectedVertex = selected;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (_graph == null || _graph.VertexCount == 0)
        {
            using var font = new Font("Segoe UI", 11);
            using var brush = new SolidBrush(Color.Gray);
            g.DrawString("Select exercises to view progression graph.", font, brush, new PointF(20, 20));
            return;
        }

        var vertices = _graph.GetAllVertices();
        int count = vertices.Count;

        // Arrange in a circular layout or radial clusters
        int cx = Width / 2;
        int cy = Height / 2;
        int radius = Math.Min(cx, cy) - 60;
        if (radius < 50) radius = 50;

        var positions = new PointF[count];
        for (int i = 0; i < count; i++)
        {
            double angle = (2 * Math.PI * i) / count - Math.PI / 2;
            float x = (float)(cx + radius * Math.Cos(angle));
            float y = (float)(cy + radius * Math.Sin(angle));
            positions[i] = new PointF(x, y);
        }

        // Draw edges
        using var edgePen = new Pen(Color.FromArgb(203, 213, 225), 1.5f);
        using var pathPen = new Pen(Color.FromArgb(239, 68, 68), 3f);
        using var subPen = new Pen(Color.FromArgb(16, 185, 129), 2.5f);
        using var weightFont = new Font("Segoe UI", 7.5f);
        using var textBrush = new SolidBrush(Color.FromArgb(100, 116, 139));

        for (int i = 0; i < count; i++)
        {
            var edges = _graph.GetEdgesByIndex(i);
            var pt1 = positions[i];

            for (int eIdx = 0; eIdx < edges.Count; eIdx++)
            {
                var edge = edges[eIdx];
                int tgtIdx = _graph.IndexOfVertex(edge.Target);
                if (tgtIdx > i) // Draw each undirected edge once
                {
                    var pt2 = positions[tgtIdx];
                    bool isPath = IsEdgeInPath(vertices[i], edge.Target);
                    bool isSub = IsEdgeInSubstitutes(vertices[i], edge.Target);

                    Pen activePen = isPath ? pathPen : (isSub ? subPen : edgePen);
                    g.DrawLine(activePen, pt1, pt2);

                    // Draw weight label in middle
                    float mx = (pt1.X + pt2.X) / 2;
                    float my = (pt1.Y + pt2.Y) / 2;
                    g.DrawString($"w:{edge.Weight:F1}", weightFont, textBrush, mx, my);
                }
            }
        }

        // Draw vertex nodes
        using var nodeFont = new Font("Segoe UI", 8, FontStyle.Bold);

        for (int i = 0; i < count; i++)
        {
            var pt = positions[i];
            string vName = vertices[i];

            bool isHighlighted = (_highlightedPath != null && _highlightedPath.Contains(vName));
            bool isSubstitute = (_highlightedSubstitutes != null && _highlightedSubstitutes.Contains(vName));
            bool isSelected = string.Equals(_selectedVertex, vName, StringComparison.OrdinalIgnoreCase);

            Color fillColor = Color.White;
            Color borderColor = Color.FromArgb(148, 163, 184);

            if (isSelected)
            {
                fillColor = Color.FromArgb(79, 70, 229); // Primary Indigo
                borderColor = Color.FromArgb(67, 56, 202);
            }
            else if (isHighlighted)
            {
                fillColor = Color.FromArgb(254, 226, 226); // Light Red
                borderColor = Color.FromArgb(239, 68, 68);
            }
            else if (isSubstitute)
            {
                fillColor = Color.FromArgb(209, 250, 229); // Light Green
                borderColor = Color.FromArgb(16, 185, 129);
            }

            bool isSignificant = isSelected || isHighlighted || isSubstitute;
            float currentSize = isSignificant ? 36 : 12;

            var rect = new RectangleF(pt.X - currentSize / 2, pt.Y - currentSize / 2, currentSize, currentSize);
            using var fillBrush = new SolidBrush(fillColor);
            using var borderPen = new Pen(borderColor, isSignificant ? 2.5f : 1f);

            g.FillEllipse(fillBrush, rect);
            g.DrawEllipse(borderPen, rect);

            // Draw label clearly for significant nodes
            if (isSignificant)
            {
                string shortLabel = TruncateLabel(vName, 14);
                using var labelBrush = new SolidBrush(isSelected ? Color.White : Color.FromArgb(15, 23, 42));
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(shortLabel, nodeFont, labelBrush, rect, sf);
            }
        }
    }

    private bool IsEdgeInPath(string v1, string v2)
    {
        if (_highlightedPath == null || _highlightedPath.Count < 2) return false;
        for (int i = 0; i < _highlightedPath.Count - 1; i++)
        {
            if ((_highlightedPath[i] == v1 && _highlightedPath[i + 1] == v2) ||
                (_highlightedPath[i] == v2 && _highlightedPath[i + 1] == v1))
                return true;
        }
        return false;
    }

    private bool IsEdgeInSubstitutes(string v1, string v2)
    {
        if (_highlightedSubstitutes == null) return false;
        return (_highlightedSubstitutes.Contains(v1) && string.Equals(_selectedVertex, v2)) ||
               (_highlightedSubstitutes.Contains(v2) && string.Equals(_selectedVertex, v1));
    }

    private string TruncateLabel(string text, int max)
    {
        if (text.Length <= max) return text;
        return text.Substring(0, max - 2) + "..";
    }
}

