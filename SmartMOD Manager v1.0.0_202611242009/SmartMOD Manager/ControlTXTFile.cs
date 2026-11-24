using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;//

namespace SmartMOD_Manager
{
    /// <summary>
    /// TXT文本文件读写类
    /// </summary>
    public class ControlTXTFile
    {
        /// <summary>
        /// 追加一行内容记录，自动加换行符
        /// </summary>
        /// <param name="filePath">txt文件路径,文件不存在则新建文件</param>
        /// <param name="lineString">增加的记录内容</param>
        public static void AppendLine(string filePath, string lineString)
        {
            File.AppendAllText(filePath, lineString + Environment.NewLine, Encoding.UTF8);
        }

        /// <summary>
        /// 追加内容到记录，不自动加换行符
        /// </summary>
        /// <param name="filePath">txt文件路径,文件不存在则新建文件</param>
        /// <param name="contentString">增加的记录内容</param>
        public static void AppendText(string filePath, string contentString)
        {
            File.AppendAllText(filePath, contentString, Encoding.UTF8);
        }

        /// <summary>
        /// 读取全部文本内容到一个字符串
        /// </summary>
        /// <param name="filePath">txt文件路径</param>
        /// <returns>字符串，如果没有找到txt文件返回null</returns>
        public static string ReadAllText(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath, Encoding.UTF8);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 读取所有行文本内容到数组中
        /// </summary>
        /// <param name="filePath">txt文件路径</param>
        /// <returns>数组，如果没有找到txt文件返回null</returns>
        public static string[] ReadAllLines(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath, Encoding.UTF8);
            }
            else
            {
                return null;
            }
        }
    }
}
