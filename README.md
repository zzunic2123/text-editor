# Design Patterns Laboratory Exercise 3 – Text Editor  
*C# WinForms implementation*

## ✨ Project Overview
This repository contains **a fully custom multi‑line text editor** written from
scratch for the “Design Patterns” course (FER, 2025).  
The control is **not** based on `TextBox`/`RichTextBox`; instead it draws text
with `Graphics` primitives, manages its own caret, selection, clipboard stack,
undo/redo logic and a plug‑in system – all while showcasing classic design
patterns.

/
└─ TextEditor/
├─ Editor/ # core model–view–controller
├─ Plugins/ # compiled & discovered at runtime
├─ Resources/ # icons, sample docs
└─ Editor.csproj # .NET 8 project file

##  Quick Start

1.  **Requirements**  
    * .NET SDK 8.0+ (Windows, x64/x86/arm64)  
    * No third‑party NuGet packages needed

2.  **Build & Run**

        cd TextEditor
        dotnet build -c Release
        dotnet run   -c Release

   The editor window appears with menu bar, tool‑bar and status bar.

3.  **Keyboard Shortcuts**

   | Action | Shortcut |
   |--------|----------|
   | Move caret | ← → ↑ ↓ |
   | Select text | *Shift* + arrows |
   | Cut / Copy / Paste | Ctrl+X / Ctrl+C / Ctrl+V |
   | Paste‑and‑Take | Ctrl+Shift+V |
   | Undo / Redo | Ctrl+Z / Ctrl+Y |
   | Delete ← / → | Backspace / Delete |
   | New line | Enter |

---

##  Features
* **Custom rendering** – lines drawn via `Graphics.DrawString`, caret as a
  1‑pixel line; double‑buffered to avoid flicker  
* **Selection painting** – blue background rectangle behind selected glyphs  
* **Unlimited undo/redo** – `UndoManager` stacks `EditAction` commands  
* **Stack‑based clipboard** – multiple clips, “paste and take” pops the stack  
* **Dynamic plug‑ins** – DLLs implementing `IPlugin` are loaded at start and
  inserted under **Plugins** menu (e.g. *Statistics*, *Capitalize*)  
* **Status bar** – live caret row/column and total line count  
* **Toolbar & menus** – enable/disable themselves according to model state  
* **100 % managed C#** – runs anywhere .NET 8 runs (Windows 10/11)  

---

## Design Patterns Implemented
Pattern | Where & Why
--------|------------
**Observer (Publisher–Subscriber)** | `CursorObserver`, `TextObserver`, `ClipboardObserver` keep UI in sync without polling
**Command** | Each edit (`InsertText`, `DeleteRange`…) is an `EditAction` enabling uniform **undo/redo**
**Singleton** | `UndoManager` and `ClipboardStack` exposed via single global instances
**Template Method** | Override of `OnPaint` lets WinForms framework call our drawing code
**Strategy / Plug‑in** | Editor behavior extended at runtime by loading DLLs that share `IPlugin` interface

---
