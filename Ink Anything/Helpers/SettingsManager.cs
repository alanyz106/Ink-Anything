using Newtonsoft.Json;
using System.IO;

namespace Ink_Anything.Helpers
{
    /// <summary>
    /// 设置文件的加载和保存操作
    /// </summary>
    public static class SettingsManager
    {
        /// <summary>
        /// 从 JSON 文件加载设置
        /// </summary>
        public static Settings LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            try
            {
                string text = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Settings>(text);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 保存设置到 JSON 文件
        /// </summary>
        public static void SaveToFile(Settings settings, string filePath)
        {
            try
            {
                string text = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, text);
            }
            catch { }
        }
    }
}
