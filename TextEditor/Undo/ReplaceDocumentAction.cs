using TextEditor.Models;

namespace TextEditor.Undo;

public class ReplaceDocumentAction : EditAction
{
    private readonly TextEditorModel model;
    private readonly string before;
    private readonly string after;

    public ReplaceDocumentAction(TextEditorModel model,
        string before, string after)
    {
        this.model = model;
        this.before = before;
        this.after  = after;
    }

    public void ExecuteDo()   => model.ReplaceDocument(after);
    public void ExecuteUndo() => model.ReplaceDocument(before);
}