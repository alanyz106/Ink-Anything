<div align="center">

<img src="Ink%20Anything/Resources/Ink%20Anything.png?raw=true" alt="LOGO" width="256" height="256"/>

# Ink-Anything

A lightweight digital whiteboard in WPF/C#, with text input and deep optimization for Seewo Boards and PowerPoint presentations.

</div>

[中文文档](README.md) | English

## Features

- **Deep Microsoft PowerPoint integration**: Auto-switches to whiteboard mode during slideshow, page navigation, ink/text auto-save, hidden slide detection, playback position memory
- **Three modes**: Slideshow mode (auto-save ink and text), Whiteboard/Blackboard mode (up to 99 pages), Screen Pen mode
- **Active Pen / pressure sensitivity support**, fine nib for writing, flip for eraser
- **Multi-touch gestures**: Two-finger zoom/pan/rotate, finger touch erasing
- **Multi-page whiteboard** with independent ink and text data per page
- **Text input**: Add text anywhere on canvas, drag to move, resize handles, color changes, undo/redo, Ctrl+multi-select, Ctrl+drag to clone
- **Hand-drawn shape recognition**: Auto-detects circles, triangles, rectangles and converts to clean shapes; supports concentric/tangent circles and cross-section recognition
- **Shape drawing tools**: Lines/arrows/dashed lines, ellipse/hyperbola/parabola (with optional foci), circle/dashed circle, cylinder/cone/cuboid, 2D/3D coordinate systems, parallel lines
- **Selection mode separation**: Automatically distinguishes ink vs text selection scope; Alt+T toggles between them
- **Eraser 3-state toggle**: Stroke erase ↔ Partial erase ↔ Exit eraser, supports erasing text
- **Enhanced stroke selection**: Drag to move, Ctrl+drag to clone, 3-state select button (enter → select all → exit)
- **Simulated calligraphy**: Speed-based or tail-based brush effects
- **Settings page with tab layout**: Categorized tabs for all settings, built-in check-for-update button
- **Auto-update**: Fetches updates from GitHub Releases, silent install via Inno Setup
- **Floating toolbar**: Draggable, collapsible, lockable, with quick settings menu
- **Teaching tools**: Countdown timer, random name picker (importable lists)
- **Ink save/load/replay**, auto-save screenshots
- **Theme switching**: Dark / Light / Follow-system
- **Global hotkeys**: Alt+S/D/E/C/V/L/T/Q, Alt+1~6 for quick color switching, Ctrl+A/Z/Y
- **Hotkey conflict detection** on startup
- **Auto-start on boot**

## Keyboard Shortcuts

| Shortcut | Function |
|---|---|
| Alt+S | Toggle pen/mouse mode |
| Alt+D | Clear screen |
| Alt+E | Cycle eraser (stroke → partial → exit) |
| Alt+C | Screenshot |
| Alt+V | Show/hide floating toolbar |
| Alt+L | Draw straight line |
| Alt+T | Text input mode (toggles ink↔text selection scope when in selection mode) |
| Alt+Q | Toggle selection mode (toggles ink↔text selection scope when in selection mode) |
| Alt+1~6 | Switch pen color (black/red/green/blue/yellow/white) |
| Ctrl+A | Select all strokes |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Shift+Esc | Exit/end slideshow |
| Escape | Exit current mode |

## Requirements

- Windows 10 or later
- .NET Framework 4.7.2
- Microsoft Office (for PPT integration)

## User Manual

For detailed usage instructions, please see [User Manual (English)](Manual_EN.md).

## Acknowledgements

This project is based on [Ink Canvas](https://github.com/WXRIW/Ink-Canvas). Thanks to the original author and all contributors.

## Donation

If you find this project helpful, feel free to buy the author a coffee~

<img src="Ink%20Anything/Resources/alipay.jpg?raw=true" alt="Alipay" width="256"/>

<img src="Ink%20Anything/Resources/wxpay.png?raw=true" alt="WeChat Pay" width="256"/>

## License

GPL-3.0 License
