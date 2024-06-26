namespace TaskBoardWf
{
    partial class TaskBoard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RubberBandBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.RubberBandBox)).BeginInit();
            this.SuspendLayout();
            // 
            // RubberBandBox
            // 
            this.RubberBandBox.BackColor = System.Drawing.Color.Transparent;
            this.RubberBandBox.Location = new System.Drawing.Point(10, 10);
            this.RubberBandBox.Name = "RubberBandBox";
            this.RubberBandBox.Size = new System.Drawing.Size(10, 10);
            this.RubberBandBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.RubberBandBox.TabIndex = 0;
            this.RubberBandBox.TabStop = false;
            // 
            // TaskBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(922, 595);
            this.Controls.Add(this.RubberBandBox);
            this.Name = "TaskBoard";
            this.Text = "TaskBoard";
            this.Activated += new System.EventHandler(this.TaskBoard_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaskBoard_FormClosing);
            this.Load += new System.EventHandler(this.TaskBoard_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskBoard_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskBoard_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskBoard_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.RubberBandBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox RubberBandBox;
        private TaskBoardWf.TaskUserControl taskUserControl1;
    }
}

