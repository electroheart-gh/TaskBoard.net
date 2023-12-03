using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskBoardWf
{
    public partial class TaskUserControl : UserControl
    {

        public IntPtr WindowHandle { get; set; }

        private bool isSelected;
        public bool IsSelected { get => isSelected; set => isSelected = value; }
        //Icon winIcon;

        //public StringBuilder taskName { get; set; } = new StringBuilder(256);
        private StringBuilder taskName = new StringBuilder(256);

        public StringBuilder TaskName
        {
            get { return taskName; }
            set { taskName = value; lblTaskName.Text = taskName.ToString(); }
        }


        public TaskUserControl(IntPtr hwnd)
        {
            InitializeComponent();

            WindowHandle = hwnd;
            //pbIcon = new PictureBox();
            pbIcon.Image = GetTaskIcon(hwnd).ToBitmap();

            //StringBuilder tn = new StringBuilder(256);
            //GetWindowText(hwnd, tn, tn.Capacity);
            //taskName = tn;

            GetWindowText(hwnd, taskName, taskName.Capacity);

        }

        public TaskUserControl()
        {
            InitializeComponent();
        }


        private const uint ICON_SMALL = 0;
        private const uint ICON_BIG = 1;
        private const uint WM_GETICON = 0x7F;
        private const int GCL_HICON = -14;
        private const int GCL_HICONSM = -34;

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClassLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        private static Icon GetTaskIcon(IntPtr hWnd)
        {
            IntPtr hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_BIG, IntPtr.Zero);
            if (hIcon == IntPtr.Zero)
            {
                hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_SMALL, IntPtr.Zero);
            }
            if (hIcon == IntPtr.Zero)
            {
                hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_SMALL, IntPtr.Zero);
            }
            if (hIcon == IntPtr.Zero)
            {
                hIcon = GetClassLong(hWnd, GCL_HICON);
            }
            if (hIcon == IntPtr.Zero)
            {
                hIcon = GetClassLong(hWnd, GCL_HICONSM);
            }
            if (hIcon != IntPtr.Zero)
            {
                return Icon.FromHandle(hIcon);
            }

            string filePath = GetExePath(hWnd);
            try
            {
                return Icon.ExtractAssociatedIcon(filePath);
            }
            catch (ArgumentException) { }

            return null;
        }

        private static string GetExePath(IntPtr hWnd)
        {
            try
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                Process process = Process.GetProcessById((int)processId);
                return process?.MainModule?.FileName;
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }


        private static Icon GetIconFromExe(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            Process process = Process.GetProcessById((int)processId);
            string filePath = process?.MainModule?.FileName;

            if (File.Exists(filePath))
            {
                return Icon.ExtractAssociatedIcon(filePath);
            }
            return null;
        }

        internal bool Renew()
        {
            StringBuilder tn = new StringBuilder(256);
            if (GetWindowText(WindowHandle, tn, tn.Capacity) == 0)
            {
                return false;
            }

            TaskName = tn;
            return true;
        }
    }
}
