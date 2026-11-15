using System;
using System.Collections.Generic;
using HomeFitPlanner.Core.DataStructures.Arrays;
using HomeFitPlanner.Core.DataStructures.Graphs;
using HomeFitPlanner.Core.DataStructures.Queues;

namespace HomeFitPlanner.Core.Algorithms;

/// <summary>
/// Hand-written graph algorithms:
/// - BFS for closest substitute exercise discovery
/// - DFS for reachable component exploration
/// - Dijkstra for Level-Up optimal progression paths with difficulty jump weights
/// - Connected Components for exercise families
/// </summary>
public static class GraphAlgorithms
{
    public static CustomArray<T> BreadthFirstSearch<T>(Graph<T> graph, T start, Func<T, bool>? filter = null, int maxDepth = 4) where T : notnull
    {
        var visited = new CustomArray<T>();
        var result = new CustomArray<T>();
        var queue = new CustomQueue<T>();
        var depthQueue = new CustomQueue<int>();

        int startIdx = graph.IndexOfVertex(start);
        if (startIdx < 0) return result;

        queue.Enqueue(start);
        depthQueue.Enqueue(0);
        visited.Add(start);

        while (!queue.IsEmpty)
        {
            var current = queue.Dequeue();
            int currentDepth = depthQueue.Dequeue();

            if (currentDepth > 0) // Exclude start node itself from substitutes list
            {
                if (filter == null || filter(current))
                {
                    result.Add(current);
                }
            }

            if (currentDepth < maxDepth)
            {
                var edges = graph.GetEdges(current);
                for (int i = 0; i < edges.Count; i++)
                {
                    var neighbor = edges[i].Target;
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                        depthQueue.Enqueue(currentDepth + 1);
                    }
                }
            }
        }

        return result;
    }

    public static CustomArray<T> DepthFirstSearch<T>(Graph<T> graph, T start) where T : notnull
    {
        var visited = new CustomArray<T>();
        DfsRecursive(graph, start, visited);
        return visited;
    }

    private static void DfsRecursive<T>(Graph<T> graph, T current, CustomArray<T> visited) where T : notnull
    {
        visited.Add(current);
        var edges = graph.GetEdges(current);
        for (int i = 0; i < edges.Count; i++)
        {
            var neighbor = edges[i].Target;
            if (!visited.Contains(neighbor))
            {
                DfsRecursive(graph, neighbor, visited);
            }
        }
    }

    public static DijkstraResult<T> Dijkstra<T>(Graph<T> graph, T source, T destination) where T : notnull
    {
        var result = new DijkstraResult<T>(source, destination);

        var allVertices = graph.GetAllVertices();
        int n = allVertices.Count;
        int srcIdx = graph.IndexOfVertex(source);
        int destIdx = graph.IndexOfVertex(destination);

        if (srcIdx < 0 || destIdx < 0) return result;

        var dist = new double[n];
        var prev = new int[n];
        var visited = new bool[n];

        for (int i = 0; i < n; i++)
        {
            dist[i] = double.PositiveInfinity;
            prev[i] = -1;
            visited[i] = false;
        }

        dist[srcIdx] = 0;

        // Using hand-written PriorityQueue
        var pq = new PriorityQueue<int>();
        // PriorityQueue is Max-heap, so we enqueue negative distance to extract smallest
        pq.Enqueue(srcIdx, 0);

        while (!pq.IsEmpty)
        {
            int u = pq.Dequeue();
            if (visited[u]) continue;
            visited[u] = true;

            if (u == destIdx) break; // Reached target

            var edges = graph.GetEdgesByIndex(u);
            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                int v = graph.IndexOfVertex(edge.Target);
                if (v < 0 || visited[v]) continue;

                double alt = dist[u] + edge.Weight;
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                    pq.Enqueue(v, -alt);
                }
            }
        }

        if (dist[destIdx] < double.PositiveInfinity)
        {
            result.IsReachable = true;
            result.TotalDistance = dist[destIdx];

            // Reconstruct path
            var pathIndices = new CustomArray<int>();
            int curr = destIdx;
            while (curr != -1)
            {
                pathIndices.Add(curr);
                curr = prev[curr];
            }

            // Reverse path to be from source to destination
            for (int i = pathIndices.Count - 1; i >= 0; i--)
            {
                result.Path.Add(allVertices[pathIndices[i]]);
            }

            // Build human-readable step explanations
            for (int i = 0; i < result.Path.Count - 1; i++)
            {
                var from = result.Path[i];
                var to = result.Path[i + 1];
                var edges = graph.GetEdges(from);
                string label = "Advance";
                for (int e = 0; e < edges.Count; e++)
                {
                    if (EqualityComparer<T>.Default.Equals(edges[e].Target, to))
                    {
                        label = string.IsNullOrEmpty(edges[e].Label) ? "Next Progression Step" : edges[e].Label;
                        break;
                    }
                }
                result.StepDescriptions.Add($"Step {i + 1}: From {from} -> {to} ({label})");
            }
        }

        return result;
    }

    public static CustomArray<CustomArray<T>> FindConnectedComponents<T>(Graph<T> graph) where T : notnull
    {
        var components = new CustomArray<CustomArray<T>>();
        var globalVisited = new CustomArray<T>();
        var allVertices = graph.GetAllVertices();

        for (int i = 0; i < allVertices.Count; i++)
        {
            var v = allVertices[i];
            if (!globalVisited.Contains(v))
            {
                var component = DepthFirstSearch(graph, v);
                components.Add(component);
                for (int j = 0; j < component.Count; j++)
                {
                    if (!globalVisited.Contains(component[j]))
                    {
                        globalVisited.Add(component[j]);
                    }
                }
            }
        }

        return components;
    }
}

