using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MiniCASE
{
    public partial class CaseToolboxView : UserControl
    {
        private StringFormat stringLeftMiddle = new StringFormat();

        public CaseToolboxView()
        {
            stringLeftMiddle.Alignment = StringAlignment.Near;
            stringLeftMiddle.LineAlignment = StringAlignment.Center;
            InitializeComponent();

            CaseShape[] allShapes = ShapesLibrary.CreateAllShapes();

            listBox1.Items.Clear();
            listBox1.Items.AddRange(allShapes);
        }

        private void CaseToolboxView_MouseDown(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.X, e.Y);
            if (index < 0 || index >= listBox1.Items.Count)
                return;

            CaseShape cs = listBox1.Items[index] as CaseShape;

            this.DoDragDrop(cs.ShapeReference, DragDropEffects.All);
        }

        private void CaseToolboxView_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void CaseToolboxView_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void CaseToolboxView_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 28;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (listBox1.Items.Count <= e.Index || e.Index < 0)
                return;

            if (e.State == DrawItemState.Selected)
                e.Graphics.FillRectangle(Brushes.LightYellow, e.Bounds);
            else
                e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);

            Point offset = new Point(0, 0);
            Rectangle rc = new Rectangle(4, 4, 30, 20);
            if (listBox1.Items[e.Index] is CaseShape)
            {
                CaseShape sd = listBox1.Items[e.Index] as CaseShape;
                sd.Bounds.Xa = 4;
                sd.Bounds.Xb = 34;
                sd.Bounds.Ya = 4;
                sd.Bounds.Yb = 24;
                ShapesLibrary.DrawShape(e.Graphics, sd, offset);
                rc.Width = e.Bounds.Width - 8;
                rc.X = e.Bounds.X + 44;
                rc.Y = e.Bounds.Y + 4;
                rc.Width = e.Bounds.Width - 44;
                rc.Height = e.Bounds.Height - 8;
                e.Graphics.DrawString(ShapesLibrary.Description(sd.ShapeReference), SystemFonts.MenuFont, SystemBrushes.ControlText, rc, stringLeftMiddle);
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            CaseToolboxView_MouseDown(sender, e);
        }
    }
}
