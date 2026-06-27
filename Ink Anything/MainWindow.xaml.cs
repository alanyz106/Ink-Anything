using AutoUpdaterDotNET;
using Ink_Anything.Helpers;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Helpers;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Application = System.Windows.Application;
using File = System.IO.File;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Timer = System.Timers.Timer;

namespace Ink_Anything
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Window Initialization

        public MainWindow()
        {
            InitializeComponent();
            _strokeTransformService = new StrokeTransformService(inkCanvas, timeMachine);

            BorderSettings.Opacity = 0;
            BorderSettings.Visibility = Visibility.Collapsed;
            BorderDrawShape.Visibility = Visibility.Collapsed;
            GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

            if (App.StartArgs.Contains("-b")) //-b border
            {
                AllowsTransparency = false;
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                Background = new SolidColorBrush(StringToColor("#FFF2F2F2"));
                Topmost = false;
            }

            ViewboxFloatingBar.Margin = new Thickness((SystemParameters.WorkArea.Width - 284) / 2, SystemParameters.WorkArea.Height - 80, -2000, -200);

            if (File.Exists("debug.ini")) Label.Visibility = Visibility.Visible;

            InitTimers();
            timeMachine.OnRedoStateChanged += TimeMachine_OnRedoStateChanged;
            timeMachine.OnUndoStateChanged += TimeMachine_OnUndoStateChanged;
            inkCanvas.Strokes.StrokesChanged += StrokesOnStrokesChanged;

            Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        #endregion

        #region Timer

        Timer timerCheckPPT = new Timer();
        Timer timerKillProcess = new Timer();

        private void InitTimers()
        {
            timerCheckPPT.Elapsed += TimerCheckPPT_Elapsed;
            timerCheckPPT.Interval = 1000;

            timerKillProcess.Elapsed += TimerKillProcess_Elapsed;
            timerKillProcess.Interval = 5000;
        }

        private void TimerKillProcess_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                string arg = "/F";
                if (Settings.Automation.IsAutoKillPptService)
                {
                    Process[] processes = Process.GetProcessesByName("PPTService");
                    if (processes.Length > 0)
                    {
                        arg += " /IM PPTService.exe";
                    }
                }
                if (arg != "/F")
                {
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo("taskkill", arg);
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.Start();
                }
            }
            catch { }
        }

        #endregion Timer

        #region Definations and Loading

        public static Settings Settings = new Settings();
        public static string settingsFileName = "Settings.json";
        internal readonly TextManager _textManager = new TextManager();
        internal StrokeTransformService _strokeTransformService;
        internal readonly WhiteboardManager _whiteboardManager = new WhiteboardManager();
        bool isLoaded = false;
        //bool isAutoUpdateEnabled = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AutoUpdater.SetOwner(this);

            //new CountdownTimerWindow().ShowDialog();
            //检查
            new Thread(new ThreadStart(() =>
            {
                try
                {
                    string VersionInfo = "";
                    if (File.Exists(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "VersionInfo.ini"))
                    {
                        VersionInfo = File.ReadAllText(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "VersionInfo.ini");
                    }
                    string Url = "http://ink.wxriw.cn:1957";
                    if (VersionInfo != "")
                    {
                        Url += "/?verinfo=" + VersionInfo;// + "&pc=" + Environment.MachineName;
                    }
                    string response = GetWebClient(Url);
                    if (response.Contains("Special Version"))
                    {
                        //isAutoUpdateEnabled = true;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Version version = Assembly.GetExecutingAssembly().GetName().Version;

                        string lastVersion = "";
                        try
                        {
                            lastVersion = File.ReadAllText(App.RootPath + "Versions.ini");
                        }
                        catch { }
                        if (!lastVersion.Contains(version.ToString()))
                        {
                            lastVersion += "\n" + version.ToString();
                            File.WriteAllText(App.RootPath + "Versions.ini", lastVersion.Trim());
                        }
                    });
                }
                catch { }
            })).Start();

            loadPenCanvas();

            //加载设置
            LoadSettings();
            if (Environment.Is64BitProcess)
            {
                PanelInkToShapeSettings.Visibility = Visibility.Collapsed;
            }

            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            SystemEvents_UserPreferenceChanged(null, null);

            LogHelper.WriteLogToFile("Ink Anything Loaded", LogHelper.LogType.Event);

            if (!Environment.Is64BitProcess)
            {
                PreloadIALibrary();
            }

            TextBlockSettingsVersion.Text = "Ink Anything v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

            LoadHotkeySettings();
            ApplyHotkeys();

            isLoaded = true;

            CheckHotkeyConflicts();
        }

        private void CheckHotkeyConflicts()
        {
            var conflicts = new List<string>();
            foreach (var binding in this.InputBindings)
            {
                if (binding is KeyBinding kb && kb.Modifiers != ModifierKeys.None)
                {
                    HotkeyModifiers mods = 0;
                    if (kb.Modifiers.HasFlag(ModifierKeys.Alt)) mods |= HotkeyModifiers.MOD_ALT;
                    if (kb.Modifiers.HasFlag(ModifierKeys.Control)) mods |= HotkeyModifiers.MOD_CONTROL;
                    if (kb.Modifiers.HasFlag(ModifierKeys.Shift)) mods |= HotkeyModifiers.MOD_SHIFT;

                    uint vk = (uint)KeyInterop.VirtualKeyFromKey(kb.Key);
                    if (!Hotkey.IsHotkeyAvailable(this, mods, vk))
                    {
                        string keyName = kb.Key.ToString();
                        string modName = kb.Modifiers.ToString().Replace(", ", "+");
                        conflicts.Add(modName + "+" + keyName);
                        LogHelper.NewLog($"Hotkey conflict: {modName}+{keyName}");
                    }
                }
            }
            if (conflicts.Count > 0)
            {
                ShowNotification("快捷键冲突：" + string.Join("、", conflicts));
                LogHelper.WriteLogToFile("Hotkey conflicts: " + string.Join(", ", conflicts), LogHelper.LogType.Info);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogHelper.WriteLogToFile("Ink Anything closing", LogHelper.LogType.Event);
            if (!CloseIsFromButton)
            {
                e.Cancel = true;
                if (MessageBox.Show("是否继续关闭 Ink Anything 画板，这将丢失当前未保存的工作。", "Ink Anything 画板", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    if (MessageBox.Show("真的狠心关闭 Ink Anything 画板吗？", "Ink Anything 画板", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        if (MessageBox.Show("是否取消关闭 Ink Anything 画板？", "Ink Anything 画板", MessageBoxButton.OKCancel, MessageBoxImage.Error) != MessageBoxResult.OK)
                        {
                            e.Cancel = false;
                        }
                    }
                }
            }
            if (e.Cancel)
            {
                LogHelper.WriteLogToFile("Ink Anything closing cancelled", LogHelper.LogType.Event);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LogHelper.WriteLogToFile("Ink Anything closed", LogHelper.LogType.Event);
        }

        private static void PreloadIALibrary()
        {
            GC.KeepAlive(typeof(InkAnalyzer));
            GC.KeepAlive(typeof(AnalysisAlternate));
            GC.KeepAlive(typeof(InkDrawingNode));
            var analyzer = new InkAnalyzer();
            analyzer.AddStrokes(new StrokeCollection() {
                new Stroke(new StylusPointCollection() {
                    new StylusPoint(114,514),
                    new StylusPoint(191,9810),
                    new StylusPoint(7,21),
                    new StylusPoint(123,789),
                })
            });
            analyzer.Analyze();
        }

        private void LoadSettings(bool isStartup = true)
        {
            var loaded = SettingsManager.LoadFromFile(App.RootPath + settingsFileName);
            if (loaded != null) Settings = loaded;

            if (Settings.Startup.IsAutoHideCanvas)
            {
                if (isStartup)
                {
                    BtnHideInkCanvas_Click(null, null);
                }
                ToggleSwitchAutoHideCanvas.IsOn = true;
            }
            else
            {
                if (isStartup)
                {
                    BtnHideInkCanvas_Click(null, null);
                    BtnHideInkCanvas_Click(null, null);
                }
                ToggleSwitchAutoHideCanvas.IsOn = false;
            }

            ToggleSwitchStartInTextMode.IsOn = Settings.Startup.IsStartInTextMode;
            if (isStartup && Settings.Startup.IsStartInTextMode)
            {
                EnterTextMode();
            }

            ToggleSwitchShowButtonEraser.IsOn = Settings.Appearance.IsShowEraserButton;

            PptNavigationPanel.Visibility =
                Settings.PowerPointSettings.IsShowPPTNavigation && isInSlideShow ? Visibility.Visible : Visibility.Collapsed;
            ToggleSwitchShowButtonPPTNavigation.IsOn = Settings.PowerPointSettings.IsShowPPTNavigation;

            ComboBoxTheme.SelectedIndex = Settings.Appearance.Theme;

            if (Settings.PowerPointSettings.PowerPointSupport)
            {
                ToggleSwitchSupportPowerPoint.IsOn = true;
                timerCheckPPT.Start();
            }
            else
            {
                ToggleSwitchSupportPowerPoint.IsOn = false;
                timerCheckPPT.Stop();
            }
            if (Settings.PowerPointSettings.IsShowCanvasAtNewSlideShow)
            {
                ToggleSwitchShowCanvasAtNewSlideShow.IsOn = true;
            }
            else
            {
                ToggleSwitchShowCanvasAtNewSlideShow.IsOn = false;
            }

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\InkCanvas" + ".lnk"))
            {
                ToggleSwitchRunAtStartup.IsOn = true;
            }

            if (Settings.Canvas != null)
            {
                drawingAttributes.Height = Settings.Canvas.InkWidth;
                drawingAttributes.Width = Settings.Canvas.InkWidth;

                InkWidthSlider.Value = Settings.Canvas.InkWidth * 2;

                if (Settings.Canvas.IsShowCursor)
                {
                    ToggleSwitchShowCursor.IsOn = true;
                    inkCanvas.ForceCursor = true;
                }
                else
                {
                    ToggleSwitchShowCursor.IsOn = false;
                    inkCanvas.ForceCursor = false;
                }

                ComboBoxPenStyle.SelectedIndex = Settings.Canvas.InkStyle;

                ComboBoxTextCursorType.SelectedIndex = Settings.Canvas.TextCursorType;

                ComboBoxEraserSize.SelectedIndex = Settings.Canvas.EraserSize;

                ComboBoxHyperbolaAsymptoteOption.SelectedIndex = (int)Settings.Canvas.HyperbolaAsymptoteOption;
            }
            else
            {
                Settings.Canvas = new Canvas();
            }

            if (Settings.Automation != null)
            {
                if (Settings.Automation.IsAutoKillPptService)
                {
                    timerKillProcess.Start();
                }
                else
                {
                    timerKillProcess.Stop();
                }

                if (Settings.Automation.IsAutoClearWhenExitingWritingMode)
                {
                    ToggleSwitchClearExitingWritingMode.IsOn = true;
                }
                else
                {
                    ToggleSwitchClearExitingWritingMode.IsOn = false;
                }


                if (Settings.Automation.IsAutoSaveStrokesAtClear)
                {
                    ToggleSwitchAutoSaveStrokesAtClear.IsOn = true;
                }
                else
                {
                    ToggleSwitchAutoSaveStrokesAtClear.IsOn = false;
                }



                if (Settings.Automation.IsAutoKillPptService)
                {
                    ToggleSwitchAutoKillPptService.IsOn = true;
                }
                else
                {
                    ToggleSwitchAutoKillPptService.IsOn = false;
                }

                if (Settings.Automation.IsSaveScreenshotsInDateFolders)
                {
                    ToggleSwitchSaveScreenshotsInDateFolders.IsOn = true;
                }
                else
                {
                    ToggleSwitchSaveScreenshotsInDateFolders.IsOn = false;
                }

                if (Settings.Automation.IsAutoSaveStrokesAtScreenshot)
                {
                    ToggleSwitchAutoSaveStrokesAtScreenshot.IsOn = true;
                }
                else
                {
                    ToggleSwitchAutoSaveStrokesAtScreenshot.IsOn = false;
                }

                if (Settings.PowerPointSettings.IsAutoSaveStrokesInPowerPoint)
                {
                    ToggleSwitchAutoSaveStrokesInPowerPoint.IsOn = true;
                }
                else
                {
                    ToggleSwitchAutoSaveStrokesInPowerPoint.IsOn = false;
                }

                if (Settings.PowerPointSettings.IsNotifyPreviousPage)
                {
                    ToggleSwitchNotifyPreviousPage.IsOn = true;
                }
                else
                {
                    ToggleSwitchNotifyPreviousPage.IsOn = false;
                }

                if (Settings.PowerPointSettings.IsNotifyHiddenPage)
                {
                    ToggleSwitchNotifyHiddenPage.IsOn = true;
                }
                else
                {
                    ToggleSwitchNotifyHiddenPage.IsOn = false;
                }

                if (Settings.PowerPointSettings.IsNoClearStrokeOnSelectWhenInPowerPoint)
                {
                    ToggleSwitchNoStrokeClearInPowerPoint.IsOn = true;
                }
                else
                {
                    ToggleSwitchNoStrokeClearInPowerPoint.IsOn = false;
                }

                if (Settings.PowerPointSettings.IsShowStrokeOnSelectInPowerPoint)
                {
                    ToggleSwitchShowStrokeOnSelectInPowerPoint.IsOn = true;
                }
                else
                {
                    ToggleSwitchShowStrokeOnSelectInPowerPoint.IsOn = false;
                }

                if (Settings.PowerPointSettings.IsSupportWPS)
                {
                    ToggleSwitchSupportWPS.IsOn = true;
                }
                else
                {
                    ToggleSwitchSupportWPS.IsOn = false;
                }

                SideControlMinimumAutomationSlider.Value = Settings.Automation.MinimumAutomationStrokeNumber;

                if (Settings.Canvas.HideStrokeWhenSelecting)
                {
                    ToggleSwitchHideInkOnMouseMode.IsOn = true;
                }
                else
                {
                    ToggleSwitchHideInkOnMouseMode.IsOn = false;
                }

                if (Settings.Canvas.UsingWhiteboard)
                {
                    ToggleSwitchUsingWhiteboard.IsOn = true;
                }
                else
                {
                    ToggleSwitchUsingWhiteboard.IsOn = false;
                }
                if (Settings.Canvas.UsingWhiteboard)
                {
                    SystemEvents_UserPreferenceChanged(null, null);
                }

                switch (Settings.Canvas.EraserType)
                {
                    case 1:
                        forcePointEraser = true;
                        break;
                    case 2:
                        forcePointEraser = false;
                        break;
                }

                ComboBoxEraserType.SelectedIndex = Settings.Canvas.EraserType;

                if (Settings.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint)
                {
                    ToggleSwitchAutoSaveScreenShotInPowerPoint.IsOn = true;
                }
                else
                {
                    ToggleSwitchAutoSaveScreenShotInPowerPoint.IsOn = false;
                }
            }
            else
            {
                Settings.Automation = new Automation();
            }

            if (Settings.Advanced != null)
            {
                if (Settings.Advanced.IsLogEnabled)
                {
                    ToggleSwitchIsLogEnabled.IsOn = true;
                }
                else
                {
                    ToggleSwitchIsLogEnabled.IsOn = false;
                }
            }
            else
            {
                Settings.Advanced = new Advanced();
            }

            if (Settings.InkToShape != null)
            {
                if (Settings.InkToShape.IsInkToShapeEnabled)
                {
                    ToggleSwitchEnableInkToShape.IsOn = true;
                }
                else
                {
                    ToggleSwitchEnableInkToShape.IsOn = false;
                }
            }
            else
            {
                Settings.InkToShape = new InkToShape();
            }
        }

        #endregion Definations and Loading



    }
}
