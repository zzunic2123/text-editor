namespace TextEditor.Undo;

public interface EditAction
{
    void ExecuteDo();
    void ExecuteUndo();
}