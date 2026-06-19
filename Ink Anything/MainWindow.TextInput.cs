using Ink_Anything.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        private Border selectedTextBorder = null;
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
                inkCanvas.Children.Add(_textOverlayCanvas);
            }
            return _textOverlayCanvas;
        }

        private void SymbolIconText_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (drawingShapeMode == 26)
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
            drawingShapeMode = 26;
            forceEraser = true;
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
            inkCanvas.IsManipulationEnabled = true;
            CancelSingleFingerDragMode();
            SymbolIconText.Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF));
            ToolTipService.SetToolTip(SymbolIconText, "文本模式：点击退出文本模式");
        }

        private void ExitTextMode()
        {
            CommitCurrentEditingText();
            drawingShapeMode = 0;
            forceEraser = false;
            SymbolIconText.Foreground = (Brush)FindResource("FloatBarForeground");
            ToolTipService.SetToolTip(SymbolIconText, "文本输入：点击进入文本模式");
            ClearResizeHandles();
            BtnPen_Click(null, null);
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
        private Point textDragStartPoint;
        private double textDragStartLeft;
        private double textDragStartTop;
        private Border draggingTextBorder = null;

        private void TextBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (drawingShapeMode != 26) return;

            var border = sender as Border;
            if (border == null) return;

            e.Handled = true;

            if (currentEditingTextBorder != null && currentEditingTextBorder != border)
            {
                CommitCurrentEditingText();
            }

            SelectTextBorder(border);

            isDraggingText = true;
            textDragStartPoint = e.GetPosition(GetTextOverlayCanvas());
            textDragStartLeft = WpfCanvas.GetLeft(border);
            textDragStartTop = WpfCanvas.GetTop(border);
            draggingTextBorder = border;
            border.CaptureMouse();
        }

        private void TextBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingText || sender != draggingTextBorder) return;

            var currentPoint = e.GetPosition(GetTextOverlayCanvas());
            var dx = currentPoint.X - textDragStartPoint.X;
            var dy = currentPoint.Y - textDragStartPoint.Y;

            WpfCanvas.SetLeft(draggingTextBorder, textDragStartLeft + dx);
            WpfCanvas.SetTop(draggingTextBorder, textDragStartTop + dy);
            UpdateResizeHandlesPosition();
        }

        private void TextBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isDraggingText || sender != draggingTextBorder) return;

            isDraggingText = false;
            draggingTextBorder.ReleaseMouseCapture();

            var newLeft = WpfCanvas.GetLeft(draggingTextBorder);
            var newTop = WpfCanvas.GetTop(draggingTextBorder);

            if (Math.Abs(newLeft - textDragStartLeft) > 1 || Math.Abs(newTop - textDragStartTop) > 1)
            {
                var data = FindTextElementData(draggingTextBorder);
                if (data != null)
                {
                    textUndoStack.CommitMove(data, textDragStartLeft, textDragStartTop, newLeft, newTop);
                    data.X = newLeft;
                    data.Y = newTop;
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

            var left = WpfCanvas.GetLeft(border) + dx;
            var top = WpfCanvas.GetTop(border) + dy;

            WpfCanvas.SetLeft(border, left);
            WpfCanvas.SetTop(border, top);
            UpdateResizeHandlesPosition();
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
                AcceptsReturn = true,
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
                if (selectedTextBorder != null)
                {
                    var data = FindTextElementData(selectedTextBorder);
                    if (data != null)
                    {
                        textUndoStack.CommitRemove(data);
                        _currentTextElements.Remove(data);
                    }
                    GetTextOverlayCanvas().Children.Remove(selectedTextBorder);
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
            if (selectedTextBorder != null && toRemove.Contains(selectedTextBorder))
            {
                ClearResizeHandles();
            }
        }

        #region 选中和缩放手柄

        private void SelectTextBorder(Border border)
        {
            if (selectedTextBorder == border) return;
            ClearResizeHandles();
            selectedTextBorder = border;
            ShowResizeHandles(border);
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
            if (resizeHandlesCanvas == null || selectedTextBorder == null) return;

            var left = WpfCanvas.GetLeft(selectedTextBorder);
            var top = WpfCanvas.GetTop(selectedTextBorder);
            var right = left + selectedTextBorder.ActualWidth;
            var bottom = top + selectedTextBorder.ActualHeight;

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

        private void ClearResizeHandles()
        {
            if (resizeHandlesCanvas != null)
            {
                GetTextOverlayCanvas().Children.Remove(resizeHandlesCanvas);
                resizeHandlesCanvas = null;
            }
            selectedTextBorder = null;
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
            selectedTextBorder = null;
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
