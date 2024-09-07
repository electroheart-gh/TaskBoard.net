using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                pbIcon.Image = WinAPI.GetTaskIcon(value).ToBitmap();
                var tn = new StringBuilder(256);
                WinAPI.GetWindowText(value, tn, tn.Capacity);
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
        private int deltaOpacity;

        //
        // Constructors
        //
        public TaskUserControl(IntPtr hwnd)
        {
            InitializeComponent();
            WindowHandle = hwnd;

            MouseWheel += new MouseEventHandler(TaskUserControl_MouseWheel);

        }

        //
        // Constants, declarations and structures
        //
        private const int DRAG_MOVE_ALLOWANCE = 5;

        //
        // Methods for display control
        //

        // Update task name and icon of Task control by setting windowHandle to windowHandle
        public bool Renew()
        {
            if (WinAPI.GetTaskHwndList().Contains(windowHandle)) {
                WindowHandle = windowHandle;
                return true;
            }
            lblTaskName.ForeColor = Color.Red;
            return false;
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
                    WinAPI.SetForegroundTask(WindowHandle);
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
                    WinAPI.CloseTask(taskControl.WindowHandle);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var taskControl in Parent.Controls.OfType<TaskUserControl>()) {
                if (taskControl.IsSelected) {
                    WinAPI.SetForegroundTask(taskControl.WindowHandle);
                }
            }
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var message = TaskName.ToString();
            message += Environment.NewLine + WinAPI.GetExePath(windowHandle);
            MessageBox.Show(message);
        }

        private void TaskUserControl_MouseHover(object sender, EventArgs e)
        {
            Focus();

            if (Program.appSettings.BackgroundThumbnail) {
                // ISSUES: Flicker before capturing window
                DisplayThumbnail(opaque: true);
                //Parent.BackgroundImage = ResizeImage(CaptureWindow(Parent.Handle));
                //Parent.BackgroundImage = ConvertToGrayscale(ResizeImage(CaptureWindow(Parent.Handle)));
                Bitmap screenImage = WinAPI.CaptureWindow(Parent.Handle);
                WinAPI.DwmUnregisterThumbnail(thumbHandle);
                thumbHandle = IntPtr.Zero;
                Parent.BackgroundImage = ConvertToGrayscale(ResizeImage(screenImage));
            }
            else {
                DisplayThumbnail();
            }
        }

        private void DisplayThumbnail(bool opaque = false)
        {
            // For safety, check and unregister thumbHandle before registering
            if (thumbHandle != IntPtr.Zero) {
                WinAPI.DwmUnregisterThumbnail(thumbHandle);
                thumbHandle = IntPtr.Zero;
            }
            int result = WinAPI.DwmRegisterThumbnail(Parent.Handle, WindowHandle, out thumbHandle);
            if (result != 0) {
                Debug.WriteLine("Failed to register thumbnail.");
                return;
            }

            //DwmQueryThumbnailSourceSize(thumbHandle, out PSIZE size);
            WinAPI.RECT destinationRect = new WinAPI.RECT {
                Left = Parent.Left,
                Top = Parent.Top,
                Right = Parent.Right,
                Bottom = Parent.Bottom
            };

            WinAPI.DWM_THUMBNAIL_PROPERTIES props = new WinAPI.DWM_THUMBNAIL_PROPERTIES {
                dwFlags = WinAPI.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_RECTDESTINATION |
                          WinAPI.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_VISIBLE |
                          WinAPI.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_OPACITY,
                rcDestination = destinationRect,
                fVisible = true,
                opacity = opaque
                          ? byte.MaxValue
                          : (byte)(Program.appSettings.ThumbnailOpacity + deltaOpacity)
                //: Math.Max(Math.Min((byte)(Program.appSettings.ThumbnailOpacity + deltaOpacity), byte.MaxValue), byte.MinValue)
            };

            WinAPI.DwmUpdateThumbnailProperties(thumbHandle, ref props);
        }

        private void TaskUserControl_MouseWheel(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("mouse wheel event " + (e.Delta > 0 ? "Up" : "Down"));
            var delta = Program.appSettings.DeltaOpacity;
            deltaOpacity += (e.Delta > 0) ? delta : -delta;
            deltaOpacity = Math.Min(deltaOpacity, byte.MaxValue - Program.appSettings.ThumbnailOpacity);
            deltaOpacity = Math.Max(deltaOpacity, byte.MinValue - Program.appSettings.ThumbnailOpacity);

            DisplayThumbnail();
        }

        private void TaskUserControl_MouseLeave(object sender, EventArgs e)
        {
            if (Program.appSettings.BackgroundThumbnail) {
                Parent.BackgroundImage = null;
            }
            else {
                WinAPI.DwmUnregisterThumbnail(thumbHandle);
                thumbHandle = IntPtr.Zero;
                deltaOpacity = 0;
            }
        }

        private static Bitmap ConvertToGrayscale(Bitmap original)
        {
            Bitmap grayscaleBitmap = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++) {
                for (int x = 0; x < original.Width; x++) {
                    Color originalColor = original.GetPixel(x, y);

                    // グレースケールの計算（標準的な輝度法）
                    int grayScale = (int)(originalColor.R * 0.3 + originalColor.G * 0.59 + originalColor.B * 0.11);

                    // 新しい色を設定
                    Color grayColor = Color.FromArgb(originalColor.A, grayScale, grayScale, grayScale);
                    grayscaleBitmap.SetPixel(x, y, grayColor);
                }
            }
            return grayscaleBitmap;
        }

        private static Bitmap ResizeImage(Bitmap image)
        {
            int newWidth = (int)(image.Width * 0.9);
            //int newWidth = (int)(image.Width * 0.5);
            int newHeight = (int)(image.Height * 0.9);
            //int newHeight = (int)(image.Height * 0.5);

            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedImage)) {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return resizedImage;
        }
        // TODO: Create menu item to save task position to place task with same task name
    }
}
