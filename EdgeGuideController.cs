using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TaskBoardWf
{
    internal class EdgeGuideController
    {
        // This class is used to create a guide rectangle for the TaskBoard control.
        // It draws rectangles on the edge of TaskBoard to indicate the controls out of the TaskBoard.
        // Gave up the full Panel overlay strategy and moving guides with right button drag due to the performance issue
        // It could be improved by four separated Panel strategy

        //
        // Constants
        //
        int guideLineWidth = 10; // Width of the guide rectangle

        //
        // Variables
        //

        internal Form ParentForm { get; set; }

        private class EdgeGuide : Panel
        {
            public EdgeGuide(Point location, Size size)
            {
                BackColor = Color.Purple;
                Location = location;
                Size = size;
            }
        }

        //
        // Constructor
        //
        public EdgeGuideController(Form parentForm)
        {
            ParentForm = parentForm;
        }

        //
        // Methods
        //
        public void ShowEdgeGuide(Control ctrl)
        {
            int? guideHeight = null;
            int? guideWidth = null;
            int? guideX = null;
            int? guideY = null;

            if (ctrl.Top > ParentForm.ClientSize.Height) {
                guideX = ctrl.Left;
                guideY = ParentForm.ClientSize.Height - guideLineWidth;
                guideHeight = guideLineWidth;
            }
            else if (ctrl.Bottom < 0) {
                guideX = ctrl.Left;
                guideY = 0;
                guideHeight = guideLineWidth;
            }

            if (ctrl.Left > ParentForm.ClientSize.Width) {
                guideX = ParentForm.ClientSize.Width - guideLineWidth;
                guideY = guideY ?? ctrl.Top;
                guideWidth = guideLineWidth;
            }
            else if (ctrl.Right < 0) {
                guideX = 0;
                guideY = guideY ?? ctrl.Top;
                guideWidth = guideLineWidth;
            }

            if (guideX != null) {
                var Guide = new EdgeGuide(new Point((int)guideX, (int)guideY), new Size(guideWidth ?? ctrl.Width, guideHeight ?? ctrl.Height));
                ParentForm.Controls.Add(Guide);
                Guide.BringToFront();
            }
        }

        public void ShowEdgeGuides(IEnumerable<Control> controls)
        {
            // To avoid modifying the collection during iteration, added ToList()
            foreach (var ctrl in controls.ToList()) {
                ShowEdgeGuide(ctrl);
            }
        }

        public void ClearGuides()
        {
            // To avoid modifying the collection during iteration, added ToList()
            foreach (var guide in ParentForm.Controls.OfType<EdgeGuide>().ToList()) {
                ParentForm.Controls.Remove(guide);
            }
        }
    }
}
