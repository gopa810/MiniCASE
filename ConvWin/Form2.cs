using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConvWin
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public string OldTag
        {
            set
            {
                textBox1.Text = value;
            }
        }

        public string NewTag
        {
            get
            {
                return textBox2.Text;
            }
        }
    }
}
