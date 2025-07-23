namespace TextEditor.Models;

public class TextEditorModel
{
    private readonly List<ICursorObserver> cursorObservers = new();
    private readonly List<ITextObserver> textObservers = new();

    public TextEditorModel(string initialText)
    {
        Lines = initialText.Split('\n').Select(line => line.Replace("\r", "")).ToList();
        CursorLocation = new Location(0, 0);
    }

    public List<string> Lines { get; }
    public Location CursorLocation { get; set; }
    public LocationRange? SelectionRange { get; set; }


    #region Iterators

    public IEnumerable<string> AllLines()
    {
        foreach (var line in Lines)
            yield return line;
    }

    public IEnumerable<string> LinesRange(int index1, int index2)
    {
        if (index1 < 0 || index1 >= Lines.Count) throw new IndexOutOfRangeException();
        for (var i = index1; i < index2 && i < Lines.Count; i++)
            yield return Lines[i];
    }

    #endregion

    #region CursorObservers

    public void AddCursorObserver(ICursorObserver observer)
    {
        if (!cursorObservers.Contains(observer))
            cursorObservers.Add(observer);
    }

    public void RemoveCursorObserver(ICursorObserver observer)
    {
        cursorObservers.Remove(observer);
    }

    private void NotifyCursorObservers()
    {
        foreach (var observer in cursorObservers) observer.UpdateCursorLocation(CursorLocation);
    }

    #endregion

    #region MoveCursor

    public void MoveCursorLeft()
    {
        var (line, col) = CursorLocation;

        if (col > 0)
            CursorLocation = new Location(line, col - 1);
        else if (line > 0)
            CursorLocation = new Location(line - 1, Lines[line - 1].Length);
        else
            return;

        NotifyCursorObservers();
    }

    public void MoveCursorRight()
    {
        var (line, col) = CursorLocation;
        if (col < Lines[line].Length)
            CursorLocation = new Location(line, col + 1);
        else if (line < Lines.Count - 1)
            CursorLocation = new Location(line + 1, 0);
        else
            return;

        NotifyCursorObservers();
    }

    public void MoveCursorUp()
    {
        var (line, col) = CursorLocation;
        if (line == 0) return;

        var newCol = Math.Min(col, Lines[line - 1].Length);
        CursorLocation = new Location(line - 1, newCol);
        NotifyCursorObservers();
    }

    public void MoveCursorDown()
    {
        var (line, col) = CursorLocation;
        if (line >= Lines.Count - 1) return;

        var newCol = Math.Min(col, Lines[line + 1].Length);
        CursorLocation = new Location(line + 1, newCol);
        NotifyCursorObservers();
    }

    public void SetCursorLocation(Location loc, bool clearSelection = true)
    {
        CursorLocation = loc;
        if (clearSelection) SelectionRange = null;
        NotifyCursorObservers();
    }

    #endregion

    #region TextObservers

    public void AddTextObserver(ITextObserver observer)
    {
        if (!textObservers.Contains(observer))
            textObservers.Add(observer);
    }

    public void RemoveTextObserver(ITextObserver observer)
    {
        textObservers.Remove(observer);
    }

    private void NotifyTextObservers()
    {
        foreach (var obs in textObservers)
            obs.UpdateText();
    }

    #endregion

    #region Selection

    public LocationRange? GetSelectionRange()
    {
        return SelectionRange;
    }

    public void SetSelectionRange(LocationRange? range)
    {
        SelectionRange = range;
        NotifyCursorObservers();
        NotifyTextObservers();
    }

    #endregion

    #region Delete

    public void DeleteBefore()
    {
        if (SelectionRange != null)
        {
            DeleteRange(SelectionRange);
            return;
        }

        var line = CursorLocation.Line;
        var col = CursorLocation.Column;

        if (line == 0 && col == 0) return;

        if (col > 0)
        {
            var l = Lines[line];
            Lines[line] = l.Remove(col - 1, 1);
            CursorLocation = new Location(line, col - 1);
        }
        else
        {
            var prevLen = Lines[line - 1].Length;
            Lines[line - 1] += Lines[line];
            Lines.RemoveAt(line);
            CursorLocation = new Location(line - 1, prevLen);
        }

        NotifyCursorObservers();
        NotifyTextObservers();
    }

    public void DeleteAfter()
    {
        if (SelectionRange != null)
        {
            DeleteRange(SelectionRange);
            return;
        }

        var line = CursorLocation.Line;
        var col = CursorLocation.Column;

        if (line == Lines.Count - 1 && col == Lines[line].Length) return;

        if (col < Lines[line].Length)
        {
            Lines[line] = Lines[line].Remove(col, 1);
        }
        else
        {
            Lines[line] += Lines[line + 1];
            Lines.RemoveAt(line + 1);
        }

        NotifyTextObservers();
    }

    public void DeleteRange(LocationRange range)
    {
        var (start, end) = (range.Start, range.End);
        if (start.Line > end.Line || (start.Line == end.Line && start.Column > end.Column))
            (start, end) = (end, start);

        if (start.Line == end.Line)
        {
            Lines[start.Line] = Lines[start.Line].Remove(
                start.Column,
                end.Column - start.Column);
        }
        else
        {
            Lines[start.Line] = Lines[start.Line][..start.Column] +
                                Lines[end.Line][end.Column..];
            foreach (var idx in Enumerable.Range(start.Line + 1, end.Line - start.Line))
                Lines.RemoveAt(start.Line + 1);
        }

        SelectionRange = null;
        CursorLocation = new Location(start.Line, start.Column);

        NotifyCursorObservers();
        NotifyTextObservers();
    }

    #endregion

    #region Insert

    public void Insert(char c)
    {
        if (SelectionRange != null)
            DeleteRange(SelectionRange);

        var (line, col) = CursorLocation;

        if (c == '\n')
        {
            var before = Lines[line][..col];
            var after = Lines[line][col..];

            Lines[line] = before;
            Lines.Insert(line + 1, after);

            CursorLocation = new Location(line + 1, 0);
        }
        else
        {
            Lines[line] = Lines[line].Insert(col, c.ToString());
            CursorLocation = new Location(line, col + 1);
        }

        SelectionRange = null;
        NotifyCursorObservers();
        NotifyTextObservers();
    }

    public void Insert(string text)
    {
        foreach (var c in text)
            Insert(c);
    }

    #endregion

    #region DocumentOperations

    public void ReplaceDocument(string text)
    {
        Lines.Clear();
        Lines.AddRange(text.Split('\n').Select(l => l.Replace("\r", "")));
        CursorLocation = new Location(0, 0);
        SelectionRange = null;

        NotifyCursorObservers();
        NotifyTextObservers();
    }

    public string GetDocumentText()
    {
        return string.Join("\n", Lines);
    }

    #endregion
}