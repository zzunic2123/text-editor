using System.Text;
using TextEditor.Models;

namespace TextEditor.Undo;

public class DeleteRangeAction : EditAction
{
    private readonly TextEditorModel model;
    private readonly LocationRange range;
    private string deletedText = "";

    public DeleteRangeAction(TextEditorModel model, LocationRange range)
    {
        this.model = model;
        this.range = range;
    }

    public void ExecuteDo()
    {
        deletedText = Extract(range);
        model.DeleteRange(range);
    }

    public void ExecuteUndo()
    {
        model.CursorLocation = range.Start;
        model.Insert(deletedText);
    }

    private string Extract(LocationRange r)
    {
        var (s, e) = (r.Start, r.End);
        if (s.Line > e.Line || (s.Line == e.Line && s.Column > e.Column))
            (s, e) = (e, s);

        if (s.Line == e.Line)
            return model.Lines[s.Line].Substring(s.Column, e.Column - s.Column);

        var sb = new StringBuilder();
        sb.Append(model.Lines[s.Line][s.Column..]).Append('\n');
        for (var i = s.Line + 1; i < e.Line; i++)
            sb.Append(model.Lines[i]).Append('\n');
        sb.Append(model.Lines[e.Line][..e.Column]);
        return sb.ToString();
    }
}