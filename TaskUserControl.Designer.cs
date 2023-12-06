namespace TaskBoardWf
{
    partial class TaskUserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.lblTaskName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pbIcon
            // 
            this.pbIcon.BackColor = System.Drawing.SystemColors.Control;
            this.pbIcon.Location = new System.Drawing.Point(15, 0);
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.Size = new System.Drawing.Size(50, 50);
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIcon.TabIndex = 0;
            this.pbIcon.TabStop = false;
            this.pbIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseDown);
            this.pbIcon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseMove);
            this.pbIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseUp);
            // 
            // lblTaskName
            // 
            this.lblTaskName.BackColor = System.Drawing.SystemColors.Control;
            this.lblTaskName.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTaskName.Location = new System.Drawing.Point(0, 50);
            this.lblTaskName.Name = "lblTaskName";
            this.lblTaskName.Size = new System.Drawing.Size(80, 30);
            this.lblTaskName.TabIndex = 1;
            this.lblTaskName.Text = "taskname";
            this.lblTaskName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblTaskName.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseDown);
            this.lblTaskName.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseMove);
            this.lblTaskName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseUp);
            // 
            // TaskUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.lblTaskName);
            this.Controls.Add(this.pbIcon);
            this.Name = "TaskUserControl";
            this.Size = new System.Drawing.Size(80, 80);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.Label lblTaskName;
    }
}
