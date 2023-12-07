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
        // TODO: make icons smaller


        //
        // Parameters and variables
        //
        public IntPtr WindowHandle { get; set; }

        private bool isSelected;
        public bool IsSelected { get => isSelected; set => isSelected = value; }

        private StringBuilder taskName = new StringBuilder(256);
        public StringBuilder TaskName
        {
            get { return taskName; }
            set { taskName = value; lblTaskName.Text = value.ToString(); }
        }

        private int isDragged;
        private Point dragStart;


        //
        // Constructors
        //
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
            TaskName = taskName;

        }

        public TaskUserControl()
        {
            InitializeComponent();
        }

        //
        // Constants, declarations and structures
        //
        private const int DRAG_MOVE_ALLOWANCE = 5;

        private const uint ICON_SMALL = 0;
        private const uint ICON_BIG = 1;
        private const uint WM_GETICON = 0x7F;
        private const int GCL_HICON = -14;
        private const int GCL_HICONSM = -34;
        
        private const int WPF_RESTORETOMAXIMIZED = 0x02;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;


        [DllImport("user32.dll")]

        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClassLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }


        //
        // Methods For Windows Icon
        //
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

        //
        // Methods for display control
        //

        // Update task name of Task control
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

        // Foreground window for the task

        private void SetForegroundTask(IntPtr hWnd)
        {
            var placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hWnd, ref placement);
            if ((placement.showCmd & SW_SHOWMINIMIZED) == SW_SHOWMINIMIZED)
            {
                if((placement.flags & WPF_RESTORETOMAXIMIZED) == WPF_RESTORETOMAXIMIZED)
                {
                    ShowWindow(hWnd, SW_SHOWMAXIMIZED);
                }
                else
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }
            }
            SetForegroundWindow(hWnd);
        }

        //
        // Event handlers
        //

        private void TaskUserControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                BringToFront();
                dragStart = e.Location;
                Console.WriteLine("mousedown ");
                Console.WriteLine("mousedown this.location: {0}", Location.ToString());
                Console.WriteLine("mousedown e.location: {0}", e.Location.ToString());
                Console.WriteLine("mousedown dragStart: {0}", dragStart.ToString());
                Console.WriteLine("mousedown cursor.position: {0}", Cursor.Position.ToString());

                // If clicking unselected Task, select it and unselect others
                if (!isSelected)
                {
                    foreach (var ctrl in Parent.Controls.OfType<TaskUserControl>())
                    {
                        ctrl.isSelected = false;
                    }
                    isSelected = true;
                }
            }
        }

        private void TaskUserControl_MouseMove(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("mousemove ");
            //Console.WriteLine("mousemove this.location: {0}", Location.ToString());
            //Console.WriteLine("mousemove e.location: {0}", e.Location.ToString());
            //Console.WriteLine("mousemove dragStart: {0}", dragStart.ToString());
            //Console.WriteLine("mousemove cursor.position: {0}", Cursor.Position.ToString());
            if (e.Button==MouseButtons.Left)
            {
                isDragged += 1;
                foreach (var ctrl in Parent.Controls.OfType<TaskUserControl>())
                {
                    // Drag selected Tasks
                    if (ctrl.isSelected)
                    {
                        ctrl.Location = new Point(ctrl.Location.X + e.Location.X - dragStart.X, ctrl.Location.Y + e.Location.Y - dragStart.Y);
                        
                    }
                }
            }
        }

        private void TaskUserControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //Console.WriteLine("mouseup ");
                //Console.WriteLine("mouseup e.location: {0}", e.Location.ToString());
                //Console.WriteLine("mouseup dragStart: {0}", dragStart.ToString());
                //Console.WriteLine("mouseup cursor.position: {0}", Cursor.Position.ToString());

                if (isDragged < DRAG_MOVE_ALLOWANCE)
                {
                    SetForegroundTask(WindowHandle);
                }
                isDragged = 0;
            }
        }

        private void TaskUserControl_MouseClick(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("mouseclick ");
            //Console.WriteLine("mouseclick e.location: {0}", e.Location.ToString());
            //Console.WriteLine("mouseclick dragStart: {0}", dragStart.ToString());
            //Console.WriteLine("mouseclick cursor.position: {0}", Cursor.Position.ToString());

        }
    }
}
