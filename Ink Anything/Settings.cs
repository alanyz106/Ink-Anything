using Newtonsoft.Json;

namespace Ink_Anything
{
    public class Settings
    {
        [JsonProperty("advanced")]
        public Advanced Advanced { get; set; } = new Advanced();
        [JsonProperty("appearance")]
        public Appearance Appearance { get; set; } = new Appearance();
        [JsonProperty("automation")]
        public Automation Automation { get; set; } = new Automation();
        [JsonProperty("behavior")]
        public PowerPointSettings PowerPointSettings { get; set; } = new PowerPointSettings();
        [JsonProperty("canvas")]
        public Canvas Canvas { get; set; } = new Canvas();
        [JsonProperty("inkToShape")]
        public InkToShape InkToShape { get; set; } = new InkToShape();
        [JsonProperty("startup")]
        public Startup Startup { get; set; } = new Startup();
        [JsonProperty("hotkeys")]
        public Hotkeys Hotkeys { get; set; } = new Hotkeys();
    }

    public class Canvas
    {
        [JsonProperty("inkWidth")]
        public double InkWidth { get; set; } = 2.5;
        [JsonProperty("isShowCursor")]
        public bool IsShowCursor { get; set; } = false;
        [JsonProperty("textCursorType")]
        public int TextCursorType { get; set; } = 0; // 0-箭头  1-文本(I)
        [JsonProperty("inkStyle")]
        public int InkStyle { get; set; } = 0;
        [JsonProperty("eraserSize")]
        public int EraserSize { get; set; } = 2;
        [JsonProperty("eraserType")]
        public int EraserType { get; set; } = 0; // 0 - 图标切换模式      1 - 面积擦     2 - 线条擦
        [JsonProperty("hideStrokeWhenSelecting")]
        public bool HideStrokeWhenSelecting { get; set; } = true;

        [JsonProperty("usingWhiteboard")]
        public bool UsingWhiteboard { get; set; }

        [JsonProperty("hyperbolaAsymptoteOption")]
        public OptionalOperation HyperbolaAsymptoteOption { get; set; } = OptionalOperation.Ask;
    }

    public enum OptionalOperation
    {
        Yes,
        No,
        Ask
    }

    public class Startup
    {
        [JsonProperty("isAutoHideCanvas")]
        public bool IsAutoHideCanvas { get; set; } = true;
        [JsonProperty("isStartInTextMode")]
        public bool IsStartInTextMode { get; set; } = false;
        [JsonProperty("isMinimizeToTray")]
        public bool IsMinimizeToTray { get; set; } = false;
    }

    public class Appearance
    {
        [JsonProperty("isShowEraserButton")]
        public bool IsShowEraserButton { get; set; } = false;
        [JsonProperty("theme")]
        public int Theme { get; set; } = 0;
    }

    public class PowerPointSettings
    {
        [JsonProperty("isShowPPTNavigation")]
        public bool IsShowPPTNavigation { get; set; } = true;
        [JsonProperty("powerPointSupport")]
        public bool PowerPointSupport { get; set; } = true;
        [JsonProperty("isShowCanvasAtNewSlideShow")]
        public bool IsShowCanvasAtNewSlideShow { get; set; } = true;
        [JsonProperty("isNoClearStrokeOnSelectWhenInPowerPoint")]
        public bool IsNoClearStrokeOnSelectWhenInPowerPoint { get; set; } = true;
        [JsonProperty("isShowStrokeOnSelectInPowerPoint")]
        public bool IsShowStrokeOnSelectInPowerPoint { get; set; } = false;
        [JsonProperty("isAutoSaveStrokesInPowerPoint")]
        public bool IsAutoSaveStrokesInPowerPoint { get; set; } = true;
        [JsonProperty("isAutoSaveScreenShotInPowerPoint")]
        public bool IsAutoSaveScreenShotInPowerPoint { get; set; } = false;
        [JsonProperty("isNotifyPreviousPage")]
        public bool IsNotifyPreviousPage { get; set; } = false;
        [JsonProperty("isNotifyHiddenPage")]
        public bool IsNotifyHiddenPage { get; set; } = true;
        [JsonProperty("isSupportWPS")]
        public bool IsSupportWPS { get; set; } = true;
    }

    public class Automation
    {
        [JsonProperty("isAutoKillPptService")]
        public bool IsAutoKillPptService { get; set; } = false;

        [JsonProperty("isSaveScreenshotsInDateFolders")]
        public bool IsSaveScreenshotsInDateFolders { get; set; } = false;

        [JsonProperty("isAutoSaveStrokesAtScreenshot")]
        public bool IsAutoSaveStrokesAtScreenshot { get; set; } = false;

        [JsonProperty("isAutoSaveStrokesAtClear")]
        public bool IsAutoSaveStrokesAtClear { get; set; } = false;

        [JsonProperty("isAutoClearWhenExitingWritingMode")]
        public bool IsAutoClearWhenExitingWritingMode { get; set; } = false;

        [JsonProperty("minimumAutomationStrokeNumber")]
        public int MinimumAutomationStrokeNumber { get; set; } = 0;

    }

    public class Advanced
    {
        [JsonProperty("isLogEnabled")]
        public bool IsLogEnabled { get; set; } = true;
    }

    public class InkToShape
    {
        [JsonProperty("isInkToShapeEnabled")]
        public bool IsInkToShapeEnabled { get; set; } = true;
    }

    public class Hotkeys
    {
        [JsonProperty("toggleCanvas")]
        public string ToggleCanvas { get; set; } = "Alt+S";
        [JsonProperty("clearScreen")]
        public string ClearScreen { get; set; } = "Alt+D";
        [JsonProperty("eraser")]
        public string Eraser { get; set; } = "Alt+E";
        [JsonProperty("screenshot")]
        public string Screenshot { get; set; } = "Alt+C";
        [JsonProperty("toggleToolbar")]
        public string ToggleToolbar { get; set; } = "Alt+V";
        [JsonProperty("drawLine")]
        public string DrawLine { get; set; } = "Alt+L";
        [JsonProperty("textMode")]
        public string TextMode { get; set; } = "Alt+T";
        [JsonProperty("selectMode")]
        public string SelectMode { get; set; } = "Alt+Q";
        [JsonProperty("penBlack")]
        public string PenBlack { get; set; } = "Alt+1";
        [JsonProperty("penRed")]
        public string PenRed { get; set; } = "Alt+2";
        [JsonProperty("penGreen")]
        public string PenGreen { get; set; } = "Alt+3";
        [JsonProperty("penBlue")]
        public string PenBlue { get; set; } = "Alt+4";
        [JsonProperty("penYellow")]
        public string PenYellow { get; set; } = "Alt+5";
        [JsonProperty("penWhite")]
        public string PenWhite { get; set; } = "Alt+6";
    }
}
