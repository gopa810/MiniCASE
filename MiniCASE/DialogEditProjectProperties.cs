using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniCASE
{
    public partial class DialogEditProjectProperties : Form
    {
        public CDProject Project { get; set; }

        public DialogEditProjectProperties()
        {
            InitializeComponent();
        }

        public void SetProject(CDProject project)
        {
            Project = project;

            textBoxProjectName.Text = project.ProjectName;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (Project != null)
            {
                Project.ProjectName = textBoxProjectName.Text;
            }
        }
    }
}
