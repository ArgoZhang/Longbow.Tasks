#if !NET45
using System.IO;
using System.Runtime.InteropServices;

namespace System
{
    internal static class OSPlatformPathExtenstions
    {
        /// <summary>
        /// 获得 当前操作系统目录分隔符的路径
        /// </summary>
        /// <param name="originalString">原始路径字符串</param>
        /// <returns></returns>
        public static string GetOSPlatformPath(this string originalString)
        {
            var sp = Path.DirectorySeparatorChar;
            var win = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            return win ? originalString.Replace('/', sp) : originalString.Replace('\\', sp);
        }
    }
}
#endif
