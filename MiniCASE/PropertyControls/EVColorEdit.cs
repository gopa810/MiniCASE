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
    public partial class EVColorEdit : UserControl, IEVContainerChild
    {
        public CDObject Object { get; set; }

        public string Key { get; set; }

        public EVContainer ContainerView { get; set; }

        public Color SelectedColor { get; set; }

        public void SetName(string name, object value)
        {
            label1.Text = name;
            if (value is Color)
            {
                SelectedColor = (Color)value;
                button1.BackColor = SelectedColor;
            }
        }

        public EVColorEdit()
        {
            InitializeComponent();
            ContainerView = null;
            Key = string.Empty;
            Object = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = SelectedColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                button1.BackColor = cd.Color;
                SelectedColor = cd.Color;

                if (Object != null)
                    Object.SetColor(Key, SelectedColor);

                if (ContainerView != null)
                    ContainerView.OnValueUpdatedImmediate();
            }
        }
    }
}
