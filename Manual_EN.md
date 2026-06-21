# Ink Anything User Manual

## Table of Contents
[1. Introduction](#intro)  
[2. Usage](#usage)  
[3. Keyboard Shortcuts](#hotkey)  
[4. Tips](#skill)

## What is Ink Anything? <span id='intro'></span>

Ink Anything is a lightweight digital whiteboard application optimized for Seewo interactive displays. Compared to the pre-installed "Seewo Whiteboard 5", it offers 5-10x faster startup, lower resource usage, and a smoother experience.

It supports two-finger zoom, drag, and rotate gestures, and is deeply integrated with Microsoft PowerPoint — allowing you to open a whiteboard or blackboard during slideshow for annotation, improving classroom efficiency. It features intelligent ink recognition that can identify circles and other shapes, automatically converting them to clean graphics.

The unified ink writing experience across slideshow and whiteboard/blackboard modes builds consistent operation habits.

### Ink Anything Modes

* **Slideshow Mode**
    * Supports pen, eraser, shape tools, text input, quick page navigation, and automatic pen change on page switch
    * Automatically saves ink and text from each slide, restored on next open, editable at any time
    * Prompts to unhide hidden slides if detected
* **Whiteboard/Blackboard Mode**
    * A full-size canvas similar to Seewo Whiteboard
    * Supports up to 99 pages with add/delete/navigate
    * Each page independently saves ink and text data
    * Multi-pen writing: tap the person icon at the bottom-left corner to switch
    * Default is blackboard, can be changed to whiteboard in settings
* **Screen Pen Mode**
    * Shows the original screen content while turning the cursor into a pen for annotation

### Ink Anything Highlights

* Fine nib for writing, flip the pen to use the eraser (stroke-based erasing) — no manual mode switching needed
* Use a finger or tilt the pen tip to precisely erase small areas
* Place your palm flat on the screen for large-area erasing — faster and more accurate than Seewo Whiteboard 5
* Fast startup
* Convenient pen color switching and screen clearing
* Auto-kills certain Seewo background processes
* Select and transform strokes: zoom, rotate, move, clone
* Two-finger gesture zoom/rotate/pan across the full canvas
* **Eraser 3-state toggle**: Stroke erase ↔ Partial erase ↔ Exit eraser
* **Text Input**: Add text anywhere on the canvas with drag, resize, color, undo/redo support; Ctrl+click to multi-select, Ctrl+drag to clone text
* **Enhanced Stroke Selection**: Drag to move selected strokes, Ctrl+drag to clone; select button 3-state toggle (enter → select all → exit), icon color indicator (deep blue = all selected, light blue = partial)
* **Hotkey Conflict Detection**: Auto-detects and notifies on startup if hotkeys conflict with other software
* Shape drawing (long-press to keep selected):
  Straight line, dashed line, arrow line, parallel lines
  Ellipse (with/without foci), hyperbola, parabola
  Circle, dashed circle, cylinder, cone, cuboid
  Coordinate systems (2D rectangular, 3D rectangular)
* Ink-to-shape: intelligent recognition of circles, triangles, special quadrilaterals
  Auto-converts to clean shapes. Recognizes concentric and tangent circles, cross-sections of spheres
* **Simulated pen calligraphy**: Speed-based or tail-based brush effects for natural handwriting

### Ink Anything Features
* **Countdown Timer**: Beautiful UI, supports near-fullscreen display
* **Random Name Picker**: Import name lists (works well with Excel), configurable number of picks
* **Save Ink**: Default save location is `Documents\Ink Anything Strokes`
* **Screenshot**: Tap the camera icon in any mode (including mouse mode) to capture and auto-save to `Pictures\Ink Anything Screenshots`. Can auto-save ink on screenshot in settings
* **PPT Auto-save Ink & Text**: Default save location is `Documents\Ink Anything Strokes`
* **Ink Replay**: Automatically replays all ink strokes on the canvas
* **Theme Switching**: Light, dark, and follow-system themes


## Usage <span id='usage'></span>

### Pen Settings
* **Color**: In pen mode, tap the color balls on the main interface, or use Alt+1~6 for quick switching
* **Width**: Tap the gear icon (Settings), go to "Canvas" → "Pen Width" to adjust (default: 5)
* **Calligraphy**: In Settings, choose "Speed" or "Tail" based calligraphy for more natural handwriting

### Eraser Usage
* **Stroke Erase**: Tap once to turn the icon gray — erases entire strokes
* **Partial Erase**: Tap again to turn the icon blue — erases within the indicated area
* **Exit Eraser**: Tap once more to return to pen mode
* **Palm Erase**: In pen mode, press your palm on the screen for erasing (not supported on all screens)
* The eraser also supports erasing text elements

### Text Input
* **Enter text mode**: Tap the text icon on the floating toolbar, or press Alt+T
* **Add text**: Left-click anywhere on the canvas, a text box appears — start typing
* **Submit text**: Press Enter, or click elsewhere to auto-submit
* **Line break**: Press Shift+Enter while editing to insert a line break
* **Edit text**: In text mode, double-click a submitted text element to re-edit (or right-click)
* **Move text**: In text mode, left-click and drag a submitted text element to move it
* **Multi-select text**: Ctrl+click text elements to select multiple
* **Clone text by drag**: After multi-selecting, Ctrl+drag to clone all selected text to a new position
* **Resize text**: Click a text element to show resize handles at the corners — drag to adjust size (font size range: 8–200)
* **Delete text**: Select then press Delete or Backspace, or use the eraser to erase
* **Text color**: Follows the current pen color
* **Text size**: Adjustable in the quick settings panel (range: 12–72, default: 24), or fine-tune via resize handles
* **Text cursor**: Switchable between arrow and I-beam cursor in settings
* **Exit text mode**: Tap the text icon again, press Alt+T, or press Escape

### Drawing Preset Shapes
* **Circle**: Click at the "Origin" position to set the center, then drag to draw
* **Parabola**: Click at the "Origin" position, then drag
* **Hyperbola**: Click and drag at the "Origin" to set asymptotes, then set the real and imaginary axes
* **Ellipse**: Click and drag at the "Origin", then set the major and minor axes (option to show/hide foci)
* **Cuboid**: First draw the front face, then click again to set the depth
* **Cone & Cylinder**: Draw in one smooth motion
* **2D Rectangular Coordinate System**: Multiple presets available
* **3D Rectangular Coordinate System**: Draws all three axes simultaneously
* **Parallel Lines**: Draw from one end to the other, optimized for specific angles

### Selection Operations
* Tap the select button to enter selection mode, then drag to select multiple strokes
* **Select button 3-state toggle**: First tap enters selection mode → second tap selects all → third tap exits selection mode
* The floating toolbar shows operation buttons: Clone, Clone to New Page, Rotate (45°/90°), Flip (horizontal/vertical), Stroke Width Adjust (enlarge/shrink/restore), Delete
* **Drag to move**: After selecting strokes, drag to move them
* **Ctrl+drag to clone**: Hold Ctrl and drag to copy selected strokes to a new position
* **Deselect**: Click outside the selected area to deselect
* Ctrl+A to select all strokes
* Delete/Backspace to delete selected strokes
* Supports two-finger gesture operations on selected strokes (zoom, move, rotate)
* Selection icon color: deep blue = all selected, light blue = partial selection

### PPT Slideshow Mode
* **Page Navigation**: When no ink is on screen, swipe with multiple fingers, use on-screen controls, or keyboard arrow keys
* **Save**: Ink and text from PPT are auto-saved; you can move the stroke folder to another computer and reuse the annotations
* The built-in whiteboard in PPT mode works the same as the standalone whiteboard
* Detects hidden slides and prompts to unhide
* Remembers last playback position and offers to jump back

### Auto Ink Recognition (Ink To Shape)
1. Draw as neatly and accurately as possible
2. Draw an ellipse inside a circle to recognize cross-sections and concentric ellipses (useful for orbital diagrams)
3. Automatically recognizes concentric and tangent circles (internal and external tangency)
4. Recognizes: circle, ellipse, triangle, rectangle, rhombus, parallelogram, square
5. Note: Only available in 32-bit mode; automatically hidden in 64-bit

### Floating Toolbar
* Draggable to any position on screen
* Collapsible/expandable (Alt+V or tap the emoji icon)
* Supports position locking to prevent accidental moves
* Quick settings popup: pen width, text size, ink-to-shape toggle, finger mode toggle, ink save/load/replay, countdown timer, random name picker

### Theme Switching
* Supports light, dark, and follow-system themes
* Auto-switches pen color configuration for blackboard/whiteboard modes (customizable via `Colors\Light.ini` / `Dark.ini`)


## Keyboard Shortcuts <span id='hotkey'></span>

### Global Shortcuts

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
| Alt+1 | Switch to black pen |
| Alt+2 | Switch to red pen |
| Alt+3 | Switch to green pen |
| Alt+4 | Switch to blue pen |
| Alt+5 | Switch to yellow pen |
| Alt+6 | Switch to white pen |
| Ctrl+A | Select all strokes |
| Ctrl+Z | Undo (prioritizes text operations) |
| Ctrl+Y | Redo (prioritizes text operations) |
| Delete / Backspace | Delete selected strokes or text |
| Shift+Esc | Exit/end slideshow |
| Escape | Exit current mode |

### PPT Slideshow Navigation

| Shortcut | Function |
|---|---|
| Down / PageDown / Right / N / Space | Next page |
| Up / PageUp / Left / P | Previous page |
| Mouse wheel | Scroll up/down to navigate pages |

### Text Editing

| Shortcut | Function |
|---|---|
| Enter | Submit current text |
| Shift+Enter | Insert line break in text |
| Escape | Cancel current text (deletes the text box) |
| Delete / Backspace | Delete selected text element (when not editing) |


## Tips <span id='skill'></span>
1. Double-tap the "Clear" button to clear the screen and hide the whiteboard at the same time.
2. Made a mistake? Tap "Undo" or press Ctrl+Z to undo. Tap "Redo" or press Ctrl+Y to restore.
3. To close Ink Anything, find the close button in Settings.
4. At any time, tap the "Blackboard" button to enter blackboard mode.
5. Running out of space? Swipe up with two fingers to pan the ink, or pinch to zoom out and make room.
6. Can't find the "Eraser" button? Flip your pen — the back end is the eraser! You can also use a finger or tilt the pen tip to precisely erase small areas. For large areas, use the back of your hand (recommended over palm, as palm sweat causes friction and large contact area may trigger multi-touch gestures).
7. Need to write text on the canvas? Press Alt+T to enter text mode, click anywhere to type — supports drag-to-move and resize handles.
8. Quick color switching: press Alt+1~6 to switch between black, red, green, blue, yellow, and white.
9. Floating toolbar in the way? Press Alt+V to quickly hide/show it.
10. Need to copy multiple text elements? Hold Ctrl and click to multi-select, then Ctrl+drag to clone all selected.
11. Need to select all ink quickly? Press Ctrl+A, or double-tap the select button.
12. Startup shows "hotkey conflict"? Another app is using the same shortcut — change it there or ignore the warning.
