using TextEditor.Models;

namespace TextEditor.Undo;

public class InsertTextAction : EditAction
{
    private readonly TextEditorModel model;
    private readonly string text;
    private Location end;

    private Location start;

    public InsertTextAction(TextEditorModel model, string text)
    {
        this.model = model;
        this.text = text;
    }

    public void ExecuteDo()
    {
        start = model.CursorLocation;
        model.Insert(text);
        end = model.CursorLocation;
    }

    public void ExecuteUndo()
    {
        model.DeleteRange(new LocationRange(start, end));
    }
}