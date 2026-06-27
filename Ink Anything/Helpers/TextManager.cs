using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfCanvas = System.Windows.Controls.Canvas;

namespace Ink_Anything.Helpers
{
    /// <summary>
    /// 封装文本输入系统共享的状态和核心操作
    /// </summary>
    public class TextManager
    {
        // 共享状态
        public WpfCanvas TextOverlayCanvas { get; set; } = null;
        public Border CurrentEditingTextBorder { get; set; } = null;
        public TextUndoStack TextUndoStack { get; } = new TextUndoStack();
        public List<TextElementData> CurrentTextElements { get; } = new List<TextElementData>();
        public List<Border> SelectedTextBorders { get; } = new List<Border>();

        // 拖拽状态
        public Dictionary<Border, Point> TextDragStartPositions { get; } = new Dictionary<Border, Point>();

        // 缩放状态
        public WpfCanvas ResizeHandlesCanvas { get; set; } = null;
        public bool IsResizing { get; set; } = false;
        public Point ResizeStartPoint { get; set; }
        public double ResizeStartFontSize { get; set; }
        public Border ResizeTargetBorder { get; set; } = null;

        // 选中高亮画刷
        public static readonly SolidColorBrush MultiSelectHighlightBrush =
            new SolidColorBrush(Color.FromArgb(0x30, 0x40, 0x9E, 0xFF));

        #region 颜色工具方法

        public static string BrushToHex(Brush brush)
        {
            if (brush is SolidColorBrush scb)
            {
                return $"#{scb.Color.A:X2}{scb.Color.R:X2}{scb.Color.G:X2}{scb.Color.B:X2}";
            }
            return "#FF000000";
        }

        public static SolidColorBrush HexToBrush(string hex)
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

        #endregion

        #region 序列化方法

        public List<TextElementData> GetCurrentTextElementDataList(WpfCanvas overlayCanvas)
        {
            var list = new List<TextElementData>();
            if (overlayCanvas == null) return list;
            foreach (var child in overlayCanvas.Children)
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

        public void SaveTextElementDataToStream(List<TextElementData> textElements, string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(textElements, Formatting.None);
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        public List<TextElementData> LoadTextElementDataFromStream(string filePath)
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

        #endregion

        #region 文本元素管理

        public void ClearAll(InkCanvas inkCanvas)
        {
            if (TextOverlayCanvas != null)
            {
                inkCanvas.Children.Remove(TextOverlayCanvas);
                TextOverlayCanvas = null;
            }
            SelectedTextBorders.Clear();
            ResizeHandlesCanvas = null;
            CurrentEditingTextBorder = null;
            CurrentTextElements.Clear();
            TextUndoStack.Clear();
        }

        #endregion
    }
}
