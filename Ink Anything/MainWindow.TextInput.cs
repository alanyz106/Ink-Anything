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
        // 共享状态已迁移到 _textManager (TextManager)
        // 访问方式: _textManager.TextOverlayCanvas, _textManager.SelectedTextBorders, 等

        private WpfCanvas GetTextOverlayCanvas()
        {
            if (_textManager.TextOverlayCanvas == null)
            {
                _textManager.TextOverlayCanvas = new WpfCanvas
                {
                    Tag = "TextOverlay",
                    IsHitTestVisible = true,
                    Background = Brushes.Transparent,
                };
                // 让 Canvas 铺满 inkCanvas，确保空白区域也能接收鼠标事件
                _textManager.TextOverlayCanvas.Width = inkCanvas.ActualWidth > 0 ? inkCanvas.ActualWidth : 1920;
                _textManager.TextOverlayCanvas.Height = inkCanvas.ActualHeight > 0 ? inkCanvas.ActualHeight : 1080;
                inkCanvas.SizeChanged += (s, e) =>
                {
                    if (_textManager.TextOverlayCanvas != null)
                    {
                        _textManager.TextOverlayCanvas.Width = e.NewSize.Width;
                        _textManager.TextOverlayCanvas.Height = e.NewSize.Height;
                    }
                };
                _textManager.TextOverlayCanvas.MouseLeftButtonDown += TextOverlayCanvas_MouseLeftButtonDown;
                inkCanvas.Children.Add(_textManager.TextOverlayCanvas);
            }
            return _textManager.TextOverlayCanvas;
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
            var pos = e.GetPosition(_textManager.TextOverlayCanvas);
            foreach (var child in _textManager.TextOverlayCanvas.Children)
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
            if (_textManager.SelectedTextBorders.Count > 0)
            {
                foreach (var b in _textManager.SelectedTextBorders) b.Background = Brushes.Transparent;
                ClearResizeHandlesOnly();
                _textManager.SelectedTextBorders.Clear();
            }

            // 转换到 inkCanvas 坐标
            var inkPos = e.GetPosition(inkCanvas);
            HandleTextModeClick(inkPos);
            e.Handled = true;
        }

        private void inkCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 选择模式下：检查是否点击了文字 Border
            if (_isInSelectionMode && _textManager.TextOverlayCanvas != null)
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
            if (_textManager.TextOverlayCanvas != null)
            {
                foreach (var child in _textManager.TextOverlayCanvas.Children)
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
            if (_textManager.SelectedTextBorders.Count > 0)
            {
                foreach (var b in _textManager.SelectedTextBorders) b.Background = Brushes.Transparent;
                ClearResizeHandlesOnly();
                _textManager.SelectedTextBorders.Clear();
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
            _textManager.CurrentEditingTextBorder = border;

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
                    _textManager.CurrentEditingTextBorder = null;
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

            if (_textManager.CurrentEditingTextBorder == border)
            {
                CommitTextBorder(border);
            }
        }

        private void CommitCurrentEditingText()
        {
            if (_textManager.CurrentEditingTextBorder != null)
            {
                CommitTextBorder(_textManager.CurrentEditingTextBorder);
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
                _textManager.CurrentEditingTextBorder = null;
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
            _textManager.CurrentEditingTextBorder = null;

            var data = new TextElementData
            {
                Content = text,
                X = WpfCanvas.GetLeft(border),
                Y = WpfCanvas.GetTop(border),
                FontSize = fontSize,
                ColorHex = TextManager.BrushToHex(foreground),
            };

            _textManager.TextUndoStack.CommitAdd(data);

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
        private Border draggingTextBorder = null;

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

            if (_textManager.CurrentEditingTextBorder != null && _textManager.CurrentEditingTextBorder != border)
            {
                CommitCurrentEditingText();
            }

            bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (isCtrlDown)
            {
                bool alreadySelected = _textManager.SelectedTextBorders.Contains(border);
                if (!alreadySelected)
                {
                    // 未选中：立即加入选中，确保参与拖动
                    _textManager.SelectedTextBorders.Add(border);
                    border.Background = TextManager.MultiSelectHighlightBrush;
                    ClearResizeHandlesOnly();
                    ShowResizeHandles(border);
                }
                // 只对原本就选中的文字记录待切换，MouseUp 未拖动时取消选中
                pendingCtrlToggleBorder = alreadySelected ? border : null;
            }
            else
            {
                ToggleOrSelectTextBorder(border, false);
                pendingCtrlToggleBorder = null;
            }

            draggingTextBorder = border;
            hasDraggedText = false;

            isDraggingText = true;
            textDragStartPoint = e.GetPosition(GetTextOverlayCanvas());
            _textManager.TextDragStartPositions.Clear();
            foreach (var b in _textManager.SelectedTextBorders)
            {
                _textManager.TextDragStartPositions[b] = new Point(WpfCanvas.GetLeft(b), WpfCanvas.GetTop(b));
            }
            border.CaptureMouse();
        }

        private void TextBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingText || _textManager.SelectedTextBorders.Count == 0) return;

            var currentPoint = e.GetPosition(GetTextOverlayCanvas());
            var dx = currentPoint.X - textDragStartPoint.X;
            var dy = currentPoint.Y - textDragStartPoint.Y;

            if (Math.Abs(dx) > 2 || Math.Abs(dy) > 2)
                hasDraggedText = true;

            foreach (var b in _textManager.SelectedTextBorders)
            {
                if (_textManager.TextDragStartPositions.TryGetValue(b, out var startPos))
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

            if (pendingCtrlToggleBorder != null && !hasDraggedText)
            {
                // Ctrl+点击未拖动：切换选中状态
                if (_textManager.SelectedTextBorders.Contains(pendingCtrlToggleBorder))
                {
                    _textManager.SelectedTextBorders.Remove(pendingCtrlToggleBorder);
                    pendingCtrlToggleBorder.Background = Brushes.Transparent;
                    ClearResizeHandlesOnly();
                    var last = GetLastSelectedTextBorder();
                    if (last != null) ShowResizeHandles(last);
                }
            }
            pendingCtrlToggleBorder = null;

            if (draggingTextBorder != null)
            {
                draggingTextBorder.ReleaseMouseCapture();
            }

            foreach (var b in _textManager.SelectedTextBorders)
            {
                if (_textManager.TextDragStartPositions.TryGetValue(b, out var startPos))
                {
                    var newLeft = WpfCanvas.GetLeft(b);
                    var newTop = WpfCanvas.GetTop(b);

                    if (Math.Abs(newLeft - startPos.X) > 1 || Math.Abs(newTop - startPos.Y) > 1)
                    {
                        var data = FindTextElementData(b);
                        if (data != null)
                        {
                            _textManager.TextUndoStack.CommitMove(data, startPos.X, startPos.Y, newLeft, newTop);
                            data.X = newLeft;
                            data.Y = newTop;
                        }
                    }
                }
            }

            draggingTextBorder = null;
        }

        private void TextBorder_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            var dx = e.DeltaManipulation.Translation.X;
            var dy = e.DeltaManipulation.Translation.Y;

            // 多选时移动所有选中的文字
            if (_textManager.SelectedTextBorders.Contains(border))
            {
                foreach (var b in _textManager.SelectedTextBorders)
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

            _textManager.CurrentEditingTextBorder = border;

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

            if (_textManager.CurrentEditingTextBorder == border)
            {
                string text = tb.Text?.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    var data = FindTextElementData(border);
                    if (data != null)
                    {
                        _textManager.TextUndoStack.CommitRemove(data);
                        _textManager.CurrentTextElements.Remove(data);
                    }
                    GetTextOverlayCanvas().Children.Remove(border);
                    _textManager.CurrentEditingTextBorder = null;
                    ClearResizeHandles();
                    return;
                }

                var oldData = FindTextElementData(border);
                if (oldData != null)
                {
                    oldData.Content = text;
                    oldData.FontSize = tb.FontSize;
                    oldData.ColorHex = TextManager.BrushToHex(tb.Foreground);
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
                _textManager.CurrentEditingTextBorder = null;
            }
        }

        #endregion

        internal void HandleTextModeKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                if (_textManager.SelectedTextBorders.Count > 0)
                {
                    var overlay = GetTextOverlayCanvas();
                    foreach (var border in _textManager.SelectedTextBorders.ToList())
                    {
                        var data = FindTextElementData(border);
                        if (data != null)
                        {
                            _textManager.TextUndoStack.CommitRemove(data);
                            _textManager.CurrentTextElements.Remove(data);
                        }
                        overlay.Children.Remove(border);
                    }
                    ClearResizeHandles();
                    e.Handled = true;
                }
                else if (_textManager.CurrentEditingTextBorder != null)
                {
                    // 编辑中按Delete不拦截，让TextBox处理
                }
            }
        }

        internal void TryEraseTextAtPoint(Point pos)
        {
            if (_textManager.TextOverlayCanvas == null) return;
            var toRemove = new List<Border>();
            foreach (var child in _textManager.TextOverlayCanvas.Children)
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
                    _textManager.TextUndoStack.CommitRemove(data);
                    _textManager.CurrentTextElements.Remove(data);
                }
                _textManager.TextOverlayCanvas.Children.Remove(border);
            }
            if (_textManager.SelectedTextBorders.Any(b => toRemove.Contains(b)))
            {
                ClearResizeHandles();
            }
        }

        #region 选中和缩放手柄

        private Border GetLastSelectedTextBorder()
        {
            return _textManager.SelectedTextBorders.Count > 0 ? _textManager.SelectedTextBorders[_textManager.SelectedTextBorders.Count - 1] : null;
        }

        private void ToggleOrSelectTextBorder(Border border, bool isCtrlDown)
        {
            if (!isCtrlDown)
            {
                // 无Ctrl：清空之前的选择，只选当前
                foreach (var b in _textManager.SelectedTextBorders) b.Background = Brushes.Transparent;
                _textManager.SelectedTextBorders.Clear();
                ClearResizeHandlesOnly();
                _textManager.SelectedTextBorders.Add(border);
                border.Background = TextManager.MultiSelectHighlightBrush;
                ShowResizeHandles(border);
            }
            else
            {
                // Ctrl：切换选中状态
                if (_textManager.SelectedTextBorders.Contains(border))
                {
                    _textManager.SelectedTextBorders.Remove(border);
                    border.Background = Brushes.Transparent;
                    ClearResizeHandlesOnly();
                    var last = GetLastSelectedTextBorder();
                    if (last != null) ShowResizeHandles(last);
                }
                else
                {
                    _textManager.SelectedTextBorders.Add(border);
                    border.Background = TextManager.MultiSelectHighlightBrush;
                    ClearResizeHandlesOnly();
                    ShowResizeHandles(border);
                }
            }
        }

        private void ShowResizeHandles(Border border)
        {
            if (_textManager.ResizeHandlesCanvas != null)
            {
                GetTextOverlayCanvas().Children.Remove(_textManager.ResizeHandlesCanvas);
            }

            _textManager.ResizeHandlesCanvas = new WpfCanvas
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

                _textManager.ResizeHandlesCanvas.Children.Add(handle);
            }

            GetTextOverlayCanvas().Children.Add(_textManager.ResizeHandlesCanvas);
            UpdateResizeHandlesPosition();
        }

        private void UpdateResizeHandlesPosition()
        {
            var target = GetLastSelectedTextBorder();
            if (_textManager.ResizeHandlesCanvas == null || target == null) return;

            var left = WpfCanvas.GetLeft(target);
            var top = WpfCanvas.GetTop(target);
            var right = left + target.ActualWidth;
            var bottom = top + target.ActualHeight;

            var handles = _textManager.ResizeHandlesCanvas.Children.OfType<Border>().ToList();
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
            if (_textManager.ResizeHandlesCanvas != null)
            {
                GetTextOverlayCanvas().Children.Remove(_textManager.ResizeHandlesCanvas);
                _textManager.ResizeHandlesCanvas = null;
            }
        }

        private void ClearResizeHandles()
        {
            ClearResizeHandlesOnly();
            foreach (var b in _textManager.SelectedTextBorders) b.Background = Brushes.Transparent;
            _textManager.SelectedTextBorders.Clear();
        }

        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var handle = sender as Border;
            if (handle == null) return;

            _textManager.IsResizing = true;
            _textManager.ResizeTargetBorder = handle.Tag as Border;
            _textManager.ResizeStartPoint = e.GetPosition(GetTextOverlayCanvas());

            var textBlock = _textManager.ResizeTargetBorder?.Child as TextBlock;
            if (textBlock != null)
            {
                _textManager.ResizeStartFontSize = textBlock.FontSize;
            }
            else
            {
                var tb = _textManager.ResizeTargetBorder?.Child as TextBox;
                _textManager.ResizeStartFontSize = tb?.FontSize ?? 24;
            }

            handle.CaptureMouse();
            e.Handled = true;
        }

        private void ResizeHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_textManager.IsResizing || _textManager.ResizeTargetBorder == null) return;

            var currentPoint = e.GetPosition(GetTextOverlayCanvas());
            var dy = currentPoint.Y - _textManager.ResizeStartPoint.Y;
            var deltaFontSize = dy * 0.3;
            var newFontSize = Math.Max(8, Math.Min(200, _textManager.ResizeStartFontSize + deltaFontSize));

            var textBlock = _textManager.ResizeTargetBorder.Child as TextBlock;
            if (textBlock != null)
            {
                textBlock.FontSize = newFontSize;
            }
            else
            {
                var tb = _textManager.ResizeTargetBorder.Child as TextBox;
                if (tb != null) tb.FontSize = newFontSize;
            }

            UpdateResizeHandlesPosition();
        }

        private void ResizeHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_textManager.IsResizing || _textManager.ResizeTargetBorder == null) return;

            var handle = sender as Border;
            handle?.ReleaseMouseCapture();

            var textBlock = _textManager.ResizeTargetBorder.Child as TextBlock;
            if (textBlock != null)
            {
                var newFontSize = textBlock.FontSize;
                if (Math.Abs(newFontSize - _textManager.ResizeStartFontSize) > 0.5)
                {
                    var data = FindTextElementData(_textManager.ResizeTargetBorder);
                    if (data != null)
                    {
                        _textManager.TextUndoStack.CommitResize(data, _textManager.ResizeStartFontSize, newFontSize);
                        data.FontSize = newFontSize;
                    }
                    try { TextSizeSlider.Value = newFontSize; } catch { }
                }
            }

            _textManager.IsResizing = false;
            _textManager.ResizeTargetBorder = null;
        }

        #endregion

        #region 撤销/重做

        internal bool TryUndoText()
        {
            if (!_textManager.TextUndoStack.CanUndo) return false;

            var entry = _textManager.TextUndoStack.Undo();
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
            if (!_textManager.TextUndoStack.CanRedo) return false;

            var entry = _textManager.TextUndoStack.Redo();
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

            return _textManager.CurrentTextElements.FirstOrDefault(d =>
                Math.Abs(d.X - left) < 1 && Math.Abs(d.Y - top) < 1 && d.Content == content);
        }

        internal List<TextElementData> GetCurrentTextElementDataList()
        {
            var list = new List<TextElementData>();
            if (_textManager.TextOverlayCanvas == null) return list;
            foreach (var child in _textManager.TextOverlayCanvas.Children)
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
                            ColorHex = TextManager.BrushToHex(textBlock.Foreground),
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
                Foreground = TextManager.HexToBrush(data.ColorHex),
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
            if (_textManager.TextOverlayCanvas != null)
            {
                inkCanvas.Children.Remove(_textManager.TextOverlayCanvas);
                _textManager.TextOverlayCanvas = null;
            }
            _textManager.SelectedTextBorders.Clear();
            _textManager.ResizeHandlesCanvas = null;
            _textManager.CurrentEditingTextBorder = null;
            _textManager.CurrentTextElements.Clear();
            _textManager.TextUndoStack.Clear();
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
