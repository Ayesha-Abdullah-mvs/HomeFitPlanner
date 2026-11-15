using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HomeFitPlanner.Core.DataStructures.Trees;
using HomeFitPlanner.Core.Models;

namespace HomeFitPlanner.UI.Controls;

/// <summary>
/// Custom GDI+ canvas for visualizing Binary Search Trees and AVL Trees.
/// Renders node durations, balance factors, parent-child branches, and tree heights.
/// </summary>
public class TreeRenderer : Control
{
    private BstNode<int, WorkoutPlan>? _bstRoot;
    private AvlNode<int, WorkoutPlan>? _avlRoot;
    private bool _isAvl = false;
    private int _treeHeight = 0;

    public TreeRenderer()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(248, 250, 252);
    }

    public void SetBst(BinarySearchTree<int, WorkoutPlan> tree)
    {
        _bstRoot = tree.Root;
        _avlRoot = null;
        _isAvl = false;
        _treeHeight = tree.GetHeight();
        Invalidate();
    }

    public void SetAvl(AvlTree<int, WorkoutPlan> tree)
    {
        _avlRoot = tree.Root;
        _bstRoot = null;
        _isAvl = true;
        _treeHeight = tree.GetHeight();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.FromArgb(15, 23, 42));

        string title = _isAvl
            ? $"Self-Balancing AVL Tree (Height: {_treeHeight}, Strictly Balanced O(log N))"
            : $"Plain Binary Search Tree (Height: {_treeHeight}, Ordered by Duration)";
        g.DrawString(title, titleFont, textBrush, 15, 12);

        if (!_isAvl && _bstRoot != null)
        {
            DrawBstNode(g, _bstRoot, Width / 2, 60, Width / 4, 1);
        }
        else if (_isAvl && _avlRoot != null)
        {
            DrawAvlNode(g, _avlRoot, Width / 2, 60, Width / 4, 1);
        }
        else
        {
            using var emptyFont = new Font("Segoe UI", 9.5f);
            using var emptyBrush = new SolidBrush(Color.Gray);
            g.DrawString("No plans saved yet. Save a weekly plan to visualize its node placement in the tree.", emptyFont, emptyBrush, 15, 45);
        }
    }

    private void DrawBstNode(Graphics g, BstNode<int, WorkoutPlan> node, int x, int y, int hOffset, int level)
    {
        int nodeRadius = 24;
        int nextY = y + 70;

        if (node.Left != null)
        {
            int nextX = x - hOffset;
            using var pen = new Pen(Color.FromArgb(148, 163, 184), 2f);
            g.DrawLine(pen, x, y + nodeRadius, nextX, nextY - nodeRadius);
            DrawBstNode(g, node.Left, nextX, nextY, Math.Max(25, hOffset / 2), level + 1);
        }

        if (node.Right != null)
        {
            int nextX = x + hOffset;
            using var pen = new Pen(Color.FromArgb(148, 163, 184), 2f);
            g.DrawLine(pen, x, y + nodeRadius, nextX, nextY - nodeRadius);
            DrawBstNode(g, node.Right, nextX, nextY, Math.Max(25, hOffset / 2), level + 1);
        }

        // Draw node circle
        var rect = new Rectangle(x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
        using var fillBrush = new SolidBrush(Color.FromArgb(79, 70, 229));
        using var borderPen = new Pen(Color.FromArgb(49, 46, 129), 2f);
        g.FillEllipse(fillBrush, rect);
        g.DrawEllipse(borderPen, rect);

        // Text: Duration and Plan name
        using var numFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        using var whiteBrush = new SolidBrush(Color.White);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString($"{node.Key}m", numFont, whiteBrush, rect, sf);

        using var labelFont = new Font("Segoe UI", 7.5f);
        using var labelBrush = new SolidBrush(Color.FromArgb(30, 41, 59));
        string name = node.Value.PlanName.Length > 12 ? node.Value.PlanName.Substring(0, 10) + ".." : node.Value.PlanName;
        g.DrawString(name, labelFont, labelBrush, x, y + nodeRadius + 3, sf);
    }

    private void DrawAvlNode(Graphics g, AvlNode<int, WorkoutPlan> node, int x, int y, int hOffset, int level)
    {
        int nodeRadius = 24;
        int nextY = y + 70;

        if (node.Left != null)
        {
            int nextX = x - hOffset;
            using var pen = new Pen(Color.FromArgb(148, 163, 184), 2f);
            g.DrawLine(pen, x, y + nodeRadius, nextX, nextY - nodeRadius);
            DrawAvlNode(g, node.Left, nextX, nextY, Math.Max(25, hOffset / 2), level + 1);
        }

        if (node.Right != null)
        {
            int nextX = x + hOffset;
            using var pen = new Pen(Color.FromArgb(148, 163, 184), 2f);
            g.DrawLine(pen, x, y + nodeRadius, nextX, nextY - nodeRadius);
            DrawAvlNode(g, node.Right, nextX, nextY, Math.Max(25, hOffset / 2), level + 1);
        }

        // Draw node circle (Teal for balanced AVL)
        var rect = new Rectangle(x - nodeRadius, y - nodeRadius, nodeRadius * 2, nodeRadius * 2);
        using var fillBrush = new SolidBrush(Color.FromArgb(13, 148, 136));
        using var borderPen = new Pen(Color.FromArgb(15, 118, 110), 2f);
        g.FillEllipse(fillBrush, rect);
        g.DrawEllipse(borderPen, rect);

        using var numFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        using var whiteBrush = new SolidBrush(Color.White);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString($"{node.Key}m", numFont, whiteBrush, rect, sf);

        // Height badge below node
        using var labelFont = new Font("Segoe UI", 7.5f);
        using var labelBrush = new SolidBrush(Color.FromArgb(30, 41, 59));
        g.DrawString($"h:{node.Height} | {node.Value.PlanName}", labelFont, labelBrush, x, y + nodeRadius + 3, sf);
    }
}

