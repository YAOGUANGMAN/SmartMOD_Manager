using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;//
using System.IO;//
using System.IO.Compression;//

namespace SmartMOD_Manager
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private string gameFolder = null;//游戏目录路径（游戏目录）
        private string gameMODSFolder = null;//游戏MODS主目录路径（用来放置未安装的MOD子目录）
        private string gameDONEFolder = null;//游戏DONE主目录路径（用来放置安装完成后的MOD子目录）
        private string gameBACKFolder = null;//游戏BACK主目录路径（与需要安装的MOD子目录同名的文件夹，用来备份游戏目录中会被替换的文件）

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------定义加载游戏的内容-----------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 加载软件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Load(object sender, EventArgs e)
        {
            //读取记录游戏列表的GAMEList.ini文本
            string[] pathList = ControlTXTFile.ReadAllLines("GAMEList.ini");
            if (pathList != null && pathList.Length > 0)
            {
                //遍历数列添加一行记录到toolStripDropDownButton1下拉菜单和toolStripTextBox1路径信息栏
                toolStripDropDownButton1.DropDownItems.Clear();//清理组件内容
                foreach (string path in pathList)
                {
                    toolStripDropDownButton1.DropDownItems.Add(path);
                    toolStripDropDownButton1.DropDownItems[toolStripDropDownButton1.DropDownItems.Count - 1].Image = imageList1.Images[0];
                    toolStripTextBox1.Text = path;
                }

                //更新界面
                RefreshFormInfo(toolStripTextBox1.Text);
            }
            else
            {
                //更新界面
                toolStripTextBox1.Text = System.Windows.Forms.Application.StartupPath;//设置为APP所在目录
                RefreshFormInfo(toolStripTextBox1.Text);

                //提示添加新游戏
                DialogResult res = MessageBox.Show("监测到游戏列表不存在，你还没有添加过任何一款游戏到MOD管理器中，是否现在就手动添加？\n\n注意：如果选择“否”，意味着应用程序所在目录即为游戏目录。", "添加游戏", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    toolStripButton1_Click(sender, e);//添加新游戏
                }
            }
        }

        /// <summary>
        /// 添加新游戏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //选择游戏目录
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;

                //读取记录游戏列表的GAMEList.ini文本
                string pathString = ControlTXTFile.ReadAllText(("GAMEList.ini"));
                if (pathString != null && pathString.Trim() != "")
                {
                    //如果已经存在这条游戏列表记录，则不需要添加
                    if (pathString.ToLower().Contains(path.ToLower()))
                    {
                        MessageBox.Show("你已经添加过该游戏了，不需要重复添加！", "添加游戏", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //当前游戏路径更新
                        toolStripTextBox1.Text = path;
                    }
                    else
                    {
                        //添加一行记录到游戏列表的GAMEList.ini文本中
                        ControlTXTFile.AppendLine("GAMEList.ini", path);

                        //当前游戏路径更新
                        toolStripDropDownButton1.DropDownItems.Add(path);
                        toolStripDropDownButton1.DropDownItems[toolStripDropDownButton1.DropDownItems.Count - 1].Image = imageList1.Images[0];
                        toolStripTextBox1.Text = path;
                    }
                }
                else
                {
                    //添加一行记录到游戏列表的GAMEList.ini文本中
                    ControlTXTFile.AppendLine("GAMEList.ini", path);

                    //当前游戏路径更新
                    toolStripDropDownButton1.DropDownItems.Add(path);
                    toolStripDropDownButton1.DropDownItems[toolStripDropDownButton1.DropDownItems.Count - 1].Image = imageList1.Images[0];
                    toolStripTextBox1.Text = path;
                }

                //更新界面
                RefreshFormInfo(toolStripTextBox1.Text);
            }
        }

        /// <summary>
        /// 切换游戏目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripDropDownButton1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            toolStripTextBox1.Text = e.ClickedItem.ToString();//获取点击的项

            //更新界面
            RefreshFormInfo(toolStripTextBox1.Text);
        }

        /// <summary>
        /// 【方法】刷新ListView界面信息
        /// </summary>
        /// <param name="gamePath"></param>
        private void RefreshFormInfo(string gamePath)
        {
            //状态信息
            toolStripProgressBar1.Value = 0;

            //设置各主目录路径
            gameFolder = gamePath;
            gameMODSFolder = gamePath + "\\_MODS";
            gameDONEFolder = gamePath + "\\_DONE";
            gameBACKFolder = gamePath + "\\_BACKUP";

            //如果不存在各主目录则创建各主目录
            ControlALLFile.CreateOrGetSubfolder(gameMODSFolder);
            ControlALLFile.CreateOrGetSubfolder(gameDONEFolder);
            ControlALLFile.CreateOrGetSubfolder(gameBACKFolder);

            //根据游戏目录；获取MODS主目录内所有子目录，更新到ListView1，获取DONE主目录内所有子目录，更新到ListView2
            GetAllFolderToListView(gameMODSFolder, this.listView1);//获取MODS主目录内所有子目录，更新到ListView1
            GetAllFolderToListView(gameDONEFolder, this.listView2);//获取DONE主目录内所有子目录，更新到ListView2

            //状态信息
            toolStripProgressBar1.Value = 100;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------定义了一些操作ListView的方法---------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 【方法】获取目录内的所有子目录信息到ListView中,并分别在子目录内创建记录子目录内文件清单的MODFileList.ini文件
        /// </summary>
        /// <param name="folderPath">文件路径</param>
        /// <param name="listView">需要获取信息的ListView中</param>
        public void GetAllFolderToListView(string folderPath, ListView listView)
        {
            //设置ListView的初始状态
            listView.Items.Clear();//清空行
            listView.Sorting = SortOrder.Ascending;//listView数据行按照递增排序【按时间戳排序MOD安装顺序为递增】

            //如果指定文件路径存在子目录，则获取指定文件路径下的所有子目录
            string[] dirPathList = ControlALLFile.CreateOrGetSubfolder(folderPath);
            if (dirPathList != null && dirPathList.Length > 0)
            {
                //遍历子目录信息添加信息到ListView行中
                foreach (var dirPath in dirPathList)
                {
                    string folderName = dirPath.Substring(dirPath.LastIndexOf("\\") + 1).ToString();//截取获得子目录名
                    AddListViewItem(folderName, dirPath, listView);//添加到ListView

                    //如果子目录内用于记录子目录内文件清单的MODFileList.ini文件不存在，则创建记录到MODFileList.ini文件
                    string modListTXTFilePath = dirPath + "\\MODFileList.ini";
                    if (!File.Exists(modListTXTFilePath))
                    {
                        //遍历将子目录内的所有文件的路径信息添加到MODFileList.ini文件中
                        ControlALLFile controlAllFile = new ControlALLFile(dirPath, true);
                        List<string> fileListPath = controlAllFile.FileListPath;//获取子目录内文件的路径到List<string>
                        foreach (string path in fileListPath)
                        {
                            string deleteCharPath = path.Replace(dirPath, "");//删除掉路径前缀，只保留子目录下级字符串
                            ControlTXTFile.AppendLine(modListTXTFilePath, deleteCharPath);//添加一行记录到txt文本
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 【方法】将数据添加到listView1中
        /// </summary>
        /// <param name="item1">项一</param>
        /// <param name="item2">项二</param>
        public void AddListViewItem(string item1, string item2, ListView listView)
        {
            //ListView控件中项的建立
            ListViewItem lvItem; //ListViewItem 成员，初学者可以暂时把该项理解为在Details模式中的一整行信息
            ListViewItem.ListViewSubItem lvSubItem; //表示 ListViewItem 的子项，也就是ListViewItem一行中的一列信息。
            lvItem = new ListViewItem();
            lvItem.Text = item1;
            lvItem.ImageIndex = 0; //如果你添加了imageslist控件并设置了关联，在此可以选择需要显示的图标序列号。

            lvSubItem = new System.Windows.Forms.ListViewItem.ListViewSubItem();
            lvSubItem.Text = item2; //声明及设置Item的子项。
            lvItem.SubItems.Add(lvSubItem); //将子项添加到Item中。

            //添加Item项代码
            listView.BeginUpdate();
            listView.Items.Add(lvItem);
            listView.EndUpdate();
        }

        /// <summary>
        /// 【方法】获取选中的文件名
        /// </summary>
        /// <returns>第一行，第0个子项</returns>
        private string GetSelectItemName(ListView listView)
        {
            try
            {
                string str = listView.SelectedItems[0].SubItems[0].Text; //读取Item子项信息。
                return str;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 【方法】获取选中的文件路径
        /// </summary>
        /// <returns>第一行，第1个子项</returns>
        private string GetSelectItemPath(ListView listView)
        {
            try
            {
                string str = listView.SelectedItems[0].SubItems[1].Text; //读取Item子项信息。
                return str;
            }
            catch
            {
                return null;
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------定义安装、卸载MOD的方法--------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 安装MOD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            //获取选择的MODS主目录内的MOD子目录
            string selectFolderName = GetSelectItemName(this.listView1);//MODS内MOD子目录的名字
            string selectFolderPath = GetSelectItemPath(this.listView1);//MODS内MOD子目录的路径
            if (selectFolderPath != null && selectFolderName != null)
            {
                //安装MOD
                SetupMODFolder(selectFolderName, selectFolderPath);
            }
            else
            {
                MessageBox.Show("未选择任何MOD！", "安装MOD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// 【方法】安装MOD文件的方法
        /// </summary>
        /// <param name="folderName">选择安装的MOD子目录的名字</param>
        /// <param name="folderPath">选择安装的MOD子目录的路径</param>
        public void SetupMODFolder(string folderName, string folderPath)
        {
            //更新状态信息
            toolStripProgressBar1.Value = 0;

            //读取MODS内MOD子目录内所有文件信息的MODFileList.ini文件内容
            string[] pathFileList = ControlTXTFile.ReadAllLines(folderPath + "\\MODFileList.ini");//读取到数列
            if (pathFileList != null && pathFileList.Length > 0)
            {
                //时间戳
                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                //遍历MODS内MOD子目录内所有文件信息列表，进行复制和删除操作
                foreach (string path in pathFileList)
                {
                    //设置文件路径
                    string gameFilePath = gameFolder + path;//游戏目录路径
                    string gameMODSFilePath = gameMODSFolder + "\\" + folderName + path;//MODS内MOD子目录内的文件路径
                    string gameBACKFilePath = gameBACKFolder + "\\" + dateTime + "_" + folderName + path;//BACK内MOD同名游戏备份子目录内的文件路径【子目录增加时间戳】

                    //复制游戏源文件，到BACK内MOD同名游戏备份子目录
                    ControlALLFile.CopyFile(gameFilePath, gameBACKFilePath, true);
                    //复制MODS内MOD子目录内的文件，到游戏目录
                    ControlALLFile.CopyFile(gameMODSFilePath, gameFilePath, true);
                }

                //设置文件夹路径
                string gameMODSSubFolder = gameMODSFolder + "\\" + folderName;//选择安装的MODS内MOD子目录的路径
                string gameDONESubFolder = gameDONEFolder + "\\" + dateTime + "_" + folderName;//需要备份的DONE内MOD子目录的路径（子目录增加时间戳）

                //移动完成安装的MODS内MOD子目录到DONE内MOD子目录
                ControlALLFile.MoveFolder(gameMODSSubFolder, gameDONESubFolder);

                //更新界面
                RefreshFormInfo(toolStripTextBox1.Text);
                //MessageBox.Show("MOD安装成功！", "安装MOD", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //更新状态信息
                toolStripProgressBar1.Value = 100;
            }
            else
            {
                MessageBox.Show("未找到MOD文件树，安装失败！", "安装MOD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 卸载MOD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            //获取选择的DONE内MOD子目录
            string selectFolderName = GetSelectItemName(this.listView2);//DONE内MOD子目录的名字
            string selectFolderPath = GetSelectItemPath(this.listView2);//DONE内MOD子目录的路径
            if (selectFolderPath != null && selectFolderName != null)
            {
                //获取当前选择的listView行
                int selectIndex = listView2.SelectedIndices[0];//当前行序号
                int lastIndex = listView2.Items.Count - 1;//最后一行的序号
                if (selectIndex < lastIndex)
                {
                    //如果选择的listView行不是最后一行，则提示卸载剩余行对应的MOD
                    DialogResult res = MessageBox.Show("你选择的MOD是后来安装MOD的依赖项，为了防止后来安装的MOD运行时出错，有必要将其一起删除，请问是否将后面安装的MOD一起删除？\n\n注意：如果选择“否”，意味着什么都不做。", "卸载MOD", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.Yes)
                    {
                        //循环获取listView剩余行对应的DONE内MOD子目录信息，并卸载MOD（包含本行对应的MOD）
                        while (selectIndex <= lastIndex)
                        {
                            //获取最后一行对应的DONE内MOD子目录信息
                            ListViewItem lastItem = listView2.Items[listView2.Items.Count - 1];
                            string folderName = lastItem.SubItems[0].Text;
                            string folderPath = lastItem.SubItems[1].Text;
                            //卸载该MOD
                            UninsMODFolder(folderName, folderPath);
                            selectIndex++;//计数增加
                        }
                    }
                    else
                    {
                        MessageBox.Show("没有卸载任何MOD，你必须连同后来安装的MOD一起卸载！", "卸载MOD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    //卸载当前选择行对应的MOD
                    UninsMODFolder(selectFolderName, selectFolderPath);
                }
            }
            else
            {
                MessageBox.Show("未选择任何MOD！", "卸载MOD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// 【方法】卸载MOD文件的方法
        /// </summary>
        /// <param name="folderName">选择卸载的MOD子目录的名字</param>
        /// <param name="folderPath">选择卸载的MOD子目录的路径</param>
        public void UninsMODFolder(string folderName, string folderPath)
        {
            //更新状态信息
            toolStripProgressBar1.Value = 0;

            //读取DONE内MOD子目录内所有文件信息的MODFileList.ini文件内容
            string[] pathFileList = ControlTXTFile.ReadAllLines(folderPath + "\\MODFileList.ini");//读取txt文本记录到数列
            if (pathFileList != null && pathFileList.Length > 0)
            {
                //遍历DONE内MOD子目录内所有文件信息列表，进行复制和删除操作
                foreach (string path in pathFileList)
                {
                    //设置文件路径
                    string gameFilePath = gameFolder + path;//游戏目录路径
                    string gameBACKFilePath = gameBACKFolder + "\\" + folderName + path;//BACK内MOD同名游戏备份子目录内的文件路径

                    //先删除游戏目录内所有MOD文件
                    ControlALLFile.DeleteFile(gameFilePath);
                    //复制BACK内MOD同名游戏备份子目录内文件，到游戏目录
                    ControlALLFile.CopyFile(gameBACKFilePath, gameFilePath, true);
                }

                //设置文件夹路径
                string gameMODSSubFolder = gameMODSFolder + "\\" + folderName.Substring(folderName.IndexOf('_') + 1);//选择卸载的MODS内MOD子目录的路径【删除子目录时间戳】
                string gameDONESubFolder = gameDONEFolder + "\\" + folderName;
                string gameBACKSubFolder = gameBACKFolder + "\\" + folderName;

                //移动完成卸载的DONE内MOD子目录到MODS内MOD子目录，删除完成卸载的BACK内MOD同名游戏备份子目录
                ControlALLFile.MoveFolder(gameDONESubFolder, gameMODSSubFolder);
                ControlALLFile.DeleteFolder(gameBACKSubFolder, true);

                //更新界面
                RefreshFormInfo(toolStripTextBox1.Text);
                //MessageBox.Show("MOD卸载成功！", "卸载MOD", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //更新状态信息
                toolStripProgressBar1.Value = 100;
            }
            else
            {
                MessageBox.Show("未找到MOD文件树，卸载失败！", "卸载MOD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------定义解压导入、压缩导出、删除的MOD的方法---------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 打开ZIP格式的MOD文件，解压缩到MODS主目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            try
            {
                //选择MOD压缩包所在的位置
                OpenFileDialog fileBrowserDialog1 = new OpenFileDialog();
                fileBrowserDialog1.Filter = "zip压缩文件 (*.zip)|*.zip|所有文件 (*.*)|*.*";
                if (fileBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    string selectFile = fileBrowserDialog1.FileName;//ZIP格式压缩包

                    //判断选择解压的MOD是否已经在MODS主目录和DONE主目录存在同名的文件夹，如果不存在则可以解压缩
                    bool existsListView = false;
                    string folderName = Path.GetFileNameWithoutExtension(selectFile);//获取选择解压的MOD文件名（压缩包名应与压缩包内基目录名一致），不包括扩展名
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        ListViewItem item = listView1.Items[i];
                        string subFolderName = item.SubItems[0].Text;//获取listView1中记录的子目录名
                        if (subFolderName.ToLower().Contains(folderName.ToLower()))//如果选择解压的文件名存在于listView1中记录中,返回true
                        {
                            existsListView = true;
                        }
                    }
                    for (int i = 0; i < listView2.Items.Count; i++)
                    {
                        ListViewItem item = listView2.Items[i];
                        string subFolderName = item.SubItems[0].Text;//获取listView2中记录的子目录名
                        if (subFolderName.ToLower().Contains(folderName.ToLower()))//如果选择解压的文件名存在于listView1中记录中,返回true
                        {
                            existsListView = true;
                        }
                    }
                    if (!existsListView)
                    {
                        //均不存在，则解压缩
                        ZipFile.ExtractToDirectory(selectFile, gameMODSFolder);

                        //更新界面
                        RefreshFormInfo(toolStripTextBox1.Text);
                        //MessageBox.Show("已成功导入到MODS库文件夹！", "导入MOD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("已安装了同名的MOD文件，请检查文件！\n\n注意：zip压缩包名应与压缩包内基目录名一致，且不能与已安装的MOD同名", "导入MOD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            catch
            {
                MessageBox.Show("操作失败！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 通过拖拽打开ZIP格式的MOD文件，解压缩到MODS主目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            // 判断拖拽的数据格式
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        //释放拖拽
        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                //获取拖拽文件的路径
                string selectFile = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

                //如果格式为zip，则可以继续操作
                if (Path.GetExtension(selectFile) == ".zip")
                {
                    //判断选择解压的MOD是否已经在MODS主目录和DONE主目录存在同名的文件夹，如果不存在则可以解压缩
                    bool existsListView = false;
                    string folderName = Path.GetFileNameWithoutExtension(selectFile);//获取选择解压的MOD文件名（压缩包名应与压缩包内基目录名一致），不包括扩展名
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        ListViewItem item = listView1.Items[i];
                        string subFolderName = item.SubItems[0].Text;//获取listView1中记录的子目录名
                        if (subFolderName.ToLower().Contains(folderName.ToLower()))//如果选择解压的文件名存在于listView1中记录中,返回true
                        {
                            existsListView = true;
                        }
                    }
                    for (int i = 0; i < listView2.Items.Count; i++)
                    {
                        ListViewItem item = listView2.Items[i];
                        string subFolderName = item.SubItems[0].Text;//获取listView2中记录的子目录名
                        if (subFolderName.ToLower().Contains(folderName.ToLower()))//如果选择解压的文件名存在于listView1中记录中,返回true
                        {
                            existsListView = true;
                        }
                    }
                    if (!existsListView)
                    {
                        //均不存在，则解压缩
                        ZipFile.ExtractToDirectory(selectFile, gameMODSFolder);

                        //更新界面
                        RefreshFormInfo(toolStripTextBox1.Text);
                        //MessageBox.Show("已成功导入到MODS库文件夹！", "导入MOD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("已安装了同名的MOD文件，请检查文件！\n\n注意：zip压缩包名应与压缩包内基目录名一致，且不能与已安装的MOD同名", "导入MOD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            catch
            {
                MessageBox.Show("操作失败！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 压缩选择的MODS内MOD子目录到压缩包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            try
            {
                //获取选择的MODS内MOD子目录
                string selectFolderName = GetSelectItemName(this.listView1);//MODS内MOD子目录的名字
                string selectFolderPath = GetSelectItemPath(this.listView1);//MODS内MOD子目录的路径
                if (selectFolderPath != null && selectFolderName != null)
                {
                    //选择压缩到的位置
                    FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string selectFilePath = folderBrowserDialog1.SelectedPath + "\\" + selectFolderName + ".zip";//需要压缩成的zip文件路径

                        //压缩
                        ZipFile.CreateFromDirectory(selectFolderPath, selectFilePath, CompressionLevel.Fastest, true);
                        MessageBox.Show("已成功导出MOD文件到指定位置！", "导出MOD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("未选择任何MOD！", "导出MOD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch
            {
                MessageBox.Show("操作失败！", "导出MOD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 删除选择的MODS内MOD子目录的文件夹及所有文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            try
            {
                //获取选择的MODS内MOD子目录
                string selectFolderName = GetSelectItemName(this.listView1);//MODS内MOD子目录的名字
                string selectFolderPath = GetSelectItemPath(this.listView1);//MODS内MOD子目录的路径
                if (selectFolderPath != null && selectFolderName != null)
                {
                    //删除MODS内MOD子目录的文件夹及所有文件
                    ControlALLFile.DeleteFolder(selectFolderPath, true);

                    //更新界面
                    RefreshFormInfo(toolStripTextBox1.Text);
                    MessageBox.Show("已成功删除选择的MOD文件！", "删除MOD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("未选择任何MOD！", "删除MOD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch
            {
                MessageBox.Show("操作失败！", "删除MOD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------定义其他必要的设置内容--------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            RefreshFormInfo(toolStripTextBox1.Text);
        }

        /// <summary>
        /// 打开游戏目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Process.Start(gameFolder);
        }

        /// <summary>
        /// 打开MODS主目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            Process.Start(gameMODSFolder);
        }

        /// <summary>
        /// listView图标选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mOD库显示图标ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        //列表
        private void mOD库显示列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        //图标
        private void dONE库显示列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView2.View = View.LargeIcon;
        }

        //列表
        private void dONE库显示列表ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listView2.View = View.Details;
        }

        /// <summary>
        /// 自动调整toolStripTextBox框大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Resize(object sender, EventArgs e)
        {
            toolStripTextBox1.Width = toolStrip2.Width - 470;
        }

        /// <summary>
        /// 关于
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("确定要退出程序吗？", "退出程序", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------程序更新于202611242006--------------------------------------------------------------------
//-------------------------------------------------------------------------作者：姚广满（Matt.Yao）-------------------------------------------------------------------
//--------------------------------------------------------------------------------順雲邊月----------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------------------------------------------------------------------------