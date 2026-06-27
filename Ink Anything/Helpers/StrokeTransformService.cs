using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;

namespace Ink_Anything.Helpers
{
    /// <summary>
    /// 笔画变换服务：翻转、旋转、缩放、删除
    /// </summary>
    public class StrokeTransformService
    {
        private readonly InkCanvas _inkCanvas;
        private readonly TimeMachine _timeMachine;

        public StrokeTransformService(InkCanvas inkCanvas, TimeMachine timeMachine)
        {
            _inkCanvas = inkCanvas;
            _timeMachine = timeMachine;
        }

        /// <summary>
        /// 获取当前选中笔画的中心点
        /// </summary>
        private Point GetSelectionCenter()
        {
            var bounds = _inkCanvas.GetSelectionBounds();
            return new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        }

        /// <summary>
        /// 对选中笔画应用变换矩阵
        /// </summary>
        private void TransformSelectedStrokes(Matrix m)
        {
            StrokeCollection targetStrokes = _inkCanvas.GetSelectedStrokes();
            foreach (Stroke stroke in targetStrokes)
            {
                stroke.Transform(m, false);
            }
        }

        /// <summary>
        /// 水平翻转
        /// </summary>
        public void FlipHorizontal()
        {
            Point center = GetSelectionCenter();
            Matrix m = new Matrix();
            m.ScaleAt(-1, 1, center.X, center.Y);
            TransformSelectedStrokes(m);
        }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        public void FlipVertical()
        {
            Point center = GetSelectionCenter();
            Matrix m = new Matrix();
            m.ScaleAt(1, -1, center.X, center.Y);
            TransformSelectedStrokes(m);
        }

        /// <summary>
        /// 旋转 45°
        /// </summary>
        public void Rotate45()
        {
            Point center = GetSelectionCenter();
            Matrix m = new Matrix();
            m.RotateAt(45, center.X, center.Y);
            TransformSelectedStrokes(m);
        }

        /// <summary>
        /// 旋转 90°
        /// </summary>
        public void Rotate90()
        {
            Point center = GetSelectionCenter();
            Matrix m = new Matrix();
            m.RotateAt(90, center.X, center.Y);
            TransformSelectedStrokes(m);
        }

        /// <summary>
        /// 调整选中笔画的粗细
        /// </summary>
        public void ChangeStrokeThickness(double multiplier)
        {
            foreach (Stroke stroke in _inkCanvas.GetSelectedStrokes())
            {
                var newWidth = stroke.DrawingAttributes.Width * multiplier;
                var newHeight = stroke.DrawingAttributes.Height * multiplier;

                if (newWidth >= DrawingAttributes.MinWidth && newWidth <= DrawingAttributes.MaxWidth
                    && newHeight >= DrawingAttributes.MinHeight && newHeight <= DrawingAttributes.MaxHeight)
                {
                    stroke.DrawingAttributes.Width = newWidth;
                    stroke.DrawingAttributes.Height = newHeight;
                }
            }
        }

        /// <summary>
        /// 恢复选中笔画的粗细为默认值
        /// </summary>
        public void RestoreStrokeThickness()
        {
            foreach (Stroke stroke in _inkCanvas.GetSelectedStrokes())
            {
                stroke.DrawingAttributes.Width = _inkCanvas.DefaultDrawingAttributes.Width;
                stroke.DrawingAttributes.Height = _inkCanvas.DefaultDrawingAttributes.Height;
            }
        }
    }
}
