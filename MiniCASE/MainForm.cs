using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Security.Permissions;

namespace MiniCASE
{
    public partial class MainForm : Form
    {
        public PanelDiagramEdit panelDiagramEdit = null;
        public PanelIntro panelIntro = null;

        public UserControl currentPanel = null;

        public MainForm()
        {
            InitializeComponent();

            MiniCaseApp.MainWindow = this;
            MiniCaseApp.OnAppStarting();

            ShowPanel("intro");

        }


        public void ShowPanel(string cmd, object data = null)
        {
            switch(cmd)
            {
                case "intro":
                    if (panelIntro == null)
                    {
                        panelIntro = new PanelIntro();
                        panelIntro.Size = panelPresent.Size;
                        panelIntro.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        panelPresent.Controls.Add(panelIntro);
                        panelIntro.Parent = panelPresent;
                        panelIntro.Location = new Point(0, 0);
                    }
                    ShowPanel(panelIntro);
                    break;
                case "diagram":
                    if (panelDiagramEdit == null)
                    {
                        panelDiagramEdit = new PanelDiagramEdit();
                        panelDiagramEdit.Size = panelPresent.Size;
                        panelDiagramEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        panelPresent.Controls.Add(panelDiagramEdit);
                        panelDiagramEdit.Parent = panelPresent;
                        panelDiagramEdit.Location = new Point(0, 0);
                    }
                    ShowPanel(panelDiagramEdit);
                    if (data != null && data is CDDiagram)
                    {
                        panelDiagramEdit.SetDiagram((CDDiagram)data);
                    }
                    break;
            }
        }

        public void ShowPanel(UserControl p)
        {
            if (p == currentPanel)
                return;
            if (p != null)
                p.Visible = true;
            if (currentPanel != null)
                currentPanel.Visible = false;
            currentPanel = p;
        }

        public bool OpenProject(string filePath)
        {
            CDProject proj = new CDProject();
            if (proj.Load(filePath))
            {
                CDProjectBase rp = new CDProjectBase();
                rp.FilePath = filePath;
                rp.ProjectName = proj.ProjectName;
                MiniCaseApp.PutRecentAtFirstPosition(rp);

                MiniCaseApp.Project = proj;
                ShowPanel("diagram", proj.RootDiagram);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True is successful saving. False is not saving.</returns>
        public bool SaveProject()
        {
            if (MiniCaseApp.Project != null)
            {
                if (string.IsNullOrEmpty(MiniCaseApp.Project.FilePath))
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        MiniCaseApp.Project.FilePath = sfd.FileName;
                    }
                }

                if (!string.IsNullOrEmpty(MiniCaseApp.Project.FilePath))
                {
                    if (MiniCaseApp.Project.Save(MiniCaseApp.Project.FilePath))
                    {
                        MiniCaseApp.PutRecentAtFirstPosition(MiniCaseApp.Project);
                        return true;
                    }
                }
            }

            return false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MiniCaseApp.OnAppStopping();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PanelIntro.OnNewProject();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PanelIntro.OnOpenProject();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject();
        }
    }
}
