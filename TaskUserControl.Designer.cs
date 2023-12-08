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
            this.components = new System.ComponentModel.Container();
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.lblTaskName = new System.Windows.Forms.Label();
            this.toolTipTaskName = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pbIcon
            // 
            this.pbIcon.BackColor = System.Drawing.Color.Transparent;
            this.pbIcon.Location = new System.Drawing.Point(16, 1);
            this.pbIcon.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.Size = new System.Drawing.Size(40, 40);
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIcon.TabIndex = 0;
            this.pbIcon.TabStop = false;
            this.pbIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseDown);
            this.pbIcon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseMove);
            this.pbIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseUp);
            // 
            // lblTaskName
            // 
            this.lblTaskName.BackColor = System.Drawing.Color.Transparent;
            this.lblTaskName.Font = new System.Drawing.Font("Meiryo UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTaskName.Location = new System.Drawing.Point(0, 40);
            this.lblTaskName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTaskName.Name = "lblTaskName";
            this.lblTaskName.Size = new System.Drawing.Size(72, 30);
            this.lblTaskName.TabIndex = 1;
            this.lblTaskName.Text = "taskname";
            this.lblTaskName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblTaskName.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseDown);
            this.lblTaskName.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseMove);
            this.lblTaskName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseUp);
            // 
            // toolTipTaskName
            // 
            this.toolTipTaskName.AutomaticDelay = 50;
            this.toolTipTaskName.AutoPopDelay = 5000;
            this.toolTipTaskName.InitialDelay = 50;
            this.toolTipTaskName.ReshowDelay = 10;
            this.toolTipTaskName.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip1_Popup);
            // 
            // TaskUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.lblTaskName);
            this.Controls.Add(this.pbIcon);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "TaskUserControl";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Size = new System.Drawing.Size(72, 72);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.Label lblTaskName;
        private System.Windows.Forms.ToolTip toolTipTaskName;
    }
}
