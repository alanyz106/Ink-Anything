<div align="center">

<img src="Ink%20Anything/Resources/Ink%20Anything.png?raw=true" alt="LOGO" width="256" height="256"/>

# Ink-Anything

A lightweight digital whiteboard in WPF/C#, with text input and deep optimization for Seewo Boards and PowerPoint presentations.

</div>

[中文文档](README.md) | English

## Features

- Optimized support for Microsoft PowerPoint — auto-switches to whiteboard mode during slideshow, with page navigation and ink/text saving
- Active Pen support (pressure sensitivity)
- Fine nib for writing, flip the pen to use the eraser (not supported by Seewo Whiteboard 5)
- Finger touch erasing
- Multi-touch gesture support (zoom, pan, rotate)
- Multi-page whiteboard management with unlimited canvas
- Hand-drawn shape recognition (circle, rectangle, triangle, etc.) with auto-conversion to clean shapes
- **Text Input**: Add text anywhere on the canvas — drag to move, resize handles, color changes, undo/redo, auto-saved in whiteboard and PPT modes; supports **Ctrl+click multi-select** and **Ctrl+drag to clone** text elements
- **Eraser 3-state toggle**: Stroke erase ↔ Partial erase ↔ Exit eraser, supports erasing text elements
- **Enhanced stroke selection**: Drag to move selected strokes, Ctrl+drag to clone, click outside to deselect; select button supports 3-state toggle (enter → select all → exit)
- Floating toolbar with lock/unlock position
- Countdown timer, random name picker, and other teaching tools
- Screenshot with auto-save
- Ink save and load
- Ink replay
- Dark / Light / Follow-system theme
- Auto-start on boot
- Global hotkeys (Alt+S/D/E/C/V/L/T/Q, Alt+1~6 for quick color switching, Ctrl+A select all)
- Hotkey conflict detection on startup with notification
- Works with other infrared touch screens

## Keyboard Shortcuts

| Shortcut | Function |
|---|---|
| Alt+S | Toggle pen/mouse mode |
| Alt+D | Clear screen |
| Alt+E | Cycle eraser (stroke → partial → exit) |
| Alt+C | Screenshot |
| Alt+V | Show/hide floating toolbar |
| Alt+L | Draw straight line |
| Alt+T | Text input mode |
| Alt+Q | Toggle selection mode |
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
