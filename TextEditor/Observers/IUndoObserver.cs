namespace TextEditor;

public interface IUndoObserver
{
    void UpdateUndoRedo(bool canUndo, bool canRedo);
}