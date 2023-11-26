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

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClassLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, int lpIconName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

        private const uint ICON_SMALL = 0;
        private const uint ICON_BIG = 1;
        private const uint WM_GETICON = 0x7F;
        private const uint GW_OWNER = 4;
        private const int GWL_EXSTYLE = -20;
        private const int GCL_HICON = -14;
        private const int GCL_HICONSM = -34;
        private const long WS_EX_NOREDIRECTIONBITMAP = 0x00200000L;
        private const long WS_EX_TOOLWINDOW = 0x00000080L;


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

        private static List<IntPtr> GetTaskList()
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


        private bool isSelecting { get; set; }
        private Point rubberBandStart { get; set; }
        private Point rubberBandEnd { get; set; }
        private Bitmap rubberBandBitmap { get; set; }


        // 変数
        bool quadMode = false;

        Point startPoint;
        Point endPoint;

        Color lineColer = Color.Red;
        int lineBorder = 1;

        Bitmap backupImage;
        Graphics gRubberBand;
        Pen linePen;


        // カーソル位置を取得
        private Point cursorPos()
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
            Bitmap canvas = new Bitmap(Board.Width, Board.Height);

            //ImageオブジェクトのGraphicsオブジェクトを作成する
            gRubberBand = Graphics.FromImage(canvas);

            // Penオブジェクトの作成
            linePen = new Pen(lineColer, lineBorder);

            // 先にバックアップしていた画像で塗り潰す
            // gr.FillRectangle(Brushes.Black, Board.ClientRectangle);


            // スタイルを指定
            linePen.DashStyle = DashStyle.Dot;
            // ラインを描画
            gRubberBand.DrawLine(linePen, p0, p1); // 上辺
            gRubberBand.DrawLine(linePen, p2, p3); // 底辺
            gRubberBand.DrawLine(linePen, p0, p2); // 左辺
            gRubberBand.DrawLine(linePen, p1, p3); // 右辺

            // PictureBox1に表示
            Board.Image = canvas;

        }

        public TaskBoard()
        {
            InitializeComponent();
        }


        private void TaskBoard_Load(object sender, EventArgs e)
        {
            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(Board.Width, Board.Height);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            // Graphics g = Graphics.FromImage(canvas);

            //黒で塗りつぶされた長方形を描画する
            using (Graphics g = Graphics.FromImage(canvas))
            {
                // g.FillRectangle(Brushes.Black, Board.ClientRectangle);
            }

            //Graphicsオブジェクトのリソースを解放する
            // g.Dispose();
            //PictureBox1に表示する

            Board.Image = canvas;
        }

        private void Board_MouseDown(object sender, MouseEventArgs e)
        {
            // 座標を保存
            startPoint.X = cursorPos().X;
            startPoint.Y = cursorPos().Y;

            isSelecting = true;
        }

        private void Board_MouseMove(object sender, MouseEventArgs e)
        {
            // マウスの左ボタンが押されている場合のみ処理
            if (isSelecting)
            {
                // 座標を取得
                endPoint.X = cursorPos().X;
                endPoint.Y = cursorPos().Y;

                // 描画
                Point p0 = new Point(startPoint.X, startPoint.Y);
                Point p1 = new Point(endPoint.X, startPoint.Y);
                Point p2 = new Point(startPoint.X, endPoint.Y);
                Point p3 = new Point(endPoint.X, endPoint.Y);
                DrawQuadLine(p0, p1, p2, p3);
            }

        }

        private void Board_MouseUp(object sender, MouseEventArgs e)
        {
            // リソースを解放
            linePen.Dispose();
            gRubberBand.Dispose();
            isSelecting = false;

        }
    }
}
