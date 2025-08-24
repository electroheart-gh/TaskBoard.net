using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskBoardWf
{
    internal class EdgeGuideController

    {
        // This class is used to create a guide rectangle for the TaskBoard control.
        // It can be used to draw a rectangle on the control to guide the user.

        //
        // Constants
        //
        int guideLineWidth = 10; // Width of the guide rectangle

        //
        // Variables
        //

        internal Form ParentForm { get; set; }

        private List<EdgeGuide> edgeGuides = new List<EdgeGuide>();

        private EdgeGuide edgeGuideOverlay = null;


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

            // test code
            //edgeGuideOverlay = new EdgeGuide(new Point(0, 0), new Size(100, 100));  
        }

        //
        // Methods
        //

        // EdgeGuideController.Add
        // Adds a new edge guide to the list of edge guides.

        public void Test()
        {
            var Guide = new EdgeGuide(new Point(100, 100), new Size(10, 10));
            ParentForm.Controls.Add(Guide);
            Guide.BringToFront();
        }

        public void AddGuide(Control ctrl)
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
                //var Guide = new EdgeGuide(new Point(100,100), new Size(100,100));
                ParentForm.Controls.Add(Guide);
                Guide.BringToFront();
                Logger.LogError($"Added edge guides.");

            }
        }

        public void AddGuides(IEnumerable<Control> controls)
        {
            foreach (var ctrl in controls) {
                AddGuide(ctrl);
            }
        }

        public void ClearGuides()
        {
            foreach (var guide in ParentForm.Controls.OfType<EdgeGuide>().ToList()) {
                ParentForm.Controls.Remove(guide);
                Logger.LogError($"Removed edge guides.");
            }
        }
    }
}
