namespace SpiralEdge
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            this.wcAwesomium = new Awesomium.Windows.Forms.WebControl(this.components);
            this.SuspendLayout();
            // 
            // wcAwesomium
            // 
            this.wcAwesomium.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wcAwesomium.Location = new System.Drawing.Point(0, 0);
            this.wcAwesomium.Size = new System.Drawing.Size(784, 561);
            this.wcAwesomium.TabIndex = 0;
            this.wcAwesomium.ViewType = Awesomium.Core.WebViewType.Offscreen;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.wcAwesomium);
            this.Name = "frmMain";
            this.Text = "Main Window";
            this.ResumeLayout(false);

        }

        #endregion

        private Awesomium.Windows.Forms.WebControl wcAwesomium;
    }
}

