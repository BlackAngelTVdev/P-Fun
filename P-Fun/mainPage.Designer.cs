namespace P_Fun
{
    partial class mainPage
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.plotPanel = new System.Windows.Forms.Panel();
            this.sidePanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // plotPanel
            // 
            this.plotPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.plotPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plotPanel.Location = new System.Drawing.Point(0, 0);
            this.plotPanel.Name = "plotPanel";
            this.plotPanel.Size = new System.Drawing.Size(600, 450);
            this.plotPanel.TabIndex = 0;
            // 
            // sidePanel
            // 
            this.sidePanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.sidePanel.Location = new System.Drawing.Point(600, 0);
            this.sidePanel.Name = "sidePanel";
            this.sidePanel.Size = new System.Drawing.Size(200, 450);
            this.sidePanel.TabIndex = 1;
            // 
            // mainPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.plotPanel);
            this.Controls.Add(this.sidePanel);
            this.Name = "mainPage";
            this.Text = "mainPage";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel plotPanel;
        private System.Windows.Forms.Panel sidePanel;
    }
}
