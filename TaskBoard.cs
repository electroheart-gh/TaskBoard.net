using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TaskBoardWf
{
    public partial class TaskBoard : Form
    {
        // TODO: Consider to write rubber band above the task icon 
        // TODO: Make it scalable and scrollable
        // TODO: Implement keyboard interface
        // TODO: Make HOTKEY and colors configurable

        //
        // Variables 
        //

        // Rubber Band (Left button drag)
        bool isSelecting;
        Point rubberBandStart;
        Color lineColor = Color.Purple;   // or Gray
        int lineBorder = 1;
        Graphics gRubberBand;

        // Scrolling (Right button drag)
        bool isScrolling = false;
        private Point scrollStart;
        private List<Rectangle> listControlGuide = new List<Rectangle>();
        int guideRectWidth = 5;

        // Global Hot Key
        HotKey hotKey;

        // Window Image
        private IntPtr thumbHandle;
        private int deltaOpacity;

        // Scroll Overlay
        private ScrollOverlay scrollOverlay;

        // Edge Controller
        private EdgeGuideController edgeController;

        //
        // Constructor
        //
        public TaskBoard()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // 追加

            scrollOverlay = new ScrollOverlay();
            edgeController=new EdgeGuideController(this);

            //this.Controls.Add(scrollOverlay);
            //scrollOverlay.BringToFront();
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

            Logger.LogError("hotkey registered");
        }

        void hotKey_HotKeyPush(object sender, EventArgs e)
        {
            if (Form.ActiveForm != this) {
                Activate();
                BringToFront();
                WindowState = FormWindowState.Maximized;
            }
            else {
                Logger.LogError("HotKeyPushed");
            }
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
            foreach (Control ctrl in Controls.OfType<TaskUserControl>()) {
                if (baseCtrl == null || baseCtrl.Bottom < ctrl.Bottom || (baseCtrl.Bottom == ctrl.Bottom && baseCtrl.Right < ctrl.Right)) {
                    baseCtrl = ctrl;
                }
            }

            if (baseCtrl == null) {
                // The first one should be place at (0, 0)
                return Point.Empty;
            }
            else if (baseCtrl.Right + baseCtrl.Width > Screen.PrimaryScreen.WorkingArea.Width) {
                // If excessing Board width, place lower
                return new Point(0, baseCtrl.Top + baseCtrl.Height);
            }
            else {
                // Otherwise, place next to the one
                return new Point(baseCtrl.Right, baseCtrl.Top);
            }
        }

        // Update Task controls on the Board, delete obsolete Task controls and add new Task controls
        public void Renew()
        {
            var runningTasks = WinAPI.GetTaskHwndList();
            var taskToRemove = new List<TaskUserControl>();

            foreach (var taskControl in Controls.OfType<TaskUserControl>()) {
                if (runningTasks.Contains(taskControl.WindowHandle)) {
                    // Remove existing Task controls from the variable to extract new tasks
                    runningTasks.Remove(taskControl.WindowHandle);
                    taskControl.Renew();
                }
                else {
                    // Add obsolete tasks to the list
                    // Disposing control here makes "foreach" not work properly
                    taskToRemove.Add(taskControl);
                }
            }
            // Dispose obsolete Task controls 
            foreach (var task in taskToRemove) {
                task.Dispose();
            }
            // Add new tasks from old to new ones
            runningTasks.Reverse();
            foreach (var newTask in runningTasks) {
                var newTaskControl = new TaskUserControl(newTask);
                newTaskControl.Location = ProposePosition();
                Controls.Add(newTaskControl);
                newTaskControl.BringToFront();
            }
            // TODO: Save tasks positions to recover the layout when restarting after crashes
            // TODO: Save tasks positions to avoid rearrange tasks every time logging in using short cut and/or MiLauncher
        }


        internal void DisplayWindowImage(IntPtr winHandle)
        {
            if (Program.appSettings.BackgroundThumbnail) {
                // ISSUES: Flicker before capturing window
                DisplayThumbnail(winHandle, opaque: true);
                //Parent.BackgroundImage = ResizeImage(CaptureWindow(Parent.Handle));
                //Parent.BackgroundImage = ConvertToGrayscale(ResizeImage(CaptureWindow(Parent.Handle)));
                Bitmap screenImage = WinAPI.CaptureWindow(Handle);
                WinAPI.DwmUnregisterThumbnail(thumbHandle);
                thumbHandle = IntPtr.Zero;
                BackgroundImage = ConvertToGrayscale(ResizeImage(screenImage));
            }
            else {
                DisplayThumbnail(winHandle);
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


        internal void DisplayThumbnail(IntPtr winHandle, bool opaque = false)
        {
            // For safety, check and unregister thumbHandle before registering
            if (thumbHandle != IntPtr.Zero) {
                WinAPI.DwmUnregisterThumbnail(thumbHandle);
                thumbHandle = IntPtr.Zero;
            }
            int result = WinAPI.DwmRegisterThumbnail(Handle, winHandle, out thumbHandle);
            if (result != 0) {
                Debug.WriteLine("Failed to register thumbnail.");
                return;
            }

            WinAPI.DWM_THUMBNAIL_PROPERTIES props = new WinAPI.DWM_THUMBNAIL_PROPERTIES {
                dwFlags = WinAPI.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_RECTDESTINATION |
                          WinAPI.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_VISIBLE |
                          WinAPI.DWM_THUMBNAIL_PROPERTIES.DWM_TNP_OPACITY,
                // Set TaskBoard itself as destination screen
                rcDestination = new WinAPI.RECT {
                    Left = ClientRectangle.Left,
                    Top = ClientRectangle.Top,
                    Right = ClientRectangle.Right,
                    Bottom = ClientRectangle.Bottom
                },
                fVisible = true,
                opacity = opaque
                          ? byte.MaxValue
                          : (byte)(Program.appSettings.ThumbnailOpacity + deltaOpacity)
                //: Math.Max(Math.Min((byte)(Program.appSettings.ThumbnailOpacity + deltaOpacity), byte.MaxValue), byte.MinValue)
            };

            WinAPI.DwmUpdateThumbnailProperties(thumbHandle, ref props);
        }

        internal void ChangeThumbnailOpacity(IntPtr winHandle, bool increase)
        {
            // Debug.WriteLine("mouse wheel event " + (e.Delta > 0 ? "Up" : "Down"));
            var delta = Program.appSettings.DeltaOpacity;
            deltaOpacity += increase ? delta : -delta;
            deltaOpacity = Math.Min(deltaOpacity, byte.MaxValue - Program.appSettings.ThumbnailOpacity);
            deltaOpacity = Math.Max(deltaOpacity, byte.MinValue - Program.appSettings.ThumbnailOpacity);

            DisplayThumbnail(winHandle);
        }

        internal void ClearWindowImage()
        {
            if (Program.appSettings.BackgroundThumbnail) {
                BackgroundImage = null;
            }
            else {
                WinAPI.DwmUnregisterThumbnail(thumbHandle);
                thumbHandle = IntPtr.Zero;
                deltaOpacity = 0;
            }
        }

        //
        // Methods for rubber band
        //
        private void TaskBoard_FormClosing(object sender, FormClosingEventArgs e)
        {
            hotKey.Dispose();
        }

        private void TaskBoard_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                rubberBandStart = PointToClient(Cursor.Position);
                isSelecting = true;
                Renew();
                foreach (var taskControl in Controls.OfType<TaskUserControl>()) {
                    taskControl.IsSelected = false;
                }
            }
            else if (e.Button == MouseButtons.Right) {
                scrollStart = e.Location;
                isScrolling = true;
                Cursor = Cursors.SizeAll;
                //Controls.Add(scrollOverlay);
                //scrollOverlay.BringToFront();
                //edgeController.AddGuides(Controls.OfType<TaskUserControl>());

                foreach (var c in Controls.OfType<TaskUserControl>().ToList()) {
                    edgeController.AddGuide(c);
                }

                Logger.LogError("Right Mouse Down" );
            }
            ClearWindowImage();
        }


        private void TaskBoard_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting) {
                // Draw rubber band
                Point rubberBandEnd = PointToClient(Cursor.Position);
                RubberBandBox.Bounds = RectangleExt.Create(rubberBandStart, rubberBandEnd);

                // Specifying nothing but the size creates noncolor canvas 
                // To avoid 0 width/height, which makes an error, add +1 to Width and Height
                var rubberBandBitmap = new Bitmap(RubberBandBox.Width + 1, RubberBandBox.Height + 1);

                // Create Graphics object for the rubber band
                gRubberBand = Graphics.FromImage(rubberBandBitmap);

                Pen linePen = new Pen(lineColor, lineBorder);
                linePen.DashStyle = DashStyle.Dot;
                // To show the right and bottom lines, DrawRectangle should be -1, which is not related to the size of Bitmap mentioned above
                gRubberBand.DrawRectangle(linePen, 0, 0, RubberBandBox.Width - 1, RubberBandBox.Height - 1);

                RubberBandBox.Image = rubberBandBitmap;
                RubberBandBox.Enabled = true;

                // Release resources
                linePen.Dispose();
                gRubberBand.Dispose();

                // Check overlapped Task controls with rubber band
                foreach (var taskControl in Controls.OfType<TaskUserControl>()) {
                    if (new Rectangle(taskControl.Location, taskControl.Size).IntersectsWith(RubberBandBox.Bounds)) {
                        taskControl.IsSelected = true;
                    }
                    else {
                        taskControl.IsSelected = false;
                    }
                }
            }
            else if (isScrolling) {
                //this.Controls.Add(scrollOverlay);
                //scrollOverlay.BringToFront();
                //ClearScrollOverlay();
                //Controls.Add(scrollOverlay);
                //scrollOverlay.BringToFront();

                // Move all controls instead of scrolling Form, which does not have Panel
                foreach (var ctrl in Controls.OfType<TaskUserControl>()) {
                    ctrl.Location = new Point(ctrl.Location.X + e.Location.X - scrollStart.X, ctrl.Location.Y + e.Location.Y - scrollStart.Y);
                    //ctrl.Invalidate(); // 追加
                }
                //Invalidate(); // 追加

                scrollStart = e.Location;

                // 遅いというか、TaskControlの残像が残るはなぜか
                // 左ドラッグとの違いはなにか, 動かす control の数? Controlの追加・削除?


                //edgeController.ClearGuides();
                //edgeController.Test();
                //edgeController.AddGuides(Controls.OfType<TaskUserControl>().Select(tc => (Control)tc).ToList());
                //edgeController.AddGuides(Controls.OfType<TaskUserControl>().ToList<Control>());

                //edgeController.AddGuides(Controls.OfType<TaskUserControl>());
                //foreach (var c in Controls.OfType<TaskUserControl>()){
                //    edgeController.AddGuide(c);
                //}

                //this.Invalidate();
                //ShowScrollOverlay();
            }
        }

        private void TaskBoard_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                // Erase rubber band
                // To avoid 0 width/height, which makes an error, add +1 to Width and Height
                RubberBandBox.Image = new Bitmap(RubberBandBox.Width + 1, RubberBandBox.Height + 1);
                // Disable to click Rubber Band Box
                RubberBandBox.Enabled = false;

                isSelecting = false;
            }
            else if (e.Button == MouseButtons.Right) {
                isScrolling = false;
                Cursor = Cursors.Default;
                //ClearScrollOverlay();
                Logger.LogError("Right Mouse Up");
                edgeController.ClearGuides();

            }
        }

        private void TaskBoard_Activated(object sender, EventArgs e)
        {
            Renew();
            // Select the icon of the next window of TaskBoard in Z order
            // SelectNextTask(Handle);
        }

        private void SelectNextTask(IntPtr handle)
        {
            throw new NotImplementedException();
        }

        private void TaskBoard_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Q && e.Alt) || (e.KeyCode == Keys.M && e.Control)) {
                // Write code to exec command for M-q
                Logger.LogError("M-q");
            }
        }

        // Show guides for TaskControl outside TaskBoard
        private void ShowScrollOverlay()
        {
            //ClearScrollOverlay();

            // Check if the entire control is outside the board
            foreach (var ctrl in Controls.OfType<TaskUserControl>()) {
                //var overlayRectangle = new Rectangle();
                //var overlayRectangle = new Rectangle(ctrl.Location, ctrl.Size);
                (int? guideHeight, int? guideWidth) = (null, null);
                //(int X, int Y, int Width, int Height) overlayRectFactor= (ctrl.Left, ctrl.Top, ctrl.Width, ctrl.Height);

                if (ctrl.Top > this.ClientSize.Height) {
                    //listControlGuide.Add(new Rectangle(ctrl.Left, this.ClientSize.Height - guideRectWidth, ctrl.Width, guideRectWidth));
                    //scrollOverlay.AddRectangle(new Rectangle(ctrl.Left, this.ClientSize.Height - guideRectWidth, ctrl.Width, guideRectWidth));
                    //overlayRectangle = new Rectangle(overlayRectangle.Left, this.ClientSize.Height - guideRectWidth, overlayRectangle.Width, guideRectWidth);
                    //overlayRectangle.Height = this.ClientSize.Height - ctrl.Top - guideRectWidth;
                    guideHeight = this.ClientSize.Height - ctrl.Top - guideRectWidth;

                }
                else if (ctrl.Bottom < 0) {
                    //listControlGuide.Add(new Rectangle(ctrl.Left, 0, ctrl.Width, guideRectWidth));
                    //scrollOverlay.AddRectangle(new Rectangle(ctrl.Left, 0, ctrl.Width, guideRectWidth));
                    //overlayRectFactor = new Rectangle(overlayRectFactor.Left, 0, overlayRectFactor.Width, guideRectWidth);
                    //overlayRectangle.Height = -ctrl.Top + guideRectWidth;
                    guideHeight = -ctrl.Top + guideRectWidth;

                }

                if (ctrl.Left > this.ClientSize.Width) {
                    //listControlGuide.Add(new Rectangle(this.ClientSize.Width - guideRectWidth, ctrl.Top, guideRectWidth, ctrl.Height));
                    //scrollOverlay.AddRectangle(new Rectangle(this.ClientSize.Width - guideRectWidth, ctrl.Top, guideRectWidth, ctrl.Height));
                    //overlayRectangle.Width = this.ClientSize.Width - ctrl.Left - guideRectWidth;
                    guideWidth = this.ClientSize.Width - ctrl.Left - guideRectWidth;

                }
                else if (ctrl.Right < 0) {
                    //listControlGuide.Add(new Rectangle(0, ctrl.Top, guideRectWidth, ctrl.Height));
                    //scrollOverlay.AddRectangle(new Rectangle(0, ctrl.Top, guideRectWidth, ctrl.Height));
                    //overlayRectangle.Width = -ctrl.Left + guideRectWidth;
                    guideWidth = -ctrl.Left + guideRectWidth;
                }

                if (guideHeight != null || guideWidth != null) {
                    var overlayRectangle = new Rectangle(ctrl.Location, new Size(guideWidth ?? ctrl.Width, guideHeight ?? ctrl.Height));
                    //var overlayRectangle = new Rectangle(100, 100, 50, 50);

                    scrollOverlay.AddRectangle(overlayRectangle);
                }

            }
            //foreach (var ctrl in Controls.OfType<TaskUserControl>()) {
            //    if (ctrl.Top > this.ClientSize.Height) {
            //        listControlGuide.Add(new Rectangle(ctrl.Left, this.ClientSize.Height - guideRectWidth, ctrl.Width, guideRectWidth));
            //    }
            //    else if (ctrl.Bottom < 0) {
            //        listControlGuide.Add(new Rectangle(ctrl.Left, 0, ctrl.Width, guideRectWidth));
            //    }
            //    else if (ctrl.Left > this.ClientSize.Width) {
            //        listControlGuide.Add(new Rectangle(this.ClientSize.Width - guideRectWidth, ctrl.Top, guideRectWidth, ctrl.Height));
            //    }
            //    else if (ctrl.Right < 0) {
            //        listControlGuide.Add(new Rectangle(0, ctrl.Top, guideRectWidth, ctrl.Height));
            //    }
            //}
        }

        private void ClearScrollOverlay()
        {
            scrollOverlay.ClearOverlayRectangles();
            Controls.Remove(scrollOverlay);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw control guides
            if (listControlGuide.Count > 0) {
                using (var guideBrush = new SolidBrush(Color.Purple)) {
                    foreach (var rect in listControlGuide) {
                        e.Graphics.FillRectangle(guideBrush, rect);
                    }
                }
            }
        }
    }
}
