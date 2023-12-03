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

namespace TaskBoard.net
{
    public partial class TaskBoard : Form
    {
        //
        // 基本設定
        //
        Dictionary<IntPtr, TaskUserControl> tasks;

        //
        // Rubber Band 関連
        //
        bool isSelecting;

        Bitmap rubberBandBitmap;
        Point rubberBandStart;
        Point rubberBandEnd;

        Color lineColor = Color.Red;
        int lineBorder = 1;

        Graphics gRubberBand;
        Pen linePen;


        //
        // コンストラクタ
        //
        public TaskBoard()
        {
            InitializeComponent();
        }


        //
        // イベント ハンドラー
        //
        private void TaskBoard_Load(object sender, EventArgs e)
        {
            //描画先とするImageオブジェクトを作成する
            //Bitmap canvas = new Bitmap(Board.Width, Board.Height);

            //ImageオブジェクトのGraphicsオブジェクトを作成する
            //黒で塗りつぶされた長方形を描画する
            // using (Graphics g = Graphics.FromImage(canvas))
            // {
            // g.FillRectangle(Brushes.Black, Board.ClientRectangle);
            // }

            Renew();

        }

        private void Board_MouseDown(object sender, MouseEventArgs e)
        {
            // 座標を保存
            rubberBandStart = cursorClientPos();

            //rubberBandStart.X = cursorPos().X;
            //rubberBandStart.Y = cursorPos().Y;

            isSelecting = true;
        }

        private void Board_MouseMove(object sender, MouseEventArgs e)
        {
            // マウスの左ボタンが押されている場合のみ処理
            if (isSelecting)
            {
                // 座標を取得
                rubberBandEnd = cursorClientPos();

                // 描画
                Point p0 = new Point(rubberBandStart.X, rubberBandStart.Y);
                Point p1 = new Point(rubberBandEnd.X, rubberBandStart.Y);
                Point p2 = new Point(rubberBandStart.X, rubberBandEnd.Y);
                Point p3 = new Point(rubberBandEnd.X, rubberBandEnd.Y);
                DrawQuadLine(p0, p1, p2, p3);

                var rectRB = new Rectangle(rubberBandStart.X, rubberBandStart.Y, rubberBandEnd.X, rubberBandEnd.Y);
                foreach (var taskControl in Controls.OfType<TaskUserControl>())
                {
                    // if rubber band overlaps tasks, set it to selected
                    if (taskControl.ClientRectangle.IntersectsWith(rectRB))
                    {
                        taskControl.IsSelected = true;
                    }
                }

            }


        }

        private void Board_MouseUp(object sender, MouseEventArgs e)
        {
            rubberBandBitmap = new Bitmap(Board.Width, Board.Height);
            Board.Image = rubberBandBitmap;

            // リソースを解放
            linePen.Dispose();
            gRubberBand.Dispose();
            isSelecting = false;

        }


        //
        // タスク情報の取得関連
        //
        private const uint GW_OWNER = 4;
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_NOREDIRECTIONBITMAP = 0x00200000L;
        private const long WS_EX_TOOLWINDOW = 0x00000080L;


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


        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        private static void TestEnumerateWindows()
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

        private static List<IntPtr> GetTaskHwndList()
        {
            var taskListAsHwnd = new List<IntPtr>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd) & GetWindow(hWnd, GW_OWNER) == IntPtr.Zero
                & ((GetWindowLong(hWnd, GWL_EXSTYLE) & (WS_EX_NOREDIRECTIONBITMAP | WS_EX_TOOLWINDOW)) == 0))
                {
                    taskListAsHwnd.Add(hWnd);
                }

                return true;
            }, IntPtr.Zero);

            return taskListAsHwnd;
        }

        //
        // 画面表示制御関連
        //
        private Point ProposePosition()
        {
            Control baseCtrl = null;

            foreach (Control ctrl in Controls.OfType<TaskUserControl>())
            {
                if (baseCtrl == null || baseCtrl.Bottom <= ctrl.Bottom || baseCtrl.Right <= ctrl.Right)
                {
                    baseCtrl = ctrl;
                }
            }

            if (baseCtrl == null)
            {
                return Point.Empty;

            }
            else if (baseCtrl.Right + baseCtrl.Width > Width)
            {
                return new Point(0, baseCtrl.Top + baseCtrl.Height);
            }
            else
            {
                return new Point(baseCtrl.Right, baseCtrl.Top);
            }
        }

        private void OldRefreshBoard()
        {
            //List<IntPtr> runningTaskListHwnd = GetTaskList();

            var hWndListRunningTask = GetTaskHwndList();
            //var hWndListTaskControl = from TaskUserControl ctrl in Controls
            //                          where ctrl.GetType() == typeof(TaskUserControl)
            //                          select ctrl.windowHandle;
            var taskControlList = from Control ctrl in Controls
                                  where ctrl.GetType() == typeof(TaskUserControl)
                                  select (TaskUserControl)ctrl;


            foreach (var item in Controls.OfType<TaskUserControl>())
            {

            }

            foreach (var taskControl in taskControlList)
            {
                if (hWndListRunningTask.Contains(taskControl.WindowHandle))
                {
                    GetWindowText(taskControl.WindowHandle, taskControl.TaskName, taskControl.TaskName.Capacity);
                    hWndListRunningTask.Remove(taskControl.WindowHandle);
                }
                else
                {
                    Controls.Remove(taskControl);
                }
            }

            // Add Task User Control in hWndListRunningTask
            foreach (var newTaskHwnd in hWndListRunningTask)
            {
                var newTask = new TaskUserControl(newTaskHwnd);
                newTask.Location = ProposePosition();
                Controls.Add(newTask);
            }
        }

        private void Renew()
        {
            var runningTasks = GetTaskHwndList();

            foreach (var taskControl in Controls.OfType<TaskUserControl>())
            {
                if (taskControl.Renew())
                {
                    runningTasks.Remove(taskControl.WindowHandle);
                }
                else
                {
                    taskControl.Dispose();
                }
            }

            foreach (var newTask in runningTasks)
            {
                var newTaskControl = new TaskUserControl(newTask);
                newTaskControl.Location = ProposePosition();
                Controls.Add(newTaskControl);
            }
        }


        //
        // Rubber Band 関連
        //

        // カーソル位置を取得
        private Point cursorClientPos()
        {
            // 画面座標でカーソルの位置を取得
            Point p = Cursor.Position;
            // 画面座標からコントロール上の座標に変換
            Point cp = this.PointToClient(p);

            return cp;
        }

        // 矩形ラインを描画
        private void DrawQuadLine(Point p0, Point p1, Point p2, Point p3)
        {
            // 画像のバックアップを取得
            // Bitmap canvasBase = new Bitmap(Board.Image);

            // 描画するImageオブジェクトを作成
            // サイズだけ指定すると無色透明のキャンバスになる
            rubberBandBitmap = new Bitmap(Board.Width, Board.Height);

            //ImageオブジェクトのGraphicsオブジェクトを作成する
            gRubberBand = Graphics.FromImage(rubberBandBitmap);

            // Penオブジェクトの作成
            linePen = new Pen(lineColor, lineBorder);

            // 先にバックアップしていた画像で塗り潰す
            // gr.FillRectangle(Brushes.Black, Board.ClientRectangle);

            // スタイルを指定
            linePen.DashStyle = DashStyle.Dot;
            // ラインを描画
            gRubberBand.DrawLine(linePen, p0, p1); // 上辺
            gRubberBand.DrawLine(linePen, p2, p3); // 底辺
            gRubberBand.DrawLine(linePen, p0, p2); // 左辺
            gRubberBand.DrawLine(linePen, p1, p3); // 右辺

            // PictureBoxに表示
            Board.Image = rubberBandBitmap;
        }





        ////
        //// タスクアイコン関連
        ////
        //class TaskIcon : PictureBox
        //{
        //    IntPtr windowHandle;
        //    StringBuilder taskName;
        //    Icon winIcon;
        //    bool isSelected;    

        //    public TaskIcon(IntPtr hwnd)
        //    {
        //        windowHandle = hwnd;
        //        taskName= new StringBuilder(256);
        //        GetWindowText(hwnd, taskName, taskName.Capacity);
        //        winIcon = GetIconFromExe(hwnd);

        //    }
        //}



    }
}
