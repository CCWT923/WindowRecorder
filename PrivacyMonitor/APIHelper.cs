using System;
using System.Runtime.InteropServices;
using System.Text;


static class APIHelper
{
    /// <summary>
    /// 通过类名或标题查找窗口
    /// </summary>
    /// <param name="ClassName">窗口类名</param>
    /// <param name="CaptionText">窗口标题</param>
    /// <returns></returns>
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern IntPtr FindWindow(string ClassName, string CaptionText);

    /// <summary>
    /// 获取活动窗口句柄
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    public static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// 获取桌面窗口句柄
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
    public static extern IntPtr GetDesktopWindow();

    /// <summary>
    /// 获取与指定窗口有特定关系的窗口句柄
    /// </summary>
    /// <param name="Hwnd">指定的窗口</param>
    /// <param name="WindowRalation">与指定窗口的关系</param>
    /// <returns>窗口句柄</returns>
    [DllImport("user32.dll", EntryPoint = "GetWindow")]
    public static extern IntPtr GetWindow(IntPtr Hwnd, WindowRelation WindowRalation);

    /// <summary>
    /// 用于指定窗口之间关系的枚举值
    /// </summary>
    public enum WindowRelation : uint
    {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST,
        GW_HWNDNEXT,
        GW_HWNDPREV,
        GW_HWNDOWNER,
        GW_CHILD,
        GW_ENABLEDPOPUP
    };
    /// <summary>
    /// 获取指定窗口标题
    /// </summary>
    /// <param name="Hwnd">要获取标题的窗口</param>
    /// <param name="StrBuffer">用于存储窗口标题文字的缓冲区</param>
    /// <param name="MaxLength">最大文本长度</param>
    /// <returns>实际拷贝到缓冲区的大小</returns>
    [DllImport("user32.dll", EntryPoint = "GetWindowText")]
    public static extern int GetWindowText(IntPtr Hwnd, StringBuilder StrBuffer, int MaxLength);

    /// <summary>
    /// 获取指定窗口的类名
    /// </summary>
    /// <param name="Hwnd">指定窗口的句柄</param>
    /// <param name="StrBuffer">用于存储窗口类名的缓冲区</param>
    /// <param name="MaxLength">最大文本长度</param>
    /// <returns>实际的文本长度</returns>
    [DllImport("user32.dll", EntryPoint = "GetClassName")]
    public static extern int GetClassName(IntPtr Hwnd, StringBuilder StrBuffer, int MaxLength);

    /// <summary>
    /// 禁用窗口，禁止与鼠标、键盘交互
    /// </summary>
    /// <param name="Hwnd">指定窗口句柄</param>
    /// <param name="Enable">该窗口允许（True）还是被禁止（FAlse）</param>
    /// <returns></returns>
    [DllImport("user32.dll", EntryPoint = "EnableWindow")]
    public static extern bool EnableWindow(IntPtr Hwnd, bool Enable);

    /// <summary>
    /// 锁定计算机
    /// </summary>
    /// <returns></returns>
    [DllImport("user32.dll", EntryPoint = "LockWorkStation")]
    public static extern bool LockWorkStation();

    /// <summary>
    /// 获取指定窗口的进程标识符（PID）
    /// </summary>
    /// <param name="Hwnd">指定窗口的句柄</param>
    /// <param name="PID">out参数，接收PID值的参数，如果为null，则不进行拷贝</param>
    /// <returns>返回创建该窗口的线程标识</returns>
    [DllImport("user32.dll")]
    public static extern int GetWindowThreadProcessId(IntPtr Hwnd, out int PID);

}