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
    public partial class DialogFindDiagram : Form
    {
        private class DProxy
        {
            public CDDiagram diagram;
            public DProxy(CDDiagram d)
            {
                diagram = d;
            }
            public override string ToString()
            {
                return diagram.GetString("Text");
            }
        }

        public DialogFindDiagram()
        {
            InitializeComponent();
            FindDiagram("");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            FindDiagram(textBox1.Text);
        }

        public void FindDiagram(string s)
        {
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            foreach(CDDiagram d in MiniCaseApp.Project.diagrams)
            {
                if (string.IsNullOrWhiteSpace(s) || d.GetString("Text").Equals(s))
                    listBox1.Items.Add(new DProxy(d));
            }
            listBox1.EndUpdate();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public CDDiagram SelectedDiagram
        {
            get
            {
                if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < listBox1.Items.Count)
                {
                    DProxy dp = (DProxy)listBox1.Items[listBox1.SelectedIndex];
                    return dp.diagram;
                }
                return null;
            }
        }
    }
}
