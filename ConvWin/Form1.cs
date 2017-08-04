using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ConvWin
{
    public partial class Form1 : Form
    {
        bool swp = false;
        bool ewp = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            string s = richTextBox1.Text;

            swp = s.StartsWith("\"");
            if (swp)
                s = s.Substring(1);

            ewp = s.EndsWith("\"");
            if (ewp)
                s = s.Substring(0, s.Length - 1);

            richTextBox2.Text = CFormat(richTextBox1.Text);
        }

        public string CFormat(string format)
        {
            int mode = 0;
            int c = 0;
            StringBuilder tag = new StringBuilder();
            StringBuilder sb = new StringBuilder();

            foreach(char s in format)
            {
                if (mode == 0)
                {
                    if (s == '%')
                    {
                        tag.Clear();
                        mode = 1;
                    }
                    else
                    {
                        sb.Append(s);
                    }
                }
                else if (mode == 1)
                {
                    if (s == '%')
                    {
                        sb.Append('%');
                    }
                    else if (Char.IsLetter(s))
                    {
                        tag.Append(s);
                        mode = 0;
                        AddTag(sb, tag, ref c);
                    }
                    else
                    {
                        tag.Append(s);
                    }
                }
            }

            return sb.ToString();
        }

        public static Dictionary<string,string> map = new Dictionary<string,string>();

        public void AddTag(StringBuilder sb, StringBuilder tag, ref int idx)
        {
            if (!map.ContainsKey(tag.ToString()))
            {
                Form2 f = new Form2();
                f.OldTag = tag.ToString();
                f.ShowDialog();
                map.Add(tag.ToString(), f.NewTag);
            }

            sb.Append(map[tag.ToString()].Replace("_", idx.ToString()));
            idx++;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debugger.Log(0, "", "----------------\n");
            foreach (string s in map.Keys)
            {
                Debugger.Log(0, "", String.Format("    map.Add(\"{0}\", \"{1}\");\n", s, map[s]));
            }
            Debugger.Log(0, "", "----------------\n");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
            richTextBox1.Paste();
            richTextBox2.SelectAll();
            richTextBox2.Copy();
        }
    }
}
