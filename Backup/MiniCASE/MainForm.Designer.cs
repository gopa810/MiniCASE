namespace MiniCASE
{
    partial class MainForm
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
            this.caseDiagramView1 = new MiniCASE.CaseDiagramView();
            this.caseToolboxView1 = new MiniCASE.CaseToolboxView();
            this.SuspendLayout();
            // 
            // caseDiagramView1
            // 
            this.caseDiagramView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.caseDiagramView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.caseDiagramView1.Location = new System.Drawing.Point(227, 1);
            this.caseDiagramView1.Name = "caseDiagramView1";
            this.caseDiagramView1.Size = new System.Drawing.Size(572, 464);
            this.caseDiagramView1.TabIndex = 0;
            // 
            // caseToolboxView1
            // 
            this.caseToolboxView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.caseToolboxView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.caseToolboxView1.Location = new System.Drawing.Point(0, 1);
            this.caseToolboxView1.Name = "caseToolboxView1";
            this.caseToolboxView1.Size = new System.Drawing.Size(221, 464);
            this.caseToolboxView1.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 467);
            this.Controls.Add(this.caseToolboxView1);
            this.Controls.Add(this.caseDiagramView1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private CaseDiagramView caseDiagramView1;
        private CaseToolboxView caseToolboxView1;
    }
}

