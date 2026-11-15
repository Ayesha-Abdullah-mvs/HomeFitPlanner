using System;

namespace HomeFitPlanner.Core.DataStructures.Stacks;

/// <summary>
/// Hand-written Undo/Redo manager powered by two LIFO stacks.
/// Used for reverting workout plan customizations and session adjustments.
/// </summary>
public class UndoRedoManager<T>
{
    private readonly CustomStack<T> _undoStack = new();
    private readonly CustomStack<T> _redoStack = new();

    public bool CanUndo => !_undoStack.IsEmpty;
    public bool CanRedo => !_redoStack.IsEmpty;

    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;

    public void RecordState(T state)
    {
        _undoStack.Push(state);
        _redoStack.Clear(); // New branch of history clears redo stack
    }

    public T? Undo(T currentState)
    {
        if (!CanUndo) return default;

        _redoStack.Push(currentState);
        return _undoStack.Pop();
    }

    public T? Redo(T currentState)
    {
        if (!CanRedo) return default;

        _undoStack.Push(currentState);
        return _redoStack.Pop();
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}

