namespace TextEditor.Undo;

public sealed class UndoManager
{
    private readonly List<IUndoObserver> observers = new();
    private readonly Stack<EditAction> redoStack = new();
    private readonly Stack<EditAction> undoStack = new();

    private UndoManager()
    {
    }

    public static UndoManager Instance { get; } = new();


    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    public void Push(EditAction action)
    {
        redoStack.Clear();
        undoStack.Push(action);
        action.ExecuteDo();
        Notify();
    }

    public void Undo()
    {
        if (undoStack.Count == 0) return;
        var a = undoStack.Pop();
        a.ExecuteUndo();
        redoStack.Push(a);
        Notify();
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;
        var a = redoStack.Pop();
        a.ExecuteDo();
        undoStack.Push(a);
        Notify();
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
        Notify();
    }

    public void AddObserver(IUndoObserver o)
    {
        if (!observers.Contains(o)) observers.Add(o);
    }

    public void RemoveObserver(IUndoObserver o)
    {
        observers.Remove(o);
    }

    private void Notify()
    {
        foreach (var o in observers)
            o.UpdateUndoRedo(CanUndo, CanRedo);
    }
}