using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskBoardWf
{
    public partial class TaskUserControl : UserControl
    {

        //
        // Parameters and variables
        //
        private IntPtr windowHandle;
        public IntPtr WindowHandle
        {
            get { return windowHandle; }
            set {
                windowHandle = value;
                pbIcon.Image = GetTaskIcon(value).ToBitmap();
                var tn = new StringBuilder(256);
                GetWindowText(value, tn, tn.Capacity);
                TaskName = tn.ToString();
            }
        }

        private String taskName;
        public String TaskName
        {
            get { return taskName; }
            set {
                taskName = value;
                lblTaskName.Text = ModifyName(value);
                lblTaskName.ForeColor = ModifyNameColor(value);

                // Due to performance issue, gave up to add exe name to the tooltips
                //toolTipTaskName.SetToolTip(this, taskName);
                toolTipTaskName.SetToolTip(lblTaskName, taskName);
                //toolTipTaskName.SetToolTip(pbIcon, taskName);
            }
        }

        private Color ModifyNameColor(string name)
        {
            var color = new Color();
            foreach (var nm in Program.appSettings.NameModifiers) {
                if (Regex.IsMatch(name, nm.Pattern)) {
                    color = Color.FromName(nm.ForeColor);
                }
            }
            return color;
        }

        private string ModifyName(string name)
        {
            foreach (var nm in Program.appSettings.NameModifiers) {
                name = Regex.Replace(name, nm.Pattern, nm.Substitution);
            }
            return name;
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set {
                isSelected = value;
                if (value) { BackColor = Color.Lavender; }  //lavender,  Gainsboro
                else { BackColor = SystemColors.Control; }
            }
        }

        private int drags;
        private Point dragStart;
        private IntPtr thumbHandle;

        //
        // Constructors
        //
        public TaskUserControl(IntPtr hwnd)
        {
            InitializeComponent();
            WindowHandle = hwnd;
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
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_RESTORE = 9;
        private const int WM_CLOSE = 0x0010;

        private const int PW_RENDERFULLCONTENT = 2;


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

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);


        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern int DwmUpdateThumbnailProperties(IntPtr hThumbnail, ref DWM_THUMBNAIL_PROPERTIES props);


        [StructLayout(LayoutKind.Sequential)]
        internal  struct PSIZE
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
        // Methods For Windows Icon
        //
        private static Icon GetTaskIcon(IntPtr hWnd)
        {
            IntPtr hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_BIG, IntPtr.Zero);
            if (hIcon == IntPtr.Zero) {
                hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_SMALL, IntPtr.Zero);
            }
            if (hIcon == IntPtr.Zero) {
                hIcon = SendMessage(hWnd, WM_GETICON, (IntPtr)ICON_SMALL, IntPtr.Zero);
            }
            if (hIcon == IntPtr.Zero) {
                hIcon = GetClassLong(hWnd, GCL_HICON);
            }
            if (hIcon == IntPtr.Zero) {
                hIcon = GetClassLong(hWnd, GCL_HICONSM);
            }
            if (hIcon != IntPtr.Zero) {
                return Icon.FromHandle(hIcon);
            }

            try {
                return Icon.ExtractAssociatedIcon(GetExePath(hWnd));
            }
            catch (ArgumentException) { }

            return null;
        }

        private static string GetExePath(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) { return null; }

            try {
                GetWindowThreadProcessId(hWnd, out uint processId);
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


        //
        // Methods for display control
        //

        // Update task name and icon of Task control by setting windowHandle to windowHandle
        public bool Renew()
        {
            if (TaskBoard.GetTaskHwndList().Contains(windowHandle)) {
                WindowHandle = windowHandle;
                return true;
            }
            else {
                lblTaskName.ForeColor = Color.Red;
                return false;
            }
        }

        // Foreground window for the task
        private void SetForegroundTask(IntPtr hWnd)
        {
            var placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hWnd, ref placement);
            if ((placement.showCmd & SW_SHOWMINIMIZED) == SW_SHOWMINIMIZED) {
                if ((placement.flags & WPF_RESTORETOMAXIMIZED) == WPF_RESTORETOMAXIMIZED) {
                    ShowWindow(hWnd, SW_SHOWMAXIMIZED);
                }
                else {
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
            BringToFront();
            Parent.BackgroundImage = null;

            // If clicking unselected Task, select it and unselect others
            if (!IsSelected) {
                foreach (var ctrl in Parent.Controls.OfType<TaskUserControl>()) {
                    ctrl.IsSelected = false;
                }
                IsSelected = true;
            }
            if (e.Button == MouseButtons.Left) {
                dragStart = e.Location;
            }
        }

        private void TaskUserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                // Count how many times it is dragged between MouseDown and MouseUp
                drags += 1;
                foreach (var ctrl in Parent.Controls.OfType<TaskUserControl>()) {
                    // Drag selected Tasks
                    if (ctrl.IsSelected) {
                        ctrl.Location = new Point(ctrl.Location.X + e.Location.X - dragStart.X, ctrl.Location.Y + e.Location.Y - dragStart.Y);
                    }
                }
            }
        }
        private void TaskUserControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                if (drags < DRAG_MOVE_ALLOWANCE) {
                    Renew();
                    SetForegroundTask(WindowHandle);
                }
                drags = 0;
            }
        }

        private void TaskUserControl_MouseClick(object sender, MouseEventArgs e)
        {
            // Gave up to use click event which cant handle dragging properly
            // Guessed that dragging is recognized by distance between original position and mouse e.location 
            // but the control moves with mouse when dragged and this makes the distance to 0
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var taskControl in Parent.Controls.OfType<TaskUserControl>()) {
                if (taskControl.IsSelected) {
                    CloseTask(taskControl.WindowHandle);
                }
            }
        }

        private void CloseTask(IntPtr hWnd)
        {
            SetForegroundTask(hWnd);

            // Special operation for Excel window
            var exeName = GetExePath(hWnd);
            if (exeName.EndsWith("excel.exe", StringComparison.OrdinalIgnoreCase)) {
                SendKeys.Send("^{F4}");
            }
            // Normal operation to close Task window
            else {
                SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var taskControl in Parent.Controls.OfType<TaskUserControl>()) {
                if (taskControl.IsSelected) {
                    SetForegroundTask(taskControl.WindowHandle);
                }
            }
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var message = TaskName.ToString();
            message += Environment.NewLine + GetExePath(windowHandle);
            MessageBox.Show(message);
        }

        private void TaskUserControl_MouseHover(object sender, EventArgs e)
        {
            IntPtr destinationHWnd = Parent.Handle;

            int result = DwmRegisterThumbnail(destinationHWnd, WindowHandle, out thumbHandle);
            if (result != 0) {
                Debug.WriteLine("Failed to register thumbnail.");
                return;
            }

            //DwmQueryThumbnailSourceSize(thumbHandle, out PSIZE size);
            RECT destinationRect = new RECT {
                Left = Parent.Left,
                Top = Parent.Top,
                Right = Parent.Right,
                Bottom = Parent.Bottom
            };
            DWM_THUMBNAIL_PROPERTIES props = new DWM_THUMBNAIL_PROPERTIES {
                dwFlags = DWM_THUMBNAIL_PROPERTIES.DWM_TNP_RECTDESTINATION |
                          DWM_THUMBNAIL_PROPERTIES.DWM_TNP_VISIBLE |
                          DWM_THUMBNAIL_PROPERTIES.DWM_TNP_OPACITY,
                rcDestination = destinationRect,
                fVisible = true,
                opacity = Program.appSettings.ThumbOpacity
            };

            DwmUpdateThumbnailProperties(thumbHandle, ref props);
        }

        private Bitmap CaptureWindow(IntPtr hWnd)
        {
            GetWindowRect(hWnd, out RECT rect);
            Bitmap bitmap = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top);
            using (Graphics g = Graphics.FromImage(bitmap)) {
                IntPtr hdc = g.GetHdc();
                PrintWindow(hWnd, hdc, PW_RENDERFULLCONTENT);
                g.ReleaseHdc(hdc);
            }
            return bitmap;
        }

        private Bitmap ResizeImage(Bitmap image)
        {
            int newWidth = (int)(image.Width * 0.9);
            int newHeight = (int)(image.Height * 0.9);

            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedImage)) {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return resizedImage;
        }

        private void TaskUserControl_MouseLeave(object sender, EventArgs e)
        {
            DwmUnregisterThumbnail(thumbHandle);
        }

        private void toolTipTaskName_Popup(object sender, PopupEventArgs e)
        {
        }

        // TODO: Create menu item to save task position to place task with same task name
    }
}
