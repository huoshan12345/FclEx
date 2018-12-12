using System.ComponentModel;

namespace FclEx.Npoi
{
    /// <summary>
    /// Office文件格式
    /// </summary>
    public enum OfficeType
    {
        /// <summary>
        /// 97-2003格式
        /// </summary>
        [Description("Office2003")]
        Office2003,
        /// <summary>
        /// 2007+格式
        /// </summary>
        [Description("Office2007")]
        Office2007
    }
}
