using TextEditor.Models;

namespace TextEditor;

public interface ICursorObserver
{
    void UpdateCursorLocation(Location loc);
}