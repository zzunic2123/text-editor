using TextEditor.Models;
using TextEditor.Undo;

namespace TextEditor;


public class Statistics : IPlugin
{
    public string Name        => "Statistika";
    public string Description => "Broji redove, riječi i slova u dokumentu";

    public void Execute(TextEditorModel model,
        UndoManager _,
        ClipboardStack __)
    {
        int lines = model.Lines.Count;
        int words = model.Lines.Sum(l => l.Split(
            (char[])null, System.StringSplitOptions.RemoveEmptyEntries).Length);
        int chars = model.Lines.Sum(l => l.Length);
        MessageBox.Show($"Redaka: {lines}\nRiječi: {words}\nSlova: {chars}",
            "Statistika", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
