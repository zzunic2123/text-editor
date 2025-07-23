using System.Text;
using TextEditor.Models;
using TextEditor.Undo;

namespace TextEditor;

public class TextEditor : Control, ICursorObserver, ITextObserver
{
    private const int LeftPadding = 5;
    private readonly int charWidth;
    private readonly ClipboardStack clipboard;
    private readonly Font font = new("Consolas", 12);
    private readonly int lineHeight;
    private readonly TextEditorModel model;
    private Location cursorLocation;
    private Location? selectionAnchor;


    public TextEditor(TextEditorModel model, ClipboardStack? clipboard = null)
    {
        this.model = model;
        this.clipboard = clipboard ?? new ClipboardStack();

        cursorLocation = model.CursorLocation;

        lineHeight = font.Height;
        charWidth = TextRenderer.MeasureText(
            "W",
            font,
            new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.NoPadding).Width;

        model.AddCursorObserver(this);
        model.AddTextObserver(this);

        DoubleBuffered = true;
        BackColor = Color.White;
        TabStop = true;
        Size = new Size(1000, 1000);
    }


    #region Helpers

    private int GetTextWidth(string text)
    {
        return text.Length * charWidth;
    }

    #endregion

    #region Paint

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        PaintSelectionBackground(g);

        var y = 0;
        foreach (var line in model.AllLines())
        {
            TextRenderer.DrawText(
                g,
                line,
                font,
                new Point(LeftPadding, y),
                Color.Black,
                TextFormatFlags.NoPadding);
            y += lineHeight;
        }

        PaintCaret(g);
    }

    private void PaintSelectionBackground(Graphics g)
    {
        var sel = model.GetSelectionRange();
        if (sel == null) return;

        var (start, end) = (sel.Start, sel.End);
        if (start.Line > end.Line || (start.Line == end.Line && start.Column > end.Column))
            (start, end) = (end, start);

        using var selBrush = new SolidBrush(Color.LightBlue);

        foreach (var (line, idx) in model.LinesRange(start.Line, end.Line + 1)
                     .Select((l, i) => (l, i + start.Line)))
        {
            var fromCol = idx == start.Line ? start.Column : 0;
            var toCol = idx == end.Line ? end.Column : line.Length;
            if (fromCol == toCol) continue;

            var x1 = LeftPadding + GetTextWidth(line[..fromCol]);
            var x2 = LeftPadding + GetTextWidth(line[..toCol]);
            var y = idx * lineHeight;
            g.FillRectangle(selBrush, x1, y, x2 - x1, lineHeight);
        }
    }

    private void PaintCaret(Graphics g)
    {
        if (cursorLocation.Line >= model.Lines.Count) return;

        var currLine = model.Lines[cursorLocation.Line];
        var x = LeftPadding + GetTextWidth(currLine[..cursorLocation.Column]);
        var y = cursorLocation.Line * lineHeight;
        g.DrawLine(Pens.Black, x, y, x, y + lineHeight);
    }

    #endregion

    #region Keyboard handling

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Right:
            case Keys.Up:
            case Keys.Down:
                HandleCursorMove(e.KeyCode, e.Shift);
                e.Handled = true;
                break;

            case Keys.Back:
                DeleteBeforeOrSelection();
                selectionAnchor = null;
                e.Handled = true;
                break;

            case Keys.Delete:
                DeleteAfterOrSelection();
                selectionAnchor = null;
                e.Handled = true;
                break;

            case Keys.Enter:
                UndoManager.Instance.Push(new InsertTextAction(model, "\n"));
                selectionAnchor = null;
                e.Handled = true;
                break;

            case Keys.C:
                CopySelection();
                e.Handled = true;
                return;

            case Keys.X:
                CutSelection();
                e.Handled = true;
                return;

            case Keys.V:
                if (e.Shift)
                    PasteAndPop();
                else
                    Paste();
                e.Handled = true;
                return;
            case Keys.Z:
                UndoManager.Instance.Undo();
                e.Handled = true;
                return;

            case Keys.Y:
                UndoManager.Instance.Redo();
                e.Handled = true;
                return;
        }
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);

        // sve znakove osim kontrolnih
        if (!char.IsControl(e.KeyChar) || e.KeyChar == '\n' || e.KeyChar == '\t')
        {
            UndoManager.Instance.Push(new InsertTextAction(model, e.KeyChar.ToString()));
            selectionAnchor = null;
            e.Handled = true;
        }
    }
    
    protected override bool IsInputKey(Keys keyData)
    {
        switch (keyData & Keys.KeyCode)
        {
            case Keys.Left:
            case Keys.Right:
            case Keys.Up:
            case Keys.Down:
            case Keys.Back:
            case Keys.Delete:
            case Keys.C:
            case Keys.X:
            case Keys.V:
            case Keys.Enter:
                return true;
            default:
                return base.IsInputKey(keyData);
        }
    }

    private void HandleCursorMove(Keys key, bool shiftHeld)
    {
        var oldLoc = model.CursorLocation;

        if (shiftHeld && selectionAnchor == null)
            selectionAnchor = oldLoc;

        switch (key)
        {
            case Keys.Left:
                model.MoveCursorLeft();
                break;
            case Keys.Right:
                model.MoveCursorRight();
                break;
            case Keys.Up:
                model.MoveCursorUp();
                break;
            case Keys.Down:
                model.MoveCursorDown();
                break;
        }

        if (shiftHeld)
        {
            var anchor = selectionAnchor ?? oldLoc;
            model.SetSelectionRange(new LocationRange(anchor, model.CursorLocation));
        }
        else
        {
            selectionAnchor = null;
            model.SetSelectionRange(null);
        }
    }

    #endregion

    #region Observer callbacks

    public void UpdateText()
    {
        Invalidate();
    }

    public void UpdateCursorLocation(Location loc)
    {
        cursorLocation = loc;
        Invalidate(); // this measn redraw
    }

    #endregion

    #region ClipboardOperations

    private void CopySelection()
    {
        var sel = model.GetSelectionRange();
        if (sel != null)
            clipboard.Push(GetText(sel));
    }

    private void CutSelection()
    {
        var sel = model.GetSelectionRange();
        if (sel == null) return;

        clipboard.Push(GetText(sel));
        model.DeleteRange(sel);
        selectionAnchor = null;
    }

    private void Paste()
    {
        if (!clipboard.Any()) return;
        InsertClipboardText(clipboard.Peek()!);
    }

    private void PasteAndPop()
    {
        var txt = clipboard.Pop();
        if (txt != null)
            InsertClipboardText(txt);
    }

    private void InsertClipboardText(string text)
    {
        if (model.SelectionRange != null)
            UndoManager.Instance.Push(new DeleteRangeAction(model, model.SelectionRange));

        UndoManager.Instance.Push(new InsertTextAction(model, text));
        selectionAnchor = null;
    }

    private string GetText(LocationRange range)
    {
        var (start, end) = (range.Start, range.End);
        if (start.Line > end.Line || (start.Line == end.Line && start.Column > end.Column))
            (start, end) = (end, start);

        if (start.Line == end.Line)
            return model.Lines[start.Line].Substring(start.Column, end.Column - start.Column);

        var sb = new StringBuilder();
        foreach (var (line, idx) in model.LinesRange(start.Line, end.Line + 1)
                     .Select((l, i) => (l, i + start.Line)))
        {
            if (idx == start.Line) sb.Append(line[start.Column..]).Append('\n');
            else if (idx == end.Line) sb.Append(line[..end.Column]);
            else sb.Append(line).Append('\n');
        }
        return sb.ToString();
    }

    #endregion

    #region DeleteHelpersUndo

    private void DeleteBeforeOrSelection()
    {
        if (model.SelectionRange != null)
        {
            UndoManager.Instance.Push(
                new DeleteRangeAction(model, model.SelectionRange));
            return;
        }

        var loc = model.CursorLocation;
        if (loc.Line == 0 && loc.Column == 0) return;

        Location start, end = loc;

        if (loc.Column > 0)
            start = new Location(loc.Line, loc.Column - 1);
        else
            start = new Location(loc.Line - 1, model.Lines[loc.Line - 1].Length);

        UndoManager.Instance.Push(
            new DeleteRangeAction(model, new LocationRange(start, end)));
    }

    private void DeleteAfterOrSelection()
    {
        if (model.SelectionRange != null)
        {
            UndoManager.Instance.Push(
                new DeleteRangeAction(model, model.SelectionRange));
            return;
        }

        var loc = model.CursorLocation;
        if (loc.Line == model.Lines.Count - 1 &&
            loc.Column == model.Lines[loc.Line].Length) return;

        Location start = loc, end;

        if (loc.Column < model.Lines[loc.Line].Length)
            end = new Location(loc.Line, loc.Column + 1);
        else
            end = new Location(loc.Line + 1, 0);

        UndoManager.Instance.Push(
            new DeleteRangeAction(model, new LocationRange(start, end)));
    }

    #endregion

    #region Public Helpers

    public void InvokeCut()
    {
        CutSelection();
    }

    public void InvokeCopy()
    {
        CopySelection();
    }

    public void InvokePaste(bool take)
    {
        if (take) PasteAndPop();
        else Paste();
    }

    public void InvokeDeleteSelection()
    {
        if (model.SelectionRange != null)
            UndoManager.Instance.Push(
                new DeleteRangeAction(model, model.SelectionRange));
    }

    public void InvokeClearDocument()
    {
        UndoManager.Instance.Push(
            new DeleteRangeAction(model,
                new LocationRange(new Location(0, 0),
                    new Location(model.Lines.Count - 1,
                        model.Lines[^1].Length))));
    }

    #endregion
}