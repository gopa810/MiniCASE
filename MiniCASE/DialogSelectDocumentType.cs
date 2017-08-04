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
    public partial class DialogSelectDocumentType : Form
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

        public DialogSelectDocumentType()
        {
            InitializeComponent();

        }

        public void SetProject(CDProject proj)
        {
            foreach (CDDocumentDefinition dd in proj.Library.DocumentTypes)
            {
                listBox1.Items.Add(dd);
            }

            foreach (CDDiagram cdd in proj.diagrams)
            {
                listBox2.Items.Add(new DProxy(cdd));
            }

        }

        public bool WillCreateNewDiagram
        {
            get
            {
                return tabControl1.SelectedIndex == 0;
            }
        }

        public CDDocumentDefinition SelectedDocumentType
        {
            get
            {
                if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < listBox1.Items.Count)
                {
                    return (CDDocumentDefinition)(listBox1.Items[listBox1.SelectedIndex]);
                }

                return null;
            }
        }

        public CDDiagram SelectedExistingDiagram
        {
            get
            {
                if (listBox2.SelectedIndex >= 0 && listBox2.SelectedIndex < listBox2.Items.Count)
                {
                    object obj = listBox2.Items[listBox2.SelectedIndex];
                    if (obj is DProxy)
                        return ((DProxy)obj).diagram;
                }

                return null;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int i = listBox2.FindString(textBox1.Text);
            if (i >= 0)
            {
                listBox2.SelectedIndex = i;
                listBox2.TopIndex = i;
            }
        }
    }

    public class FindItemComparer : IComparer<string>
    {
        public string text = "";

        public int Compare(string x, string y)
        {
            // Get the value of the leading Roman numerals.
            int xarabic = GetRomanValue(x);
            int yarabic = GetRomanValue(y);

            if (xarabic == yarabic)
                return x.CompareTo(y);

            // Compare them.
            return xarabic.CompareTo(yarabic);
        }

        public int GetRomanValue(string s)
        {
            if (s.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
                return 0;

            if (s.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) > 0)
                return 1;

            return 2;
        }

    }
}
