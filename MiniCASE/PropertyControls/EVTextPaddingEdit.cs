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
    public partial class EVTextPaddingEdit : UserControl, IEVContainerChild
    {
        public CDObject Object { get; set; }

        public string Key { get; set; }

        public EVContainer ContainerView { get; set; }

        public CSTextPadding SelectedPadding { get; set; }


        public EVTextPaddingEdit()
        {
            InitializeComponent();
        }

        public void SetName(string name, object value)
        {
            label1.Text = name;
            if (value is CSTextPadding)
            {
                SelectedPadding = (CSTextPadding)value;
                textBox1.Text = SelectedPadding.Top.ToString();
                textBox2.Text = SelectedPadding.Left.ToString();
                textBox3.Text = SelectedPadding.Right.ToString();
                textBox4.Text = SelectedPadding.Bottom.ToString();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out SelectedPadding.Top))
            {
                if (Object != null)
                    Object.SetTextPadding(Key, SelectedPadding);

                if (ContainerView != null)
                    ContainerView.OnValueUpdated();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox2.Text, out SelectedPadding.Left))
            {
                if (Object != null)
                    Object.SetTextPadding(Key, SelectedPadding);

                if (ContainerView != null)
                    ContainerView.OnValueUpdated();
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox3.Text, out SelectedPadding.Right))
            {
                if (Object != null)
                    Object.SetTextPadding(Key, SelectedPadding);

                if (ContainerView != null)
                    ContainerView.OnValueUpdated();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox4.Text, out SelectedPadding.Bottom))
            {
                if (Object != null)
                    Object.SetTextPadding(Key, SelectedPadding);

                if (ContainerView != null)
                    ContainerView.OnValueUpdated();
            }
        }
    }
}
