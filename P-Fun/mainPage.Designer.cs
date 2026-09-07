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
            this.chkBTC = new System.Windows.Forms.CheckBox();
            this.chkETH = new System.Windows.Forms.CheckBox();
            this.chkBNB = new System.Windows.Forms.CheckBox();
            this.chkSOL = new System.Windows.Forms.CheckBox();
            this.chkXRP = new System.Windows.Forms.CheckBox();
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
            this.sidePanel.Controls.Add(this.chkXRP);
            this.sidePanel.Controls.Add(this.chkSOL);
            this.sidePanel.Controls.Add(this.chkBNB);
            this.sidePanel.Controls.Add(this.chkETH);
            this.sidePanel.Controls.Add(this.chkBTC);
            this.sidePanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.sidePanel.Location = new System.Drawing.Point(600, 0);
            this.sidePanel.Name = "sidePanel";
            this.sidePanel.Size = new System.Drawing.Size(200, 450);
            this.sidePanel.TabIndex = 1;
            // 
            // chkBTC
            // 
            this.chkBTC.AutoSize = true;
            this.chkBTC.Checked = true;
            this.chkBTC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBTC.Location = new System.Drawing.Point(20, 20);
            this.chkBTC.Name = "chkBTC";
            this.chkBTC.Size = new System.Drawing.Size(75, 19);
            this.chkBTC.TabIndex = 0;
            this.chkBTC.Text = "BTC/USDT";
            this.chkBTC.UseVisualStyleBackColor = true;
            // 
            // chkETH
            // 
            this.chkETH.AutoSize = true;
            this.chkETH.Checked = true;
            this.chkETH.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkETH.Location = new System.Drawing.Point(20, 50);
            this.chkETH.Name = "chkETH";
            this.chkETH.Size = new System.Drawing.Size(76, 19);
            this.chkETH.TabIndex = 1;
            this.chkETH.Text = "ETH/USDT";
            this.chkETH.UseVisualStyleBackColor = true;
            // 
            // chkBNB
            // 
            this.chkBNB.AutoSize = true;
            this.chkBNB.Checked = true;
            this.chkBNB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBNB.Location = new System.Drawing.Point(20, 80);
            this.chkBNB.Name = "chkBNB";
            this.chkBNB.Size = new System.Drawing.Size(77, 19);
            this.chkBNB.TabIndex = 2;
            this.chkBNB.Text = "BNB/USDT";
            this.chkBNB.UseVisualStyleBackColor = true;
            // 
            // chkSOL
            // 
            this.chkSOL.AutoSize = true;
            this.chkSOL.Checked = true;
            this.chkSOL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSOL.Location = new System.Drawing.Point(20, 110);
            this.chkSOL.Name = "chkSOL";
            this.chkSOL.Size = new System.Drawing.Size(76, 19);
            this.chkSOL.TabIndex = 3;
            this.chkSOL.Text = "SOL/USDT";
            this.chkSOL.UseVisualStyleBackColor = true;
            // 
            // chkXRP
            // 
            this.chkXRP.AutoSize = true;
            this.chkXRP.Checked = true;
            this.chkXRP.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkXRP.Location = new System.Drawing.Point(20, 140);
            this.chkXRP.Name = "chkXRP";
            this.chkXRP.Size = new System.Drawing.Size(77, 19);
            this.chkXRP.TabIndex = 4;
            this.chkXRP.Text = "XRP/USDT";
            this.chkXRP.UseVisualStyleBackColor = true;
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
        private System.Windows.Forms.CheckBox chkBTC;
        private System.Windows.Forms.CheckBox chkETH;
        private System.Windows.Forms.CheckBox chkBNB;
        private System.Windows.Forms.CheckBox chkSOL;
        private System.Windows.Forms.CheckBox chkXRP;
    }
}