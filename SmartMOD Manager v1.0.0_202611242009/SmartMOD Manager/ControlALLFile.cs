using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;//
using System.Collections;//

namespace SmartMOD_Manager
{
    /// <summary>
    /// 文件操作类
    /// </summary>
    public class ControlALLFile
    {
        //字段声明
        private List<string> fileListPath = new List<string>();
        private List<string> fileListName = new List<string>();

        /// <summary>
        /// 文件路径
        /// </summary>
        public List<string> FileListPath
        {
            get { return fileListPath; }
        }

        /// <summary>
        /// 文件名称
        /// </summary>
        public List<string> FileListName
        {
            get { return fileListName; }
        }

        /// <summary>
        /// 构造函数，通过遍历文件夹获取文件名称，路径到的List<string>列表
        /// </summary>
        /// <param name="sourceFolder">文件夹路径</param>
        /// <param name="nextFold">是否继续查找更深路径</param>
        public ControlALLFile(string sourceFolder, bool nextFolder)
        {
            DirectoryInfo dirSource = new DirectoryInfo(sourceFolder);
            GetAllFileList(dirSource, nextFolder);
        }

        /// <summary>
        /// 【方法】获取目录内所有的文件
        /// </summary>
        /// <param name="sourceFolder">文件目录信息实例</param>
        /// <param name="nextFold">查找更深目录</param>
        public void GetAllFileList(DirectoryInfo sourceFolder, bool nextFolder = true)
        {
            try
            {
                //遍历获取当前目录内所有文件，添加文件信息到List<string>列表
                foreach (FileInfo fi in sourceFolder.GetFiles())
                {
                    fileListPath.Add(fi.FullName);
                    fileListName.Add(fi.Name);
                }

                //如果需要向更深的目录获取文件，则递归获取子目录内所有文件
                if (nextFolder)
                {
                    //递归向更深的子目录
                    foreach (DirectoryInfo sourceSubfolder in sourceFolder.GetDirectories())
                    {
                        GetAllFileList(sourceSubfolder, nextFolder);
                    }
                }
            }
            catch
            { }
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------定义了一些静态方法用来操作文件-------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 创建目录或获取子目录，如果存在目录则返回子目录路径，如果不存在目录则创建
        /// </summary>
        /// <param name="dirPath">目录路径</param>
        /// <returns>一个存放子目录路径的数组</returns>
        public static string[] CreateOrGetSubfolder(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                string[] dirList = Directory.GetDirectories(dirPath);
                return dirList;
            }
            else
            {
                Directory.CreateDirectory(dirPath);
                return null;
            }
        }

        /// <summary>
        /// 复制单个文件，自动创建目标所在的文件夹
        /// </summary>
        /// <param name="srcFile">源文件路径</param>
        /// <param name="dstFile">目标文件路径</param>
        /// <param name="overwrite">是否覆盖</param>
        /// <returns>如果成功返回true，否则为false</returns>
        public static bool CopyFile(string srcFile, string dstFile, bool overwrite = true)
        {
            try
            {
                if (File.Exists(srcFile))
                {
                    // 获取目标文件所在目录，不存在就创建
                    string dstDir = Path.GetDirectoryName(dstFile);
                    Directory.CreateDirectory(dstDir);

                    //拷贝文件
                    File.Copy(srcFile, dstFile, overwrite);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 移动某个文件夹到新路径，移动后原始目录将删除
        /// </summary>
        /// <param name="srcFolder">源文件夹路径</param>
        /// <param name="dstFolder">目标文件夹路径</param>
        /// <returns>如果成功返回true，否则为false（移动到已存在的目录则报错返回false）</returns>
        public static bool MoveFolder(string srcFolder, string dstFolder)
        {
            try
            {
                if (Directory.Exists(srcFolder))
                {
                    Directory.Move(srcFolder, dstFolder);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除指定文件夹
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <param name="overwrite">是否覆盖</param>
        /// <returns>如果成功返回true，否则为false</returns>
        public static bool DeleteFolder(string dirPath, bool overwrite = true)
        {
            try
            {
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, overwrite);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除指定文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否成功</returns>
        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断一个路径是文件还是文件夹【未使用】
        /// </summary>
        /// <param name="path">文件或文件夹的路径</param>
        /// <returns>如果是文件夹则返回true,否则为false</returns>
        public static bool IsFolder(string path)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                if ((fileInfo.Attributes & FileAttributes.Directory) != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}