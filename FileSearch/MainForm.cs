using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace 文件搜索
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 搜索状态，为true时处于搜索状态，为false时停止搜索
        /// </summary>
        private bool state = false;
        /// <summary>
        /// 文件数
        /// </summary>
        private int fileCount = 0;
        /// <summary>
        /// 文件夹数
        /// </summary>
        private int dirCount = 0;
        /// <summary>
        /// 搜索起始时刻
        /// </summary>
        private DateTime startTime;
        /// <summary>
        /// 搜索结束时刻
        /// </summary>
        private DateTime endTime;
        /// <summary>
        /// 关键字
        /// </summary>
        private String keyWord="";
        /// <summary>
        /// 临时搜索路径
        /// </summary>
        private String pathStr = "";
        /// <summary>
        /// 临时搜索文件夹路径数组
        /// </summary>
        string[] directoryNames;
        /// <summary>
        /// 临时搜索文件名数组
        /// </summary>
        string[] fileNames;
        /// <summary>
        /// 搜索文件路径
        /// </summary>
        String path = "";
        /// <summary>
        /// 磁盘符数组，未选择路径时默认在四个盘里搜索
        /// </summary>
        String[] defaultPath = { "C:\\", "D:\\", "E:\\", "F:\\" };

        /// <summary>
        /// 嵌套循环搜索方法
        /// </summary>
        /// <param name="path">搜索路径</param>
        public void fun(string path)
        {
            if(state)
            {
                try
                {
                    directoryNames = Directory.GetDirectories(path);//获取目录
                    fileNames = Directory.GetFiles(path);//获取文件名
                    //遍历每一个文件名
                    foreach (string file in fileNames)
                    {
                        //只对文件绝对路径中的文件名字符串进行对比匹配，避免与上一级目录中含有关键词重复
                        pathStr = file.Substring(file.LastIndexOf("\\"));
                        if (pathStr.Contains(keyWord))
                        {
                            //文件名中含有关键字，则将文件绝对路径添加到列表中
                            lbFileList.Items.Add(file.ToString());
                            System.Windows.Forms.Application.DoEvents(); //必须加注这句代码，否则listBox将因为循环执行太快而来不及显示信息
                        }
                        fileCount++;//文件计数加 1
                    }
                    //遍历每一个文件夹
                    foreach (string directory in directoryNames)
                    {
                        //只对文件绝对路径中的最后一个文件夹名的字符串进行对比匹配，避免与上一级目录中含有关键词重复
                        pathStr = directory.Substring(directory.LastIndexOf("\\"));
                        if (pathStr.Contains(keyWord))
                        {
                            //文件夹名中含有关键字，则将文件夹绝对路径添加到列表中
                            lbFileList.Items.Add(directory.ToString());
                            System.Windows.Forms.Application.DoEvents();
                        }
                        dirCount++;//文件夹计数加 1
                        fun(directory);//嵌套搜索当前文件夹
                    }
                }
                catch(Exception e)
                {
                    //异常处理
                    Console.WriteLine(e.ToString());
                }
            }
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 搜索按钮点击事件响应方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            //获取路径输入框文本
            path = this.tbPath.Text;
            //判断
            if (btnSearch.Text == "搜索" && tbWord.Text != "")
            {
                if (Directory.Exists(path) || path == "")
                {
                    //路径名存在或路径名为空时启动搜索线程开始搜索
                    Thread searchThread = new Thread(new ThreadStart(SearchThread));
                    searchThread.Start();
                }
                else
                {
                    //路径不存在，弹出消息框并重新聚焦路劲选择按钮
                    MessageBox.Show("此路径不存在!", "提示", MessageBoxButtons.OK);
                    btnScan.Focus();
                }
            }
            else if (tbWord.Text == "")
            {
                //关键字为空弹出提示信息
                MessageBox.Show("请输入要查找的文件夹或文件名");
            }      
            else
            {
                btnSearch.Text = "搜索";
                state = false;
            }
        }

        /// <summary>
        /// 路劲输入框文本改变事件响应方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbPath_TextChanged(object sender, EventArgs e)
        {
            string path = this.tbPath.Text;
            if (Directory.Exists(path) || path == "")
            {
                //路径名存在或路径名为空时可以进行搜索，此时搜索按钮可用
                btnSearch.Enabled = true;
            }
            else
            {
                //不符合可搜索状态，搜索按钮不可用
                btnSearch.Enabled = false;
            }
        }

        /// <summary>
        /// 路径选择按钮点击事件响应方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScan_Click(object sender, EventArgs e)
        {
            //实例化文件夹选择对话框
            FolderBrowserDialog fBD = new FolderBrowserDialog();
            if (fBD.ShowDialog() == DialogResult.Cancel)
            {
                //取消选择文件夹时重新聚焦路径选择按钮
                tbPath.Focus();
                return;
            }
            else
            {
                //选择好搜索路径时准备就绪并聚焦搜索按钮
                this.tbPath.Text = fBD.SelectedPath;
                lbl1.Text = "准备就绪...";
                btnSearch.Focus();
            }      
        }

        /// <summary>
        /// 主窗体加载事件响应方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            //在桌面生成1000个测试用文件
            //Directory.CreateDirectory("C:/Users/Lenovo/Desktop/Test");
            //for (int i = 0; i < 100; i++)
            //{
            //    string filename = "C:/Users/Lenovo/Desktop/Test/";
            //    for (int k = 0; k < 10; k++)
            //    {
            //        filename = "C:/Users/Lenovo/Desktop/Test/";
            //        for (int j = 0; j < i; j++)
            //            filename += "1";
            //        File.Create(filename + "a" + k + ".txt");
            //    }
            //}
        }

        /// <summary>
        /// 搜索线程
        /// </summary>
        public void SearchThread()
        {
            //异步线程操作控件时不进行交叉线程处理检测
            Control.CheckForIllegalCrossThreadCalls = false;
            //清空文件列表
            lbFileList.Items.Clear();
            System.Windows.Forms.Application.DoEvents();
            //清空文件计数
            fileCount = 0;
            //清空文件夹计数
            dirCount = 0;
            //初始化开始搜索时间
            startTime = DateTime.Now;
            //搜索状态为true，开始搜索
            state = true;
            //搜索按钮文字变为“暂停”
            btnSearch.Text = "暂停";
            //状态显示文本显示当前状态
            lbl1.Text = "正在搜素...";
            //获取搜索关键字
            keyWord = tbWord.Text;
            if (path == "")
            {
                //搜索路径为空时使用默认路径，遍历默认路径字符串数组
                foreach (String cPath in defaultPath)
                    fun(cPath);
            }
            else
            {
                //在用户选择路径下进行搜索
                fun(path);
            }
            //搜索完毕，记录结束时间
            endTime = DateTime.Now;
            //状态显示
            lbl1.Text = "查找完成 共查找" + fileCount + "个文件" + dirCount + "个文件夹 " + calculateTime(startTime, endTime);
            //恢复搜索按钮显示文字，并变为可用状态
            btnSearch.Text = "搜索";
            btnSearch.Enabled = true;
        }

        /// <summary>
        /// 计算搜索总共用时
        /// </summary>
        /// <param name="start">起始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>返回计算好后的时间字符串</returns>
        public string calculateTime(DateTime start,DateTime end)
        {
            //计算搜索时间用临时变量
            int hour = 0;
            int minute = 0;
            int second = 0;
            int millisecond = 0;
            double totalMilliseconds = (end - start).TotalMilliseconds;
            if (totalMilliseconds < 1000)
            {
                //用时小于1000毫秒
                return "用时" + totalMilliseconds + "毫秒";
            }
            else if (totalMilliseconds < 60 * 1000)
            {
                //用时大于等于1秒小于1000毫秒
                second = (int)(totalMilliseconds / 1000);
                millisecond = (int)(totalMilliseconds % 1000);
                return "用时" + second + "秒" + millisecond + "毫秒";
            }
            else if (totalMilliseconds < 60 * 1000 * 60)
            {
                //用时大于等于1分钟小于1小时
                minute = (int)(totalMilliseconds / 60000);
                second = (int)((totalMilliseconds % 60000) / 1000);
                millisecond = (int)(totalMilliseconds % 1000);
                return "用时" + minute + "分" + second + "秒" + millisecond + "毫秒";
            }
            else if (totalMilliseconds < 60 * 1000 * 60 * 24)
            {
                //用时大于等于1小时小于一天
                hour = (int)(totalMilliseconds / 3600000);
                minute = (int)((totalMilliseconds % 3600000) / 60000);
                second = (int)(((totalMilliseconds % 3600000) % 60000) / 1000);
                millisecond = (int)(totalMilliseconds % 1000);
                return "用时" + hour + "时" + minute + "分" + second + "秒" + millisecond + "毫秒";
            }
            else
            {
                //用时大于等于一天
                return "用时" + totalMilliseconds / 86400000 + "天";
            }
        }
        /// <summary>
        /// 鼠标点击路径输入框事件响应方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbPath_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标点击路径输入框时显示状态
            lbl1.Text = "请添加搜索的范围路径...";
        }

        /// <summary>
        /// 文件列表双击事件响应方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbFileList_DoubleClick(object sender, EventArgs e)
        {
            //获取鼠标双击列表所在项的文件或文件夹绝对路径
            string path = lbFileList.SelectedItem.ToString();
            if (Directory.Exists(path))
            {
                //若为文件夹，调用资源管理器打开当前路径
                Process.Start("explorer.exe", path);
            }
            else
            {
                //若为文件，调用资源管理器定位到当前文件并选中
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                psi.Arguments = " /select," + path;
                System.Diagnostics.Process.Start(psi);
            }
        }
    }
}
