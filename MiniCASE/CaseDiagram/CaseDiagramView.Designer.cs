namespace MiniCASE
{
    partial class CaseDiagramView
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
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.SuspendLayout();
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hScrollBar1.Location = new System.Drawing.Point(0, 377);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(537, 17);
            this.hScrollBar1.TabIndex = 0;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.vScrollBar1.Location = new System.Drawing.Point(540, 0);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 376);
            this.vScrollBar1.TabIndex = 1;
            this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
            // 
            // CaseDiagramView
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.hScrollBar1);
            this.Name = "CaseDiagramView";
            this.Size = new System.Drawing.Size(560, 394);
            this.MouseCaptureChanged += new System.EventHandler(this.CaseDiagramView_MouseCaptureChanged);
            this.MouseLeave += new System.EventHandler(this.CaseDiagramView_MouseLeave);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CaseDiagramView_Paint);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CaseDiagramView_MouseMove);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CaseDiagramView_MouseDoubleClick);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.CaseDiagramView_DragDrop);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CaseDiagramView_KeyUp);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CaseDiagramView_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CaseDiagramView_MouseDown);
            this.MouseHover += new System.EventHandler(this.CaseDiagramView_MouseHover);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CaseDiagramView_MouseUp);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.CaseDiagramView_DragEnter);
            this.SizeChanged += new System.EventHandler(this.CaseDiagramView_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CaseDiagramView_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
    }
}
