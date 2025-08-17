using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TaskBoardWf
{

    /// <summary>
    ///     holds the overlay for the scrollable control.
    /// </summary>
    internal class ScrollOverlay : Panel
    {
        //
        // Variables
        //
        private List<Rectangle> overlayRectangles = new List<Rectangle>();

        public ScrollOverlay()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Dock = DockStyle.Fill;
        }

        protected override CreateParams CreateParams
        {
            get {
                var cp = base.CreateParams;
                const int WS_EX_TRANSPARENT = 0x20;
                cp.ExStyle |= WS_EX_TRANSPARENT;
                return cp;
            }
        }

        public void AddRectangle(Rectangle rectangle)
        {
            overlayRectangles.Add(rectangle);
            this.Invalidate();
        }

        public void ClearOverlayRectangles()
        {
            overlayRectangles.Clear();
            this.Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var brush = new SolidBrush(Color.FromArgb(100, Color.Purple))) {
                foreach (var r in overlayRectangles) {
                    e.Graphics.FillRectangle(brush, r);
                }
            }

        }
    }
}
