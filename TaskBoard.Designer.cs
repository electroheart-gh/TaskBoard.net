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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskBoard));
            this.Board = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.taskUserControl1 = new TaskBoardWf.TaskUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.Board)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // Board
            // 
            this.Board.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Board.Location = new System.Drawing.Point(0, 0);
            this.Board.Name = "Board";
            this.Board.Size = new System.Drawing.Size(948, 595);
            this.Board.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Board.TabIndex = 0;
            this.Board.TabStop = false;
            this.Board.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Board_MouseDown);
            this.Board.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Board_MouseMove);
            this.Board.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Board_MouseUp);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(212, 208);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(111, 74);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // taskUserControl1
            // 
            this.taskUserControl1.IsSelected = false;
            this.taskUserControl1.Location = new System.Drawing.Point(525, 132);
            this.taskUserControl1.Name = "taskUserControl1";
            this.taskUserControl1.Size = new System.Drawing.Size(80, 80);
            this.taskUserControl1.TabIndex = 2;
            this.taskUserControl1.TaskName = ((System.Text.StringBuilder)(resources.GetObject("taskUserControl1.TaskName")));
            // TODO: Code generation for '' failed because of Exception 'Invalid Primitive Type: System.IntPtr. Consider using CodeObjectCreateExpression.'.
            // 
            // TaskBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 595);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.Board);
            this.Name = "TaskBoard";
            this.Text = "TaskBoard";
            this.Load += new System.EventHandler(this.TaskBoard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Board)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Board;
        private System.Windows.Forms.PictureBox pictureBox1;
        private TaskBoardWf.TaskUserControl taskUserControl1;
    }
}

