using System.Windows.Forms;

namespace TextEditor;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    private MenuStrip menuStrip1;
    private StatusStrip statusStrip1;
    private ToolStripStatusLabel lblCursor;
    private ToolStripStatusLabel lblLines;
    private ToolStripMenuItem miOpen, miSave, miExit;
    private ToolStripMenuItem miUndo, miRedo, miCut, miCopy,
                              miPaste, miPasteTake, miDeleteSel, miClearDoc;
    private ToolStripMenuItem miHome, miEnd;
    private ToolStripMenuItem miPlugins;
    private ToolStrip toolStrip1;
    private ToolStripButton tsbUndo, tsbRedo, tsbCut, tsbCopy, tsbPaste;


    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        menuStrip1 = new MenuStrip();
        var miFile = new ToolStripMenuItem("File");
        miOpen = new ToolStripMenuItem("Open");
        miSave = new ToolStripMenuItem("Save");
        miExit = new ToolStripMenuItem("Exit");
        miFile.DropDownItems.AddRange(new ToolStripItem[]
        { miOpen, miSave, new ToolStripSeparator(), miExit });

        toolStrip1 = new ToolStrip();
        tsbUndo  = new ToolStripButton { Text = "Undo"  };
        tsbRedo  = new ToolStripButton { Text = "Redo"  };
        tsbCut   = new ToolStripButton { Text = "Cut"   };
        tsbCopy  = new ToolStripButton { Text = "Copy"  };
        tsbPaste = new ToolStripButton { Text = "Paste" };
        toolStrip1.Items.AddRange(new ToolStripItem[]
        { tsbUndo, tsbRedo, new ToolStripSeparator(),
            tsbCut, tsbCopy, tsbPaste });
        toolStrip1.Location = new System.Drawing.Point(0, 24);
        Controls.Add(toolStrip1);
        
        var miEdit = new ToolStripMenuItem("Edit");
        miUndo       = new ToolStripMenuItem("Undo",  null, null, Keys.Control | Keys.Z);
        miRedo       = new ToolStripMenuItem("Redo",  null, null, Keys.Control | Keys.Y);
        miCut        = new ToolStripMenuItem("Cut",   null, null, Keys.Control | Keys.X);
        miCopy       = new ToolStripMenuItem("Copy",  null, null, Keys.Control | Keys.C);
        miPaste      = new ToolStripMenuItem("Paste", null, null, Keys.Control | Keys.V);
        miPasteTake  = new ToolStripMenuItem("Paste and Take", null, null,
                                             Keys.Control | Keys.Shift | Keys.V);
        miDeleteSel  = new ToolStripMenuItem("Delete selection");
        miClearDoc   = new ToolStripMenuItem("Clear document");
        miEdit.DropDownItems.AddRange(new ToolStripItem[]
        {
            miUndo, miRedo, new ToolStripSeparator(),
            miCut, miCopy, miPaste, miPasteTake, new ToolStripSeparator(),
            miDeleteSel, miClearDoc
        });

        var miMove = new ToolStripMenuItem("Move");
        miHome = new ToolStripMenuItem("Cursor to document start");
        miEnd  = new ToolStripMenuItem("Cursor to document end");
        miMove.DropDownItems.AddRange(new ToolStripItem[] { miHome, miEnd });

        miPlugins = new ToolStripMenuItem("Plugins");
        menuStrip1.Items.Add(miPlugins);
        
        menuStrip1.Items.AddRange(new ToolStripItem[] { miFile, miEdit, miMove });
        menuStrip1.Location = new System.Drawing.Point(0, 0);
        menuStrip1.Size     = new System.Drawing.Size(800, 24);

        statusStrip1 = new StatusStrip();
        lblCursor = new ToolStripStatusLabel("Ln 1, Col 1");
        lblLines  = new ToolStripStatusLabel("Lines: 1");
        statusStrip1.Items.AddRange(new ToolStripItem[] { lblCursor, lblLines });
        statusStrip1.Location = new System.Drawing.Point(0, 428);
        statusStrip1.Size     = new System.Drawing.Size(800, 22);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize    = new System.Drawing.Size(800, 450);
        Controls.Add(statusStrip1);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Text = "Text Editor";

        ResumeLayout(false);
        PerformLayout();
    }
}
