using AutoUpdaterDotNET;
using Ink_Anything.Helpers;
using iNKORE.UI.WPF.Modern.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Ink_Anything
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex mutex;

        public static string[] StartArgs = null;
        public static string RootPath = Environment.GetEnvironmentVariable("APPDATA") + "\\Ink Anything\\";

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Ink_Anything.MainWindow.ShowNewMessage("抱歉，出现未预期的异常，可能导致 Ink Anything 画板运行不稳定。\n建议保存墨迹后重启应用。", true);
            LogHelper.NewLog(e.Exception.ToString());
            e.Handled = true;
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            if (!StoreHelper.IsStoreApp) RootPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            LogHelper.InitLog();
            LogHelper.NewLog(string.Format("Ink Anything Starting (Version: {0})", Assembly.GetExecutingAssembly().GetName().Version.ToString()));

            bool ret;
            mutex = new System.Threading.Mutex(true, "Ink_Anything", out ret);

            if (!ret && !e.Args.Contains("-m")) //-m multiple
            {
                LogHelper.NewLog("Detected existing instance");
                MessageBox.Show("已有一个程序实例正在运行");
                LogHelper.NewLog("Ink Anything automatically closed");
                Environment.Exit(0);
            }

            StartArgs = e.Args;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (!StoreHelper.IsStoreApp)
            {
                AutoUpdater.ParseUpdateInfoEvent += AutoUpdater_ParseUpdateInfoEvent;
                AutoUpdater.Start("https://api.github.com/repos/alanyz106/Ink-Anything/releases/latest");
                AutoUpdater.ApplicationExitEvent += () =>
                {
                    Environment.Exit(0);
                };
            }
        }

        private static void AutoUpdater_ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            try
            {
                JObject release = JObject.Parse(args.RemoteData);

                if (release["message"] != null)
                {
                    LogHelper.NewLog("GitHub API 返回错误: " + release["message"]);
                    return;
                }

                string tagName = release["tag_name"]?.ToString() ?? "";
                string version = tagName.TrimStart('v');

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

                if (!string.IsNullOrEmpty(downloadUrl) && !string.IsNullOrEmpty(version))
                {
                    args.UpdateInfo = new UpdateInfoEventArgs
                    {
                        CurrentVersion = version,
                        DownloadURL = downloadUrl,
                        ChangelogURL = changelog,
                        Mandatory = new Mandatory { Value = false },
                        InstallerArgs = "/VERYSILENT /SUPPRESSMSGBOXES /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS"
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.NewLog("AutoUpdater 解析更新信息失败: " + ex.ToString());
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            try
            {
                if (System.Windows.Forms.SystemInformation.MouseWheelScrollLines == -1)
                    e.Handled = false;
                else
                    try
                    {
                        ScrollViewerEx SenderScrollViewer = (ScrollViewerEx)sender;
                        SenderScrollViewer.ScrollToVerticalOffset(SenderScrollViewer.VerticalOffset - e.Delta * 10 * System.Windows.Forms.SystemInformation.MouseWheelScrollLines / (double)120);
                        e.Handled = true;
                    }
                    catch
                    {
                    }
            }
            catch
            {
            }
        }
    }
}
