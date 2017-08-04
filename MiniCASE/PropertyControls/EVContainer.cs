using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniCASE
{
    public partial class EVContainer : UserControl
    {
        public EVContainer()
        {
            InitializeComponent();
            tm.Interval = 1000;
            tm.Tick += new EventHandler(tm_Tick);
            Panels = new List<Control>();
        }

        ~EVContainer()
        {
            tm.Stop();
        }

        private Timer tm = new Timer();
        public UserControl EditView { get; set; }

        public List<Control> Panels;

        void tm_Tick(object sender, EventArgs e)
        {
            OnValueUpdatedImmediate();
            tm.Stop();
        }

        public void OnValueUpdated()
        {
            tm.Stop();
            tm.Start();
        }

        public void OnValueUpdatedImmediate()
        {
            if (EditView != null)
                EditView.Invalidate();
        }

        public int TotalPanelsHeight = 0;

        public void RecalculatePositions()
        {
            int x = 0;
            int childWidth = panel1.Width - 16;
            foreach (Control uc in Panels)
            {
                uc.Location = new Point(0, x);
                uc.Width = childWidth;
                uc.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                x += uc.Height + 2;
            }

            TotalPanelsHeight = x;
        }

        public void ClearPanels()
        {
            Debugger.Log(0, "", "------------------\nPanels Clear\n");
            Panels.Clear();
            panel1.Controls.Clear();
        }

        public void AddPanel(UserControl uc)
        {
            //panel1.Controls.Add(uc);
            Debugger.Log(0, "", "Panel add:" + uc.GetType().Name + "\n");
            Panels.Add(uc);

            RecalculatePositions();
        }

        public void CommitPanels()
        {
            Debugger.Log(0,"", "Panels:" + Panels.Count + "\n");
            panel1.Controls.AddRange(Panels.ToArray<Control>());
            RecalculatePositions();
            //panel1.AutoScrollPosition = Point.Empty;
            //panel1.Height = TotalPanelsHeight + 22;
        }

    }

    public interface IEVContainerChild
    {
        void SetName(string name, object value);
    }
}
