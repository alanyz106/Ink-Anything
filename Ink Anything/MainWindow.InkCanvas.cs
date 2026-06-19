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
                drawingAttributes.Color = ((SolidColorBrush)BtnColorRed.Background).Color;

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
                    if (StackPanelPPTControls.Visibility == Visibility.Visible)
                    {
                        if (gest.ApplicationGesture == ApplicationGesture.Left)
                        {
                            BtnPPTSlidesDown_Click(BtnPPTSlidesDown, null);
                        }
                        if (gest.ApplicationGesture == ApplicationGesture.Right)
                        {
                            BtnPPTSlidesUp_Click(BtnPPTSlidesUp, null);
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
                if (inkCanvas1.EditingMode == InkCanvasEditingMode.Ink || drawingShapeMode != 0)
                {
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

            if (inkCanvas.EditingMode == InkCanvasEditingMode.Select)
            {
                SymbolIconSelect.Foreground = new SolidColorBrush(Color.FromRgb(0, 136, 255));
            }
            else
            {
                SymbolIconSelect.Foreground = new SolidColorBrush(FloatBarForegroundColor);
            }
        }

        #endregion Ink Anything

        #region Hotkeys

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (StackPanelPPTControls.Visibility != Visibility.Visible || currentMode != 0) return;
            if (e.Delta >= 120)
            {
                BtnPPTSlidesUp_Click(BtnPPTSlidesUp, null);
            }
            else if (e.Delta <= -120)
            {
                BtnPPTSlidesDown_Click(BtnPPTSlidesDown, null);
            }
        }

        private void Main_Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (StackPanelPPTControls.Visibility != Visibility.Visible || currentMode != 0) return;

            if (e.Key == Key.Down || e.Key == Key.PageDown || e.Key == Key.Right || e.Key == Key.N || e.Key == Key.Space)
            {
                BtnPPTSlidesDown_Click(BtnPPTSlidesDown, null);
            }
            if (e.Key == Key.Up || e.Key == Key.PageUp || e.Key == Key.Left || e.Key == Key.P)
            {
                BtnPPTSlidesUp_Click(BtnPPTSlidesUp, null);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                KeyExit(null, null);
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void back_HotKey(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                inkCanvas.Strokes.Remove(inkCanvas.Strokes[inkCanvas.Strokes.Count - 1]);
            }
            catch { }
        }

        private void KeyExit(object sender, ExecutedRoutedEventArgs e)
        {
            BtnPPTSlideShowEnd_Click(BtnPPTSlideShowEnd, null);
        }

        private void KeyChangeToDrawTool(object sender, ExecutedRoutedEventArgs e)
        {
            if (inkCanvas.Visibility == Visibility.Collapsed)
            {
                BtnHideInkCanvas_Click(sender, e);
            }
        }

        private void KeyChangeToSelect(object sender, ExecutedRoutedEventArgs e)
        {
            if (inkCanvas.Visibility == Visibility.Visible)
            {
                BtnHideInkCanvas_Click(sender, e);
            }
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
                BtnFingerDragMode.Content = "单指\n拖动";
            }
            else
            {
                isSingleFingerDragMode = true;
                BtnFingerDragMode.Content = "多指\n拖动";
            }
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                inkCanvas.Select(new StrokeCollection());
            }
            var item = timeMachine.Undo();
            ApplyHistoryToCanvas(item);
        }
        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                inkCanvas.Select(new StrokeCollection());
            }
            var item = timeMachine.Redo();
            ApplyHistoryToCanvas(item);
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
