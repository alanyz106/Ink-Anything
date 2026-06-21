using Ink_Anything.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfCanvas = System.Windows.Controls.Canvas;

namespace Ink_Anything
{
    public partial class MainWindow : Window
    {
        #region Selection Gestures

        #region Floating Control

        object lastBorderMouseDownObject;

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastBorderMouseDownObject = sender;
        }

        bool isStrokeSelectionCloneOn = false;
        private void BorderStrokeSelectionClone_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            if (isStrokeSelectionCloneOn)
            {
                BorderStrokeSelectionClone.Background = Brushes.Transparent;

                isStrokeSelectionCloneOn = false;
            }
            else
            {
                BorderStrokeSelectionClone.Background = new SolidColorBrush(StringToColor("#FF1ED760"));

                isStrokeSelectionCloneOn = true;
            }
        }

        private void BorderStrokeSelectionCloneToNewBoard_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            var strokes = inkCanvas.GetSelectedStrokes();
            inkCanvas.Select(new StrokeCollection());
            strokes = strokes.Clone();
            BtnWhiteBoardAdd_Click(null, null);
            inkCanvas.Strokes.Add(strokes);
        }

        private void BorderStrokeSelectionDelete_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject == sender)
            {
                SymbolIconDelete_MouseUp(sender, e);
            }
        }

        private void GridPenWidthDecrease_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            ChangeStrokeThickness(0.8);
        }

        private void GridPenWidthIncrease_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            ChangeStrokeThickness(1.25);
        }

        private void ChangeStrokeThickness(double multipler)
        {
            foreach (Stroke stroke in inkCanvas.GetSelectedStrokes())
            {
                var newWidth = stroke.DrawingAttributes.Width * multipler;
                var newHeight = stroke.DrawingAttributes.Height * multipler;

                if (newWidth >= DrawingAttributes.MinWidth && newWidth <= DrawingAttributes.MaxWidth
                    && newHeight >= DrawingAttributes.MinHeight && newHeight <= DrawingAttributes.MaxHeight)
                {
                    stroke.DrawingAttributes.Width = newWidth;
                    stroke.DrawingAttributes.Height = newHeight;
                }
            }
            if (DrawingAttributesHistory.Count > 0)
            {

                timeMachine.CommitStrokeDrawingAttributesHistory(DrawingAttributesHistory);
                DrawingAttributesHistory = new Dictionary<Stroke, Tuple<DrawingAttributes, DrawingAttributes>>();
                foreach (var item in DrawingAttributesHistoryFlag)
                {
                    item.Value.Clear();
                }
            }
        }

        private void GridPenWidthRestore_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            foreach (Stroke stroke in inkCanvas.GetSelectedStrokes())
            {
                stroke.DrawingAttributes.Width = inkCanvas.DefaultDrawingAttributes.Width;
                stroke.DrawingAttributes.Height = inkCanvas.DefaultDrawingAttributes.Height;
            }
        }

        private void ImageFlipHorizontal_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            Matrix m = new Matrix();

            FrameworkElement fe = e.Source as FrameworkElement;
            Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
            center = new Point(inkCanvas.GetSelectionBounds().Left + inkCanvas.GetSelectionBounds().Width / 2,
                inkCanvas.GetSelectionBounds().Top + inkCanvas.GetSelectionBounds().Height / 2);
            center = m.Transform(center);

            m.ScaleAt(-1, 1, center.X, center.Y);

            StrokeCollection targetStrokes = inkCanvas.GetSelectedStrokes();
            foreach (Stroke stroke in targetStrokes)
            {
                stroke.Transform(m, false);
            }
            if (DrawingAttributesHistory.Count > 0)
            {
                var collecion = new StrokeCollection();
                foreach (var item in DrawingAttributesHistory)
                {
                    collecion.Add(item.Key);
                }
                timeMachine.CommitStrokeDrawingAttributesHistory(DrawingAttributesHistory);
                DrawingAttributesHistory = new Dictionary<Stroke, Tuple<DrawingAttributes, DrawingAttributes>>();
                foreach (var item in DrawingAttributesHistoryFlag)
                {
                    item.Value.Clear();
                }
            }
        }

        private void ImageFlipVertical_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            Matrix m = new Matrix();

            FrameworkElement fe = e.Source as FrameworkElement;
            Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
            center = new Point(inkCanvas.GetSelectionBounds().Left + inkCanvas.GetSelectionBounds().Width / 2,
                inkCanvas.GetSelectionBounds().Top + inkCanvas.GetSelectionBounds().Height / 2);
            center = m.Transform(center);

            m.ScaleAt(1, -1, center.X, center.Y);

            StrokeCollection targetStrokes = inkCanvas.GetSelectedStrokes();
            foreach (Stroke stroke in targetStrokes)
            {
                stroke.Transform(m, false);
            }
            if (DrawingAttributesHistory.Count > 0)
            {
                timeMachine.CommitStrokeDrawingAttributesHistory(DrawingAttributesHistory);
                DrawingAttributesHistory = new Dictionary<Stroke, Tuple<DrawingAttributes, DrawingAttributes>>();
                foreach (var item in DrawingAttributesHistoryFlag)
                {
                    item.Value.Clear();
                }
            }
        }

        private void ImageRotate45_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            Matrix m = new Matrix();

            FrameworkElement fe = e.Source as FrameworkElement;
            Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
            center = new Point(inkCanvas.GetSelectionBounds().Left + inkCanvas.GetSelectionBounds().Width / 2,
                inkCanvas.GetSelectionBounds().Top + inkCanvas.GetSelectionBounds().Height / 2);
            center = m.Transform(center);

            m.RotateAt(45, center.X, center.Y);

            StrokeCollection targetStrokes = inkCanvas.GetSelectedStrokes();
            foreach (Stroke stroke in targetStrokes)
            {
                stroke.Transform(m, false);
            }
            if (DrawingAttributesHistory.Count > 0)
            {
                timeMachine.CommitStrokeDrawingAttributesHistory(DrawingAttributesHistory);
                DrawingAttributesHistory = new Dictionary<Stroke, Tuple<DrawingAttributes, DrawingAttributes>>();
                foreach (var item in DrawingAttributesHistoryFlag)
                {
                    item.Value.Clear();
                }
            }
        }

        private void ImageRotate90_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            Matrix m = new Matrix();

            FrameworkElement fe = e.Source as FrameworkElement;
            Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
            center = new Point(inkCanvas.GetSelectionBounds().Left + inkCanvas.GetSelectionBounds().Width / 2,
                inkCanvas.GetSelectionBounds().Top + inkCanvas.GetSelectionBounds().Height / 2);
            center = m.Transform(center);

            m.RotateAt(90, center.X, center.Y);

            StrokeCollection targetStrokes = inkCanvas.GetSelectedStrokes();
            foreach (Stroke stroke in targetStrokes)
            {
                stroke.Transform(m, false);
            }
            if (DrawingAttributesHistory.Count > 0)
            {
                var collecion = new StrokeCollection();
                foreach (var item in DrawingAttributesHistory)
                {
                    collecion.Add(item.Key);
                }
                timeMachine.CommitStrokeDrawingAttributesHistory(DrawingAttributesHistory);
                DrawingAttributesHistory = new Dictionary<Stroke, Tuple<DrawingAttributes, DrawingAttributes>>();
                foreach (var item in DrawingAttributesHistoryFlag)
                {
                    item.Value.Clear();
                }
            }
        }

        #endregion


        #region 自定义框选（同时选中墨迹和文字）

        private Border _rubberBand;
        private bool _isRubberBandActive = false;
        private Point _rubberBandStart;

        private void RubberBand_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GridInkCanvasSelectionCover.Visibility == Visibility.Visible) return;

            var pos = e.GetPosition(inkCanvas);

            // 点击在文字上时不做框选（由 TextInput 的 handler 处理）
            if (FindTextBorderAtPoint(pos) != null) return;

            // 点击空白区域时清除文字选中
            ClearTextSelection();

            _rubberBandStart = pos;
            _isRubberBandActive = true;
            inkCanvas.CaptureMouse();
            ShowRubberBand(_rubberBandStart);
            e.Handled = true;
        }

        private void RubberBand_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRubberBandActive || _rubberBand == null) return;
            var current = e.GetPosition(inkCanvas);
            double x = Math.Min(_rubberBandStart.X, current.X);
            double y = Math.Min(_rubberBandStart.Y, current.Y);
            double w = Math.Abs(current.X - _rubberBandStart.X);
            double h = Math.Abs(current.Y - _rubberBandStart.Y);
            _rubberBand.RenderTransform = new TranslateTransform(x, y);
            _rubberBand.Width = w;
            _rubberBand.Height = h;
        }

        private void RubberBand_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isRubberBandActive) return;
            _isRubberBandActive = false;
            inkCanvas.ReleaseMouseCapture();

            var current = e.GetPosition(inkCanvas);
            double x = Math.Min(_rubberBandStart.X, current.X);
            double y = Math.Min(_rubberBandStart.Y, current.Y);
            double w = Math.Abs(current.X - _rubberBandStart.X);
            double h = Math.Abs(current.Y - _rubberBandStart.Y);
            var selRect = new Rect(x, y, w, h);

            HideRubberBand();
            PerformRubberBandSelection(selRect);
            e.Handled = true;
        }

        private void PerformRubberBandSelection(Rect selRect)
        {
            // 选中范围内的墨迹
            var selectedStrokes = new StrokeCollection();
            foreach (Stroke stroke in inkCanvas.Strokes)
            {
                var strokeBounds = stroke.GetBounds();
                if (strokeBounds.Width > 0 && strokeBounds.Height > 0 && selRect.IntersectsWith(strokeBounds))
                {
                    selectedStrokes.Add(stroke);
                }
            }
            if (selectedStrokes.Count > 0)
            {
                inkCanvas.Select(selectedStrokes);
                // 选中范围内的文字（inkCanvas_SelectionChanged 中也会调用 SelectTextsWithinStrokeBounds，
                // 但这里也显式选中，确保文字选中效果立即生效）
                SelectTextsInRect(selRect);
            }
            else
            {
                // 没有墨迹被选中，清除墨迹选中状态
                isProgramChangeStrokeSelection = true;
                inkCanvas.Select(new StrokeCollection());
                isProgramChangeStrokeSelection = false;

                // 选中范围内的文字
                SelectTextsInRect(selRect);

                // 仅选中文字时，显示覆盖层（覆盖层已支持文字拖拽）和选中矩形
                if (selectedTextBorders.Count > 0)
                {
                    ShowMultiTextSelectionRect();
                    GridInkCanvasSelectionCover.Visibility = Visibility.Visible;
                    updateBorderStrokeSelectionControlLocation();
                }
                else
                {
                    HideMultiTextSelectionRect();
                }
            }
        }

        private void ShowRubberBand(Point start)
        {
            if (_rubberBand == null)
            {
                _rubberBand = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF)),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromArgb(0x10, 0x40, 0x9E, 0xFF)),
                    IsHitTestVisible = false,
                };
            }
            _rubberBand.RenderTransform = new TranslateTransform(start.X, start.Y);
            _rubberBand.Width = 0;
            _rubberBand.Height = 0;
            if (!inkCanvas.Children.Contains(_rubberBand))
                inkCanvas.Children.Add(_rubberBand);
        }

        private void HideRubberBand()
        {
            if (_rubberBand != null && inkCanvas.Children.Contains(_rubberBand))
                inkCanvas.Children.Remove(_rubberBand);
        }

        private void RegisterRubberBandHandlers()
        {
            inkCanvas.PreviewMouseLeftButtonDown += RubberBand_MouseDown;
            inkCanvas.PreviewMouseMove += RubberBand_MouseMove;
            inkCanvas.PreviewMouseLeftButtonUp += RubberBand_MouseUp;
        }

        private void UnregisterRubberBandHandlers()
        {
            inkCanvas.PreviewMouseLeftButtonDown -= RubberBand_MouseDown;
            inkCanvas.PreviewMouseMove -= RubberBand_MouseMove;
            inkCanvas.PreviewMouseLeftButtonUp -= RubberBand_MouseUp;
        }

        #endregion


        bool isGridInkCanvasSelectionCoverMouseDown = false;
        StrokeCollection StrokesSelectionClone = new StrokeCollection();
        bool isDraggingSelection = false;
        Point selectionDragStartPoint;
        Dictionary<Stroke, Point[]> strokeDragStartPositions;

        private void GridInkCanvasSelectionCover_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 点击在选中区域外 → 取消选中并收起覆盖层
            var pos = e.GetPosition(inkCanvas);

            // 检查墨迹选中区域
            var bounds = inkCanvas.GetSelectionBounds();
            bool isOutsideStroke = !double.IsNaN(bounds.Left) &&
                (pos.X < bounds.Left - 5 || pos.X > bounds.Right + 5 || pos.Y < bounds.Top - 5 || pos.Y > bounds.Bottom + 5);

            // 检查文字选中区域（墨迹无选中时）
            bool isOutsideText = false;
            if (double.IsNaN(bounds.Left) && selectedTextBorders.Count > 0)
            {
                var textBounds = GetTextSelectionBounds();
                if (textBounds.HasValue)
                {
                    isOutsideText = pos.X < textBounds.Value.Left - 10 || pos.X > textBounds.Value.Right + 10 ||
                                    pos.Y < textBounds.Value.Top - 10 || pos.Y > textBounds.Value.Bottom + 10;
                }
            }

            if (isOutsideStroke || isOutsideText)
            {
                inkCanvas.Select(new StrokeCollection());
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                e.Handled = true;
                return;
            }

            bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (isStrokeSelectionCloneOn || isCtrlDown)
            {
                // 克隆模式（按钮开启或按住 Ctrl）：复制一份，拖动副本
                StrokeCollection strokes = inkCanvas.GetSelectedStrokes();
                isProgramChangeStrokeSelection = true;
                inkCanvas.Select(new StrokeCollection());
                StrokesSelectionClone = strokes.Clone();
                inkCanvas.Select(strokes);
                isProgramChangeStrokeSelection = false;
                inkCanvas.Strokes.Add(StrokesSelectionClone);
                // 记录克隆的初始位置用于拖动
                strokeDragStartPositions = new Dictionary<Stroke, Point[]>();
                foreach (Stroke stroke in StrokesSelectionClone)
                {
                    var points = stroke.StylusPoints.ToArray();
                    strokeDragStartPositions[stroke] = points.Select(p => new Point(p.X, p.Y)).ToArray();
                }
                // 克隆文字
                if (selectedTextBorders.Count > 0)
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
                    var last = GetLastSelectedTextBorder();
                    if (last != null)
                    {
                        last.Background = MultiSelectHighlightBrush;
                        ShowResizeHandles(last);
                    }
                }
            }
            else
            {
                strokeDragStartPositions = new Dictionary<Stroke, Point[]>();
                foreach (Stroke stroke in inkCanvas.GetSelectedStrokes())
                {
                    var points = stroke.StylusPoints.ToArray();
                    strokeDragStartPositions[stroke] = points.Select(p => new Point(p.X, p.Y)).ToArray();
                }
                // 记录文字起始位置
                textDragStartPositions.Clear();
                foreach (var b in selectedTextBorders)
                {
                    textDragStartPositions[b] = new Point(WpfCanvas.GetLeft(b), WpfCanvas.GetTop(b));
                }
            }

            isGridInkCanvasSelectionCoverMouseDown = true;
            isDraggingSelection = false;
            selectionDragStartPoint = e.GetPosition(inkCanvas);
            GridInkCanvasSelectionCover.CaptureMouse();
            e.Handled = true;
        }

        private void GridInkCanvasSelectionCover_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isGridInkCanvasSelectionCoverMouseDown) return;
            var currentPoint = e.GetPosition(inkCanvas);
            var dx = currentPoint.X - selectionDragStartPoint.X;
            var dy = currentPoint.Y - selectionDragStartPoint.Y;

            if (!isDraggingSelection && (Math.Abs(dx) > 2 || Math.Abs(dy) > 2))
            {
                isDraggingSelection = true;
            }

            if (isDraggingSelection)
            {
                foreach (var kvp in strokeDragStartPositions)
                {
                    var stroke = kvp.Key;
                    var originalPoints = kvp.Value;
                    var newPoints = new StylusPointCollection();
                    for (int i = 0; i < originalPoints.Length; i++)
                    {
                        newPoints.Add(new StylusPoint(originalPoints[i].X + dx, originalPoints[i].Y + dy));
                    }
                    stroke.StylusPoints = newPoints;
                }
                // 同步移动文字
                foreach (var b in selectedTextBorders)
                {
                    if (textDragStartPositions.TryGetValue(b, out var startPos))
                    {
                        WpfCanvas.SetLeft(b, startPos.X + dx);
                        WpfCanvas.SetTop(b, startPos.Y + dy);
                    }
                }
                updateBorderStrokeSelectionControlLocation();
            }
        }

        private void GridInkCanvasSelectionCover_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && isGridInkCanvasSelectionCoverMouseDown)
            {
                // 提交文字移动/克隆的撤销记录
                if (isDraggingSelection)
                {
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
                }

                GridInkCanvasSelectionCover.ReleaseMouseCapture();
                isGridInkCanvasSelectionCoverMouseDown = false;
                isDraggingSelection = false;
                isDraggingClone = false;
                StrokesSelectionClone = new StrokeCollection();
            }
        }

        private bool _isSelectAllActive = false;

        private bool _isInSelectionMode = false;
        private int _previousDrawingShapeMode = 0;

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            forceEraser = true;
            if (!_isInSelectionMode)
            {
                // 进入选择模式前保存当前模式
                _previousDrawingShapeMode = drawingShapeMode;
            }
            drawingShapeMode = 0;
            if (_isInSelectionMode)
            {
                if (_isSelectAllActive)
                {
                    // 全选状态 → 退出选择模式
                    ExitSelectionMode();
                }
                else
                {
                    // 部分选中状态 → 全选
                    _isSelectAllActive = true;
                    StrokeCollection selectedStrokes = new StrokeCollection();
                    foreach (Stroke stroke in inkCanvas.Strokes)
                    {
                        if (stroke.GetBounds().Width > 0 && stroke.GetBounds().Height > 0)
                        {
                            selectedStrokes.Add(stroke);
                        }
                    }
                    inkCanvas.Select(selectedStrokes);
                    SelectAllTextElements();
                }
            }
            else
            {
                // 非选择模式 → 进入选择模式
                _isSelectAllActive = false;
                _isInSelectionMode = true;
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                if (_textOverlayCanvas != null)
                    _textOverlayCanvas.Background = null;
                RegisterRubberBandHandlers();
            }
            UpdateSelectIconState();
        }

        private void ExitSelectionMode()
        {
            _isSelectAllActive = false;
            _isInSelectionMode = false;
            UnregisterRubberBandHandlers();
            HideRubberBand();
            // 恢复到进入选择模式之前的模式（如文本模式 drawingShapeMode=26）
            drawingShapeMode = _previousDrawingShapeMode;
            if (drawingShapeMode == 26)
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                inkCanvas.ForceCursor = true;
                // 使用用户设置的光标类型
                inkCanvas.Cursor = Settings.Canvas.TextCursorType == 1 ? Cursors.IBeam : Cursors.Arrow;
                // 恢复文本图标高亮
                SymbolIconText.Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF));
                ToolTipService.SetToolTip(SymbolIconText, "文本模式：点击退出文本模式 (Alt+T)");
            }
            else
            {
                inkCanvas.ForceCursor = false;
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
            inkCanvas.IsManipulationEnabled = true;
            ClearTextSelection();
            if (_textOverlayCanvas != null)
                _textOverlayCanvas.Background = Brushes.Transparent;
        }

        internal void UpdateSelectIconState()
        {
            if (!_isInSelectionMode)
            {
                // 非选择模式：默认颜色
                SymbolIconSelect.Foreground = new SolidColorBrush(FloatBarForegroundColor);
            }
            else if (_isSelectAllActive)
            {
                // 全选状态：深蓝高亮
                SymbolIconSelect.Foreground = new SolidColorBrush(Color.FromRgb(0, 136, 255));
            }
            else
            {
                // 部分选中状态：浅蓝高亮
                SymbolIconSelect.Foreground = new SolidColorBrush(Color.FromRgb(0x7C, 0xB9, 0xE8));
            }
        }

        double BorderStrokeSelectionControlWidth = 490.0;
        double BorderStrokeSelectionControlHeight = 80.0;
        bool isProgramChangeStrokeSelection = false;

        private void inkCanvas_SelectionChanged(object sender, EventArgs e)
        {
            if (isProgramChangeStrokeSelection) return;
            if (inkCanvas.GetSelectedStrokes().Count == 0)
            {
                _isSelectAllActive = false;
                UpdateSelectionControlVisibility();
                // 墨迹取消选中但还有文字时，由矩形继续管理文字选中
                if (selectedTextBorders.Count > 0) ShowMultiTextSelectionRect();
                else HideMultiTextSelectionRect();
            }
            else
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Visible;
                inkCanvas.ReleaseMouseCapture();
                BorderStrokeSelectionClone.Background = Brushes.Transparent;
                isStrokeSelectionCloneOn = false;
                SelectTextsWithinStrokeBounds();
                updateBorderStrokeSelectionControlLocation();
            }
            UpdateSelectIconState();
        }

        private void updateBorderStrokeSelectionControlLocation()
        {
            var strokeBounds = inkCanvas.GetSelectionBounds();
            double left, right, bottom;

            if (!double.IsNaN(strokeBounds.Left))
            {
                left = strokeBounds.Left;
                right = strokeBounds.Right;
                bottom = strokeBounds.Bottom;
            }
            else
            {
                var textBounds = GetTextSelectionBounds();
                if (textBounds.HasValue)
                {
                    left = textBounds.Value.Left;
                    right = textBounds.Value.Right;
                    bottom = textBounds.Value.Bottom;
                }
                else
                {
                    return;
                }
            }

            // 合并文字选区的边界
            var textBounds2 = GetTextSelectionBounds();
            if (textBounds2.HasValue)
            {
                left = Math.Min(left, textBounds2.Value.Left);
                right = Math.Max(right, textBounds2.Value.Right);
                bottom = Math.Max(bottom, textBounds2.Value.Bottom);
            }

            double borderLeft = (left + right - BorderStrokeSelectionControlWidth) / 2;
            double borderTop = bottom + 15;
            if (borderLeft < 0) borderLeft = 0;
            if (borderTop < 0) borderTop = 0;
            if (Width - borderLeft < BorderStrokeSelectionControlWidth || double.IsNaN(borderLeft)) borderLeft = Width - BorderStrokeSelectionControlWidth;
            if (Height - borderTop < BorderStrokeSelectionControlHeight || double.IsNaN(borderTop)) borderTop = Height - BorderStrokeSelectionControlHeight;
            BorderStrokeSelectionControl.Margin = new Thickness(borderLeft, borderTop, 0, 0);
        }

        private void UpdateSelectionControlPosition()
        {
            updateBorderStrokeSelectionControlLocation();
            UpdateMultiTextSelectionRectPosition();
        }

        private Rect? GetTextSelectionBounds()
        {
            if (selectedTextBorders.Count == 0) return null;
            double left = double.MaxValue, top = double.MaxValue;
            double right = double.MinValue, bottom = double.MinValue;
            foreach (var b in selectedTextBorders)
            {
                var l = WpfCanvas.GetLeft(b);
                var t = WpfCanvas.GetTop(b);
                left = Math.Min(left, l);
                top = Math.Min(top, t);
                right = Math.Max(right, l + b.ActualWidth);
                bottom = Math.Max(bottom, t + b.ActualHeight);
            }
            if (left >= right || top >= bottom) return null;
            return new Rect(left, top, right - left, bottom - top);
        }

        private void GridInkCanvasSelectionCover_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.Mode = ManipulationModes.All;
        }

        private void GridInkCanvasSelectionCover_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (StrokeManipulationHistory?.Count > 0)
            {
                timeMachine.CommitStrokeManipulationHistory(StrokeManipulationHistory);
                foreach (var item in StrokeManipulationHistory)
                {
                    StrokeInitialHistory[item.Key] = item.Value.Item2;
                }
                StrokeManipulationHistory = null;
            }
            if (DrawingAttributesHistory.Count > 0)
            {
                timeMachine.CommitStrokeDrawingAttributesHistory(DrawingAttributesHistory);
                DrawingAttributesHistory = new Dictionary<Stroke, Tuple<DrawingAttributes, DrawingAttributes>>();
                foreach (var item in DrawingAttributesHistoryFlag)
                {
                    item.Value.Clear();
                }
            }
        }

        private void GridInkCanvasSelectionCover_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            try
            {
                if (dec.Count >= 1)
                {
                    ManipulationDelta md = e.DeltaManipulation;
                    Vector trans = md.Translation;
                    double rotate = md.Rotation;
                    Vector scale = md.Scale;

                    Matrix m = new Matrix();

                    FrameworkElement fe = e.Source as FrameworkElement;
                    Point center = new Point(fe.ActualWidth / 2, fe.ActualHeight / 2);
                    center = new Point(inkCanvas.GetSelectionBounds().Left + inkCanvas.GetSelectionBounds().Width / 2,
                        inkCanvas.GetSelectionBounds().Top + inkCanvas.GetSelectionBounds().Height / 2);
                    center = m.Transform(center);

                    m.Translate(trans.X, trans.Y);
                    m.ScaleAt(scale.X, scale.Y, center.X, center.Y);

                    StrokeCollection strokes = inkCanvas.GetSelectedStrokes();
                    if (StrokesSelectionClone.Count != 0)
                    {
                        strokes = StrokesSelectionClone;
                    }
                    else if (Settings.Gesture.IsEnableTwoFingerRotationOnSelection)
                    {
                        m.RotateAt(rotate, center.X, center.Y);
                    }
                    foreach (Stroke stroke in strokes)
                    {
                        stroke.Transform(m, false);

                        try
                        {
                            stroke.DrawingAttributes.Width *= md.Scale.X;
                            stroke.DrawingAttributes.Height *= md.Scale.Y;
                        }
                        catch { }
                    }
                    // 同步移动文字
                    foreach (var b in selectedTextBorders)
                    {
                        WpfCanvas.SetLeft(b, WpfCanvas.GetLeft(b) + trans.X);
                        WpfCanvas.SetTop(b, WpfCanvas.GetTop(b) + trans.Y);
                    }
                    updateBorderStrokeSelectionControlLocation();
                }
            }
            catch { }
        }

        private void GridInkCanvasSelectionCover_TouchDown(object sender, TouchEventArgs e)
        {
        }

        private void GridInkCanvasSelectionCover_TouchUp(object sender, TouchEventArgs e)
        {
        }

        Point lastTouchPointOnGridInkCanvasCover = new Point(0, 0);
        private void GridInkCanvasSelectionCover_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            dec.Add(e.TouchDevice.Id);
            if (dec.Count == 1)
            {
                TouchPoint touchPoint = e.GetTouchPoint(null);
                centerPoint = touchPoint.Position;
                lastTouchPointOnGridInkCanvasCover = touchPoint.Position;

                if (isStrokeSelectionCloneOn)
                {
                    StrokeCollection strokes = inkCanvas.GetSelectedStrokes();
                    isProgramChangeStrokeSelection = true;
                    inkCanvas.Select(new StrokeCollection());
                    StrokesSelectionClone = strokes.Clone();
                    inkCanvas.Select(strokes);
                    isProgramChangeStrokeSelection = false;
                    inkCanvas.Strokes.Add(StrokesSelectionClone);
                }
            }
        }

        private void GridInkCanvasSelectionCover_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            dec.Remove(e.TouchDevice.Id);
            if (dec.Count >= 1) return;
            isProgramChangeStrokeSelection = false;
            if (lastTouchPointOnGridInkCanvasCover == e.GetTouchPoint(null).Position)
            {
                if (lastTouchPointOnGridInkCanvasCover.X < inkCanvas.GetSelectionBounds().Left ||
                    lastTouchPointOnGridInkCanvasCover.Y < inkCanvas.GetSelectionBounds().Top ||
                    lastTouchPointOnGridInkCanvasCover.X > inkCanvas.GetSelectionBounds().Right ||
                    lastTouchPointOnGridInkCanvasCover.Y > inkCanvas.GetSelectionBounds().Bottom)
                {
                    inkCanvas.Select(new StrokeCollection());
                    StrokesSelectionClone = new StrokeCollection();
                }
            }
            else if (inkCanvas.GetSelectedStrokes().Count == 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                StrokesSelectionClone = new StrokeCollection();
            }
            else
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Visible;
                StrokesSelectionClone = new StrokeCollection();
            }
        }

        #region 统一选择：文字 + 墨迹

        private Border FindTextBorderAtPoint(Point pos)
        {
            if (_textOverlayCanvas == null) return null;
            var result = VisualTreeHelper.HitTest(inkCanvas, pos);
            if (result != null)
            {
                var depObj = result.VisualHit;
                while (depObj != null)
                {
                    if (depObj is Border border && border.Tag as string == "TextElement")
                        return border;
                    depObj = VisualTreeHelper.GetParent(depObj);
                }
            }
            return null;
        }

        private void HandleSelectionTextClick(Border border, bool isCtrlDown, Point clickPos)
        {
            if (!isCtrlDown)
            {
                foreach (var b in selectedTextBorders) b.Background = Brushes.Transparent;
                selectedTextBorders.Clear();
                ClearResizeHandlesOnly();
                selectedTextBorders.Add(border);
                border.Background = MultiSelectHighlightBrush;
                ShowResizeHandles(border);
            }
            else
            {
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
            UpdateSelectionControlVisibility();
            if (selectedTextBorders.Count > 0) ShowMultiTextSelectionRect();
            else HideMultiTextSelectionRect();

            // 初始化拖拽状态
            draggingTextBorder = border;
            isDraggingClone = false;
            hasDraggedText = false;
            isDraggingText = true;
            textDragStartPoint = clickPos;
            textDragStartPositions.Clear();
            foreach (var b in selectedTextBorders)
            {
                textDragStartPositions[b] = new Point(WpfCanvas.GetLeft(b), WpfCanvas.GetTop(b));
            }
            border.CaptureMouse();
        }

        private void ClearTextSelection()
        {
            foreach (var b in selectedTextBorders) b.Background = Brushes.Transparent;
            selectedTextBorders.Clear();
            ClearResizeHandlesOnly();
            HideMultiTextSelectionRect();
            UpdateSelectionControlVisibility();
        }

        private void SelectTextsInRect(Rect selRect)
        {
            if (_textOverlayCanvas == null) return;
            foreach (var child in _textOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    var left = WpfCanvas.GetLeft(border);
                    var top = WpfCanvas.GetTop(border);
                    var rect = new Rect(left, top, border.ActualWidth, border.ActualHeight);
                    if (selRect.IntersectsWith(rect) && !selectedTextBorders.Contains(border))
                    {
                        selectedTextBorders.Add(border);
                        border.Background = MultiSelectHighlightBrush;
                    }
                }
            }
        }

        /// <summary>
        /// 在 GridInkCanvasSelectionCover 上显示统一的文字选中矩形
        /// 矩形直接处理鼠标拖拽（覆盖层的事件路由不可靠 for text-only）
        /// </summary>
        private Rectangle _multiTextSelectionRect = null;
        private bool _isDraggingMultiRect = false;
        private bool _isMultiRectClone = false;
        private Point _multiRectDragStart;
        private Dictionary<Border, Point> _multiRectTextStartPos = new Dictionary<Border, Point>();

        private void ShowMultiTextSelectionRect()
        {
            HideMultiTextSelectionRect();
            if (selectedTextBorders.Count == 0) return;

            var bounds = GetTextSelectionBounds();
            if (bounds == null) return;
            var b = bounds.Value;

            _multiTextSelectionRect = new Rectangle
            {
                Tag = "MultiTextSelectionRect",
                Stroke = new SolidColorBrush(Color.FromArgb(0x80, 0x40, 0x9E, 0xFF)),
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = new SolidColorBrush(Color.FromArgb(0x10, 0x40, 0x9E, 0xFF)),
                RadiusX = 3,
                RadiusY = 3,
                Cursor = Cursors.SizeAll,
                IsHitTestVisible = true,
                Width = Math.Max(b.Width + 8, 10),
                Height = Math.Max(b.Height + 8, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(b.Left - 4, b.Top - 4, 0, 0),
            };
            _multiTextSelectionRect.MouseDown += MultiTextRect_MouseDown;
            _multiTextSelectionRect.MouseMove += MultiTextRect_MouseMove;
            _multiTextSelectionRect.MouseUp += MultiTextRect_MouseUp;
            // 放在覆盖层父容器（同级而非子级），避免继承覆盖层的 Opacity="0.01"
            var parentGrid = GridInkCanvasSelectionCover.Parent as Grid;
            if (parentGrid != null)
                parentGrid.Children.Add(_multiTextSelectionRect);
        }

        private void HideMultiTextSelectionRect()
        {
            _isDraggingMultiRect = false;
            _isMultiRectClone = false;
            if (_multiTextSelectionRect != null)
            {
                var parentGrid = GridInkCanvasSelectionCover.Parent as Grid;
                if (parentGrid != null && parentGrid.Children.Contains(_multiTextSelectionRect))
                    parentGrid.Children.Remove(_multiTextSelectionRect);
                _multiTextSelectionRect.MouseDown -= MultiTextRect_MouseDown;
                _multiTextSelectionRect.MouseMove -= MultiTextRect_MouseMove;
                _multiTextSelectionRect.MouseUp -= MultiTextRect_MouseUp;
                _multiTextSelectionRect = null;
            }
        }

        private void UpdateMultiTextSelectionRectPosition()
        {
            if (_multiTextSelectionRect == null) return;
            var bounds = GetTextSelectionBounds();
            if (bounds == null) { HideMultiTextSelectionRect(); return; }
            var b = bounds.Value;
            _multiTextSelectionRect.Width = Math.Max(b.Width + 8, 10);
            _multiTextSelectionRect.Height = Math.Max(b.Height + 8, 10);
            _multiTextSelectionRect.Margin = new Thickness(b.Left - 4, b.Top - 4, 0, 0);
        }

        private void MultiTextRect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 有墨迹时由覆盖层的统一处理
            if (inkCanvas.GetSelectedStrokes().Count > 0) return;

            bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            _isMultiRectClone = false;
            _isDraggingMultiRect = false;
            _multiRectDragStart = e.GetPosition(inkCanvas);
            _multiRectTextStartPos.Clear();

            if (isCtrlDown && selectedTextBorders.Count > 0)
            {
                // Ctrl+拖拽：立即克隆所有选中的文字
                var originals = selectedTextBorders.ToList();
                foreach (var b in originals) b.Background = Brushes.Transparent;
                selectedTextBorders.Clear();
                foreach (var orig in originals)
                {
                    var clone = CloneTextBorder(orig);
                    if (clone != null)
                    {
                        selectedTextBorders.Add(clone);
                        _multiRectTextStartPos[clone] = new Point(WpfCanvas.GetLeft(clone), WpfCanvas.GetTop(clone));
                    }
                }
                _isMultiRectClone = true;
                ShowMultiTextSelectionRect();
            }
            else
            {
                foreach (var b in selectedTextBorders)
                {
                    _multiRectTextStartPos[b] = new Point(WpfCanvas.GetLeft(b), WpfCanvas.GetTop(b));
                }
            }

            _multiTextSelectionRect.CaptureMouse();
            e.Handled = true;
        }

        private void MultiTextRect_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_multiTextSelectionRect.IsMouseCaptured) return;
            var currentPoint = e.GetPosition(inkCanvas);
            var dx = currentPoint.X - _multiRectDragStart.X;
            var dy = currentPoint.Y - _multiRectDragStart.Y;

            if (!_isDraggingMultiRect && (Math.Abs(dx) > 2 || Math.Abs(dy) > 2))
                _isDraggingMultiRect = true;

            if (_isDraggingMultiRect)
            {
                foreach (var b in selectedTextBorders)
                {
                    if (_multiRectTextStartPos.TryGetValue(b, out var startPos))
                    {
                        WpfCanvas.SetLeft(b, startPos.X + dx);
                        WpfCanvas.SetTop(b, startPos.Y + dy);
                    }
                }
                UpdateMultiTextSelectionRectPosition();
            }
        }

        private void MultiTextRect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_multiTextSelectionRect.IsMouseCaptured) return;
            _multiTextSelectionRect.ReleaseMouseCapture();

            if (_isDraggingMultiRect)
            {
                foreach (var b in selectedTextBorders)
                {
                    if (_multiRectTextStartPos.TryGetValue(b, out var startPos))
                    {
                        var newLeft = WpfCanvas.GetLeft(b);
                        var newTop = WpfCanvas.GetTop(b);
                        if (Math.Abs(newLeft - startPos.X) > 1 || Math.Abs(newTop - startPos.Y) > 1)
                        {
                            if (_isMultiRectClone)
                            {
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
            }
            _isDraggingMultiRect = false;
            _isMultiRectClone = false;
            _multiRectTextStartPos.Clear();
            e.Handled = true;
        }

        private void SelectTextsWithinStrokeBounds()
        {
            if (_textOverlayCanvas == null) return;
            var bounds = inkCanvas.GetSelectionBounds();
            if (double.IsNaN(bounds.Left)) return;

            foreach (var child in _textOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    var left = System.Windows.Controls.Canvas.GetLeft(border);
                    var top = System.Windows.Controls.Canvas.GetTop(border);
                    var rect = new Rect(left, top, border.ActualWidth, border.ActualHeight);
                    if (bounds.IntersectsWith(rect) && !selectedTextBorders.Contains(border))
                    {
                        selectedTextBorders.Add(border);
                        border.Background = MultiSelectHighlightBrush;
                    }
                }
            }
            UpdateSelectionControlVisibility();
            // 仅无墨迹选中时显示矩形（有墨迹时由覆盖层处理拖拽）
            if (inkCanvas.GetSelectedStrokes().Count == 0) ShowMultiTextSelectionRect();
        }

        private void SelectAllTextElements()
        {
            if (_textOverlayCanvas == null) return;
            foreach (var child in _textOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    if (!selectedTextBorders.Contains(border))
                    {
                        selectedTextBorders.Add(border);
                        border.Background = MultiSelectHighlightBrush;
                    }
                }
            }
            UpdateSelectionControlVisibility();
            if (inkCanvas.GetSelectedStrokes().Count == 0) ShowMultiTextSelectionRect();
        }

        private void UpdateSelectionControlVisibility()
        {
            bool hasStrokeSelection = inkCanvas.GetSelectedStrokes().Count > 0;
            bool hasTextSelection = selectedTextBorders.Count > 0;
            BorderStrokeSelectionControl.Visibility = (hasStrokeSelection || hasTextSelection)
                ? Visibility.Visible : Visibility.Collapsed;
            if (hasStrokeSelection || hasTextSelection)
                GridInkCanvasSelectionCover.Visibility = Visibility.Visible;
            else
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
            if (hasStrokeSelection || hasTextSelection)
                UpdateSelectionControlPosition();
        }

        private void DeleteSelectedTextBorders()
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
            HideMultiTextSelectionRect();
        }

        #endregion

        #endregion Selection Gestures
    }
}
