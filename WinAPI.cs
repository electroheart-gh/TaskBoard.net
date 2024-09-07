using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TaskBoardWf
{
    internal static class WinAPI
    {
        //
        // Constants, declaration, and methods to get Windows' Task information
        //
        internal const uint GW_OWNER = 4;
        internal const int GWL_EXSTYLE = -20;
        internal const long WS_EX_NOREDIRECTIONBITMAP = 0x00200000L;
        internal const long WS_EX_TOOLWINDOW = 0x00000080L;
        internal const long WS_EX_APPWINDOW = 0x00040000L;

        internal const uint ICON_SMALL = 0;
        internal const uint ICON_BIG = 1;
        internal const uint WM_GETICON = 0x7F;
        internal const int GCL_HICON = -14;
        internal const int GCL_HICONSM = -34;

        internal const int WPF_RESTORETOMAXIMIZED = 0x02;
        internal const int SW_SHOWMINIMIZED = 2;
        internal const int SW_SHOWMAXIMIZED = 3;
        internal const int SW_RESTORE = 9;
        internal const int WM_CLOSE = 0x0010;

        internal const int PW_RENDERFULLCONTENT = 2;

        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        [DllImport("user32.dll")]
        internal static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetClassLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        internal static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("dwmapi.dll", SetLastError = true)]
        internal static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll", SetLastError = true)]
        internal static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll", SetLastError = true)]
        internal static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport("dwmapi.dll", SetLastError = true)]
        internal static extern int DwmUpdateThumbnailProperties(IntPtr hThumbnail, ref DWM_THUMBNAIL_PROPERTIES props);


        [StructLayout(LayoutKind.Sequential)]
        internal struct PSIZE
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public RECT rcDestination;
            public RECT rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;

            public const int DWM_TNP_RECTDESTINATION = 0x00000001;
            public const int DWM_TNP_RECTSOURCE = 0x00000002;
            public const int DWM_TNP_OPACITY = 0x00000004;
            public const int DWM_TNP_VISIBLE = 0x00000008;
            public const int DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        internal struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }

        //
        // Facade
        //
        public static List<IntPtr> GetTaskHwndList()
        {
            var taskListAsHwnd = new List<IntPtr>();

            WinAPI.EnumWindows(
                (hWnd, lParam) =>
                {
                    if (Program.appSettings.ExperimentalTaskList) {
                        var windowText = new StringBuilder(256);
                        WinAPI.GetWindowText(hWnd, windowText, windowText.Capacity);
                        Logger.LogInfo($"GetWindowText:  {windowText}");

                        Logger.LogInfo($"Visible: {WinAPI.IsWindowVisible(hWnd)}");
                        Logger.LogInfo($"Owner: {WinAPI.GetWindow(hWnd, WinAPI.GW_OWNER)}");
                        Logger.LogInfo($"REDIRECT: {WinAPI.GetWindowLong(hWnd, WinAPI.GWL_EXSTYLE) & (WinAPI.WS_EX_NOREDIRECTIONBITMAP)}");
                        Logger.LogInfo($"TOOL: {WinAPI.GetWindowLong(hWnd, WinAPI.GWL_EXSTYLE) & (WinAPI.WS_EX_TOOLWINDOW)}");
                        Logger.LogInfo($"APPWINDOW: {WinAPI.GetWindowLong(hWnd, WinAPI.GWL_EXSTYLE) & (WinAPI.WS_EX_APPWINDOW)}");
                        Logger.LogInfo($"Owner Min: {WinAPI.IsIconic(WinAPI.GetWindow(hWnd, WinAPI.GW_OWNER))}");

                        if (!WinAPI.IsWindowVisible(hWnd)) return true;
                        var exStyle = WinAPI.GetWindowLong(hWnd, WinAPI.GWL_EXSTYLE);
                        if ((exStyle & WinAPI.WS_EX_NOREDIRECTIONBITMAP) != 0) return true;
                        if ((exStyle & WinAPI.WS_EX_TOOLWINDOW) != 0) return true;
                        var ownerHWnd = WinAPI.GetWindow(hWnd, WinAPI.GW_OWNER);
                        if ((ownerHWnd != IntPtr.Zero) && ((exStyle & WinAPI.WS_EX_APPWINDOW) == 0 || WinAPI.IsIconic(ownerHWnd))) return true;

                        // Join the club!
                        taskListAsHwnd.Add(hWnd);
                    }
                    // Magic spells to select windows on the taskbar 
                    else if (WinAPI.IsWindowVisible(hWnd) &&
                        WinAPI.GetWindow(hWnd, WinAPI.GW_OWNER) == IntPtr.Zero &&
                        (WinAPI.GetWindowLong(hWnd, WinAPI.GWL_EXSTYLE) & (WinAPI.WS_EX_NOREDIRECTIONBITMAP | WinAPI.WS_EX_TOOLWINDOW)) == 0) {
                        taskListAsHwnd.Add(hWnd);
                    }
                    return true;
                },
                IntPtr.Zero);

            return taskListAsHwnd;
        }

        internal static Icon GetTaskIcon(IntPtr hWnd)
        {
            IntPtr hIcon = WinAPI.SendMessage(hWnd, WinAPI.WM_GETICON, (IntPtr)WinAPI.ICON_BIG, IntPtr.Zero);
            if (hIcon == IntPtr.Zero) {
                hIcon = WinAPI.SendMessage(hWnd, WinAPI.WM_GETICON, (IntPtr)WinAPI.ICON_SMALL, IntPtr.Zero);
            }
            if (hIcon == IntPtr.Zero) {
                hIcon = WinAPI.SendMessage(hWnd, WinAPI.WM_GETICON, (IntPtr)WinAPI.ICON_SMALL, IntPtr.Zero);
            }
            if (hIcon == IntPtr.Zero) {
                hIcon = WinAPI.GetClassLong(hWnd, WinAPI.GCL_HICON);
            }
            if (hIcon == IntPtr.Zero) {
                hIcon = WinAPI.GetClassLong(hWnd, WinAPI.GCL_HICONSM);
            }
            if (hIcon != IntPtr.Zero) {
                return Icon.FromHandle(hIcon);
            }

            try {
                return Icon.ExtractAssociatedIcon(GetExePath(hWnd));
            }
            catch (ArgumentException e) {
                Logger.LogWarning(e.Message);
            }

            return null;
        }

        internal static string GetExePath(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) { return null; }

            try {
                WinAPI.GetWindowThreadProcessId(hWnd, out uint processId);
                Process process = Process.GetProcessById((int)processId);
                return process?.MainModule?.FileName;
            }
            catch (ArgumentException) {
                return string.Empty;
            }
            catch (InvalidOperationException) {
                return string.Empty;
            }
            // To handle access privilege error from Chrome etc, catch Win32Exception
            catch (System.ComponentModel.Win32Exception) {
                return string.Empty;
            }
        }

        internal static void SetForegroundTask(IntPtr hWnd)
        {
            var placement = new WinAPI.WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            WinAPI.GetWindowPlacement(hWnd, ref placement);
            if ((placement.showCmd & WinAPI.SW_SHOWMINIMIZED) == WinAPI.SW_SHOWMINIMIZED) {
                if ((placement.flags & WinAPI.WPF_RESTORETOMAXIMIZED) == WinAPI.WPF_RESTORETOMAXIMIZED) {
                    WinAPI.ShowWindow(hWnd, WinAPI.SW_SHOWMAXIMIZED);
                }
                else {
                    WinAPI.ShowWindow(hWnd, WinAPI.SW_RESTORE);
                }
            }
            WinAPI.SetForegroundWindow(hWnd);
        }

        internal static void CloseTask(IntPtr hWnd)
        {
            SetForegroundTask(hWnd);

            // Special operation for Excel window
            var exeName = GetExePath(hWnd);
            if (exeName.EndsWith("excel.exe", StringComparison.OrdinalIgnoreCase)) {
                SendKeys.Send("^{F4}");
            }
            // Normal operation to close Task window
            else {
                WinAPI.SendMessage(hWnd, WinAPI.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        internal static Bitmap CaptureWindow(IntPtr hWnd)
        {
            WinAPI.GetWindowRect(hWnd, out WinAPI.RECT rect);
            Bitmap bitmap = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top);
            using (Graphics g = Graphics.FromImage(bitmap)) {
                IntPtr hdc = g.GetHdc();
                WinAPI.PrintWindow(hWnd, hdc, WinAPI.PW_RENDERFULLCONTENT);
                g.ReleaseHdc(hdc);
            }
            return bitmap;
        }
    }
}
