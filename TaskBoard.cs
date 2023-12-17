﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace TaskBoardWf
{
    public partial class TaskBoard : Form
    {
        // TODO: write rubber band above the task icon 
        // TODO: Make it scalable and scrollable
        // TODO: Implement keyboard interface

        //
        // Variables for Rubber Band 
        //
        bool isSelecting;

        Point rubberBandStart;
        Point rubberBandEnd;

        Color lineColor = Color.Purple;   // or Gray
        int lineBorder = 1;

        Graphics gRubberBand;
        Pen linePen;

        HotKey hotKey;


        //
        // Constructor
        //
        public TaskBoard()
        {
            InitializeComponent();
        }


        //
        // Event Handlers
        //
        private void TaskBoard_Load(object sender, EventArgs e)
        {

            WindowState = FormWindowState.Maximized;

            // Initialize displaying Task controls on the Board using Renew()
            Renew();

            // Global Hot Key
            hotKey = new HotKey(MOD_KEY.ALT, Keys.Q);  // Keys.MButton not work
            hotKey.HotKeyPush += new EventHandler(hotKey_HotKeyPush);

            // TODO: Consider not to display the task board on the task bar
        }

        void hotKey_HotKeyPush(object sender, EventArgs e)
        {
            Activate();
            BringToFront();
            WindowState = FormWindowState.Maximized;
            //MessageBox.Show("ホットキーが押されました。");
        }

        private void Board_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Renew();
                rubberBandStart = PointToClient(Cursor.Position);
                isSelecting = true;
                foreach (var taskControl in Controls.OfType<TaskUserControl>())
                {
                    taskControl.IsSelected = false;
                }
            }
        }

        private void Board_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                rubberBandEnd = PointToClient(Cursor.Position);

                // Draw rubber band
                Point p0 = new Point(rubberBandStart.X, rubberBandStart.Y);
                Point p1 = new Point(rubberBandEnd.X, rubberBandStart.Y);
                Point p2 = new Point(rubberBandStart.X, rubberBandEnd.Y);
                Point p3 = new Point(rubberBandEnd.X, rubberBandEnd.Y);
                DrawQuadLine(p0, p1, p2, p3);

                // Check overlapped Task controls with rubber band
                var rectRB = RectangleExt.Create(rubberBandStart, rubberBandEnd);

                foreach (var taskControl in Controls.OfType<TaskUserControl>())
                {
                    if (new Rectangle(taskControl.Location, taskControl.Size).IntersectsWith(rectRB))
                    {
                        taskControl.IsSelected = true;
                    }
                    else
                    {
                        taskControl.IsSelected = false;
                    }
                }
            }
        }

        private void Board_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Erase rubber band
                // TODO: !!!! Change Board control to Rubber band control to minimize its size for memory usage etc.
                RubberBandBox.Image = new Bitmap(RubberBandBox.Width, RubberBandBox.Height);
                isSelecting = false;
            }
        }


        //
        // Constants, declaration, and methods to get Windows' Task information
        //
        private const uint GW_OWNER = 4;
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_NOREDIRECTIONBITMAP = 0x00200000L;
        private const long WS_EX_TOOLWINDOW = 0x00000080L;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);


        // Get window handles of the windows on the taskbar
        public static List<IntPtr> GetTaskHwndList()
        {
            var taskListAsHwnd = new List<IntPtr>();

            EnumWindows(
                (hWnd, lParam) =>
                {
                    // Magic spells to select windows on the taskbar 
                    if (IsWindowVisible(hWnd) & GetWindow(hWnd, GW_OWNER) == IntPtr.Zero
                    & ((GetWindowLong(hWnd, GWL_EXSTYLE) & (WS_EX_NOREDIRECTIONBITMAP | WS_EX_TOOLWINDOW)) == 0))
                    {
                        taskListAsHwnd.Add(hWnd);
                    }
                    return true;
                },
                IntPtr.Zero);

            return taskListAsHwnd;
        }


        //
        // Methods for display control
        //

        // Propose where to place new Task control
        private Point ProposePosition()
        {
            Control baseCtrl = null;

            // TODO: Consider where to place the new Task
            // TODO: Consider to disallow overlapping controls

            // Next to the most bottom and most right Task control
            foreach (Control ctrl in Controls.OfType<TaskUserControl>())
            {
                if (baseCtrl == null || baseCtrl.Bottom < ctrl.Bottom || (baseCtrl.Bottom == ctrl.Bottom && baseCtrl.Right < ctrl.Right))
                {
                    baseCtrl = ctrl;
                }
            }

            if (baseCtrl == null)
            {
                // The first one should be place at (0, 0)
                return Point.Empty;
            }
            else if (baseCtrl.Right + baseCtrl.Width > Width)
            {
                // If excessing Board width, place lower
                return new Point(0, baseCtrl.Top + baseCtrl.Height);
            }
            else
            {
                // Otherwise, place next to the one
                return new Point(baseCtrl.Right, baseCtrl.Top);
            }
        }

        // Update Task controls on the Board, delete obsolete Task controls and add new Task controls
        public void Renew()
        {
            var runningTasks = GetTaskHwndList();
            var taskToRemove = new List<TaskUserControl>();

            foreach (var taskControl in Controls.OfType<TaskUserControl>())
            {
                if (runningTasks.Contains(taskControl.WindowHandle))
                {
                    // Remove existing Task controls from the variable to extract new tasks
                    runningTasks.Remove(taskControl.WindowHandle);
                    taskControl.Renew();
                }
                else
                {
                    // Add obsolete tasks to the list
                    // Disposing control here makes "foreach" not work properly
                    taskToRemove.Add(taskControl);
                }
            }
            // Dispose obsolete Task controls 
            foreach (var task in taskToRemove)
            {
                task.Dispose();
            }
            // Add new tasks
            foreach (var newTask in runningTasks)
            {
                var newTaskControl = new TaskUserControl(newTask);
                newTaskControl.Location = ProposePosition();
                Controls.Add(newTaskControl);
                newTaskControl.BringToFront();
            }
            // TODO: Save tasks positions to recover the layout when restarting after crashes
        }


        //
        // Methods for rubber band
        //

        // Draw quad line
        // TODO: !!!! Change size and place of picture box for RubberBand
        private void DrawQuadLine(Point p0, Point p1, Point p2, Point p3)
        {
            // Specifying nothing but the size creates noncolor canvas 
            var rubberBandBitmap = new Bitmap(RubberBandBox.Width, RubberBandBox.Height);

            // Create Graphics object for the rubber band
            gRubberBand = Graphics.FromImage(rubberBandBitmap);

            linePen = new Pen(lineColor, lineBorder);
            linePen.DashStyle = DashStyle.Dot;

            gRubberBand.DrawLine(linePen, p0, p1); // top
            gRubberBand.DrawLine(linePen, p2, p3); // bottom
            gRubberBand.DrawLine(linePen, p0, p2); // left
            gRubberBand.DrawLine(linePen, p1, p3); // right

            RubberBandBox.Image = rubberBandBitmap;

            // Release resources
            linePen.Dispose();
            gRubberBand.Dispose();
        }

        private void TaskBoard_FormClosing(object sender, FormClosingEventArgs e)
        {
            hotKey.Dispose();
        }

        private void TaskBoard_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Renew();
                rubberBandStart = PointToClient(Cursor.Position);
                isSelecting = true;
                foreach (var taskControl in Controls.OfType<TaskUserControl>())
                {
                    taskControl.IsSelected = false;
                }
            }
        }

        private void TaskBoard_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                rubberBandEnd = PointToClient(Cursor.Position);
                RubberBandBox.Bounds = RectangleExt.Create(rubberBandStart, rubberBandEnd);

                // Draw rubber band
                // Specifying nothing but the size creates noncolor canvas 
                //var rubberBandBitmap = new Bitmap(Math.Max(RubberBandBox.Width, 1), Math.Max(RubberBandBox.Height, 1));
                // To avoid 0 width/height, which makes an error, add +1 to Width and Height
                var rubberBandBitmap = new Bitmap(RubberBandBox.Width + 1, RubberBandBox.Height + 1);

                // Create Graphics object for the rubber band
                gRubberBand = Graphics.FromImage(rubberBandBitmap);

                linePen = new Pen(lineColor, lineBorder);
                linePen.DashStyle = DashStyle.Dot;
                // To show the right and bottom lines, DrawRectangle should be -1, which is not related to the size of Bitmap mentioned above
                gRubberBand.DrawRectangle(linePen, 0, 0, RubberBandBox.Width - 1, RubberBandBox.Height - 1);

                RubberBandBox.Image = rubberBandBitmap;
                RubberBandBox.Enabled = true;

                // Release resources
                linePen.Dispose();
                gRubberBand.Dispose();

                // Draw rubber band
                //Point p0 = new Point(rubberBandStart.X, rubberBandStart.Y);
                //Point p1 = new Point(rubberBandEnd.X, rubberBandStart.Y);
                //Point p2 = new Point(rubberBandStart.X, rubberBandEnd.Y);
                //Point p3 = new Point(rubberBandEnd.X, rubberBandEnd.Y);

                // TODO: !!!! Change size and place of picture box for RubberBand
                // DrawQuadLine(p0, p1, p2, p3);

                // Check overlapped Task controls with rubber band
                //var rectRB = RectangleExt.Create(rubberBandStart, rubberBandEnd);

                foreach (var taskControl in Controls.OfType<TaskUserControl>())
                {
                    if (new Rectangle(taskControl.Location, taskControl.Size).IntersectsWith(RubberBandBox.Bounds))
                    {
                        taskControl.IsSelected = true;
                    }
                    else
                    {
                        taskControl.IsSelected = false;
                    }
                }
            }
        }

        private void TaskBoard_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Erase rubber band
                // TODO: Change Board control to Rubber band control to minimize its size for memory usage etc.
                RubberBandBox.Image = new Bitmap(RubberBandBox.Width + 1, RubberBandBox.Height + 1);
                RubberBandBox.Enabled = false;

                isSelecting = false;
            }
        }
    }
}
