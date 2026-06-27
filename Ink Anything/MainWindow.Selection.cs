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

            _strokeTransformService.ChangeStrokeThickness(0.8);
            CommitDrawingAttributesHistory();
        }

        private void GridPenWidthIncrease_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            _strokeTransformService.ChangeStrokeThickness(1.25);
            CommitDrawingAttributesHistory();
        }

        private void GridPenWidthRestore_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            _strokeTransformService.RestoreStrokeThickness();
        }

        private void ImageFlipHorizontal_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            _strokeTransformService.FlipHorizontal();
            CommitDrawingAttributesHistory();
        }

        private void ImageFlipVertical_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            _strokeTransformService.FlipVertical();
            CommitDrawingAttributesHistory();
        }

        private void ImageRotate45_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            _strokeTransformService.Rotate45();
            CommitDrawingAttributesHistory();
        }

        private void ImageRotate90_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender) return;

            _strokeTransformService.Rotate90();
            CommitDrawingAttributesHistory();
        }

        private void CommitDrawingAttributesHistory()
        {
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

        #endregion


        #region 自定义框选（同时选中墨迹和文字）

        private Border _rubberBand;
        private bool _isRubberBandActive = false;
        private Point _rubberBandStart;

        private void RubberBand_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GridInkCanvasSelectionCover.Visibility == Visibility.Visible) return;

            var pos = e.GetPosition(inkCanvas);

            // 墨迹选择模式下点击文字不做特殊处理（由 rubber band 统一处理）
            if (_selectionScope != SelectionScope.Ink)
            {
                // 点击在文字上时不做框选（由 TextInput 的 handler 处理）
                if (FindTextBorderAtPoint(pos) != null) return;
            }

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
            if (_selectionScope == SelectionScope.Text)
            {
                // 文字选择模式：只选中范围内的文字
                isProgramChangeStrokeSelection = true;
                inkCanvas.Select(new StrokeCollection());
                isProgramChangeStrokeSelection = false;

                SelectTextsInRect(selRect);

                if (_textManager.SelectedTextBorders.Count > 0)
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
            else
            {
                // 墨迹选择模式：只选中范围内的墨迹
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
                }
                else
                {
                    isProgramChangeStrokeSelection = true;
                    inkCanvas.Select(new StrokeCollection());
                    isProgramChangeStrokeSelection = false;
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
            if (double.IsNaN(bounds.Left) && _textManager.SelectedTextBorders.Count > 0)
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
                _textManager.TextDragStartPositions.Clear();
                foreach (var b in _textManager.SelectedTextBorders)
                {
                    _textManager.TextDragStartPositions[b] = new Point(WpfCanvas.GetLeft(b), WpfCanvas.GetTop(b));
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
                updateBorderStrokeSelectionControlLocation();
            }
        }

        private void GridInkCanvasSelectionCover_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released && isGridInkCanvasSelectionCoverMouseDown)
            {
                GridInkCanvasSelectionCover.ReleaseMouseCapture();
                isGridInkCanvasSelectionCoverMouseDown = false;
                isDraggingSelection = false;
                StrokesSelectionClone = new StrokeCollection();
            }
        }

        private enum SelectionScope { Ink, Text }
        private SelectionScope _selectionScope = SelectionScope.Ink;

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
                // 已在选择模式 → 退出（全选由 Ctrl+A 触发）
                ExitSelectionMode();
            }
            else
            {
                // 非选择模式 → 进入选择模式
                // 根据进入前的模式决定选择范围：文本模式(26)进来只选文字，否则只选墨迹
                _selectionScope = (_previousDrawingShapeMode == 26) ? SelectionScope.Text : SelectionScope.Ink;
                _isSelectAllActive = false;
                _isInSelectionMode = true;
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                if (_textManager.TextOverlayCanvas != null)
                    _textManager.TextOverlayCanvas.Background = (_selectionScope == SelectionScope.Text) ? Brushes.Transparent : null;
                RegisterRubberBandHandlers();
                // 更新文本图标：文本选择范围用浅蓝高亮，墨迹选择范围恢复默认
                if (_selectionScope == SelectionScope.Text)
                {
                    SymbolIconText.Foreground = new SolidColorBrush(Color.FromRgb(0x7C, 0xB9, 0xE8));
                    ToolTipService.SetToolTip(SymbolIconText, $"文本选择：再次点击进入墨迹选择 ({Settings.Hotkeys.TextMode})");
                }
                else
                {
                    SymbolIconText.Foreground = (Brush)FindResource("FloatBarForeground");
                    ToolTipService.SetToolTip(SymbolIconText, $"文本输入：点击进入文本模式 ({Settings.Hotkeys.TextMode})");
                }
            }
            UpdateSelectIconState();
        }

        private void ExitSelectionMode()
        {
            _isSelectAllActive = false;
            _isInSelectionMode = false;
            UnregisterRubberBandHandlers();
            HideRubberBand();
            // 恢复到进入选择模式之前的模式
            drawingShapeMode = _previousDrawingShapeMode;
            if (drawingShapeMode == 26)
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                inkCanvas.ForceCursor = true;
                // 使用用户设置的光标类型
                inkCanvas.Cursor = Settings.Canvas.TextCursorType == 1 ? Cursors.IBeam : Cursors.Arrow;
                // 恢复文本图标高亮
                SymbolIconText.Foreground = new SolidColorBrush(Color.FromRgb(0x40, 0x9E, 0xFF));
                ToolTipService.SetToolTip(SymbolIconText, $"文本模式：点击退出文本模式 ({Settings.Hotkeys.TextMode})");
            }
            else
            {
                inkCanvas.ForceCursor = false;
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                // 退出到非文本模式时，恢复文本图标默认颜色
                SymbolIconText.Foreground = (Brush)FindResource("FloatBarForeground");
                ToolTipService.SetToolTip(SymbolIconText, $"文本输入：点击进入文本模式 ({Settings.Hotkeys.TextMode})");
            }
            inkCanvas.IsManipulationEnabled = true;
            ClearTextSelection();
            if (_textManager.TextOverlayCanvas != null)
                _textManager.TextOverlayCanvas.Background = Brushes.Transparent;
        }

        /// <summary>
        /// 在选择模式中切换选择范围（墨迹↔文字），切换到目标范围的部分选中状态
        /// </summary>
        private void SwitchSelectionScope(SelectionScope newScope)
        {
            if (!_isInSelectionMode || _selectionScope == newScope) return;

            // 清除当前所有选中
            isProgramChangeStrokeSelection = true;
            inkCanvas.Select(new StrokeCollection());
            isProgramChangeStrokeSelection = false;
            ClearTextSelection();
            HideMultiTextSelectionRect();

            _selectionScope = newScope;
            _isSelectAllActive = false;

            // 同步更新退出选择模式时恢复的模式
            _previousDrawingShapeMode = (newScope == SelectionScope.Text) ? 26 : 0;

            // 更新文本覆盖层点击穿透（文字选择模式需要可点击，墨迹选择模式不需要）
            if (_textManager.TextOverlayCanvas != null)
                _textManager.TextOverlayCanvas.Background = (newScope == SelectionScope.Text) ? Brushes.Transparent : null;

            // 更新光标
            if (newScope == SelectionScope.Text)
            {
                inkCanvas.ForceCursor = true;
                inkCanvas.Cursor = Cursors.Arrow;
                // 文本选择范围：浅蓝高亮文本图标
                SymbolIconText.Foreground = new SolidColorBrush(Color.FromRgb(0x7C, 0xB9, 0xE8));
                ToolTipService.SetToolTip(SymbolIconText, "文本选择：再次点击进入墨迹选择 (Alt+T)");
            }
            else
            {
                inkCanvas.ForceCursor = false;
                // 墨迹选择范围：恢复文本图标默认颜色
                SymbolIconText.Foreground = (Brush)FindResource("FloatBarForeground");
                ToolTipService.SetToolTip(SymbolIconText, $"文本输入：点击进入文本模式 ({Settings.Hotkeys.TextMode})");
            }

            UpdateSelectionControlVisibility();
            UpdateSelectIconState();
        }

        internal void UpdateSelectIconState()
        {
            if (!_isInSelectionMode)
            {
                // 非选择模式：默认颜色
                SymbolIconSelect.Foreground = new SolidColorBrush(FloatBarForegroundColor);
                ToolTipService.SetToolTip(SymbolIconSelect, $"选择：点击进入选择模式，再点一次退出 ({Settings.Hotkeys.SelectMode}，Ctrl+A 全选)");
            }
            else
            {
                // 选择模式：浅蓝高亮
                SymbolIconSelect.Foreground = new SolidColorBrush(Color.FromRgb(0x7C, 0xB9, 0xE8));
                string mode = _selectionScope == SelectionScope.Text ? "文字" : "墨迹";
                ToolTipService.SetToolTip(SymbolIconSelect, $"选择模式（仅{ mode }）：点击退出选择模式 ({Settings.Hotkeys.SelectMode}，Ctrl+A 全选)");
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
                if (_textManager.SelectedTextBorders.Count > 0) ShowMultiTextSelectionRect();
                else HideMultiTextSelectionRect();
            }
            else
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Visible;
                inkCanvas.ReleaseMouseCapture();
                BorderStrokeSelectionClone.Background = Brushes.Transparent;
                isStrokeSelectionCloneOn = false;
                BorderStrokeSelectionControl.Visibility = Visibility.Visible;
                if (_selectionScope != SelectionScope.Text)
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
            if (_textManager.SelectedTextBorders.Count == 0) return null;
            double left = double.MaxValue, top = double.MaxValue;
            double right = double.MinValue, bottom = double.MinValue;
            foreach (var b in _textManager.SelectedTextBorders)
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
                    foreach (var b in _textManager.SelectedTextBorders)
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
            if (_textManager.TextOverlayCanvas == null) return null;
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
                foreach (var b in _textManager.SelectedTextBorders) b.Background = Brushes.Transparent;
                _textManager.SelectedTextBorders.Clear();
                ClearResizeHandlesOnly();
                _textManager.SelectedTextBorders.Add(border);
                border.Background = TextManager.MultiSelectHighlightBrush;
                ShowResizeHandles(border);
            }
            else
            {
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
            UpdateSelectionControlVisibility();
            if (_textManager.SelectedTextBorders.Count > 0) ShowMultiTextSelectionRect();
            else HideMultiTextSelectionRect();

            // 初始化拖拽状态
            draggingTextBorder = border;
            hasDraggedText = false;
            isDraggingText = true;
            textDragStartPoint = clickPos;
            _textManager.TextDragStartPositions.Clear();
            foreach (var b in _textManager.SelectedTextBorders)
            {
                _textManager.TextDragStartPositions[b] = new Point(WpfCanvas.GetLeft(b), WpfCanvas.GetTop(b));
            }
            border.CaptureMouse();
        }

        private void ClearTextSelection()
        {
            foreach (var b in _textManager.SelectedTextBorders) b.Background = Brushes.Transparent;
            _textManager.SelectedTextBorders.Clear();
            ClearResizeHandlesOnly();
            HideMultiTextSelectionRect();
            UpdateSelectionControlVisibility();
        }

        private void SelectTextsInRect(Rect selRect)
        {
            if (_textManager.TextOverlayCanvas == null) return;
            foreach (var child in _textManager.TextOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    var left = WpfCanvas.GetLeft(border);
                    var top = WpfCanvas.GetTop(border);
                    var rect = new Rect(left, top, border.ActualWidth, border.ActualHeight);
                    if (selRect.IntersectsWith(rect) && !_textManager.SelectedTextBorders.Contains(border))
                    {
                        _textManager.SelectedTextBorders.Add(border);
                        border.Background = TextManager.MultiSelectHighlightBrush;
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
        private Point _multiRectDragStart;
        private Dictionary<Border, Point> _multiRectTextStartPos = new Dictionary<Border, Point>();

        private void ShowMultiTextSelectionRect()
        {
            HideMultiTextSelectionRect();
            if (_textManager.SelectedTextBorders.Count == 0) return;

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

            _isDraggingMultiRect = false;
            _multiRectDragStart = e.GetPosition(inkCanvas);
            _multiRectTextStartPos.Clear();

            foreach (var b in _textManager.SelectedTextBorders)
            {
                _multiRectTextStartPos[b] = new Point(WpfCanvas.GetLeft(b), WpfCanvas.GetTop(b));
            }

            _multiTextSelectionRect?.CaptureMouse();
            e.Handled = true;
        }

        private void MultiTextRect_MouseMove(object sender, MouseEventArgs e)
        {
            if (_multiTextSelectionRect == null || !_multiTextSelectionRect.IsMouseCaptured) return;
            var currentPoint = e.GetPosition(inkCanvas);
            var dx = currentPoint.X - _multiRectDragStart.X;
            var dy = currentPoint.Y - _multiRectDragStart.Y;

            if (!_isDraggingMultiRect && (Math.Abs(dx) > 2 || Math.Abs(dy) > 2))
                _isDraggingMultiRect = true;

            if (_isDraggingMultiRect)
            {
                foreach (var b in _textManager.SelectedTextBorders)
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
            if (_multiTextSelectionRect == null || !_multiTextSelectionRect.IsMouseCaptured) return;
            _multiTextSelectionRect.ReleaseMouseCapture();

            if (_isDraggingMultiRect)
            {
                foreach (var b in _textManager.SelectedTextBorders)
                {
                    if (_multiRectTextStartPos.TryGetValue(b, out var startPos))
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
            }

            _isDraggingMultiRect = false;
            _multiRectTextStartPos.Clear();
            e.Handled = true;
        }

        private void SelectTextsWithinStrokeBounds()
        {
            if (_selectionScope == SelectionScope.Ink) return;
            if (_textManager.TextOverlayCanvas == null) return;
            var bounds = inkCanvas.GetSelectionBounds();
            if (double.IsNaN(bounds.Left)) return;

            foreach (var child in _textManager.TextOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    var left = System.Windows.Controls.Canvas.GetLeft(border);
                    var top = System.Windows.Controls.Canvas.GetTop(border);
                    var rect = new Rect(left, top, border.ActualWidth, border.ActualHeight);
                    if (bounds.IntersectsWith(rect) && !_textManager.SelectedTextBorders.Contains(border))
                    {
                        _textManager.SelectedTextBorders.Add(border);
                        border.Background = TextManager.MultiSelectHighlightBrush;
                    }
                }
            }
            UpdateSelectionControlVisibility();
            // 仅无墨迹选中时显示矩形（有墨迹时由覆盖层处理拖拽）
            if (inkCanvas.GetSelectedStrokes().Count == 0) ShowMultiTextSelectionRect();
        }

        private void SelectAllTextElements()
        {
            if (_textManager.TextOverlayCanvas == null) return;
            foreach (var child in _textManager.TextOverlayCanvas.Children)
            {
                if (child is Border border && border.Tag as string == "TextElement")
                {
                    if (!_textManager.SelectedTextBorders.Contains(border))
                    {
                        _textManager.SelectedTextBorders.Add(border);
                        border.Background = TextManager.MultiSelectHighlightBrush;
                    }
                }
            }
            UpdateSelectionControlVisibility();
            if (inkCanvas.GetSelectedStrokes().Count == 0) ShowMultiTextSelectionRect();
        }

        private void UpdateSelectionControlVisibility()
        {
            bool hasStrokeSelection = inkCanvas.GetSelectedStrokes().Count > 0;
            bool hasTextSelection = _textManager.SelectedTextBorders.Count > 0;
            // 墨迹悬浮工具栏（复制、旋转、镜像等）只在墨迹选中时显示
            BorderStrokeSelectionControl.Visibility = hasStrokeSelection
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
            HideMultiTextSelectionRect();
        }

        #endregion

        #endregion Selection Gestures
    }
}
