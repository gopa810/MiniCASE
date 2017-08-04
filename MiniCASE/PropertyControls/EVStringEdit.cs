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
    public partial class EVStringEdit : UserControl, IEVContainerChild
    {
        public CDObject Object { get; set; }

        public string Key { get; set; }

        public EVContainer ContainerView { get; set; }

        public EVStringEdit()
        {
            InitializeComponent();

            ContainerView = null;
            Key = string.Empty;
            Object = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (Object != null)
                Object.SetString(Key, textBox1.Text);

            if (ContainerView != null)
                ContainerView.OnValueUpdated();
        }

        public void SetName(string name, object value)
        {
            label1.Text = name;
            textBox1.Text = value.ToString();
        }
    }
}
