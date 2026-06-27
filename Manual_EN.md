# Ink Anything User Manual

## Table of Contents
[1. Introduction](#intro)  
[2. Usage](#usage)  
[3. Keyboard Shortcuts](#hotkey)  
[4. Tips](#skill)

## What is Ink Anything? <span id='intro'></span>

Ink Anything is a lightweight digital whiteboard optimized for Microsoft PowerPoint. It supports ink writing, text input, shape recognition, and multi-touch gestures.

### Three Modes

* **Slideshow Mode**: Opens automatically during PowerPoint slideshow. Supports pen, eraser, shapes, text input, page navigation, auto-save ink/text per slide. Detects hidden slides and remembers playback position.
* **Whiteboard/Blackboard Mode**: Free-form canvas with up to 99 pages, each with independent ink/text data. Default is blackboard, changeable in settings.
* **Screen Pen Mode**: Annotate over any screen content by turning the cursor into a pen.


## Usage <span id='usage'></span>

### Pen Settings
* **Switch color**: Tap the color balls on the main interface, or press Alt+1~6
* **Adjust width**: Settings → Canvas → Pen Width (default: 5)
* **Calligraphy**: In Settings, choose "Speed" or "Tail" based brush effects

### Eraser Usage
The eraser icon cycles through three states:
1. **Gray (Stroke Erase)**: Tap once — erases entire strokes
2. **Blue (Partial Erase)**: Tap again — erases within the indicated area
3. **Exit**: Tap once more to return to pen mode
* Press palm on screen to erase (supported on most touch screens)
* Eraser also works on text elements

### Text Input
* **Enter text mode**: Tap the text icon on the floating toolbar, or press Alt+T
* **Add text**: Left-click anywhere on the canvas, a text box appears — start typing
* **Submit text**: Press Enter, or click elsewhere to auto-submit
* **Line break**: Press Shift+Enter while editing
* **Edit text**: In text mode, double-click submitted text to re-edit, or right-click
* **Move text**: In text mode, left-click and drag submitted text
* **Multi-select text**: Ctrl+click text elements to select multiple
* **Clone text**: After multi-selecting, Ctrl+drag to copy all selected text to a new position
* **Resize text**: Click a text element, then drag the corner handles (font size range: 8–200)
* **Delete text**: Select then press Delete or Backspace, or use the eraser
* **Text color**: Follows current pen color
* **Text size**: Adjust in quick settings panel (range: 12–72, default: 24)
* **Text cursor**: Switch between arrow and I-beam in settings
* **Text selection mode**: In selection mode, press Alt+T to switch to text selection scope (text icon glows light blue). Press Alt+T again to return to ink selection scope.
* **Exit text mode**: Tap the text icon again, press Alt+T, or press Escape

### Drawing Preset Shapes
* **Circle**: Click at the "Origin" position to set center, then drag
* **Ellipse**: Click and drag at "Origin", set major and minor axes (option to show foci)
* **Hyperbola**: Click and drag at "Origin" to set asymptotes, then set real and imaginary axes (option to show foci)
* **Parabola**: Click at "Origin", then drag
* **Cuboid**: Draw the front face, then click again to set depth
* **Cone & Cylinder**: Draw in one smooth motion
* **2D Coordinate System**: Multiple presets available
* **3D Coordinate System**: Draws all three axes simultaneously
* **Parallel Lines**: Draw from one end to the other, optimized for specific angles

### Selection Operations
* **Enter selection mode**: Tap the select button. Selection scope depends on context:
  * From pen mode → ink scope (only operates on ink strokes)
  * From text mode → text scope (only operates on text elements)
  * Toggle scope with Alt+T or Alt+Q
* **3-state toggle**: First tap enters selection → second tap selects all → third tap exits selection
* **Action buttons**: After selecting, the floating toolbar shows Clone, Clone to New Page, Rotate (45°/90°), Flip (horizontal/vertical), Stroke Width Adjust (enlarge/shrink/restore), Delete
* **Drag to move**: Drag selected items to move
* **Ctrl+drag to clone**: Hold Ctrl and drag to copy selected items
* **Deselect**: Click outside the selected area
* **Select all**: Ctrl+A selects all within current scope
* **Delete**: Press Delete or Backspace while selected
* Supports two-finger gestures on selected strokes (zoom, move, rotate)

### PPT Slideshow Mode
* **Page navigation**: When no ink is on screen, swipe with multiple fingers, use on-screen controls, or keyboard arrow keys
* **Auto-save**: Ink and text are saved per slide; stroke folder is portable to other computers
* Detects hidden slides and prompts to unhide
* Remembers last playback position and offers to jump back

### Auto Ink Recognition (Ink To Shape)
1. Draw as neatly as possible
2. Draw an ellipse inside a circle for cross-section/concentric recognition (useful for orbital diagrams)
3. Automatically recognizes concentric and tangent circles
4. Recognizes: circle, ellipse, triangle, rectangle, rhombus, parallelogram, square
5. Note: Only available in 32-bit mode; hidden in 64-bit

### Floating Toolbar
* Draggable to any position on screen
* Collapsible/expandable (Alt+V or tap the emoji icon)
* Lockable to prevent accidental movement
* Quick settings: pen width, text size, ink-to-shape toggle, finger mode toggle, ink save/load/replay, countdown timer, random name picker
* Ink floating toolbar (clone/rotate/flip/delete) only shows when ink strokes are selected

### Settings Page
Tap the gear icon to open settings. Tab-based layout:
* **Behavior**: Canvas hiding, finger mode, startup behavior
* **Canvas**: Pen width, eraser type, cursor display
* **Gesture**: Two-finger gestures (zoom/pan/rotate)
* **Ink-to-Shape**: Shape recognition toggle
* **Appearance**: Theme (light/dark/follow-system)
* **PPT**: PowerPoint integration settings
* **Advanced**: Special screens, touch sensitivity, logging
* **Automation**: Auto-kill processes, auto-save screenshots/ink
* **Check for Updates**: Button at the top of settings page

All settings auto-save; some require restart.

### Theme Switching
* Light, dark, and follow-system themes
* Auto-switches pen color config for blackboard/whiteboard modes (customizable via `Colors\Light.ini` / `Dark.ini`)


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
| Alt+T | Text input mode (toggles ink↔text selection scope when in selection mode) |
| Alt+Q | Toggle selection mode (toggles ink↔text selection scope when in selection mode) |
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
1. Double-tap "Clear" to clear the screen and hide the whiteboard at the same time.
2. Made a mistake? Press Ctrl+Z to undo, Ctrl+Y to redo.
3. Running out of space? Swipe up with two fingers to pan, or pinch to zoom out.
4. Flip the pen for eraser. Use a finger or tilt the pen tip for small areas, or the back of your hand for large areas.
5. Press Alt+T to enter text mode, click anywhere to type.
6. Quick color switching: Alt+1~6.
7. Floating toolbar in the way? Press Alt+V to hide/show.
8. In selection mode, press Alt+T to toggle between ink and text selection scope.
9. Click "Check Update" in settings to manually check for new versions.
10. Startup shows "hotkey conflict"? Another app is using the same shortcut.