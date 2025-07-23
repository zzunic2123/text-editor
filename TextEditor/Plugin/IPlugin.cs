using TextEditor.Models;
using TextEditor.Undo;

namespace TextEditor;


public interface IPlugin
{
    string Name        { get; }
    string Description { get; }
    void   Execute(TextEditorModel model,
        UndoManager     undo,
        ClipboardStack  clip);
}
