using System.Collections.Generic;
using System.Windows.Ink;

namespace Ink_Anything.Helpers
{
    /// <summary>
    /// 白板多页管理器，封装页面数据存储和页面切换逻辑
    /// </summary>
    public class WhiteboardManager
    {
        public const int MaxPages = 101; // 0 用于非白板备份，1-99 用于白板页面

        // 每页的数据存储
        public StrokeCollection[] StrokeCollections { get; } = new StrokeCollection[MaxPages];
        public bool[] WhiteboardLastModeIsRedo { get; } = new bool[MaxPages];
        public List<TextElementData>[] TextElementCollections { get; } = new List<TextElementData>[MaxPages];
        public TimeMachineHistory[][] TimeMachineHistories { get; } = new TimeMachineHistory[MaxPages][];

        // 页面状态
        public int CurrentIndex { get; set; } = 1;
        public int TotalCount { get; set; } = 1;

        /// <summary>
        /// 保存当前页面数据到指定索引
        /// </summary>
        public void SavePageData(int index, TimeMachineHistory[] history, List<TextElementData> textElements)
        {
            TimeMachineHistories[index] = history;
            TextElementCollections[index] = textElements;
        }

        /// <summary>
        /// 获取指定索引的页面数据
        /// </summary>
        public (TimeMachineHistory[] history, List<TextElementData> textElements) GetPageData(int index)
        {
            return (TimeMachineHistories[index], TextElementCollections[index]);
        }

        /// <summary>
        /// 清除指定索引的页面数据
        /// </summary>
        public void ClearPageData(int index)
        {
            TimeMachineHistories[index] = null;
            TextElementCollections[index] = null;
            StrokeCollections[index] = null;
        }

        /// <summary>
        /// 插入新页面（将后续页面数据右移）
        /// </summary>
        public void InsertPage(int atIndex)
        {
            for (int i = TotalCount; i > atIndex; i--)
            {
                TimeMachineHistories[i] = TimeMachineHistories[i - 1];
                TextElementCollections[i] = TextElementCollections[i - 1];
                StrokeCollections[i] = StrokeCollections[i - 1];
            }
            TimeMachineHistories[atIndex] = null;
            TextElementCollections[atIndex] = null;
            StrokeCollections[atIndex] = null;
        }

        /// <summary>
        /// 删除页面（将后续页面数据左移）
        /// </summary>
        public void DeletePage(int atIndex)
        {
            if (atIndex != TotalCount)
            {
                for (int i = atIndex; i <= TotalCount; i++)
                {
                    TimeMachineHistories[i] = TimeMachineHistories[i + 1];
                    TextElementCollections[i] = TextElementCollections[i + 1];
                    StrokeCollections[i] = StrokeCollections[i + 1];
                }
            }
        }

        /// <summary>
        /// 重置所有页面数据
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < MaxPages; i++)
            {
                StrokeCollections[i] = null;
                WhiteboardLastModeIsRedo[i] = false;
                TextElementCollections[i] = null;
                TimeMachineHistories[i] = null;
            }
            CurrentIndex = 1;
            TotalCount = 1;
        }
    }
}
