using Ink_Anything.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace Ink_Anything
{
    public partial class MainWindow : Window
    {
        #region Ink Anything Functions

        DrawingAttributes drawingAttributes;
        private void loadPenCanvas()
        {
            SetColors();
            try
            {
                //drawingAttributes = new DrawingAttributes();
                drawingAttributes = inkCanvas.DefaultDrawingAttributes;
                drawingAttributes.Color = ColorPenRed;

                drawingAttributes.Height = 2.5;
                drawingAttributes.Width = 2.5;

                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                inkCanvas.Gesture += InkCanvas_Gesture;
            }
            catch { }
        }
        //ApplicationGesture lastApplicationGesture = ApplicationGesture.AllGestures;
        DateTime lastGestureTime = DateTime.Now;
        private void InkCanvas_Gesture(object sender, InkCanvasGestureEventArgs e)
        {
            ReadOnlyCollection<GestureRecognitionResult> gestures = e.GetGestureRecognitionResults();
            try
            {
                foreach (GestureRecognitionResult gest in gestures)
                {
                    //Trace.WriteLine(string.Format("Gesture: {0}, Confidence: {1}", gest.ApplicationGesture, gest.RecognitionConfidence));
                    if (PptNavigationBtn.Visibility == Visibility.Visible)
                    {
                        if (gest.ApplicationGesture == ApplicationGesture.Left)
                        {
                            BtnPPTSlidesDown_Click(null, null);
                        }
                        if (gest.ApplicationGesture == ApplicationGesture.Right)
                        {
                            BtnPPTSlidesUp_Click(null, null);
                        }
                    }
                }
            }
            catch { }
        }

        private void inkCanvas_EditingModeChanged(object sender, RoutedEventArgs e)
        {
            var inkCanvas1 = sender as InkCanvas;
            if (inkCanvas1 == null) return;
            if (Settings.Canvas.IsShowCursor)
            {
                if (inkCanvas1.EditingMode == InkCanvasEditingMode.Ink)
                {
                    inkCanvas1.Cursor = Cursors.Pen;
                    inkCanvas1.ForceCursor = true;
                }
                else if (drawingShapeMode == 26) // 文本模式
                {
                    switch (Settings.Canvas.TextCursorType)
                    {
                        case 1: inkCanvas1.Cursor = Cursors.IBeam; break;
                        default: inkCanvas1.Cursor = Cursors.Arrow; break;
                    }
                    inkCanvas1.ForceCursor = true;
                }
                else if (drawingShapeMode != 0) // 其他绘图模式
                {
                    inkCanvas1.Cursor = Cursors.Pen;
                    inkCanvas1.ForceCursor = true;
                }
                else
                {
                    inkCanvas1.ForceCursor = false;
                }
            }
            else
            {
                inkCanvas1.ForceCursor = false;
            }
            if (inkCanvas1.EditingMode == InkCanvasEditingMode.Ink) forcePointEraser = !forcePointEraser;

            UpdateSelectIconState();
        }

        #endregion Ink Anything

        #region Hotkeys

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (PptNavigationBtn.Visibility != Visibility.Visible || currentMode != 0) return;
            if (e.Delta >= 120)
            {
                BtnPPTSlidesUp_Click(null, null);
            }
            else if (e.Delta <= -120)
            {
                BtnPPTSlidesDown_Click(null, null);
            }
        }

        private void Main_Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Q || e.SystemKey == Key.Q) && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                BtnSelect_Click(null, null);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                KeySelectAll(sender, null);
                e.Handled = true;
                return;
            }

            if (PptNavigationBtn.Visibility != Visibility.Visible || currentMode != 0) return;

            if (e.Key == Key.Down || e.Key == Key.PageDown || e.Key == Key.Right || e.Key == Key.N || e.Key == Key.Space)
            {
                BtnPPTSlidesDown_Click(null, null);
            }
            if (e.Key == Key.Up || e.Key == Key.PageUp || e.Key == Key.Left || e.Key == Key.P)
            {
                BtnPPTSlidesUp_Click(null, null);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (drawingShapeMode == 26)
                {
                    ExitTextMode();
                    e.Handled = true;
                    return;
                }
                KeyExit(null, null);
            }
            if (drawingShapeMode == 26)
            {
                HandleTextModeKeyDown(e);
            }
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                var selected = inkCanvas.GetSelectedStrokes();
                if (selected.Count > 0)
                {
                    inkCanvas.Strokes.Remove(selected);
                    GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void back_HotKey(object sender, ExecutedRoutedEventArgs e)
        {
            BtnUndo_Click(sender, e);
        }

        private void redo_HotKey(object sender, ExecutedRoutedEventArgs e)
        {
            BtnRedo_Click(sender, e);
        }

        private void KeyExit(object sender, ExecutedRoutedEventArgs e)
        {
            BtnPPTSlideShowEnd_Click(null, null);
        }

        private void KeyChangeToDrawTool(object sender, ExecutedRoutedEventArgs e)
        {
            BtnHideInkCanvas_Click(sender, e);
        }

        private void KeyClear(object sender, ExecutedRoutedEventArgs e)
        {
            BtnClear_Click(null, null);
        }

        private void KeyText(object sender, ExecutedRoutedEventArgs e)
        {
            SymbolIconText_MouseUp(sender, null);
        }

        private void KeySelectAll(object sender, ExecutedRoutedEventArgs e)
        {
            if (inkCanvas.Strokes.Count == 0) return;
            inkCanvas.EditingMode = InkCanvasEditingMode.Select;
            _isSelectAllActive = true;
            StrokeCollection allStrokes = new StrokeCollection();
            foreach (Stroke stroke in inkCanvas.Strokes)
            {
                if (stroke.GetBounds().Width > 0 && stroke.GetBounds().Height > 0)
                {
                    allStrokes.Add(stroke);
                }
            }
            if (allStrokes.Count > 0)
            {
                inkCanvas.Select(allStrokes);
            }
            UpdateSelectIconState();
        }

        private void KeyChangeToSelect(object sender, ExecutedRoutedEventArgs e)
        {
            LogHelper.NewLog("KeyChangeToSelect triggered");
            SymbolIconSelect_MouseUp(null, null);
        }

        private void KeyChangeToEraser(object sender, ExecutedRoutedEventArgs e)
        {
            if (ImageEraserMask.Visibility == Visibility.Visible)
            {
                BorderPenColorRed_MouseUp(null, null);
            }
            else
            {
                BtnErase_Click(sender, e);
            }
        }

        private void KeyCapture(object sender, ExecutedRoutedEventArgs e)
        {
            BtnScreenshot_Click(sender, e);
        }

        private void KeyDrawLine(object sender, ExecutedRoutedEventArgs e)
        {
            SetColorByIndex();
            BtnDrawLine_Click(lastMouseDownSender, e);
        }

        private void KeyHide(object sender, ExecutedRoutedEventArgs e)
        {
            SymbolIconEmoji_MouseUp(sender, null);
        }

        private void KeyChangeToPen1(object sender, ExecutedRoutedEventArgs e)
        {
            BtnColorBlack_Click(null, null);
        }

        private void KeyChangeToPen2(object sender, ExecutedRoutedEventArgs e)
        {
            BtnColorRed_Click(null, null);
        }

        private void KeyChangeToPen3(object sender, ExecutedRoutedEventArgs e)
        {
            BtnColorGreen_Click(null, null);
        }

        private void KeyChangeToPen4(object sender, ExecutedRoutedEventArgs e)
        {
            BtnColorBlue_Click(null, null);
        }

        private void KeyChangeToPen5(object sender, ExecutedRoutedEventArgs e)
        {
            BtnColorYellow_Click(null, null);
        }

        private void KeyChangeToPen6(object sender, ExecutedRoutedEventArgs e)
        {
            BorderPenColorWhite_MouseUp(null, null);
        }

        #endregion Hotkeys

        #region Left Side Panel

        #region Other Controls

        private void BtnPenWidthDecrease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InkWidthSlider.Value -= 1;
            }
            catch { }
        }

        private void BtnPenWidthIncrease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InkWidthSlider.Value += 1;
            }
            catch { }
        }


        private void BtnFingerDragMode_Click(object sender, RoutedEventArgs e)
        {
            if (isSingleFingerDragMode)
            {
                isSingleFingerDragMode = false;
            }
            else
            {
                isSingleFingerDragMode = true;
            }
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                inkCanvas.Select(new StrokeCollection());
            }
            if (TryUndoText()) return;
            var item = timeMachine.Undo();
            if (item != null) ApplyHistoryToCanvas(item);
        }
        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                inkCanvas.Select(new StrokeCollection());
            }
            if (TryRedoText()) return;
            var item = timeMachine.Redo();
            if (item != null) ApplyHistoryToCanvas(item);
        }
        private void Btn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!isLoaded) return;
            try
            {
                if (((Button)sender).IsEnabled)
                {
                    ((UIElement)((Button)sender).Content).Opacity = 1;
                }
                else
                {
                    ((UIElement)((Button)sender).Content).Opacity = 0.25;
                }
            }
            catch { }
        }
        #endregion Other Controls

        #endregion Left Side Panel
    }
}
