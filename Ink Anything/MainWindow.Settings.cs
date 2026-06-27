using AutoUpdaterDotNET;
using Ink_Anything.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ink_Anything
{
    public partial class MainWindow : Window
    {
        #region Settings

        #region Behavior

        private void ToggleSwitchRunAtStartup_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            if (ToggleSwitchRunAtStartup.IsOn)
            {
                StartAutomaticallyCreate("InkCanvas");
            }
            else
            {
                StartAutomaticallyDel("InkCanvas");
            }
        }

        private void ToggleSwitchSupportPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;

            Settings.PowerPointSettings.PowerPointSupport = ToggleSwitchSupportPowerPoint.IsOn;
            SaveSettingsToFile();

            if (Settings.PowerPointSettings.PowerPointSupport)
            {
                timerCheckPPT.Start();
            }
            else
            {
                timerCheckPPT.Stop();
            }
        }

        private void ToggleSwitchShowCanvasAtNewSlideShow_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;

            Settings.PowerPointSettings.IsShowCanvasAtNewSlideShow = ToggleSwitchShowCanvasAtNewSlideShow.IsOn;
            SaveSettingsToFile();
        }

        #endregion

        #region Startup

        private void ToggleSwitchAutoHideCanvas_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;

            Settings.Startup.IsAutoHideCanvas = ToggleSwitchAutoHideCanvas.IsOn;
            if (ToggleSwitchAutoHideCanvas.IsOn && ToggleSwitchStartInTextMode.IsOn)
            {
                ToggleSwitchStartInTextMode.IsOn = false;
                ShowNotification("自动隐藏画板与启动时进入文本模式不能同时开启");
            }
            SaveSettingsToFile();
        }

        #endregion

        #region Appearance


        private void ToggleSwitchShowButtonEraser_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;

            Settings.Appearance.IsShowEraserButton = ToggleSwitchShowButtonEraser.IsOn;
            SaveSettingsToFile();
        }
        private void ToggleSwitchShowButtonPPTNavigation_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;

            Settings.PowerPointSettings.IsShowPPTNavigation = ToggleSwitchShowButtonPPTNavigation.IsOn;
            SaveSettingsToFile();

            PptNavigationPanel.Visibility =
                Settings.PowerPointSettings.IsShowPPTNavigation && isInSlideShow ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Appearance.Theme = ComboBoxTheme.SelectedIndex;
            SystemEvents_UserPreferenceChanged(null, null);
            SaveSettingsToFile();
        }


        private void ToggleSwitchShowCursor_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;

            Settings.Canvas.IsShowCursor = ToggleSwitchShowCursor.IsOn;
            inkCanvas_EditingModeChanged(inkCanvas, null);

            SaveSettingsToFile();
        }

        #endregion

        #region Canvas

        private void ComboBoxPenStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Canvas.InkStyle = ComboBoxPenStyle.SelectedIndex;
            SaveSettingsToFile();
        }

        private void ComboBoxTextCursorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Canvas.TextCursorType = ComboBoxTextCursorType.SelectedIndex;
            inkCanvas_EditingModeChanged(inkCanvas, null);
            SaveSettingsToFile();
        }

        private void ComboBoxEraserSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Canvas.EraserSize = ComboBoxEraserSize.SelectedIndex;
            SaveSettingsToFile();
        }


        private void ComboBoxEraserType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Canvas.EraserType = ComboBoxEraserType.SelectedIndex;
            SaveSettingsToFile();
        }

        private void InkWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isLoaded) return;

            drawingAttributes.Height = ((Slider)sender).Value / 2;
            drawingAttributes.Width = ((Slider)sender).Value / 2;

            Settings.Canvas.InkWidth = ((Slider)sender).Value / 2;

            SaveSettingsToFile();
        }

        private void ComboBoxHyperbolaAsymptoteOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Canvas.HyperbolaAsymptoteOption = (OptionalOperation)ComboBoxHyperbolaAsymptoteOption.SelectedIndex;
            SaveSettingsToFile();
        }

        #endregion

        #region Automation

        private void ToggleSwitchAutoKillPptService_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Automation.IsAutoKillPptService = ToggleSwitchAutoKillPptService.IsOn;
            SaveSettingsToFile();

            if (Settings.Automation.IsAutoKillPptService)
            {
                timerKillProcess.Start();
            }
            else
            {
                timerKillProcess.Stop();
            }
        }

        private void ToggleSwitchSaveScreenshotsInDateFolders_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Automation.IsSaveScreenshotsInDateFolders = ToggleSwitchSaveScreenshotsInDateFolders.IsOn;
            SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveStrokesAtScreenshot_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Automation.IsAutoSaveStrokesAtScreenshot = ToggleSwitchAutoSaveStrokesAtScreenshot.IsOn;
            ToggleSwitchAutoSaveStrokesAtClear.Header =
                ToggleSwitchAutoSaveStrokesAtScreenshot.IsOn ? "清屏时自动截图并保存墨迹" : "清屏时自动截图";
            SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveStrokesAtClear_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Automation.IsAutoSaveStrokesAtClear = ToggleSwitchAutoSaveStrokesAtClear.IsOn;
            SaveSettingsToFile();
        }


        private void ToggleSwitchExitingWritingMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Automation.IsAutoClearWhenExitingWritingMode = ToggleSwitchClearExitingWritingMode.IsOn;
            SaveSettingsToFile();
        }

        private void ToggleSwitchHideInkOnMouseMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Canvas.HideStrokeWhenSelecting = ToggleSwitchHideInkOnMouseMode.IsOn;
            SaveSettingsToFile();
        }

        private void ToggleSwitchUsingWhiteboard_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Canvas.UsingWhiteboard = ToggleSwitchUsingWhiteboard.IsOn;
            SystemEvents_UserPreferenceChanged(null, null);
            SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveStrokesInPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.PowerPointSettings.IsAutoSaveStrokesInPowerPoint = ToggleSwitchAutoSaveStrokesInPowerPoint.IsOn;
            SaveSettingsToFile();
        }

        private void ToggleSwitchNotifyPreviousPage_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.PowerPointSettings.IsNotifyPreviousPage = ToggleSwitchNotifyPreviousPage.IsOn;
            SaveSettingsToFile();
        }

        private void ToggleSwitchNotifyHiddenPage_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.PowerPointSettings.IsNotifyHiddenPage = ToggleSwitchNotifyHiddenPage.IsOn;
            SaveSettingsToFile();
        }

        private void ToggleSwitchNoStrokeClearInPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.PowerPointSettings.IsNoClearStrokeOnSelectWhenInPowerPoint = ToggleSwitchNoStrokeClearInPowerPoint.IsOn;
            SaveSettingsToFile();
        }


        private void ToggleSwitchShowStrokeOnSelectInPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.PowerPointSettings.IsShowStrokeOnSelectInPowerPoint = ToggleSwitchShowStrokeOnSelectInPowerPoint.IsOn;
            SaveSettingsToFile();
        }

        private void SideControlMinimumAutomationSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Automation.MinimumAutomationStrokeNumber = (int)SideControlMinimumAutomationSlider.Value;
            SaveSettingsToFile();
        }

        private void ToggleSwitchAutoSaveScreenShotInPowerPoint_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.PowerPointSettings.IsAutoSaveScreenShotInPowerPoint = ToggleSwitchAutoSaveScreenShotInPowerPoint.IsOn;
            SaveSettingsToFile();
        }

        #endregion

        #endregion

        #region Reset

        public static void SetSettingsToRecommendation()
        {
            bool IsAutoKillPptService = Settings.Automation.IsAutoKillPptService;
            Settings = new Settings();
            Settings.Appearance.IsShowEraserButton = false;
            Settings.Startup.IsAutoHideCanvas = true;
            Settings.Automation.IsAutoKillPptService = IsAutoKillPptService;
            Settings.Canvas.InkWidth = 2.5;
        }

        private void BtnResetToSuggestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isLoaded = false;
                SetSettingsToRecommendation();
                SaveSettingsToFile();
                LoadSettings(false);
                isLoaded = true;

                if (ToggleSwitchRunAtStartup.IsOn == false)
                {
                    ToggleSwitchRunAtStartup.IsOn = true;
                }
            }
            catch { }
            SymbolIconResetSuggestionComplete.Visibility = Visibility.Visible;
            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(5000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SymbolIconResetSuggestionComplete.Visibility = Visibility.Collapsed;
                });
            })).Start();
        }

        private void BtnResetToDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isLoaded = false;
                File.Delete("settings.json");
                Settings = new Settings();
                LoadSettings(false);
                isLoaded = true;
            }
            catch { }
            SymbolIconResetDefaultComplete.Visibility = Visibility.Visible;
            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(5000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SymbolIconResetDefaultComplete.Visibility = Visibility.Collapsed;
                });
            })).Start();
        }
        #endregion

        #region Ink To Shape

        private void ToggleSwitchEnableInkToShape_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.InkToShape.IsInkToShapeEnabled = ToggleSwitchEnableInkToShape.IsOn;
            SaveSettingsToFile();
        }

        #endregion

        #region Advanced

        private void ToggleSwitchIsLogEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Advanced.IsLogEnabled = ToggleSwitchIsLogEnabled.IsOn;
            SaveSettingsToFile();
        }

        private void ToggleSwitchStartInTextMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Startup.IsStartInTextMode = ToggleSwitchStartInTextMode.IsOn;
            if (ToggleSwitchStartInTextMode.IsOn && ToggleSwitchAutoHideCanvas.IsOn)
            {
                ToggleSwitchAutoHideCanvas.IsOn = false;
                ShowNotification("自动隐藏画板与启动时进入文本模式不能同时开启");
            }
            SaveSettingsToFile();
        }

        private void ToggleSwitchMinimizeToTray_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;
            Settings.Startup.IsMinimizeToTray = ToggleSwitchMinimizeToTray.IsOn;
            SaveSettingsToFile();
        }

        #endregion

        #region Check Update

        private void BtnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (StoreHelper.IsStoreApp)
            {
                ShowNewMessage("请通过 Microsoft Store 检查更新");
                return;
            }

            BtnCheckUpdate.IsEnabled = false;
            BtnCheckUpdate.Content = "检查中...";

            System.Threading.ThreadPool.QueueUserWorkItem(_ => CheckForUpdate());
        }

        private void CheckForUpdate()
        {
            try
            {
                string localVersionString = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Version localVersion = new Version(localVersionString);

                var request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/alanyz106/Ink-Anything/releases/latest");
                request.UserAgent = "Ink-Anything";
                request.Method = "GET";
                request.Timeout = 10000;

                string responseText;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                }

                JObject release = JObject.Parse(responseText);
                string tagName = release["tag_name"]?.ToString() ?? "";
                string remoteVersionStr = tagName.TrimStart('v');

                if (string.IsNullOrEmpty(remoteVersionStr))
                {
                    Dispatcher.Invoke(() => ShowNewMessage("检查更新失败：无法获取远程版本信息"));
                    return;
                }

                Version remoteVersion = new Version(remoteVersionStr);

                if (remoteVersion.CompareTo(localVersion) > 0)
                {
                    string downloadUrl = "";
                    string changelog = release["html_url"]?.ToString() ?? "";
                    foreach (var asset in release["assets"] ?? new JArray())
                    {
                        string name = asset["name"]?.ToString() ?? "";
                        if (name.Contains("Setup") && name.EndsWith(".exe"))
                        {
                            downloadUrl = asset["browser_download_url"]?.ToString() ?? "";
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(downloadUrl))
                    {
                        string finalUrl = downloadUrl;
                        string finalChangelog = changelog;
                        string finalVersion = remoteVersionStr;
                        Dispatcher.Invoke(() =>
                        {
                            var updateArgs = new UpdateInfoEventArgs
                            {
                                CurrentVersion = finalVersion,
                                DownloadURL = finalUrl,
                                ChangelogURL = finalChangelog,
                                Mandatory = new Mandatory { Value = false },
                                InstallerArgs = "/VERYSILENT /SUPPRESSMSGBOXES /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS"
                            };
                            AutoUpdater.SetOwner(this);
                            AutoUpdater.ShowUpdateForm(updateArgs);
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() => ShowNewMessage("发现新版本 v" + remoteVersionStr + "，但未找到安装包"));
                    }
                }
                else
                {
                    Dispatcher.Invoke(() => ShowNewMessage("当前已是最新版本"));
                }
            }
            catch (Exception ex)
            {
                LogHelper.NewLog("检查更新失败: " + ex.ToString());
                Dispatcher.Invoke(() => ShowNewMessage("检查更新失败，请检查网络连接后重试"));
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    BtnCheckUpdate.IsEnabled = true;
                    BtnCheckUpdate.Content = "检查更新";
                });
            }
        }

        #endregion

        #region Settings Tab

        private void SettingsTab_Checked(object sender, RoutedEventArgs e)
        {
            if (!isLoaded) return;

            // 隐藏所有面板
            PanelBehavior.Visibility = Visibility.Collapsed;
            PanelCanvas.Visibility = Visibility.Collapsed;
            PanelAppearance.Visibility = Visibility.Collapsed;
            PanelPowerPoint.Visibility = Visibility.Collapsed;
            PanelAutomation.Visibility = Visibility.Collapsed;
            PanelHotkeys.Visibility = Visibility.Collapsed;

            // 根据选中的 RadioButton 显示对应面板
            if (TabBtnBehavior.IsChecked == true) PanelBehavior.Visibility = Visibility.Visible;
            else if (TabBtnCanvas.IsChecked == true) PanelCanvas.Visibility = Visibility.Visible;
            else if (TabBtnAppearance.IsChecked == true) PanelAppearance.Visibility = Visibility.Visible;
            else if (TabBtnPowerPoint.IsChecked == true) PanelPowerPoint.Visibility = Visibility.Visible;
            else if (TabBtnAutomation.IsChecked == true) PanelAutomation.Visibility = Visibility.Visible;
            else if (TabBtnHotkeys.IsChecked == true) PanelHotkeys.Visibility = Visibility.Visible;
        }

        #endregion

        #region Hotkeys

        private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            e.Handled = true;

            // 只接受修饰键+普通键的组合，不接受单独的修饰键
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            var modifiers = Keyboard.Modifiers;
            if (modifiers == ModifierKeys.None) return;

            string gesture = FormatKeyGesture(modifiers, key);
            if (string.IsNullOrEmpty(gesture)) return;

            textBox.Text = gesture;
        }

        private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                textBox.Text = "请按下快捷键...";
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                textBox.Foreground = System.Windows.Media.Brushes.Black;
                if (textBox.Text == "请按下快捷键...")
                {
                    // 恢复原值
                    LoadSingleHotkey(textBox);
                }
            }
        }

        private string FormatKeyGesture(ModifierKeys modifiers, Key key)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");

            // 数字键显示为数字
            string keyName;
            if (key >= Key.D0 && key <= Key.D9)
                keyName = (key - Key.D0).ToString();
            else if (key >= Key.NumPad0 && key <= Key.NumPad9)
                keyName = (key - Key.NumPad0).ToString();
            else
                keyName = key.ToString();

            parts.Add(keyName);
            return string.Join("+", parts);
        }

        private void LoadHotkeySettings()
        {
            HotkeyToggleCanvas.Text = Settings.Hotkeys.ToggleCanvas;
            HotkeyClearScreen.Text = Settings.Hotkeys.ClearScreen;
            HotkeyEraser.Text = Settings.Hotkeys.Eraser;
            HotkeyScreenshot.Text = Settings.Hotkeys.Screenshot;
            HotkeyToggleToolbar.Text = Settings.Hotkeys.ToggleToolbar;
            HotkeyDrawLine.Text = Settings.Hotkeys.DrawLine;
            HotkeyTextMode.Text = Settings.Hotkeys.TextMode;
            HotkeySelectMode.Text = Settings.Hotkeys.SelectMode;
            HotkeyPenBlack.Text = Settings.Hotkeys.PenBlack;
            HotkeyPenRed.Text = Settings.Hotkeys.PenRed;
            HotkeyPenGreen.Text = Settings.Hotkeys.PenGreen;
            HotkeyPenBlue.Text = Settings.Hotkeys.PenBlue;
            HotkeyPenYellow.Text = Settings.Hotkeys.PenYellow;
            HotkeyPenWhite.Text = Settings.Hotkeys.PenWhite;
        }

        private void LoadSingleHotkey(System.Windows.Controls.TextBox textBox)
        {
            if (textBox == HotkeyToggleCanvas) textBox.Text = Settings.Hotkeys.ToggleCanvas;
            else if (textBox == HotkeyClearScreen) textBox.Text = Settings.Hotkeys.ClearScreen;
            else if (textBox == HotkeyEraser) textBox.Text = Settings.Hotkeys.Eraser;
            else if (textBox == HotkeyScreenshot) textBox.Text = Settings.Hotkeys.Screenshot;
            else if (textBox == HotkeyToggleToolbar) textBox.Text = Settings.Hotkeys.ToggleToolbar;
            else if (textBox == HotkeyDrawLine) textBox.Text = Settings.Hotkeys.DrawLine;
            else if (textBox == HotkeyTextMode) textBox.Text = Settings.Hotkeys.TextMode;
            else if (textBox == HotkeySelectMode) textBox.Text = Settings.Hotkeys.SelectMode;
            else if (textBox == HotkeyPenBlack) textBox.Text = Settings.Hotkeys.PenBlack;
            else if (textBox == HotkeyPenRed) textBox.Text = Settings.Hotkeys.PenRed;
            else if (textBox == HotkeyPenGreen) textBox.Text = Settings.Hotkeys.PenGreen;
            else if (textBox == HotkeyPenBlue) textBox.Text = Settings.Hotkeys.PenBlue;
            else if (textBox == HotkeyPenYellow) textBox.Text = Settings.Hotkeys.PenYellow;
            else if (textBox == HotkeyPenWhite) textBox.Text = Settings.Hotkeys.PenWhite;
        }

        private void SaveHotkeySettings()
        {
            Settings.Hotkeys.ToggleCanvas = HotkeyToggleCanvas.Text;
            Settings.Hotkeys.ClearScreen = HotkeyClearScreen.Text;
            Settings.Hotkeys.Eraser = HotkeyEraser.Text;
            Settings.Hotkeys.Screenshot = HotkeyScreenshot.Text;
            Settings.Hotkeys.ToggleToolbar = HotkeyToggleToolbar.Text;
            Settings.Hotkeys.DrawLine = HotkeyDrawLine.Text;
            Settings.Hotkeys.TextMode = HotkeyTextMode.Text;
            Settings.Hotkeys.SelectMode = HotkeySelectMode.Text;
            Settings.Hotkeys.PenBlack = HotkeyPenBlack.Text;
            Settings.Hotkeys.PenRed = HotkeyPenRed.Text;
            Settings.Hotkeys.PenGreen = HotkeyPenGreen.Text;
            Settings.Hotkeys.PenBlue = HotkeyPenBlue.Text;
            Settings.Hotkeys.PenYellow = HotkeyPenYellow.Text;
            Settings.Hotkeys.PenWhite = HotkeyPenWhite.Text;
            SaveSettingsToFile();
        }

        private System.Windows.Input.KeyGesture ParseKeyGesture(string gestureStr)
        {
            if (string.IsNullOrWhiteSpace(gestureStr)) return null;

            var parts = gestureStr.Split('+');
            if (parts.Length < 2) return null;

            var modifiers = ModifierKeys.None;
            string keyPart = null;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                switch (part)
                {
                    case "Ctrl": modifiers |= ModifierKeys.Control; break;
                    case "Alt": modifiers |= ModifierKeys.Alt; break;
                    case "Shift": modifiers |= ModifierKeys.Shift; break;
                    case "Win": modifiers |= ModifierKeys.Windows; break;
                    default: keyPart = part; break;
                }
            }

            if (keyPart == null) return null;

            // 数字键
            if (keyPart.Length == 1 && keyPart[0] >= '0' && keyPart[0] <= '9')
            {
                var key = (Key)(Key.D0 + (keyPart[0] - '0'));
                return new System.Windows.Input.KeyGesture(key, modifiers);
            }

            // 尝试解析 Key 枚举
            if (Enum.TryParse(keyPart, out Key parsedKey))
            {
                return new System.Windows.Input.KeyGesture(parsedKey, modifiers);
            }

            return null;
        }

        internal void ApplyHotkeys()
        {
            // 移除所有 Alt 系列的 KeyBinding（保留 Ctrl 系列）
            var toRemove = new System.Collections.Generic.List<KeyBinding>();
            foreach (var binding in InputBindings)
            {
                if (binding is KeyBinding kb && kb.Modifiers.HasFlag(ModifierKeys.Alt))
                {
                    toRemove.Add(kb);
                }
            }
            foreach (var kb in toRemove)
            {
                InputBindings.Remove(kb);
            }

            // 重新绑定
            var hotkeys = Settings.Hotkeys;
            AddHotkeyBinding(hotkeys.ToggleCanvas, "HotKey_ChangeToDrawTool");
            AddHotkeyBinding(hotkeys.ClearScreen, "HotKey_Clear");
            AddHotkeyBinding(hotkeys.Eraser, "HotKey_ChangeToEraser");
            AddHotkeyBinding(hotkeys.Screenshot, "HotKey_Capture");
            AddHotkeyBinding(hotkeys.ToggleToolbar, "HotKey_Hide");
            AddHotkeyBinding(hotkeys.DrawLine, "HotKey_DrawLine");
            AddHotkeyBinding(hotkeys.TextMode, "HotKey_Text");
            AddHotkeyBinding(hotkeys.SelectMode, "HotKey_ChangeToSelect");
            AddHotkeyBinding(hotkeys.PenBlack, "HotKey_ChangeToPen1");
            AddHotkeyBinding(hotkeys.PenRed, "HotKey_ChangeToPen2");
            AddHotkeyBinding(hotkeys.PenGreen, "HotKey_ChangeToPen3");
            AddHotkeyBinding(hotkeys.PenBlue, "HotKey_ChangeToPen4");
            AddHotkeyBinding(hotkeys.PenYellow, "HotKey_ChangeToPen5");
            AddHotkeyBinding(hotkeys.PenWhite, "HotKey_ChangeToPen6");

            UpdateHotkeyTooltips();
        }

        private void UpdateHotkeyTooltips()
        {
            var h = Settings.Hotkeys;

            ToolTipService.SetToolTip(SymbolIconEmoji, $"按住可拖动，点击可隐藏工具栏（{h.ToggleToolbar}）");
            ToolTipService.SetToolTip(GridModeToggleMouse, $"当前是鼠标模式，点击切换到画笔模式 ({h.ToggleCanvas})");
            ToolTipService.SetToolTip(GridModeToggle, $"当前是画笔模式，点击切换到鼠标模式 ({h.ToggleCanvas})");

            ToolTipService.SetToolTip(BorderPenColorBlack, $"黑色画笔 ({h.PenBlack})");
            ToolTipService.SetToolTip(BorderPenColorRed, $"红色画笔 ({h.PenRed})");
            ToolTipService.SetToolTip(BorderPenColorGreen, $"绿色画笔 ({h.PenGreen})");
            ToolTipService.SetToolTip(BorderPenColorBlue, $"蓝色画笔 ({h.PenBlue})");
            ToolTipService.SetToolTip(BorderPenColorYellow, $"黄色画笔 ({h.PenYellow})");
            ToolTipService.SetToolTip(BorderPenColorWhite, $"白色画笔 ({h.PenWhite})");

            ToolTipService.SetToolTip(EraserContainer, $"橡皮擦：擦除墨迹（点击切换笔迹擦/范围擦） ({h.Eraser})");
            ToolTipService.SetToolTip(SymbolIconDelete, $"清屏：清除画布上所有墨迹 ({h.ClearScreen})");
            ToolTipService.SetToolTip(SymbolIconSelect, $"选择：点击进入选择模式，再点一次退出 ({h.SelectMode}，Ctrl+A 全选)");
            ToolTipService.SetToolTip(SymbolIconText, $"文本输入：点击画板添加文字 ({h.TextMode})");
        }

        private void AddHotkeyBinding(string gestureStr, string commandName)
        {
            var gesture = ParseKeyGesture(gestureStr);
            if (gesture == null) return;

            var command = TryFindResource(commandName) as System.Windows.Input.RoutedUICommand;
            if (command == null) return;

            InputBindings.Add(new KeyBinding(command, gesture));
        }

        #endregion

        public static void SaveSettingsToFile()
        {
            SettingsManager.SaveToFile(Settings, App.RootPath + settingsFileName);
        }

        private void SCManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}
