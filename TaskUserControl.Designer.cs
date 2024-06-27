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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.detailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblTaskName = new System.Windows.Forms.Label();
            this.toolTipTaskName = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbIcon
            // 
            this.pbIcon.BackColor = System.Drawing.Color.Transparent;
            this.pbIcon.ContextMenuStrip = this.contextMenuStrip1;
            this.pbIcon.Location = new System.Drawing.Point(16, 1);
            this.pbIcon.Margin = new System.Windows.Forms.Padding(4);
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.Size = new System.Drawing.Size(40, 40);
            this.pbIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbIcon.TabIndex = 0;
            this.pbIcon.TabStop = false;
            this.pbIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseDown);
            this.pbIcon.MouseLeave += new System.EventHandler(this.TaskUserControl_MouseLeave);
            this.pbIcon.MouseHover += new System.EventHandler(this.TaskUserControl_MouseHover);
            this.pbIcon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseMove);
            this.pbIcon.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem,
            this.detailsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(110, 48);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // detailsToolStripMenuItem
            // 
            this.detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
            this.detailsToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.detailsToolStripMenuItem.Text = "Details";
            this.detailsToolStripMenuItem.Click += new System.EventHandler(this.detailsToolStripMenuItem_Click);
            // 
            // lblTaskName
            // 
            this.lblTaskName.BackColor = System.Drawing.Color.Transparent;
            this.lblTaskName.ContextMenuStrip = this.contextMenuStrip1;
            this.lblTaskName.Font = new System.Drawing.Font("Meiryo UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTaskName.Location = new System.Drawing.Point(0, 41);
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
            this.toolTipTaskName.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTipTaskName_Popup);
            // 
            // TaskUserControl
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.lblTaskName);
            this.Controls.Add(this.pbIcon);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TaskUserControl";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Size = new System.Drawing.Size(72, 72);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseDown);
            this.MouseLeave += new System.EventHandler(this.TaskUserControl_MouseLeave);
            this.MouseHover += new System.EventHandler(this.TaskUserControl_MouseHover);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TaskUserControl_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.Label lblTaskName;
        private System.Windows.Forms.ToolTip toolTipTaskName;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detailsToolStripMenuItem;
    }
}
