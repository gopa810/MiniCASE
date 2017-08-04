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
    public partial class EVTextAlignEdit : UserControl
    {
        public CDObject Object { get; set; }

        public string Key { get; set; }

        public EVContainer ContainerView { get; set; }

        public void SetName(string name, object value)
        {
            label1.Text = name;
        }


        public EVTextAlignEdit()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Object != null)
                Object.SetTextAlign(Key, ((Button)sender).Tag.ToString());

            if (ContainerView != null)
                ContainerView.OnValueUpdatedImmediate();
        }
    }
}
