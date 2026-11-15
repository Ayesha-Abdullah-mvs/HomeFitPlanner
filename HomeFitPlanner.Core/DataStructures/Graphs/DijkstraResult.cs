using HomeFitPlanner.Core.DataStructures.Arrays;

namespace HomeFitPlanner.Core.DataStructures.Graphs;

public class DijkstraResult<T>
{
    public T Source { get; set; }
    public T Target { get; set; }
    public double TotalDistance { get; set; }
    public bool IsReachable { get; set; }
    public CustomArray<T> Path { get; set; } = new();
    public CustomArray<string> StepDescriptions { get; set; } = new();

    public DijkstraResult(T source, T target)
    {
        Source = source;
        Target = target;
        TotalDistance = double.PositiveInfinity;
        IsReachable = false;
    }
}

