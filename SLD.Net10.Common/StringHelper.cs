using System;

namespace SLD.Net10.Common
{
    /// <summary>
    /// 字符串通用工具类
    /// </summary>
    public static class StringHelper
    {
        #region Guid 相关
        /// <summary>
        /// 标准Guid（带横杠）
        /// 示例：6574218b-8322-4475-8611-925542f81234
        /// </summary>
        public static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 32位无横杠Guid
        /// 示例：6574218b832244758611925542f81234
        /// </summary>
        public static string GetGuidWithoutDash()
        {
            return Guid.NewGuid().ToString("N");
        }
        #endregion

        #region 时间+Guid组合（适合业务主键ID）
        /// <summary>
        /// 【年月日时分秒 + 无横杠Guid】默认下划线分隔
        /// 格式：yyyyMMddHHmmss_Guid32
        /// 示例：20260804162015_6574218b832244758611925542f81234
        /// </summary>
        public static string GetDateTimeWithGuid(string separator = "_")
        {
            var timePart = DateTime.Now.ToString("yyyyMMddHHmmss");
            var guidPart = GetGuidWithoutDash();
            return $"{timePart}{separator}{guidPart}";
        }

        /// <summary>
        /// 使用UTC时间，避免时区差异，【时间+Guid】
        /// </summary>
        public static string GetUtcDateTimeWithGuid(string separator = "_")
        {
            var timePart = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guidPart = GetGuidWithoutDash();
            return $"{timePart}{separator}{guidPart}";
        }

        /// <summary>
        /// 自定义时间格式 + Guid
        /// </summary>
        /// <param name="dateFormat">时间格式</param>
        /// <param name="separator">分隔符</param>
        public static string GetCustomDateTimeWithGuid(string dateFormat, string separator = "_")
        {
            var timePart = DateTime.Now.ToString(dateFormat);
            var guidPart = GetGuidWithoutDash();
            return $"{timePart}{separator}{guidPart}";
        }
        #endregion
    }
}