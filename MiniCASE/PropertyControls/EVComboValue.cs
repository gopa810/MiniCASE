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
    public partial class EVComboValue : UserControl, IEVContainerChild
    {
        public CDObject Object { get; set; }

        public string Key { get; set; }

        public EVContainer ContainerView { get; set; }

        public string SelectedValue { get; set; }

        public CSParameterType SelectedValueType { get; set; }

        public void SetOptions(CSParameterType paramType, params object[] args)
        {
            SelectedValueType = paramType;
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(args);
        }

        public void AddOption(object obj)
        {
            comboBox1.Items.Add(obj);
        }

        public void SetName(string name, object value)
        {
            label1.Text = name;
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(value);
        }


        public EVComboValue()
        {
            InitializeComponent();
            ContainerView = null;
            Key = string.Empty;
            Object = null;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0 && comboBox1.SelectedIndex < comboBox1.Items.Count)
            {
                object SelectedObject = comboBox1.Items[comboBox1.SelectedIndex];
                SelectedValue = SelectedObject.ToString();

                if (Object != null)
                {
                    switch(SelectedValueType)
                    {
                        case CSParameterType.FontSize:
                            Object.SetFloat(Key, (float)SelectedObject);
                            break;
                        case CSParameterType.LineWidth:
                            Object.SetFloat(Key, (float)SelectedObject);
                            break;
                        case CSParameterType.LineCap:
                            Object.SetObject(Key, comboBox1.Items[comboBox1.SelectedIndex]);
                            break;
                        case CSParameterType.LinePattern:
                            Object.SetObject(Key, Enum.Parse(typeof(System.Drawing.Drawing2D.DashStyle), SelectedValue));
                            break;
                        case CSParameterType.LinePath:
                            Object.SetObject(Key, Enum.Parse(typeof(ConnectionsMode), SelectedValue));
                            break;
                    }
                }

                if (ContainerView != null)
                    ContainerView.OnValueUpdatedImmediate();
            }
        }
    }
}
