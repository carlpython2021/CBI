using System;

namespace NanJingNanStation
{
    /// <summary>
    /// 发车口信息类，包含发车口占用锁闭信息、区间占用状态信息、允许或禁止发车信息
    /// </summary>
    // 在C#中，public关键字用于修饰类（或成员），表示该类对所有其他代码都是可见和可访问的（即“公开的”）。
    // 如果不加public，则类默认为internal级别，表示只能在同一个程序集中访问，其他程序集无法访问到该类。
    // 因此，加public可以确保DeparturePortInfo类在引用此程序集的其他项目中也能被访问和使用。
    public class DeparturePortInfo
    {
        /// <summary>
        /// 发车口占用锁闭信息（如"空闲"、"占用"、"锁闭"等）
        /// </summary>
        public string PortOccupyLockStatus = "空闲";

        /// <summary>
        /// 区间占用状态信息（如"空闲"、"占用"）
        /// </summary>
        public string SectionOccupyStatus = "空闲";

        /// <summary>
        /// 允许或禁止发车信息（如"允许发车"、"禁止发车"）
        /// </summary>
        public string AllowDepartureStatus = "允许发车";
        public string DepartureDirection = "允许发车";

    }
}

