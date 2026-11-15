using System;
using System.Collections.Generic;
using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.DataStructures.Graphs;

public class GraphEdge<T>
{
    public T Target { get; }
    public double Weight { get; set; }
    public string Label { get; set; }

    public GraphEdge(T target, double weight, string label = "")
    {
        Target = target;
        Weight = weight;
        Label = label;
    }
}

/// <summary>
/// Hand-written generic graph structure represented via adjacency lists.
/// Supports weighted directed/undirected edges, BFS substitute discovery,
/// DFS component exploration, and Dijkstra's algorithm for Level-Up paths.
/// </summary>
public class Graph<T> where T : notnull
{
    private readonly CustomArray<T> _vertices = new();
    private readonly CustomArray<CustomArray<GraphEdge<T>>> _adjacencyList = new();
    private readonly IEqualityComparer<T> _comparer;

    public int VertexCount => _vertices.Count;

    public Graph(IEqualityComparer<T>? comparer = null)
    {
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    public int AddVertex(T vertex)
    {
        int existingIndex = IndexOfVertex(vertex);
        if (existingIndex >= 0) return existingIndex;

        int newIndex = _vertices.Count;
        _vertices.Add(vertex);
        _adjacencyList.Add(new CustomArray<GraphEdge<T>>());
        return newIndex;
    }

    public void AddEdge(T source, T target, double weight, bool bidirectional = true, string label = "")
    {
        int srcIdx = AddVertex(source);
        int tgtIdx = AddVertex(target);

        // Check if edge already exists
        var srcEdges = _adjacencyList[srcIdx];
        bool found = false;
        for (int i = 0; i < srcEdges.Count; i++)
        {
            if (_comparer.Equals(srcEdges[i].Target, target))
            {
                srcEdges[i].Weight = weight;
                srcEdges[i].Label = label;
                found = true;
                break;
            }
        }
        if (!found)
        {
            srcEdges.Add(new GraphEdge<T>(target, weight, label));
        }

        if (bidirectional)
        {
            var tgtEdges = _adjacencyList[tgtIdx];
            bool foundRev = false;
            for (int i = 0; i < tgtEdges.Count; i++)
            {
                if (_comparer.Equals(tgtEdges[i].Target, source))
                {
                    tgtEdges[i].Weight = weight;
                    tgtEdges[i].Label = label;
                    foundRev = true;
                    break;
                }
            }
            if (!foundRev)
            {
                tgtEdges.Add(new GraphEdge<T>(source, weight, label));
            }
        }
    }

    public int IndexOfVertex(T vertex)
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            if (_comparer.Equals(_vertices[i], vertex))
                return i;
        }
        return -1;
    }

    public T GetVertex(int index) => _vertices[index];

    public CustomArray<T> GetAllVertices() => _vertices.Clone();

    public CustomArray<GraphEdge<T>> GetEdges(T vertex)
    {
        int idx = IndexOfVertex(vertex);
        if (idx < 0) return new CustomArray<GraphEdge<T>>();
        return _adjacencyList[idx].Clone();
    }

    public CustomArray<GraphEdge<T>> GetEdgesByIndex(int index)
    {
        if (index < 0 || index >= _adjacencyList.Count)
            return new CustomArray<GraphEdge<T>>();
        return _adjacencyList[index].Clone();
    }

    public void Clear()
    {
        _vertices.Clear();
        _adjacencyList.Clear();
    }
}

