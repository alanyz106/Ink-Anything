using Ink_Anything.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WpfCanvas = System.Windows.Controls.Canvas;

namespace Ink_Anything
{
    public partial class MainWindow : Window
    {
        private WpfCanvas _textOverlayCanvas = null;
        private Border currentEditingTextBorder = null;
        private TextUndoStack textUndoStack = new TextUndoStack();
        private List<TextElementData> _currentTextElements = new List<TextElementData>();

        private List<Border> selectedTextBorders = new List<Border>();
        private static readonly SolidColorBrush MultiSelectHighlightBrush = new SolidColorBrush(Color.FromArgb(0x30, 0x40, 0x9E, 0xFF));
        private WpfCanvas resizeHandlesCanvas = null;
        private bool isResizing = false;
        private Point resizeStartPoint;
        private double resizeStartFontSize;
        private Border resizeTargetBorder = null;

        private WpfCanvas GetTextOverlayCanvas()
        {
            if (_textOverlayCanvas == null)
            {
                _textOverlayCanvas = new WpfCanvas
                {
                    Tag = "TextOverlay",
                    IsHitTestVisible = true,
                    Background = Brushes.Transparent,
                };
                // 让 Canvas 铺满 inkCanvas，确保空白区域也能接收鼠标事件
                _textOverlayCanvas.Width = inkCanvas.ActualWidth > 0 ? inkCanvas.ActualWidth : 1920;
                _textOverlayCanvas.Height = inkCanvas.ActualHeight > 0 ? inkCanvas.ActualHeight : 1080;
                inkCanvas.SizeChanged += (s, e) =>
                {
                    if (_textOverlayCanvas != null)
                    {
                        _textOverlayCanvas.Width = e.NewSize.Width;
                        _textOverlayCanvas.Height = e.NewSize.Height;
                    }
                };
                _textOverlayCanvas.MouseLeftButtonDown += TextOverlayCanvas_MouseLeftButtonDown;
                inkCanvas.Children.Add(_textOverlayCanvas);
            }
            return _textOverlayCanvas;
        }

        private void SymbolIconText_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isInSelectionMode)
            {
                // 在选择模式中：切换选择范围（墨迹↔文字），进入部分选中状态
                SwitchSelectionScope(_selectionScope == SelectionScope.Text ? SelectionScope.Ink : SelectionScope.Text);
            }
            else if (drawingShapeMode == 26)
            {
                ExitTextMode();
            }
            else
            {
                EnterTextMode();
            }
        }

        private void EnterTextMode()
        {
            if (Main_Grid.Background == Brushes.Transparent)
            {
                Main_Grid.Background = new SolidColorBrush(StringToColor("#01FFFFFF"));
                inkCanvas.Visibility = Visibility.Visible;
                inkCanvas.IsHitTestVisible = true;
                GridBackgroundCoverHolder.Visibility = Visibility.Visible;
                StackPanelCanvasControls.Visibility = Visibility.Visible;
                StackPanelCanvacMain.Visibility = Visibility.Collapsed;
            }
            drawingShapeMode = 26;
            forceEraser = true;
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
            inkCanvas.IsManipulationEnabled = true;
            CancelSingleFingerDragMode();
            SymbolIconText.Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF));
            ToolTipService.SetToolTip(SymbolIconText, "文本模式：点击退出文本模式 (Alt+T)");
        }

        private void ExitTextMode()
        {
            CommitCurrentEditingText();
            drawingShapeMode = 0;
            forceEraser = false;
            SymbolIconText.Foreground = (Brush)FindResource("FloatBarForeground");
            ToolTipService.SetToolTip(SymbolIconText, "文本输入：点击进入文本模式 (Alt+T)");
            ClearResizeHandles();
            BtnPen_Click(null, null);
        }

        private void TextOverlayCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (drawingShapeMode != 26) return;

            // 检查点击是否在某个文字边框上
            var pos = e.GetPosition(_textOverlayCanvas);
            foreach (var child in _textOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    var left = WpfCanvas.GetLeft(border);
                    var top = WpfCanvas.GetTop(border);
                    if (pos.X >= left && pos.X <= left + border.ActualWidth &&
                        pos.Y >= top && pos.Y <= top + border.ActualHeight)
                    {
                        return; // 点在文字上，让文字的 MouseLeftButtonDown 处理
                    }
                }
            }

            // 点在空白处：清除选中状态，创建新文本框
            if (selectedTextBorders.Count > 0)
            {
                foreach (var b in selectedTextBorders) b.Background = Brushes.Transparent;
                ClearResizeHandlesOnly();
                selectedTextBorders.Clear();
            }

            // 转换到 inkCanvas 坐标
            var inkPos = e.GetPosition(inkCanvas);
            HandleTextModeClick(inkPos);
            e.Handled = true;
        }

        private void inkCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 选择模式下：检查是否点击了文字 Border
            if (_isInSelectionMode && _textOverlayCanvas != null)
            {
                // 墨迹选择模式下不处理文字点击
                if (_selectionScope != SelectionScope.Ink)
                {
                    var selPos = e.GetPosition(inkCanvas);
                    bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                    Border hitBorder = FindTextBorderAtPoint(selPos);
                    if (hitBorder != null)
                    {
                        if (inkCanvas.GetSelectedStrokes().Count > 0)
                        {
                            // 有墨迹选中：让 cover 的 handler 统一处理墨迹+文字拖拽
                            return;
                        }
                        // 纯文字选中：preview handler 处理选中和拖拽
                        HandleSelectionTextClick(hitBorder, isCtrlDown, selPos);
                        e.Handled = true;
                        return;
                    }
                }
                ClearTextSelection();
            }

            if (drawingShapeMode != 26) return;

            // 检查点击是否在某个文字边框上
            var pos = e.GetPosition(inkCanvas);
            if (_textOverlayCanvas != null)
            {
                foreach (var child in _textOverlayCanvas.Children)
                {
                    if (child is Border border && border.Tag as string == "TextElement")
                    {
                        var left = WpfCanvas.GetLeft(border);
                        var top = WpfCanvas.GetTop(border);
                        if (pos.X >= left && pos.X <= left + border.ActualWidth &&
                            pos.Y >= top && pos.Y <= top + border.ActualHeight)
                        {
                            return; // 点在文字上，让文字自己的 MouseLeftButtonDown 处理
                        }
                    }
                }
            }

            // 点在空白处：清除选中状态，创建新文本框
            if (selectedTextBorders.Count > 0)
            {
                foreach (var b in selectedTextBorders) b.Background = Brushes.Transparent;
                ClearResizeHandlesOnly();
                selectedTextBorders.Clear();
            }

            HandleTextModeClick(pos);
            e.Handled = true;
        }

        internal void HandleTextModeClick(Point pos)
        {
            CommitCurrentEditingText();
            ClearResizeHandles();

            double fontSize = 24;
            try { fontSize = TextSizeSlider.Value; } catch { }

            var color = inkCanvas.DefaultDrawingAttributes.Color;
            var colorHex = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

            var tb = new TextBox
            {
                FontSize = fontSize,
                Foreground = new SolidColorBrush(color),
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                MinWidth = 30,
                MinHeight = 20,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = false,
                Padding = new Thickness(2, 1, 2, 1),
                CaretBrush = new SolidColorBrush(color),
            };

            var border = new Border
            {
                Child = tb,
                CornerRadius = new CornerRadius(3),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF)),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0xFF, 0xFF)),
                Tag = "TextElement",
                Cursor = Cursors.IBeam,
            };

            var overlay = GetTextOverlayCanvas();
            overlay.Children.Add(border);
            WpfCanvas.SetLeft(border, pos.X);
            WpfCanvas.SetTop(border, pos.Y);
            currentEditingTextBorder = border;

            tb.KeyDown += TextBox_KeyDown;
            tb.LostFocus += TextBox_LostFocus;

            tb.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;

            if (e.Key == Key.Escape)
            {
                var border = tb.Parent as Border;
                if (border != null)
                {
                    GetTextOverlayCanvas().Children.Remove(border);
                    currentEditingTextBorder = null;
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    // Shift+Enter: 插入换行
                    int caretIndex = tb.CaretIndex;
                    tb.Text = tb.Text.Insert(caretIndex, Environment.NewLine);
                    tb.CaretIndex = caretIndex + Environment.NewLine.Length;
                    e.Handled = true;
                }
                else
                {
                    // Enter: 提交文本
                    CommitTextBorder(tb.Parent as Border);
                    e.Handled = true;
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;
            var border = tb.Parent as Border;
            if (border == null) return;

            if (currentEditingTextBorder == border)
            {
                CommitTextBorder(border);
            }
        }

        private void CommitCurrentEditingText()
        {
            if (currentEditingTextBorder != null)
            {
                CommitTextBorder(currentEditingTextBorder);
            }
        }

        private void CommitTextBorder(Border border)
        {
            var tb = border.Child as TextBox;
            if (tb == null) return;

            string text = tb.Text?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                GetTextOverlayCanvas().Children.Remove(border);
                currentEditingTextBorder = null;
                return;
            }

            var fontSize = tb.FontSize;
            var foreground = tb.Foreground;
            var fontFamily = tb.FontFamily;

            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = foreground,
                FontFamily = fontFamily,
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(2, 1, 2, 1),
            };

            border.Child = textBlock;
            border.Cursor = Cursors.SizeAll;
            border.Background = Brushes.Transparent;
            border.BorderThickness = new Thickness(0);
            currentEditingTextBorder = null;

            var data = new TextElementData
            {
                Content = text,
                X = WpfCanvas.GetLeft(border),
                Y = WpfCanvas.GetTop(border),
                FontSize = fontSize,
                ColorHex = BrushToHex(foreground),
            };

            textUndoStack.CommitAdd(data);

            border.MouseLeftButtonDown += TextBorder_MouseLeftButtonDown;
            border.MouseLeftButtonUp += TextBorder_MouseLeftButtonUp;
            border.MouseMove += TextBorder_MouseMove;
            border.MouseRightButtonUp += TextBorder_MouseRightButtonUp;
            border.ManipulationDelta += TextBorder_ManipulationDelta;
        }

        #region 文本拖拽移动

        private bool isDraggingText = false;
        private bool hasDraggedText = false;
        private Point textDragStartPoint;
        private Dictionary<Border, Point> textDragStartPositions = new Dictionary<Border, Point>();
        private Border draggingTextBorder = null;
        private bool isDraggingClone = false;

        private Border pendingCtrlCloneSource = null;
        private Border pendingCtrlToggleBorder = null;

        private void TextBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (drawingShapeMode != 26) return;

            var border = sender as Border;
            if (border == null) return;

            // 双击：进入编辑模式（Ctrl按下时不触发，避免与多选/拖动冲突）
            bool ctrlHeld = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (e.ClickCount == 2 && !ctrlHeld && border.Child is TextBlock)
            {
                EditTextBorder(border);
                e.Handled = true;
                return;
            }

            e.Handled = true;

            if (currentEditingTextBorder != null && currentEditingTextBorder != border)
            {
                CommitCurrentEditingText();
            }

            bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (isCtrlDown)
            {
                bool alreadySelected = selectedTextBorders.Contains(border);
                if (!alreadySelected)
                {
                    // 未选中：立即加入选中，确保参与拖动
                    selectedTextBorders.Add(border);
                    border.Background = MultiSelectHighlightBrush;
                    ClearResizeHandlesOnly();
                    ShowResizeHandles(border);
                }
                // 只对原本就选中的文字记录待切换，MouseUp 未拖动时取消选中
                pendingCtrlToggleBorder = alreadySelected ? border : null;
                pendingCtrlCloneSource = border;
            }
            else
            {
                ToggleOrSelectTextBorder(border, false);
                pendingCtrlToggleBorder = null;
                pendingCtrlCloneSource = null;
            }

            draggingTextBorder = border;
            isDraggingClone = false;
            hasDraggedText = false;

            isDraggingText = true;
            textDragStartPoint = e.GetPosition(GetTextOverlayCanvas());
            textDragStartPositions.Clear();
            foreach (var b in selectedTextBorders)
            {
                textDragStartPositions[b] = new Point(WpfCanvas.GetLeft(b), WpfCanvas.GetTop(b));
            }
            border.CaptureMouse();
        }

        private void TextBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingText || selectedTextBorders.Count == 0) return;

            var currentPoint = e.GetPosition(GetTextOverlayCanvas());
            var dx = currentPoint.X - textDragStartPoint.X;
            var dy = currentPoint.Y - textDragStartPoint.Y;

            if (Math.Abs(dx) > 2 || Math.Abs(dy) > 2)
                hasDraggedText = true;

            // Ctrl+拖动：超过阈值时克隆所有选中的文字
            if (pendingCtrlCloneSource != null && !isDraggingClone && (Math.Abs(dx) > 3 || Math.Abs(dy) > 3))
            {
                var originals = selectedTextBorders.ToList();
                foreach (var b in originals) b.Background = Brushes.Transparent;
                selectedTextBorders.Clear();
                textDragStartPositions.Clear();

                foreach (var orig in originals)
                {
                    var clone = CloneTextBorder(orig);
                    if (clone != null)
                    {
                        selectedTextBorders.Add(clone);
                        textDragStartPositions[clone] = new Point(WpfCanvas.GetLeft(clone), WpfCanvas.GetTop(clone));
                    }
                }

                isDraggingClone = true;
                draggingTextBorder = selectedTextBorders.FirstOrDefault();
                var last = GetLastSelectedTextBorder();
                if (last != null)
                {
                    last.Background = MultiSelectHighlightBrush;
                    ShowResizeHandles(last);
                }

                pendingCtrlCloneSource = null;
            }

            foreach (var b in selectedTextBorders)
            {
                if (textDragStartPositions.TryGetValue(b, out var startPos))
                {
                    WpfCanvas.SetLeft(b, startPos.X + dx);
                    WpfCanvas.SetTop(b, startPos.Y + dy);
                }
            }
            UpdateResizeHandlesPosition();
        }

        private void TextBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isDraggingText) return;

            isDraggingText = false;

            if (pendingCtrlToggleBorder != null)
            {
                if (hasDraggedText)
                {
                    // Ctrl+拖动完成：清除原始选中状态（克隆已在 MouseMove 中处理）
                    foreach (var b in selectedTextBorders) b.Background = Brushes.Transparent;
                    ClearResizeHandlesOnly();
                    selectedTextBorders.Clear();
                }
                else
                {
                    // Ctrl+点击未拖动：切换选中状态
                    if (selectedTextBorders.Contains(pendingCtrlToggleBorder))
                    {
                        selectedTextBorders.Remove(pendingCtrlToggleBorder);
                        pendingCtrlToggleBorder.Background = Brushes.Transparent;
                        ClearResizeHandlesOnly();
                        var last = GetLastSelectedTextBorder();
                        if (last != null) ShowResizeHandles(last);
                    }
                }
            }
            pendingCtrlToggleBorder = null;
            pendingCtrlCloneSource = null;

            if (draggingTextBorder != null)
            {
                draggingTextBorder.ReleaseMouseCapture();
            }

            foreach (var b in selectedTextBorders)
            {
                if (textDragStartPositions.TryGetValue(b, out var startPos))
                {
                    var newLeft = WpfCanvas.GetLeft(b);
                    var newTop = WpfCanvas.GetTop(b);

                    if (Math.Abs(newLeft - startPos.X) > 1 || Math.Abs(newTop - startPos.Y) > 1)
                    {
                        if (isDraggingClone)
                        {
                            // 克隆体拖拽结束，提交 Add
                            var cloneData = new TextElementData
                            {
                                Content = (b.Child as TextBlock)?.Text ?? "",
                                X = newLeft,
                                Y = newTop,
                                FontSize = (b.Child as TextBlock)?.FontSize ?? 24,
                                ColorHex = BrushToHex((b.Child as TextBlock)?.Foreground ?? Brushes.Black),
                            };
                            textUndoStack.CommitAdd(cloneData);
                        }
                        else
                        {
                            var data = FindTextElementData(b);
                            if (data != null)
                            {
                                textUndoStack.CommitMove(data, startPos.X, startPos.Y, newLeft, newTop);
                                data.X = newLeft;
                                data.Y = newTop;
                            }
                        }
                    }
                }
            }

            isDraggingClone = false;
            draggingTextBorder = null;
        }

        private void TextBorder_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            var dx = e.DeltaManipulation.Translation.X;
            var dy = e.DeltaManipulation.Translation.Y;

            // 多选时移动所有选中的文字
            if (selectedTextBorders.Contains(border))
            {
                foreach (var b in selectedTextBorders)
                {
                    WpfCanvas.SetLeft(b, WpfCanvas.GetLeft(b) + dx);
                    WpfCanvas.SetTop(b, WpfCanvas.GetTop(b) + dy);
                }
            }
            else
            {
                WpfCanvas.SetLeft(border, WpfCanvas.GetLeft(border) + dx);
                WpfCanvas.SetTop(border, WpfCanvas.GetTop(border) + dy);
            }
            UpdateResizeHandlesPosition();
        }

        private Border CloneTextBorder(Border source)
        {
            var srcText = source.Child as TextBlock;
            if (srcText == null) return null;

            var textBlock = new TextBlock
            {
                Text = srcText.Text,
                FontSize = srcText.FontSize,
                Foreground = srcText.Foreground,
                FontFamily = srcText.FontFamily,
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(2, 1, 2, 1),
            };

            var border = new Border
            {
                Child = textBlock,
                Tag = "TextElement",
                Cursor = Cursors.SizeAll,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
            };

            var overlay = GetTextOverlayCanvas();
            overlay.Children.Add(border);
            WpfCanvas.SetLeft(border, WpfCanvas.GetLeft(source) + 10);
            WpfCanvas.SetTop(border, WpfCanvas.GetTop(source) + 10);

            border.MouseLeftButtonDown += TextBorder_MouseLeftButtonDown;
            border.MouseLeftButtonUp += TextBorder_MouseLeftButtonUp;
            border.MouseMove += TextBorder_MouseMove;
            border.MouseRightButtonUp += TextBorder_MouseRightButtonUp;
            border.ManipulationDelta += TextBorder_ManipulationDelta;

            return border;
        }

        #endregion

        #region 双击编辑

        private void TextBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (drawingShapeMode != 26) return;
            EditTextBorder(sender as Border);
            e.Handled = true;
        }


        private void EditTextBorder(Border border)
        {
            if (border == null) return;
            var textBlock = border.Child as TextBlock;
            if (textBlock == null) return;

            ClearResizeHandles();
            CommitCurrentEditingText();

            string text = textBlock.Text;
            var fontSize = textBlock.FontSize;
            var foreground = textBlock.Foreground;
            var fontFamily = textBlock.FontFamily;

            var tb = new TextBox
            {
                Text = text,
                FontSize = fontSize,
                Foreground = foreground,
                FontFamily = fontFamily,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                MinWidth = 30,
                MinHeight = 20,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = false,
                Padding = new Thickness(2, 1, 2, 1),
                CaretBrush = foreground,
            };

            border.Child = tb;
            border.Cursor = Cursors.IBeam;
            border.Background = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0xFF, 0xFF));
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF));
            border.BorderThickness = new Thickness(1);

            currentEditingTextBorder = border;

            tb.KeyDown += TextBox_KeyDown;
            tb.LostFocus += TextBox_EditLostFocus;
            tb.Focus();
            tb.SelectAll();
        }

        private void TextBox_EditLostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;
            var border = tb.Parent as Border;
            if (border == null) return;

            if (currentEditingTextBorder == border)
            {
                string text = tb.Text?.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    var data = FindTextElementData(border);
                    if (data != null)
                    {
                        textUndoStack.CommitRemove(data);
                        _currentTextElements.Remove(data);
                    }
                    GetTextOverlayCanvas().Children.Remove(border);
                    currentEditingTextBorder = null;
                    ClearResizeHandles();
                    return;
                }

                var oldData = FindTextElementData(border);
                if (oldData != null)
                {
                    oldData.Content = text;
                    oldData.FontSize = tb.FontSize;
                    oldData.ColorHex = BrushToHex(tb.Foreground);
                }

                var textBlock = new TextBlock
                {
                    Text = text,
                    FontSize = tb.FontSize,
                    Foreground = tb.Foreground,
                    FontFamily = tb.FontFamily,
                    TextWrapping = TextWrapping.Wrap,
                    Padding = new Thickness(2, 1, 2, 1),
                };

                border.Child = textBlock;
                border.Cursor = Cursors.SizeAll;
                border.Background = Brushes.Transparent;
                border.BorderThickness = new Thickness(0);
                currentEditingTextBorder = null;
            }
        }

        #endregion

        internal void HandleTextModeKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                if (selectedTextBorders.Count > 0)
                {
                    var overlay = GetTextOverlayCanvas();
                    foreach (var border in selectedTextBorders.ToList())
                    {
                        var data = FindTextElementData(border);
                        if (data != null)
                        {
                            textUndoStack.CommitRemove(data);
                            _currentTextElements.Remove(data);
                        }
                        overlay.Children.Remove(border);
                    }
                    ClearResizeHandles();
                    e.Handled = true;
                }
                else if (currentEditingTextBorder != null)
                {
                    // 编辑中按Delete不拦截，让TextBox处理
                }
            }
        }

        internal void TryEraseTextAtPoint(Point pos)
        {
            if (_textOverlayCanvas == null) return;
            var toRemove = new List<Border>();
            foreach (var child in _textOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    var left = WpfCanvas.GetLeft(border);
                    var top = WpfCanvas.GetTop(border);
                    var right = left + border.ActualWidth;
                    var bottom = top + border.ActualHeight;

                    if (pos.X >= left - 10 && pos.X <= right + 10 && pos.Y >= top - 10 && pos.Y <= bottom + 10)
                    {
                        toRemove.Add(border);
                    }
                }
            }
            foreach (var border in toRemove)
            {
                var data = FindTextElementData(border);
                if (data != null)
                {
                    textUndoStack.CommitRemove(data);
                    _currentTextElements.Remove(data);
                }
                _textOverlayCanvas.Children.Remove(border);
            }
            if (selectedTextBorders.Any(b => toRemove.Contains(b)))
            {
                ClearResizeHandles();
            }
        }

        #region 选中和缩放手柄

        private Border GetLastSelectedTextBorder()
        {
            return selectedTextBorders.Count > 0 ? selectedTextBorders[selectedTextBorders.Count - 1] : null;
        }

        private void ToggleOrSelectTextBorder(Border border, bool isCtrlDown)
        {
            if (!isCtrlDown)
            {
                // 无Ctrl：清空之前的选择，只选当前
                foreach (var b in selectedTextBorders) b.Background = Brushes.Transparent;
                selectedTextBorders.Clear();
                ClearResizeHandlesOnly();
                selectedTextBorders.Add(border);
                border.Background = MultiSelectHighlightBrush;
                ShowResizeHandles(border);
            }
            else
            {
                // Ctrl：切换选中状态
                if (selectedTextBorders.Contains(border))
                {
                    selectedTextBorders.Remove(border);
                    border.Background = Brushes.Transparent;
                    ClearResizeHandlesOnly();
                    var last = GetLastSelectedTextBorder();
                    if (last != null) ShowResizeHandles(last);
                }
                else
                {
                    selectedTextBorders.Add(border);
                    border.Background = MultiSelectHighlightBrush;
                    ClearResizeHandlesOnly();
                    ShowResizeHandles(border);
                }
            }
        }

        private void ShowResizeHandles(Border border)
        {
            if (resizeHandlesCanvas != null)
            {
                GetTextOverlayCanvas().Children.Remove(resizeHandlesCanvas);
            }

            resizeHandlesCanvas = new WpfCanvas
            {
                Tag = "ResizeHandles",
                IsHitTestVisible = true,
            };

            var positions = new[]
            {
                new { H = HorizontalAlignment.Left, V = VerticalAlignment.Top, Cursor = Cursors.SizeNWSE },
                new { H = HorizontalAlignment.Right, V = VerticalAlignment.Top, Cursor = Cursors.SizeNESW },
                new { H = HorizontalAlignment.Left, V = VerticalAlignment.Bottom, Cursor = Cursors.SizeNESW },
                new { H = HorizontalAlignment.Right, V = VerticalAlignment.Bottom, Cursor = Cursors.SizeNWSE },
            };

            foreach (var pos in positions)
            {
                var handle = new Border
                {
                    Width = 8,
                    Height = 8,
                    Background = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF)),
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Cursor = pos.Cursor,
                    Tag = border,
                };

                handle.MouseLeftButtonDown += ResizeHandle_MouseLeftButtonDown;
                handle.MouseLeftButtonUp += ResizeHandle_MouseLeftButtonUp;
                handle.MouseMove += ResizeHandle_MouseMove;

                resizeHandlesCanvas.Children.Add(handle);
            }

            GetTextOverlayCanvas().Children.Add(resizeHandlesCanvas);
            UpdateResizeHandlesPosition();
        }

        private void UpdateResizeHandlesPosition()
        {
            var target = GetLastSelectedTextBorder();
            if (resizeHandlesCanvas == null || target == null) return;

            var left = WpfCanvas.GetLeft(target);
            var top = WpfCanvas.GetTop(target);
            var right = left + target.ActualWidth;
            var bottom = top + target.ActualHeight;

            var handles = resizeHandlesCanvas.Children.OfType<Border>().ToList();
            if (handles.Count < 4) return;

            SetHandlePosition(handles[0], left - 4, top - 4);
            SetHandlePosition(handles[1], right - 4, top - 4);
            SetHandlePosition(handles[2], left - 4, bottom - 4);
            SetHandlePosition(handles[3], right - 4, bottom - 4);
        }

        private void SetHandlePosition(Border handle, double left, double top)
        {
            WpfCanvas.SetLeft(handle, left);
            WpfCanvas.SetTop(handle, top);
        }

        private void ClearResizeHandlesOnly()
        {
            if (resizeHandlesCanvas != null)
            {
                GetTextOverlayCanvas().Children.Remove(resizeHandlesCanvas);
                resizeHandlesCanvas = null;
            }
        }

        private void ClearResizeHandles()
        {
            ClearResizeHandlesOnly();
            foreach (var b in selectedTextBorders) b.Background = Brushes.Transparent;
            selectedTextBorders.Clear();
        }

        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var handle = sender as Border;
            if (handle == null) return;

            isResizing = true;
            resizeTargetBorder = handle.Tag as Border;
            resizeStartPoint = e.GetPosition(GetTextOverlayCanvas());

            var textBlock = resizeTargetBorder?.Child as TextBlock;
            if (textBlock != null)
            {
                resizeStartFontSize = textBlock.FontSize;
            }
            else
            {
                var tb = resizeTargetBorder?.Child as TextBox;
                resizeStartFontSize = tb?.FontSize ?? 24;
            }

            handle.CaptureMouse();
            e.Handled = true;
        }

        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isResizing || resizeTargetBorder == null) return;

            var currentPoint = e.GetPosition(GetTextOverlayCanvas());
            var dy = currentPoint.Y - resizeStartPoint.Y;
            var deltaFontSize = dy * 0.3;
            var newFontSize = Math.Max(8, Math.Min(200, resizeStartFontSize + deltaFontSize));

            var textBlock = resizeTargetBorder.Child as TextBlock;
            if (textBlock != null)
            {
                textBlock.FontSize = newFontSize;
            }
            else
            {
                var tb = resizeTargetBorder.Child as TextBox;
                if (tb != null) tb.FontSize = newFontSize;
            }

            UpdateResizeHandlesPosition();
        }

        private void ResizeHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isResizing || resizeTargetBorder == null) return;

            var handle = sender as Border;
            handle?.ReleaseMouseCapture();

            var textBlock = resizeTargetBorder.Child as TextBlock;
            if (textBlock != null)
            {
                var newFontSize = textBlock.FontSize;
                if (Math.Abs(newFontSize - resizeStartFontSize) > 0.5)
                {
                    var data = FindTextElementData(resizeTargetBorder);
                    if (data != null)
                    {
                        textUndoStack.CommitResize(data, resizeStartFontSize, newFontSize);
                        data.FontSize = newFontSize;
                    }
                    try { TextSizeSlider.Value = newFontSize; } catch { }
                }
            }

            isResizing = false;
            resizeTargetBorder = null;
        }

        #endregion

        #region 撤销/重做

        internal bool TryUndoText()
        {
            if (!textUndoStack.CanUndo) return false;

            var entry = textUndoStack.Undo();
            switch (entry.Action)
            {
                case TextAction.Add:
                    RemoveTextBorderByData(entry.Data);
                    break;
                case TextAction.Remove:
                    RecreateTextElementFromData(entry.Data);
                    break;
                case TextAction.Move:
                    MoveTextBorderByData(entry.Data, entry.OldX, entry.OldY);
                    entry.Data.X = entry.OldX;
                    entry.Data.Y = entry.OldY;
                    break;
                case TextAction.Resize:
                    ResizeTextBorderByData(entry.Data, entry.OldFontSize);
                    entry.Data.FontSize = entry.OldFontSize;
                    break;
            }
            return true;
        }

        internal bool TryRedoText()
        {
            if (!textUndoStack.CanRedo) return false;

            var entry = textUndoStack.Redo();
            switch (entry.Action)
            {
                case TextAction.Add:
                    RecreateTextElementFromData(entry.Data);
                    break;
                case TextAction.Remove:
                    RemoveTextBorderByData(entry.Data);
                    break;
                case TextAction.Move:
                    MoveTextBorderByData(entry.Data, entry.NewX, entry.NewY);
                    entry.Data.X = entry.NewX;
                    entry.Data.Y = entry.NewY;
                    break;
                case TextAction.Resize:
                    ResizeTextBorderByData(entry.Data, entry.NewFontSize);
                    entry.Data.FontSize = entry.NewFontSize;
                    break;
            }
            return true;
        }

        #endregion

        #region 辅助方法

        private void RemoveTextBorderByData(TextElementData data)
        {
            var border = FindTextBorderByData(data);
            if (border != null)
            {
                GetTextOverlayCanvas().Children.Remove(border);
            }
        }

        private void MoveTextBorderByData(TextElementData data, double x, double y)
        {
            var border = FindTextBorderByData(data);
            if (border != null)
            {
                WpfCanvas.SetLeft(border, x);
                WpfCanvas.SetTop(border, y);
                UpdateResizeHandlesPosition();
            }
        }

        private void ResizeTextBorderByData(TextElementData data, double fontSize)
        {
            var border = FindTextBorderByData(data);
            if (border != null)
            {
                var textBlock = border.Child as TextBlock;
                if (textBlock != null) textBlock.FontSize = fontSize;
                UpdateResizeHandlesPosition();
            }
        }

        private Border FindTextBorderByData(TextElementData data)
        {
            var overlay = GetTextOverlayCanvas();
            foreach (var child in overlay.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    if (Math.Abs(WpfCanvas.GetLeft(border) - data.X) < 1 && Math.Abs(WpfCanvas.GetTop(border) - data.Y) < 1)
                    {
                        var tb = border.Child as TextBlock;
                        if (tb != null && tb.Text == data.Content)
                        {
                            return border;
                        }
                    }
                }
            }
            return null;
        }

        private TextElementData FindTextElementData(Border border)
        {
            var left = WpfCanvas.GetLeft(border);
            var top = WpfCanvas.GetTop(border);
            string content = null;

            var textBlock = border.Child as TextBlock;
            if (textBlock != null) content = textBlock.Text;
            else
            {
                var tb = border.Child as TextBox;
                if (tb != null) content = tb.Text;
            }

            return _currentTextElements.FirstOrDefault(d =>
                Math.Abs(d.X - left) < 1 && Math.Abs(d.Y - top) < 1 && d.Content == content);
        }

        private string BrushToHex(Brush brush)
        {
            if (brush is SolidColorBrush scb)
            {
                return $"#{scb.Color.A:X2}{scb.Color.R:X2}{scb.Color.G:X2}{scb.Color.B:X2}";
            }
            return "#FF000000";
        }

        private Brush HexToBrush(string hex)
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch
            {
                return Brushes.Black;
            }
        }

        internal List<TextElementData> GetCurrentTextElementDataList()
        {
            var list = new List<TextElementData>();
            if (_textOverlayCanvas == null) return list;
            foreach (var child in _textOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    var textBlock = border.Child as TextBlock;
                    if (textBlock != null)
                    {
                        list.Add(new TextElementData
                        {
                            Content = textBlock.Text,
                            X = WpfCanvas.GetLeft(border),
                            Y = WpfCanvas.GetTop(border),
                            FontSize = textBlock.FontSize,
                            ColorHex = BrushToHex(textBlock.Foreground),
                        });
                    }
                }
            }
            return list;
        }

        internal void RecreateTextElementFromData(TextElementData data)
        {
            var textBlock = new TextBlock
            {
                Text = data.Content,
                FontSize = data.FontSize,
                Foreground = HexToBrush(data.ColorHex),
                FontFamily = new FontFamily("Microsoft YaHei UI"),
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(2, 1, 2, 1),
            };

            var border = new Border
            {
                Child = textBlock,
                Tag = "TextElement",
                Cursor = Cursors.SizeAll,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
            };

            var overlay = GetTextOverlayCanvas();
            overlay.Children.Add(border);
            WpfCanvas.SetLeft(border, data.X);
            WpfCanvas.SetTop(border, data.Y);

            border.MouseLeftButtonDown += TextBorder_MouseLeftButtonDown;
            border.MouseLeftButtonUp += TextBorder_MouseLeftButtonUp;
            border.MouseMove += TextBorder_MouseMove;
            border.MouseRightButtonUp += TextBorder_MouseRightButtonUp;
            border.ManipulationDelta += TextBorder_ManipulationDelta;
        }

        internal void ClearTextElementsFromCanvas()
        {
            if (_textOverlayCanvas != null)
            {
                inkCanvas.Children.Remove(_textOverlayCanvas);
                _textOverlayCanvas = null;
            }
            selectedTextBorders.Clear();
            resizeHandlesCanvas = null;
            currentEditingTextBorder = null;
            _currentTextElements.Clear();
            textUndoStack.Clear();
        }

        internal void SaveTextElementDataToStream(List<TextElementData> textElements, string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(textElements, Formatting.None);
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        internal List<TextElementData> LoadTextElementDataFromStream(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<TextElementData>>(json);
            }
            catch
            {
                return null;
            }
        }

        internal void LoadTextElementsToCanvas(List<TextElementData> textElements)
        {
            if (textElements == null) return;
            foreach (var data in textElements)
            {
                RecreateTextElementFromData(data);
            }
        }

        #endregion
    }
}
