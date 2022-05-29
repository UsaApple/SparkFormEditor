using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SparkFormEditor
{
    internal enum EnumMousePointPosition
    {
        MouseSizeNone = 0, //'无   
        MouseSizeRight = 1, //'拉伸右边框   
        MouseSizeLeft = 2, //'拉伸左边框   
        MouseSizeBottom = 3, //'拉伸下边框   
        MouseSizeTop = 4, //'拉伸上边框   
        MouseSizeTopLeft = 5, //'拉伸左上角   
        MouseSizeTopRight = 6, //'拉伸右上角   
        MouseSizeBottomLeft = 7, //'拉伸左下角   
        MouseSizeBottomRight = 8, //'拉伸右下角   
        MouseDrag = 9   // '鼠标拖动   
    }

    /// <summary>
    /// 线的种类；枚举类型
    /// </summary>
    internal enum LineTypes
    {
        //定义目前至少9中线的样式
        None,               //没有线  //TODO：如果不显示某条线，可以先绘制背景色的一条线来遮挡

        RectLine,           //行的四边单红线
        RectDoubleLine,     //行的四边双红线

        BottomLine,         //行的下边单红线
        BottomDoubleLine,   //行的下边双红线

        TopLine,            //行的上边单红线
        TopDoubleLine,      //行的上边双红线

        TopBottomLine,      //行的上下边单红线
        TopBottomDoubleLine,//行的上下边双红线

        //可扩展，虚线、波浪线等……
    }

    [Flags]
    internal enum AnimationFlags : int
    {
        Roll = 0x0000, // Uses a roll animation.
        HorizontalPositive = 0x00001, // Animates the window from left to right. This flag can be used with roll or slide animation.
        HorizontalNegative = 0x00002, // Animates the window from right to left. This flag can be used with roll or slide animation.
        VerticalPositive = 0x00004, // Animates the window from top to bottom. This flag can be used with roll or slide animation.
        VerticalNegative = 0x00008, // Animates the window from bottom to top. This flag can be used with roll or slide animation.
        Center = 0x00010, // Makes the window appear to collapse inward if Hide is used or expand outward if the Hide is not used.
        Hide = 0x10000, // Hides the window. By default, the window is shown.
        Activate = 0x20000, // Activates the window.
        Slide = 0x40000, // Uses a slide animation. By default, roll animation is used.
        Blend = 0x80000, // Uses a fade effect. This flag can be used only with a top-level window.
        Mask = 0xfffff,
    }

    internal enum CompareResult
    {
        /// <summary>
        /// Item do not appears
        /// </summary>
        Hidden,
        /// <summary>
        /// Item appears
        /// </summary>
        Visible,
        /// <summary>
        /// Item appears and will selected
        /// </summary>
        VisibleAndSelected
    }

    internal enum DenoteOrientation
    {
        Northwest,
        Northeast,
        Southeast,
        Southwest
    }

    /// <summary>
    /// Specifies the zoom mode for the <see cref="CoolPrintPreviewControl"/> control.
    /// </summary>
    internal enum ZoomMode
    {
        /// <summary>
        /// Show the preview in actual size.
        /// </summary>
        ActualSize,
        /// <summary>
        /// Show a full page.
        /// </summary>
        FullPage,
        /// <summary>
        /// Show a full page width.
        /// </summary>
        PageWidth,
        /// <summary>
        /// Show two full pages.
        /// </summary>
        OnePages,
        TwoPages,
        ThreePages,
        FourPages,
        FivePages,
        SixPages,
        /// <summary>
        /// Use the zoom factor specified by the <see cref="CoolPrintPreviewControl.Zoom"/> property.
        /// </summary>
        Custom
    }

    /// <summary>
    /// 打印页方式
    /// </summary>
    public enum PrintPageType
    {
        /// <summary>
        /// 当前页
        /// </summary>
        Current = 0,
        /// <summary>
        /// 全部打印
        /// </summary>
        All,
        /// <summary>
        /// 续打
        /// </summary>
        Continue
    }
    /// <summary>
    /// 插入行方式
    /// </summary>
    public enum InsertRowType
    {
        /// <summary>
        /// 插入上行
        /// </summary>
        Up,
        /// <summary>
        /// 插入下行
        /// </summary>
        Down
    }

    /// <summary>
    /// Types of animation of the pop-up window.
    /// </summary>
    [Flags]
    internal enum PopupAnimations
    {
        /// <summary>
        /// Uses no animation.
        /// </summary>
        None = 0,
        /// <summary>
        /// Animates the window from left to right. This flag can be used with roll or slide animation.
        /// </summary>
        LeftToRight = 0x00001,
        /// <summary>
        /// Animates the window from right to left. This flag can be used with roll or slide animation.
        /// </summary>
        RightToLeft = 0x00002,
        /// <summary>
        /// Animates the window from top to bottom. This flag can be used with roll or slide animation.
        /// </summary>
        TopToBottom = 0x00004,
        /// <summary>
        /// Animates the window from bottom to top. This flag can be used with roll or slide animation.
        /// </summary>
        BottomToTop = 0x00008,
        /// <summary>
        /// Makes the window appear to collapse inward if it is hiding or expand outward if the window is showing.
        /// </summary>
        Center = 0x00010,
        /// <summary>
        /// Uses a slide animation.
        /// </summary>
        Slide = 0x40000,
        /// <summary>
        /// Uses a fade effect.
        /// </summary>
        Blend = 0x80000,
        /// <summary>
        /// Uses a roll animation.
        /// </summary>
        Roll = 0x100000,
        /// <summary>
        /// Uses a default animation.
        /// </summary>
        SystemDefault = 0x200000,
    }

    /// <summary>
    /// 编辑器类别
    /// </summary>
    public enum EditorTypeEnum
    {

        [System.ComponentModel.Description("NURSE")]
        护士 = 0,
        [System.ComponentModel.Description("EMR")]
        医生
    }

    /// <summary>
    /// 表单类别
    /// </summary>
    public enum FormTypeEnum
    {
        /// <summary>
        /// 住院
        /// </summary>
        住院 = 0,
        /// <summary>
        /// 门诊
        /// </summary>
        门诊 = 1
    }
}
