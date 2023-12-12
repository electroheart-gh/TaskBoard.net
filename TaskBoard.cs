using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TaskBoardWf
{
    public partial class TaskBoard : Form
    {
        // TODO: write rubber band above the task icon 

        //
        // Variables for Rubber Band 
        //
        bool isSelecting;

        Point rubberBandStart;
        Point rubberBandEnd;

        Color lineColor = Color.Purple;   // Gray
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

        private void TaskBoard_FormClosing(object sender, FormClosingEventArgs e)
        {
            hotKey.Dispose();
        }
    }



    /// <summary>
    /// グローバルホットキーを登録するクラス。
    /// 使用後は必ずDisposeすること。
    /// </summary>
    public class HotKey : IDisposable
    {
        HotKeyForm form;
        /// <summary>
        /// ホットキーが押されると発生する。
        /// </summary>
        public event EventHandler HotKeyPush;

        /// <summary>
        /// ホットキーを指定して初期化する。
        /// 使用後は必ずDisposeすること。
        /// </summary>
        /// <param name="modKey">修飾キー</param>
        /// <param name="key">キー</param>
        public HotKey(MOD_KEY modKey, Keys key)
        {
            form = new HotKeyForm(modKey, key, raiseHotKeyPush);
        }

        private void raiseHotKeyPush()
        {
            if (HotKeyPush != null)
            {
                HotKeyPush(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            form.Dispose();
        }

        private class HotKeyForm : Form
        {
            [DllImport("user32.dll")]
            extern static int RegisterHotKey(IntPtr HWnd, int ID, MOD_KEY MOD_KEY, Keys KEY);

            [DllImport("user32.dll")]
            extern static int UnregisterHotKey(IntPtr HWnd, int ID);

            const int WM_HOTKEY = 0x0312;
            int id;
            ThreadStart proc;

            public HotKeyForm(MOD_KEY modKey, Keys key, ThreadStart proc)
            {
                this.proc = proc;
                for (int i = 0x0000; i <= 0xbfff; i++)
                {
                    if (RegisterHotKey(this.Handle, i, modKey, key) != 0)
                    {
                        id = i;
                        break;
                    }
                }
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_HOTKEY)
                {
                    if ((int)m.WParam == id)
                    {
                        proc();
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                UnregisterHotKey(this.Handle, id);
                base.Dispose(disposing);
            }
        }
    }

    /// <summary>
    /// HotKeyクラスの初期化時に指定する修飾キー
    /// </summary>
    public enum MOD_KEY : int
    {
        ALT = 0x0001,
        CONTROL = 0x0002,
        SHIFT = 0x0004,
    }
}
