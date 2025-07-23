using System.Text.RegularExpressions;
using TextEditor.Models;
using TextEditor.Undo;

namespace TextEditor;

public class CapitalLetter : IPlugin
{
    public string Name        => "VelikoSlovo";
    public string Description => "Svako prvo slovo riječi u veliko";

    public void Execute(TextEditorModel model,
        UndoManager undo,
        ClipboardStack __)
    {
        string before = model.GetDocumentText();
        string after  = string.Join("\n", model.Lines.Select(
            l => Regex.Replace(l, @"\b\p{L}",
                m => m.Value.ToUpper())));
        undo.Push(new ReplaceDocumentAction(model, before, after));
    }
}