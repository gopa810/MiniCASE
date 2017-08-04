using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniCASE
{
    public partial class PanelIntro : UserControl
    {
        public LinkLabel[] recents = null;

        public PanelIntro()
        {
            InitializeComponent();

            recents = new LinkLabel[] { linkLabel1, linkLabel2, linkLabel3, linkLabel4, linkLabel5, linkLabel6 };

            UpdateRecents();
        }

        private void PanelIntro_SizeChanged(object sender, EventArgs e)
        {
            Size size = panel1.Size;
            Size sizeParent = Size;

            panel1.Location = new Point(sizeParent.Width/2 - size.Width/2, sizeParent.Height/2 - size.Height/2);

        }

        private void buttonCreateNew_Click(object sender, EventArgs e)
        {
            OnNewProject();
        }

        public static void OnNewProject()
        {
            CDProject proj = new CDProject();
            proj.InitProject();

            DialogEditProjectProperties epp = new DialogEditProjectProperties();

            epp.SetProject(proj);
            if (epp.ShowDialog() == DialogResult.OK)
            {
                MiniCaseApp.Project = proj;
                MiniCaseApp.MainWindow.ShowPanel("diagram", proj.RootDiagram);
            }
        }

        private void buttonOpenExisting_Click(object sender, EventArgs e)
        {
            OnOpenProject();
        }

        public static void OnOpenProject()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                MiniCaseApp.MainWindow.OpenProject(ofd.FileName);
            }
        }

        private void linkRecentLabel_Click(object sender, EventArgs e)
        {
            if (sender is LinkLabel)
            {
                LinkLabel link = (LinkLabel)sender;
                if (link.Tag is CDProjectBase)
                {
                    CDProjectBase mpp = (CDProjectBase)link.Tag;
                    MiniCaseApp.PutRecentAtFirstPosition(mpp);
                    if (!MiniCaseApp.MainWindow.OpenProject(mpp.FilePath))
                    {
                        MiniCaseApp.RemoveRecent(mpp);
                    }
                }
            }
        }

        public void UpdateRecents()
        {
            CDProjectBase[] rp = MiniCaseApp.RecentProjects;

            for(int i = 0; i < recents.Length; i++)
            {
                if (rp.Length > i)
                {
                    recents[i].Visible = true;
                    recents[i].Tag = rp[i];
                    recents[i].Text = rp[i].ProjectName;
                }
                else
                {
                    recents[i].Visible = false;
                }
            }
        }
    }
}
