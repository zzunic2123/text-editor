namespace TextEditor;

public class ClipboardStack
{
    private readonly List<IClipboardObserver> observers = new();
    private readonly Stack<string> texts = new();

    #region Operations

    public void Push(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        texts.Push(text);
        Notify();
    }

    public string? Pop()
    {
        if (texts.Count == 0) return null;
        var t = texts.Pop();
        Notify();
        return t;
    }

    public string? Peek()
    {
        return texts.Count == 0 ? null : texts.Peek();
    }

    public bool Any()
    {
        return texts.Count > 0;
    }

    public void Clear()
    {
        if (texts.Count == 0) return;
        texts.Clear();
        Notify();
    }

    #endregion

    #region ClipboardObserver

    public void AddClipboardObserver(IClipboardObserver obs)
    {
        if (!observers.Contains(obs)) observers.Add(obs);
    }

    public void RemoveClipboardObserver(IClipboardObserver obs)
    {
        observers.Remove(obs);
    }

    private void Notify()
    {
        foreach (var obs in observers) obs.UpdateClipboard();
    }

    #endregion
}