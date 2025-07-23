using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TextEditor.Models;
using TextEditor.Undo;

namespace TextEditor;

public partial class Form1 : Form,
                              IUndoObserver,
                              IClipboardObserver,
                              ICursorObserver,
                              ITextObserver
{
    private readonly ClipboardStack  clipboard;
    private readonly TextEditorModel model;
    private readonly TextEditor      editor;
    private readonly List<IPlugin> plugins = new();

    public Form1()
    {
        InitializeComponent();

        clipboard = new ClipboardStack();
        model     = new TextEditorModel("Prvi \nDrugi \nTreći ");
        editor    = new TextEditor(model, clipboard) { Dock = DockStyle.Fill };
        Controls.Add(editor);
        Controls.SetChildIndex(editor, 0);

        clipboard.AddClipboardObserver(this);
        UndoManager.Instance.AddObserver(this);
        model.AddCursorObserver(this);
        model.AddTextObserver(this);

        WireHandlers();
        UpdateUI();
        UpdateStatus();
        LoadPlugins();
    }

    private void WireHandlers()
    {
        miExit.Click      += (_,__) => Close();
        miOpen.Click      += (_,__) => DoOpen();
        miSave.Click      += (_,__) => DoSave();
        miUndo.Click      += (_,__) => UndoManager.Instance.Undo();
        miRedo.Click      += (_,__) => UndoManager.Instance.Redo();
        miCut.Click       += (_,__) => editor.InvokeCut();
        miCopy.Click      += (_,__) => editor.InvokeCopy();
        miPaste.Click     += (_,__) => editor.InvokePaste(false);
        miPasteTake.Click += (_,__) => editor.InvokePaste(true);
        miDeleteSel.Click += (_,__) => editor.InvokeDeleteSelection();
        miClearDoc.Click  += (_,__) => editor.InvokeClearDocument();
        miHome.Click      += (_,__) => model.SetCursorLocation(new Location(0,0));
        miEnd.Click       += (_,__) =>
        {
            int last = model.Lines.Count-1;
            model.SetCursorLocation(new Location(last, model.Lines[last].Length));
        };
        tsbUndo.Click  += (_,__) => miUndo.PerformClick();
        tsbRedo.Click  += (_,__) => miRedo.PerformClick();
        tsbCut.Click   += (_,__) => miCut.PerformClick();
        tsbCopy.Click  += (_,__) => miCopy.PerformClick();
        tsbPaste.Click += (_,__) => miPaste.PerformClick();

    }

    public void UpdateClipboard()                { UpdateUI(); }
    public void UpdateCursorLocation(Location _) { UpdateUI(); UpdateStatus(); }
    public void UpdateUndoRedo(bool _, bool __)  { UpdateUI(); }
    public void UpdateText()                     { UpdateStatus(); }

    private void UpdateUI()
    {
        bool hasSel   = model.SelectionRange != null;
        bool canUndo  = UndoManager.Instance.CanUndo;
        bool canRedo  = UndoManager.Instance.CanRedo;
        bool canPaste = clipboard.Any();

        miUndo.Enabled = canUndo;
        miRedo.Enabled = canRedo;
        miCut.Enabled  =
        miCopy.Enabled =
        miDeleteSel.Enabled = hasSel;
        miPaste.Enabled     =
        miPasteTake.Enabled = canPaste;
        
        miUndo.Enabled = tsbUndo.Enabled = canUndo;
        miRedo.Enabled = tsbRedo.Enabled = canRedo;
        miCut.Enabled  = tsbCut.Enabled  =
            miCopy.Enabled = tsbCopy.Enabled =
                miDeleteSel.Enabled             = hasSel;
        miPaste.Enabled = tsbPaste.Enabled = canPaste;
        miPasteTake.Enabled = canPaste;

    }

    private void UpdateStatus()
    {
        int line = model.CursorLocation.Line + 1;
        int col  = model.CursorLocation.Column + 1;
        lblCursor.Text = $"Ln {line}, Col {col}";
        lblLines.Text  = $"Lines: {model.Lines.Count}";
    }

    private void DoOpen()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title  = "Open file"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        string txt = File.ReadAllText(dlg.FileName);
        model.ReplaceDocument(txt);
        UndoManager.Instance.Clear();
    }

    private void DoSave()
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title  = "Save file"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(dlg.FileName, model.GetDocumentText());
    }
    private void LoadPlugins()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        if (!Directory.Exists(dir)) return;

        foreach (var dll in Directory.EnumerateFiles(dir, "*.dll"))
        {
            try
            {
                var asm         = Assembly.LoadFrom(dll);
                var pluginTypes = asm.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var type in pluginTypes)
                {
                    var plugin = (IPlugin)Activator.CreateInstance(type)!;
                    plugins.Add(plugin);

                    var item = new ToolStripMenuItem(plugin.Name)
                        { ToolTipText = plugin.Description };
                    item.Click += (_, __) =>
                        plugin.Execute(model, UndoManager.Instance, clipboard);
                    miPlugins.DropDownItems.Add(item);
                }
            }
            catch { }
        }
    }

}
