using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace PrivacyMonitor
{
    public partial class MainForm : Form
    {
        public MainForm(string[] args)
        {
            InitializeComponent();

            #region 命令行参数
            if(args.Length == 0)
            {
                //Console.WriteLine("无参数！");
            }
            else
            {
                if(args[0] == "-Hide")
                {
                    notifyIcon1.Visible = true;
                    this.ShowInTaskbar = false;
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                }
            }
            #endregion

            DBOperator = new DataHelper();
            //数据库是否存在
            if(!File.Exists(DBOperator.GetDataFileName))
            {
                DBOperator.InitDatabase();
            }
            else
            {
                //如果存在，并且未初始化
                if(!DBOperator.IsInitialized)
                {
                    DBOperator.InitDatabase();
                }
                else
                {
                    DBOperator.Open();
                }
            }

            Lbl_RecordCount.Text = "0";

            #region 开机启动
#if !DEBUG
            RegistryKey currentUser = Registry.CurrentUser;
            RegistryKey key = currentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            key.SetValue("Recorder", GetAppPath() + " -Hide");
#endif
#endregion

        }
        #region 全局变量
        //前台窗口1
        IntPtr ForegroundWindow0 = IntPtr.Zero;
        IntPtr ForegroundWindow1 = IntPtr.Zero;
        //前台窗口文本
        string ForeWinText = "";
        string ForeWinText1 = "";
        //最大字符长度
        const int MaxLen = 1024;
        //窗口字符缓冲区
        StringBuilder WindowTextBuffer = new StringBuilder(MaxLen);
        //类名
        string ClassName = "";

        //进程标识符
        int CurrentPID = 0;
        //int CopiedLen = 0;
        Process CurrentProc = null;
        DateTime StartTime;
        DateTime EndTime;
        long Duration = 0L;
        bool DurationFlag = false;
        string ProcPath = "";
        //数据库操作对象
        DataHelper DBOperator = null;

        long counter = 0;
        bool IdPassport = false;
        long IdleTime = 0;
        int ClickCounter = 0;

        #endregion

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Record();
            IdleTime = (GetLastInputTime() / 1000);
        }
        API.InputInfo InputInfo = new API.InputInfo();
        /// <summary>
        /// 系统空闲时间
        /// </summary>
        /// <returns></returns>
        public long GetLastInputTime()
        {
            InputInfo.cbSize = Marshal.SizeOf(InputInfo);
            if (!API.GetLastInputInfo(ref InputInfo))
                return 0;
            else
                return Environment.TickCount - (long)InputInfo.dwTime;
        }

        #region 获取当前程序的路径
        private string GetAppPath()
        {
            return this.GetType().Assembly.Location;
        }
        #endregion

        #region 处理并记录
        private void Record()
        {

            //判断是否已经做了时间差的处理
            if (!DurationFlag)
            {
                //开始时间
                StartTime = DateTime.Now;
                DurationFlag = true;
            }

            //超时自动删除许可
            if(IdleTime > 60*30)
            {
                IdPassport = false;
                ClickCounter = 0;
            }

            DlgText = "";
            //获取后一个前台窗口句柄和文本
            ForegroundWindow0 = API.GetForegroundWindow();
            //窗口文本
            API.GetWindowText(ForegroundWindow0, WindowTextBuffer, MaxLen);
            ForeWinText = WindowTextBuffer.ToString();
            //IdPassport = File.Exists(@"D:\ID");


            if (ForeWinText != ForeWinText1 || ForegroundWindow0 != ForegroundWindow1)
            {
                //结束时间
                EndTime = DateTime.Now;

                //持续时间
                Duration = (EndTime.Ticks - StartTime.Ticks) / 10000000;
                DurationFlag = false;

                //去掉时间为0的
                if (Duration > 0 && IdleTime < 1 * 60)
                {
                    try
                    {
                        //窗口文本
                        API.GetWindowText(ForegroundWindow1, WindowTextBuffer, MaxLen);
                        ForeWinText = WindowTextBuffer.ToString();

                        //类名
                        API.GetClassName(ForegroundWindow1, WindowTextBuffer, MaxLen);
                        ClassName = WindowTextBuffer.ToString();

                        //进程ID和进程名
                        API.GetWindowThreadProcessId(ForegroundWindow1, out CurrentPID);
                        CurrentProc = Process.GetProcessById(CurrentPID);

                        //进程路径
                        ProcPath = GetProcessPath(CurrentProc.ProcessName + ".exe");

                        //插入数据
                        DBOperator.InsertData("Record", new string[] {
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            ForeWinText == string.Empty ? "NULL" : ForeWinText,
                            ClassName == string.Empty ? "NULL" : ClassName,
                            CurrentProc.ProcessName,
                            ProcPath,
                            StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            Duration.ToString(),
                            DlgText
                        });

                        Lbl_RecordCount.Text = (++counter).ToString();
                    }
                    catch(Exception ex)
                    {
                        //throw ex;
                        DBOperator.InsertData("Log", new string[]
                        {
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            ex.TargetSite.Name,
                            ex.Message
                        });
                        //DBOperator.InsertData("Log", "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + ex.TargetSite.Name + "','" + ex.Message + "');");
                    }
                }
                else if(IdleTime > 1* 60)
                {
                    DBOperator.InsertData
                        ("record",new string[] {
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            "--IDLE",
                            "",
                            "",
                            "",
                            StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            Duration.ToString(),
                            DlgText }
                        );
                }

            }
            //更新句柄
            ForegroundWindow1 = ForegroundWindow0;
            ForeWinText1 = ForeWinText;
        }
        #endregion
        /// <summary>
        /// 获取指定对话框的内容
        /// </summary>
        /// <param name="HwndParent">要获取文本的父窗口句柄</param>
        /// <returns></returns>
        private void GetDialogText(IntPtr HwndParent)
        {
            API.EnumChildWindows(HwndParent, new API.CallBack(EnumChildWnd), 0);
        }
        StringBuilder buff = new StringBuilder(1024);
        string DlgText = "";
        //这里对每个子窗口进行获取文本的操作
        private bool EnumChildWnd(IntPtr hwnd, int x)
        {
            if(API.GetWindowText(hwnd, buff, buff.Capacity) > 0)
            {

                DlgText += buff.ToString() + ";";
            }
            return true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            timer1.Interval = 100;
            timer1.Enabled = true;
        }
        /// <summary>
        /// 获取指定进程的路径
        /// </summary>
        /// <param name="ProcessName">指定的进程名，需要.exe</param>
        /// <returns></returns>
        private string GetProcessPath(string ProcessName)
        {
            string[] properties = { "Name", "ExecutablePath" };
            //注意这里的进程名要加单引号
            SelectQuery s = new SelectQuery("Win32_Process", "Name = '" + ProcessName + "'", properties);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(s);
            try
            {
                foreach (ManagementObject o in searcher.Get())
                {
                    return o["ExecutablePath"].ToString();
                }

            }
            catch(Exception ex)
            {
                DBOperator.InsertData("Log", new string[] {
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ex.TargetSite.Name,
                    ex.Message
                });
                //DBOperator.InsertData("Log", "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + ex.TargetSite.Name + "','" + ex.Message + "');");
            }
            return string.Empty;
        }

        private void Picture_Hide_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(WindowState == FormWindowState.Minimized)
            {
                this.Show();
                WindowState = FormWindowState.Normal;
                this.Activate();
                this.ShowInTaskbar = true;
                notifyIcon1.Visible = false;
            }
        }
        /// <summary>
        /// 最小化时隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if(WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                this.Hide();
            }
        }
        /// <summary>
        /// 取消关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
        /// <summary>
        /// 显示提示窗口
        /// </summary>
        private void ShowAlert()
        {
            AlertWindow alertWindow = new AlertWindow();
            alertWindow.ShowDialog();
        }
        private void Picture_Close_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_StartStop_Click(object sender, EventArgs e)
        {
            //停用和启用
            if(timer1.Enabled == true)
            {
                ClickCounter++;
                if(ClickCounter >= 10)
                {
                    timer1.Enabled = false;
                    Lbl_RecordCount.Text = "Stoped";
                    //btn_StartStop.Enabled = false;
                    ClickCounter = 0;
                }
            }
            else
            {
                ClickCounter++;
                if(ClickCounter >= 5)
                {
                    timer1.Enabled = true;
                    Lbl_RecordCount.Text = "Running...";
                    ClickCounter = 0;
                }
            }
        }
        /// <summary>
        /// 如何启用一个许可
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                ClickCounter++;
                if(ClickCounter >= 5)
                {
                    IdPassport = true;
                    //DBOperator.InsertData("Log", "VALUES(" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",'解锁成功','');");
                    notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon1.BalloonTipTitle = "Tip";
                    notifyIcon1.BalloonTipText = "UNLOCKED!";
                    notifyIcon1.ShowBalloonTip(1000);
                }
            }
        }
    }


}
