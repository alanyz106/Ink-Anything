using Ink_Anything.Helpers;
using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Application = System.Windows.Application;

namespace Ink_Anything
{
    public partial class MainWindow : Window
    {
        #region Right Side Panel

        public static bool CloseIsFromButton = false;
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            CloseIsFromButton = true;
            Close();
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");

            CloseIsFromButton = true;
            Application.Current.Shutdown();
        }

        private async void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (BorderSettings.Tag as Visibility? == Visibility.Visible)
            {
                BorderSettings.Tag = Visibility.Collapsed;
                BorderSettings.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(50)));
                await Task.Delay(60);
                BorderSettings.Visibility = Visibility.Collapsed;
            }
            else
            {
                // 将设置面板定位在悬浮工具栏上方
                double toolbarLeft = ViewboxFloatingBar.Margin.Left;
                double toolbarTop = ViewboxFloatingBar.Margin.Top;
                double toolbarWidth = ViewboxFloatingBar.ActualWidth > 0
                    ? ViewboxFloatingBar.ActualWidth : 284;

                double settingsLeft = toolbarLeft + (toolbarWidth - 400) / 2;
                double settingsTop = toolbarTop - 500 - 15;

                // 不超出屏幕边界
                settingsLeft = Math.Max(5, Math.Min(settingsLeft,
                    SystemParameters.WorkArea.Width - 400 - 5));
                settingsTop = Math.Max(5, settingsTop);

                BorderSettings.Margin = new Thickness(settingsLeft, settingsTop, 0, 0);

                BorderSettings.Tag = Visibility.Visible;
                BorderSettings.Visibility = Visibility.Visible;
                BorderSettings.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)));

                // 不要问为什么
                await Task.Delay(160);
                BorderSettings.Visibility = Visibility.Visible;
            }
        }

        private void BtnThickness_Click(object sender, RoutedEventArgs e)
        {

        }

        bool forceEraser = false;

        private void BtnErase_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint)
            {
                // 第三态：退出橡皮擦，回到画笔
                forceEraser = false;
                forcePointEraser = false;
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                drawingShapeMode = 0;
                ImageEraser.Visibility = Visibility.Visible;
                GeometryDrawingEraser.Brush = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
                ToolTipService.SetToolTip(EraserContainer, "橡皮擦：点击切换到笔画擦除");
            }
            else if (inkCanvas.EditingMode == InkCanvasEditingMode.EraseByStroke)
            {
                // 第二态：部分擦除
                forcePointEraser = true;
                inkCanvas.EraserShape = new EllipseStylusShape(50, 50);
                inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                GeometryDrawingEraser.Brush = new SolidColorBrush(Color.FromRgb(0x23, 0xA9, 0xF2));
                ImageEraser.Visibility = Visibility.Collapsed;
                ToolTipService.SetToolTip(EraserContainer, "部分擦除：点击退出橡皮擦");
            }
            else
            {
                // 第一态：笔画擦除
                forceEraser = true;
                forcePointEraser = false;
                inkCanvas.EraserShape = new EllipseStylusShape(5, 5);
                inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                drawingShapeMode = 0;
                GeometryDrawingEraser.Brush = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
                ImageEraser.Visibility = Visibility.Collapsed;
                ToolTipService.SetToolTip(EraserContainer, "笔画擦除：点击切换到部分擦除");
            }
            inkCanvas_EditingModeChanged(inkCanvas, null);
            CancelSingleFingerDragMode();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            forceEraser = false;
            BorderClearInDelete.Visibility = Visibility.Collapsed;

            if (currentMode == 0)
            {
                BorderPenColorRed_MouseUp(BorderPenColorRed, null);
            }
            else
            {
                if (Settings.Canvas.UsingWhiteboard)
                {
                    BorderPenColorBlack_MouseUp(BorderPenColorBlack, null);
                }
                else
                {
                    BorderPenColorWhite_MouseUp(BorderPenColorWhite, null);
                }
            }
            if (inkCanvas.Strokes.Count != 0)
            {
                int whiteboardIndex = CurrentWhiteboardIndex;
                if (currentMode == 0)
                {
                    whiteboardIndex = 0;
                }
                strokeCollections[whiteboardIndex] = inkCanvas.Strokes.Clone();

            }

            ClearStrokes(false);
            ClearTextElementsFromCanvas();

            CancelSingleFingerDragMode();
        }

        private void BtnClear_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BtnHideInkCanvas_Click(null, null);
        }

        private void CancelSingleFingerDragMode()
        {
            if (ToggleSwitchDrawShapeBorderAutoHide.IsOn)
            {
                BorderDrawShape.Visibility = Visibility.Collapsed;
            }
            GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
            //Label.Content = "isSingleFingerDragMode=" + isSingleFingerDragMode.ToString();
            if (isSingleFingerDragMode)
            {
                BtnFingerDragMode_Click(null, null);
            }
            isLongPressSelected = false;
        }

        private void BtnHideControl_Click(object sender, RoutedEventArgs e)
        {
        }

        int currentMode = 0;

        private void BtnSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (Main_Grid.Background == Brushes.Transparent)
            {
                if (currentMode == 0)
                {
                    currentMode++;
                    GridBackgroundCover.Visibility = Visibility.Collapsed;

                    SaveStrokes(true);
                    ClearStrokes(true);
                    RestoreStrokes();
                }
                Topmost = true;
                BtnHideInkCanvas_Click(null, e);
            }
            else
            {
                switch ((++currentMode) % 2)
                {
                    case 0: //屏幕模式
                        currentMode = 0;
                        GridBackgroundCover.Visibility = Visibility.Collapsed;

                        SaveStrokes();
                        ClearStrokes(true);
                        RestoreStrokes(true);

                        Topmost = true;
                        break;
                    case 1: //黑板或白板模式
                        currentMode = 1;
                        GridBackgroundCover.Visibility = Visibility.Visible;

                        SaveStrokes(true);
                        ClearStrokes(true);
                        RestoreStrokes();

                        Topmost = false;
                        break;
                }
            }
        }

        private void BtnSwitchTheme_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light)
            {
                GridBackgroundCover.Background = new SolidColorBrush(StringToColor("#FFF2F2F2"));
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
            else
            {
                GridBackgroundCover.Background = new SolidColorBrush(StringToColor("#FF1A1A1A"));
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            SetColorByIndex();
        }
        private void SetColorByIndex()
        {
            if (currentMode != 0 || GridInkCanvasSelectionCover.Visibility != Visibility.Collapsed)
                if (inkColor == 0)
                {
                    BtnColorBlack_Click(null, null);
                }
                else if (inkColor == 1)
                {
                    BtnColorRed_Click(null, null);
                }
                else if (inkColor == 2)
                {
                    BtnColorGreen_Click(null, null);
                }
                else if (inkColor == 3)
                {
                    BtnColorBlue_Click(null, null);
                }
                else if (inkColor == 4)
                {
                    BtnColorYellow_Click(null, null);
                }
                else if (inkColor == 5)
                {
                    BorderPenColorWhite_MouseUp(null, null);
                }
        }

        int BoundsWidth = 5;
        private void ToggleSwitchModeFinger_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = sender as iNKORE.UI.WPF.Modern.Controls.ToggleSwitch;
            if (toggle == null) return;

            // 同步两个浮动栏手指模式开关
            if (toggle != ToggleSwitchFloatBarModeFinger)
                ToggleSwitchFloatBarModeFinger.IsOn = toggle.IsOn;
            if (toggle != ToggleSwitchFloatBarQuickModeFinger)
                ToggleSwitchFloatBarQuickModeFinger.IsOn = toggle.IsOn;

            ToggleSwitchAutoEnterModeFinger.IsOn = toggle.IsOn;
            BoundsWidth = toggle.IsOn ? 15 : 5;
        }

        private void BtnHideInkCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (Main_Grid.Background == Brushes.Transparent)
            {
                Main_Grid.Background = new SolidColorBrush(StringToColor("#01FFFFFF"));
                if (Settings.Canvas.HideStrokeWhenSelecting)
                {
                    inkCanvas.Visibility = Visibility.Visible;
                    inkCanvas.IsHitTestVisible = true;
                }
                else
                {
                    inkCanvas.IsHitTestVisible = true;
                    inkCanvas.Visibility = Visibility.Visible;
                }
                GridBackgroundCoverHolder.Visibility = Visibility.Visible;
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

                if (ImageEraserMask.Visibility == Visibility.Visible)
                    BtnColorRed_Click(sender, null);
            }
            else
            {
                if (isLoaded && Settings.Automation.IsAutoClearWhenExitingWritingMode)
                {
                    if (inkCanvas.Strokes.Count > 0)
                    {
                        if (Settings.Automation.IsAutoSaveStrokesAtClear && inkCanvas.Strokes.Count >
                            Settings.Automation.MinimumAutomationStrokeNumber)
                        {
                            SaveScreenShot(true);
                        }

                        BtnClear_Click(null, null);
                    }
                }
                if (Settings.Canvas.HideStrokeWhenSelecting)
                    inkCanvas.Visibility = Visibility.Collapsed;
                else
                {
                    inkCanvas.IsHitTestVisible = false;
                    inkCanvas.Visibility = Visibility.Visible;
                }

                Main_Grid.Background = Brushes.Transparent;

                GridBackgroundCoverHolder.Visibility = Visibility.Collapsed;
                if (currentMode != 0)
                {
                    SaveStrokes();
                    RestoreStrokes(true);
                }
            }

            if (Main_Grid.Background == Brushes.Transparent)
            {
                StackPanelCanvasControls.Visibility = Visibility.Collapsed;
                StackPanelCanvacMain.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanelCanvasControls.Visibility = Visibility.Visible;
                StackPanelCanvacMain.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSwitchSide_Click(object sender, RoutedEventArgs e)
        {
        }


        private void StackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Right Side Panel (Buttons - Color)

        int inkColor = 1;

        const int ColorSwiftOpacityDurationOn = 150;
        const int ColorSwiftOpacityDurationOff = 50;
        private void ColorSwitchCheck()
        {
            //EraserContainer.Background = null;
            ImageEraser.Visibility = Visibility.Visible;
            if (Main_Grid.Background == Brushes.Transparent)
            {
                if (currentMode == 1)
                {
                    currentMode = 0;
                    GridBackgroundCover.Visibility = Visibility.Collapsed;
                }
                BtnHideInkCanvas_Click(null, null);
            }

            StrokeCollection strokes = inkCanvas.GetSelectedStrokes();
            if (strokes.Count != 0)
            {
                foreach (Stroke stroke in strokes)
                {
                    try
                    {
                        stroke.DrawingAttributes.Color = inkCanvas.DefaultDrawingAttributes.Color;
                    }
                    catch { }
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
            else
            {
                inkCanvas.IsManipulationEnabled = true;
                if (drawingShapeMode != 26)
                {
                    drawingShapeMode = 0;
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                }
                CancelSingleFingerDragMode();
                forceEraser = false;

                // 改变选中提示
                ViewboxBtnColorBlackContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOff)));
                ViewboxBtnColorBlueContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOff)));
                ViewboxBtnColorGreenContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOff)));
                ViewboxBtnColorRedContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOff)));
                ViewboxBtnColorYellowContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOff)));
                ViewboxBtnColorWhiteContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOff)));
                switch (inkColor)
                {
                    case 0:
                        ViewboxBtnColorBlackContent.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOn)));
                        break;
                    case 1:
                        ViewboxBtnColorRedContent.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOn)));
                        break;
                    case 2:
                        ViewboxBtnColorGreenContent.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOn)));
                        break;
                    case 3:
                        ViewboxBtnColorBlueContent.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOn)));
                        break;
                    case 4:
                        ViewboxBtnColorYellowContent.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOn)));
                        break;
                    case 5:
                        ViewboxBtnColorWhiteContent.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(ColorSwiftOpacityDurationOn)));
                        break;
                }
            }

            isLongPressSelected = false;
        }

        private void BtnColorBlack_Click(object sender, RoutedEventArgs e)
        {
            inkColor = 0;
            forceEraser = false;
            inkCanvas.DefaultDrawingAttributes.Color = Colors.Black;

            ColorSwitchCheck();
        }

        private void BtnColorRed_Click(object sender, RoutedEventArgs e)
        {
            inkColor = 1;
            forceEraser = false;
            inkCanvas.DefaultDrawingAttributes.Color = Colors.Red;
            ColorSwitchCheck();
        }

        private void BtnColorGreen_Click(object sender, RoutedEventArgs e)
        {
            inkColor = 2;
            forceEraser = false;
            inkCanvas.DefaultDrawingAttributes.Color = Colors.Green;
            ColorSwitchCheck();
        }

        private void BtnColorBlue_Click(object sender, RoutedEventArgs e)
        {
            inkColor = 3;
            forceEraser = false;
            inkCanvas.DefaultDrawingAttributes.Color = Colors.Blue;
            ColorSwitchCheck();
        }

        private void BtnColorYellow_Click(object sender, RoutedEventArgs e)
        {
            inkColor = 4;
            forceEraser = false;
            inkCanvas.DefaultDrawingAttributes.Color = Colors.Yellow;
            ColorSwitchCheck();
        }

        private Color StringToColor(string colorStr)
        {
            Byte[] argb = new Byte[4];
            for (int i = 0; i < 4; i++)
            {
                char[] charArray = colorStr.Substring(i * 2 + 1, 2).ToCharArray();
                //string str = "11";
                Byte b1 = toByte(charArray[0]);
                Byte b2 = toByte(charArray[1]);
                argb[i] = (Byte)(b2 | (b1 << 4));
            }
            return Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);//#FFFFFFFF
        }

        private static byte toByte(char c)
        {
            byte b = (byte)"0123456789ABCDEF".IndexOf(c);
            return b;
        }

        #endregion
    }
}
