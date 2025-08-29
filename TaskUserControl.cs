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
                // abend at the next line
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

        //
        // Constructor
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
        // Methods
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
            ((TaskBoard)this.FindForm()).ClearWindowImage();

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
            ((TaskBoard)this.FindForm()).DisplayWindowImage(windowHandle);
        }

        private void TaskUserControl_MouseWheel(object sender, MouseEventArgs e)
        {
            ((TaskBoard)this.FindForm()).ChangeThumbnailOpacity(windowHandle, e.Delta > 0);
        }

        private void TaskUserControl_MouseLeave(object sender, EventArgs e)
        {
            ((TaskBoard)this.FindForm()).ClearWindowImage();
        }

        private void TaskUserControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) {
                e.IsInputKey = true;
            }
        }

        // TODO: Create menu item to save task position to place task with same task name
    }
}
