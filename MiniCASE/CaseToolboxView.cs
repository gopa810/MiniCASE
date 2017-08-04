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
        private CDDocumentDefinition docDef = null;
        private CDContext mContext = new CDContext();

        public CaseToolboxView()
        {
            stringLeftMiddle.Alignment = StringAlignment.Near;
            stringLeftMiddle.LineAlignment = StringAlignment.Center;
            InitializeComponent();

            listBox1.Items.Clear();
            //listBox1.Items.AddRange(CDDocumentDefinition.Default.shapes.ToArray<CDShapeDefinition>());
        }

        private void CaseToolboxView_MouseDown(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.X, e.Y);
            if (index < 0 || index >= listBox1.Items.Count)
                return;

            CDShapeDefinition cs = listBox1.Items[index] as CDShapeDefinition;

            this.DoDragDrop(cs.Name, DragDropEffects.All);
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

        private CDShape p_drawingShape = new CDShape(null, null, Guid.Empty);

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (listBox1.Items.Count <= e.Index || e.Index < 0)
                return;

            if (e.State == DrawItemState.Selected)
                e.Graphics.FillRectangle(Brushes.LightYellow, e.Bounds);
            else
                e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);

            mContext.Graphics = e.Graphics;
            mContext.Offset = e.Bounds.Location;

            Rectangle rc = new Rectangle(4, 4, 30, 20);
            if (listBox1.Items[e.Index] is CDShapeDefinition)
            {
                p_drawingShape.Project = MiniCaseApp.Project;
                p_drawingShape.Definition = (CDShapeDefinition)listBox1.Items[e.Index];
                p_drawingShape.Bounds.Left = e.Bounds.X + 4;
                p_drawingShape.Bounds.Right = e.Bounds.X + 34;
                p_drawingShape.Bounds.Top = e.Bounds.Y + 4;
                p_drawingShape.Bounds.Bottom = e.Bounds.Y + 24;
                rc.Offset(e.Bounds.Location);

                CDGraphics.DrawShape(mContext, p_drawingShape, rc);
                rc.Width = e.Bounds.Width - 8;
                rc.X = e.Bounds.X + 44;
                rc.Y = e.Bounds.Y + 4;
                rc.Width = e.Bounds.Width - 44;
                rc.Height = e.Bounds.Height - 8;
                e.Graphics.DrawString(p_drawingShape.Definition.Description, SystemFonts.MenuFont, SystemBrushes.ControlText, rc, stringLeftMiddle);
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            CaseToolboxView_MouseDown(sender, e);
        }

        public void SetDiagramClass(CDDocumentDefinition dd)
        {
            docDef = dd;
            listBox1.Items.Clear();

            if (docDef != null)
            {
                foreach (CDShapeDefinition sd in docDef.shapes)
                {
                    if (sd != null && sd.UserVisible)
                        listBox1.Items.Add(sd);
                }
            }
        }

    }
}
