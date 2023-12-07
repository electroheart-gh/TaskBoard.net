using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskBoardWf
{
    public partial class TaskBoard : Form
    {

        //
        // Variables for Rubber Band 
        // TODO: write rubber band above the task icon 
        //
        bool isSelecting;

        Point rubberBandStart;
        Point rubberBandEnd;

        Color lineColor = Color.Red;
        int lineBorder = 1;

        Graphics gRubberBand;
        Pen linePen;


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
            // Initialize displaying Task controls on the Board using Renew()
            Renew();

            // TODO: Do not display the task board on the task bar
        }

        private void Board_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Renew();
                rubberBandStart = PointToClient(Cursor.Position);
                isSelecting = true;
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
                // TODO: Change Board control to Rubber band control to minimize its size for memory usage etc.
                Board.Image = new Bitmap(Board.Width, Board.Height);
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


        private static void DebugEnumerateWindows()
        {
            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd) & GetWindow(hWnd, GW_OWNER) == IntPtr.Zero
                & ((GetWindowLong(hWnd, GWL_EXSTYLE) & (WS_EX_NOREDIRECTIONBITMAP | WS_EX_TOOLWINDOW)) == 0))
                {
                    StringBuilder windowTitle = new StringBuilder(256);
                    GetWindowText(hWnd, windowTitle, windowTitle.Capacity);
                    Console.WriteLine($"Window Title: {windowTitle}");
                }
                return true;
            }, IntPtr.Zero);
        }

        // Get window handles of the windows on the taskbar
        private static List<IntPtr> GetTaskHwndList()
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
            // TODO: Disallow overlapping controls

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
        private void Renew()
        {
            var runningTasks = GetTaskHwndList();

            foreach (var taskControl in Controls.OfType<TaskUserControl>())
            {
                if (taskControl.Renew())
                {
                    // Remove updated tasks from the variable to extract new tasks
                    runningTasks.Remove(taskControl.WindowHandle);
                }
                else
                {
                    // Delete obsolete tasks
                    taskControl.Dispose();
                }
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
        private void DrawQuadLine(Point p0, Point p1, Point p2, Point p3)
        {

            // Specifying nothing but the size creates noncolor canvas 
            var rubberBandBitmap = new Bitmap(Board.Width, Board.Height);

            // Create Graphics object for the rubber band
            gRubberBand = Graphics.FromImage(rubberBandBitmap);

            linePen = new Pen(lineColor, lineBorder);
            linePen.DashStyle = DashStyle.Dot;

            gRubberBand.DrawLine(linePen, p0, p1); // top
            gRubberBand.DrawLine(linePen, p2, p3); // bottom
            gRubberBand.DrawLine(linePen, p0, p2); // left
            gRubberBand.DrawLine(linePen, p1, p3); // right

            Board.Image = rubberBandBitmap;

            // Release resources
            linePen.Dispose();
            gRubberBand.Dispose();
        }
    }
}
