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

        

        private void InitializeComponent()
        {
            plotPanel = new ScottPlot.WinForms.FormsPlot();
            sidePanel = new Panel();
            SuspendLayout();
            // 
            // plotPanel
            // 
            plotPanel.Dock = DockStyle.Fill;
            plotPanel.Location = new Point(0, 0);
            plotPanel.Name = "plotPanel";
            plotPanel.Size = new Size(761, 695);
            plotPanel.TabIndex = 0;
            // 
            // sidePanel
            // 
            sidePanel.Dock = DockStyle.Right;
            sidePanel.Location = new Point(761, 0);
            sidePanel.Name = "sidePanel";
            sidePanel.Size = new Size(200, 695);
            sidePanel.TabIndex = 1;
            // 
            // mainPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(961, 695);
            Controls.Add(plotPanel);
            Controls.Add(sidePanel);
            Name = "mainPage";
            Text = "mainPage";
            ResumeLayout(false);
        }


        private ScottPlot.WinForms.FormsPlot plotPanel;
        private System.Windows.Forms.Panel sidePanel;
    }
}
