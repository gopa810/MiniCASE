using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace MiniCASE
{
    public partial class CaseDiagramView : UserControl
    {
        public enum ConnectionsMode
        {
            Straight,
            Rectangular,
            Path
        };

        public enum MouseMoveMode {
            None,
            SelectedShapeMove,
            OffsetMove,
            ConnectionMake,
            ReconnectStartPoint,
            ReconnectEndPoint
        };

        public event EventHandler<SelectedShapeEventArgs> SelectedShapeChanged;
        public event EventHandler<SelectedShapeEventArgs> MouseOverShape;

        private BufferedGraphicsContext context;
        private BufferedGraphics grafx;

        protected StringFormat centerStringFormat = new StringFormat();
        public CaseDiagram diagram = null;
        public Point offset = new Point(0,0);
        public double scale = 1.0;
        public DiagramDrawMatrix matrix = new DiagramDrawMatrix();
        public ConnectionsMode ConnectionDrawMode = ConnectionsMode.Straight;

        // tracking values
        private CaseShape lastMouseOverShape = null;
        private CaseShape lastSelectedShape = null;
        private CaseDiagramConnection lastSelectedConnection = null;
        private Point mouseButtonDownStartPoint;
        private Point mouseButtonDownStartOffset;
        private bool mouseButtonDown = false;
        private MouseMoveMode mouseMode = MouseMoveMode.None;
        private Point mouseLogicalConnStart = new Point();
        private Point mouseLogicalConnEnd = new Point();

        private bool key_control = false;
        private bool key_shift = false;

        public CaseDiagramView()
        {
            // initialization of module helper objects
            centerStringFormat.Alignment = StringAlignment.Center;
            centerStringFormat.LineAlignment = StringAlignment.Center;

            // initialize ui
            InitializeComponent();

            // init of diagram

            diagram = new CaseDiagram();

            CaseShape cs = new CaseShape();
            cs.Bounds = new RectangleD(50, 50, 130, 100);
            diagram.AddShape(cs);

            CaseShape cs2 = new CaseShape();
            cs2.Bounds = new RectangleD(150, 150, 240, 200);
            diagram.AddShape(cs2);

            CaseShape cs3 = new CaseShape();
            cs3.Bounds = new RectangleD(50, 90, 140, 140);
            diagram.AddShape(cs3);

            diagram.AddConnection(cs.id, cs2.id, 0);
            diagram.AddConnection(cs.id, cs3.id, 0);
            diagram.AddConnection(cs2.id, cs.id, 0);

            // ------------------------------------------
            // init of drawing helpers
            context = BufferedGraphicsManager.Current;

            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);

            grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));

            DrawToBuffer(grafx.Graphics);

            this.ClientSize = new Size(2000, 2000);
        }

        private void CaseDiagramView_Paint(object sender, PaintEventArgs e)
        {
            grafx.Render(e.Graphics);
        }

        private void CaseDiagramView_SizeChanged(object sender, EventArgs e)
        {
            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);
            if (grafx != null)
            {
                grafx.Dispose();
                grafx = null;
            }
            grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
            RedrawClientArea();
        }

        private void RedrawClientArea()
        {
            DrawToBuffer(grafx.Graphics);
            this.Refresh();
        }

        private void DrawToBuffer(Graphics g)
        {
            grafx.Graphics.FillRectangle(SystemBrushes.Control, 0, 0, this.Width, this.Height);
            if (diagram == null)
            {
                RectangleF rect = new RectangleF(new PointF(0, 0), new SizeF(this.Width, 48));

                g.DrawString("No Diagram", SystemFonts.MenuFont, Brushes.Black, rect, centerStringFormat);
                vScrollBar1.Visible = false;
                hScrollBar1.Visible = false;
            }
            else
            {
                Rectangle area = diagram.DiagramRect;
                if (area.Width < this.Width)
                {
                    hScrollBar1.Visible = false;
                }
                else
                {
                    hScrollBar1.Visible = true;
                    hScrollBar1.Minimum = area.Left;
                    hScrollBar1.Maximum = area.Right;
                }
                if (area.Height < this.Height)
                {
                    vScrollBar1.Visible = false;
                }
                else
                {
                    vScrollBar1.Visible = true;
                    vScrollBar1.Minimum = area.Top;
                    vScrollBar1.Maximum = area.Bottom;
                }
                DrawDiagramToBuffer(g);
            }
        }

        private void DrawDiagramToBuffer(Graphics g)
        {
            lastSelectedConnection = null;
            //
            // drawing connections
            //
            foreach (CaseDiagramConnection conn in diagram.ConnArray)
            {
                if (!conn.validCoordinates)
                {
                    CalculateConnectionsCoordinates();
                }

                if (conn.validCoordinates)
                {
                    Point p1;
                    Point p2 = new Point(0, 0);
                    int count = 0;
                    Pen pen = conn.selected ? Pens.DarkGreen : Pens.Black;
                    foreach (Point p in conn.coordinates)
                    {
                        if (count == 0)
                        {
                            p2 = p;
                        }
                        else
                        {
                            p1 = p2;
                            p2 = p;
                            g.DrawLine(pen, p1.X + offset.X, p1.Y + offset.Y, p2.X + offset.X, p2.Y + offset.Y);
                        }
                        count++;
                    }
                }

                if (conn.selected)
                    lastSelectedConnection = conn;
            }

            //
            // drawing shapes
            //
            foreach (CaseShape shape in diagram.ShapeArray)
            {
                ShapesLibrary.DrawShape(g, shape, offset);
            }

            if (mouseMode == MouseMoveMode.ConnectionMake || mouseMode == MouseMoveMode.ReconnectStartPoint
                || mouseMode == MouseMoveMode.ReconnectEndPoint)
            {
                g.DrawLine(Pens.Red, mouseLogicalConnStart.X + offset.X, mouseLogicalConnStart.Y + offset.Y,
                    mouseLogicalConnEnd.X + offset.X, mouseLogicalConnEnd.Y + offset.Y);
            }

            // drawing anchors for selected connection
            if (lastSelectedConnection != null)
            {
                if (lastSelectedConnection.coordinates.Length >= 4)
                {
                    Point a = lastSelectedConnection.coordinates[1];
                    g.FillRectangle(Brushes.Green, a.X + offset.X - 4, a.Y + offset.Y - 4, 8, 8);

                    a = lastSelectedConnection.coordinates[lastSelectedConnection.coordinates.Length - 2];
                    g.FillRectangle(Brushes.Green, a.X + offset.X - 4, a.Y + offset.Y - 4, 8, 8);
                }
            }

        }

        private void CalculateConnectionsCoordinates()
        {
            if (ConnectionDrawMode == ConnectionsMode.Straight)
            {
                foreach (CaseDiagramConnection conn in diagram.ConnArray)
                {
                    CaseShape shape1 = diagram.FindShape(conn.startId);
                    CaseShape shape2 = diagram.FindShape(conn.endId);

                    Point c1 = shape1.Bounds.CenterPoint;
                    Point c2 = shape2.Bounds.CenterPoint;

                    DiagramPath dp = new DiagramPath();
                    dp.areaPath = new Point[] { c1, c2 };
                    conn.path = dp;
                }
            }
            else if (ConnectionDrawMode == ConnectionsMode.Path)
            {
                matrix.InitStart();
                // dividing axis' to ranges
                foreach (CaseShape shape in diagram.ShapeArray)
                {
                    matrix.InsertHorizontalDivider(shape.Bounds.Xa);
                    matrix.InsertHorizontalDivider(shape.Bounds.Xb);
                    matrix.InsertVerticalDivider(shape.Bounds.Ya);
                    matrix.InsertVerticalDivider(shape.Bounds.Yb);
                }

                //matrix.LogRanges();

                // creating matrix of areas
                matrix.CreateMatrix();

                // assigning matrix areas to shapes
                foreach (CaseShape shape in diagram.ShapeArray)
                {
                    int r1, r2;
                    int c1, c2;
                    matrix.GetIndicesForRange(matrix.hRanges, shape.Bounds.Xa, shape.Bounds.Xb, out c1, out c2);
                    matrix.GetIndicesForRange(matrix.vRanges, shape.Bounds.Ya, shape.Bounds.Yb, out r1, out r2);

                    shape.matrixAreas.Clear();
                    for (int c = c1; c <= c2 && c >= 0; c++)
                    {
                        for (int r = r1; r <= r2 && r >= 0; r++)
                        {
                            matrix.aMatrix[c, r].shape = shape;
                            shape.matrixAreas.Add(new Point(c, r));
                        }
                    }
                }

                //matrix.LogAreas();

                // finding paths for connections
                foreach (CaseDiagramConnection conn in diagram.ConnArray)
                {
                    conn.path = matrix.FindPath(conn.startId, conn.endId);
                }

                // allocation of slots for lines
                matrix.ClearSlots();
                foreach (CaseDiagramConnection conn in diagram.ConnArray)
                {
                    matrix.AllocateSlots(conn.path.areaPath, conn);
                }


                matrix.RecalculateSlotsPositions();

                //matrix.LogSlots();

                foreach (CaseDiagramConnection conn in diagram.ConnArray)
                {
                    matrix.CalculatePoints(conn);
                }
            }
        }


        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void CaseDiagramView_MouseMove(object sender, MouseEventArgs e)
        {
            if (diagram == null)
                return;

            Cursor cursorToSet = null;

            if (mouseButtonDown)
            {
                //Debugger.Log(0, "", "Moved -- \n");
                if (mouseMode == MouseMoveMode.ConnectionMake)
                {
                    diagram.ClearHighlight();
                    if (lastSelectedShape != null)
                        lastSelectedShape.Highlighted = true;
                    CaseShape shape = matrix.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
                    if (shape != null)
                        shape.Highlighted = true;
                    mouseLogicalConnEnd.X = e.X - offset.X;
                    mouseLogicalConnEnd.Y = e.Y - offset.Y;
                }
                if (mouseMode == MouseMoveMode.SelectedShapeMove)
                {
                    Size moveOffset = new Size(e.X - mouseButtonDownStartPoint.X - offset.X, 
                        e.Y - mouseButtonDownStartPoint.Y - offset.Y);
                    diagram.SetSelectionOffset(moveOffset);
                    CalculateConnectionsCoordinates();
                    //diagram.InvalidateMovedShapesConnections();
                }
                else if (mouseMode == MouseMoveMode.OffsetMove)
                {
                    offset.X = /*mouseButtonDownStartOffset.X +*/ e.X - mouseButtonDownStartPoint.X;
                    offset.Y = /*mouseButtonDownStartOffset.Y +*/ e.Y - mouseButtonDownStartPoint.Y;
                }
                else if (mouseMode == MouseMoveMode.ReconnectStartPoint)
                {
                    if (lastSelectedConnection != null)
                    {
                        diagram.ClearHighlight();
                        CaseShape startShape = diagram.FindShape(lastSelectedConnection.endId);
                        if (startShape != null)
                            startShape.Highlighted = true;
                        CaseShape shape = matrix.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
                        if (shape != null)
                            shape.Highlighted = true;
                        mouseLogicalConnEnd.X = e.X - offset.X;
                        mouseLogicalConnEnd.Y = e.Y - offset.Y;
                    }
                }
                else if (mouseMode == MouseMoveMode.ReconnectEndPoint)
                {
                    diagram.ClearHighlight();
                    CaseShape startShape = diagram.FindShape(lastSelectedConnection.startId);
                    if (startShape != null)
                        startShape.Highlighted = true;
                    CaseShape shape = matrix.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
                    if (shape != null)
                        shape.Highlighted = true;
                    mouseLogicalConnEnd.X = e.X - offset.X;
                    mouseLogicalConnEnd.Y = e.Y - offset.Y;
                }
                RedrawClientArea();
            }
            else
            {
                cursorToSet = Cursors.Arrow;
                bool anchorStart;
                Point pt;
                CaseShape shape = matrix.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
                if (shape != lastMouseOverShape)
                {
                    if (shape != null)
                        Debugger.Log(0, "", string.Format("Found shape: {0}\n", shape.id));
                    else
                        Debugger.Log(0, "", "No shape is under cursor\n");
                    EventHandler<SelectedShapeEventArgs> handler = MouseOverShape;
                    if (handler != null)
                    {
                        handler(this, new SelectedShapeEventArgs(shape));
                    }

                    lastMouseOverShape = shape;
                }

                if (shape == null)
                {
                    CaseDiagramConnection conn = matrix.GetConnectionAtPoint(e.X - offset.X, e.Y - offset.Y);
                    if (conn != null)
                    {
                        Debugger.Log(0, "", string.Format("Found connection: {0} -> {1}\n", conn.startId, conn.endId));
                        cursorToSet = Cursors.Hand;
                    }
                    //else
                        //Debugger.Log(0, "", "Connection not found\n");
                }

                if (diagram.SelectedCount == 1 && lastSelectedShape != null && TestHitConnectionAnchor(lastSelectedShape.Bounds, e.X - offset.X, e.Y - offset.Y))
                {
                    cursorToSet = Cursors.Cross;
                }
                else if (lastSelectedConnection != null && TestHitConnectionAnchor2(lastSelectedConnection.coordinates, e.X - offset.X, e.Y - offset.Y, out anchorStart, out pt))
                {
                    cursorToSet = Cursors.Cross;
                }

                Cursor.Current = cursorToSet;
            }
        }

        private bool TestHitConnectionAnchor(RectangleD bnd, int eX, int eY)
        {
            Point pt = bnd.CenterPoint;
            return (PointNearPoint(pt.X, bnd.Ya, eX, eY, 4)
                 || PointNearPoint(pt.X, bnd.Yb, eX, eY, 4)
                 || PointNearPoint(bnd.Xa, pt.Y, eX, eY, 4)
                 || PointNearPoint(bnd.Xb, pt.Y, eX, eY, 4));
        }

        private bool TestHitConnectionAnchor2(Point[] coordinates, int eX, int eY, out bool startPoint, out Point pt)
        {
            if (coordinates == null || coordinates.Length < 4)
            {
                startPoint = false;
                pt = new Point();
                return false;
            }

            bool retval = false;
            Point start = coordinates[1];
            Point end = coordinates[coordinates.Length - 2];

            pt = start;
            startPoint = false;

            if (PointNearPoint(start.X, start.Y, eX, eY, 4))
            {
                Debugger.Log(0, "", "identified start\n");
                startPoint = true;
                pt = end;
                retval = true;
            }
            else if (PointNearPoint(end.X, end.Y, eX, eY, 4))
            {
                Debugger.Log(0, "", "identified end\n");
                startPoint = false;
                pt = start;
                retval = true;
            }


            return retval;
        }

        private bool PointNearPoint(int x1, int y1, int x2, int y2, int distance)
        {
            return (Math.Abs(x1 - x2) <= distance && Math.Abs(y1 - y2) <= distance);
        }

        private void CaseDiagramView_MouseUp(object sender, MouseEventArgs e)
        {
            mouseButtonDown = false;
            if (diagram != null)
            {
                if (mouseMode == MouseMoveMode.OffsetMove)
                {
                    diagram.ConfirmSelectionPosition();
                }
                else if (mouseMode == MouseMoveMode.SelectedShapeMove)
                {
                    diagram.ConfirmSelectionPosition();
                }
                else if (mouseMode == MouseMoveMode.ConnectionMake)
                {
                    CaseShape shape = matrix.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

                    if (shape != null && lastSelectedShape != null)
                    {
                        diagram.AddConnection(lastSelectedShape.id, shape.id, 0);
                    }
                    diagram.ClearHighlight();
                }
                else if (mouseMode == MouseMoveMode.ReconnectStartPoint)
                {
                    CaseShape shape = matrix.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

                    if (lastSelectedConnection != null && shape != null)
                    {
                        if (lastSelectedConnection.endId != shape.id)
                        {
                            lastSelectedConnection.startId = shape.id;
                        }
                        lastSelectedConnection.visible = true;
                        lastSelectedConnection.validCoordinates = false;
                    }
                    diagram.ClearHighlight();
                }
                else if (mouseMode == MouseMoveMode.ReconnectEndPoint)
                {
                    CaseShape shape = matrix.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

                    if (lastSelectedConnection != null && shape != null)
                    {
                        if (lastSelectedConnection.startId != shape.id)
                        {
                            lastSelectedConnection.endId = shape.id;
                        }
                        lastSelectedConnection.visible = true;
                        lastSelectedConnection.validCoordinates = false;
                    }
                    diagram.ClearHighlight();
                }

                mouseMode = MouseMoveMode.None;
                RedrawClientArea();
            }


        }

        private void CaseDiagramView_MouseLeave(object sender, EventArgs e)
        {

        }

        private void CaseDiagramView_MouseHover(object sender, EventArgs e)
        {

        }

        private void CaseDiagramView_MouseDown(object sender, MouseEventArgs e)
        {
            bool connAnchorStart = false;

            mouseButtonDown = true;
            mouseButtonDownStartPoint = new Point(e.X-offset.X, e.Y-offset.Y);
            mouseButtonDownStartOffset = new Point(offset.X, offset.Y);
            mouseMode = MouseMoveMode.None;

            if (lastSelectedShape != null && TestHitConnectionAnchor(lastSelectedShape.Bounds, e.X-offset.X, e.Y-offset.Y))
            {
                mouseMode = MouseMoveMode.ConnectionMake;
                mouseLogicalConnStart.X = e.X - offset.X;
                mouseLogicalConnStart.Y = e.Y - offset.Y;
            }
            else if (lastSelectedConnection != null && TestHitConnectionAnchor2(lastSelectedConnection.coordinates, e.X - offset.X, e.Y - offset.Y, out connAnchorStart, out mouseLogicalConnStart))
            {
                lastSelectedConnection.visible = false;
                if (connAnchorStart)
                {
                    mouseMode = MouseMoveMode.ReconnectStartPoint;
                }
                else
                {
                    mouseMode = MouseMoveMode.ReconnectEndPoint;
                }
            }
            else if (diagram != null)
            {
                CaseShape shape = matrix.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
                lastSelectedShape = shape;

                if (shape == null)
                {
                    if (!key_control)
                    {
                        diagram.ClearSelection();
                    }

                    CaseDiagramConnection conn = matrix.GetConnectionAtPoint(e.X - offset.X, e.Y - offset.Y);
                    if (conn != null)
                    {
                        diagram.ClearSelection();
                        conn.selected = true;
                    }
                }
                else
                {
                    if (key_control)
                    {
                        shape.Selected = !shape.Selected;
                    }
                    else
                    {
                        diagram.ClearSelection();
                        if (shape.Selected == false)
                            diagram.ClearSelection();
                        shape.Selected = true;
                    }
                }

                diagram.SaveSelectionPosition();
                if (diagram.SelectedCount > 0 && mouseMode == MouseMoveMode.None)
                    mouseMode = MouseMoveMode.SelectedShapeMove;
                else
                    mouseMode = MouseMoveMode.OffsetMove;
                RedrawClientArea();
            }
        }

        private void CaseDiagramView_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void CaseDiagramView_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void CaseDiagramView_MouseCaptureChanged(object sender, EventArgs e)
        {

        }

        private void CaseDiagramView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Control)
                key_control = true;
            if (e.KeyCode == Keys.Shift)
                key_shift = true;
        }

        private void CaseDiagramView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Control)
                key_control = false;
            if (e.KeyCode == Keys.Shift)
                key_shift = false;
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                diagram.DeleteSelection();
                RedrawClientArea();
            }
        }

        private void CaseDiagramView_DragDrop(object sender, DragEventArgs e)
        {
            string str = "";
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                str = (string)e.Data.GetData(DataFormats.Text);
            }
            if (str.StartsWith("MiniCase.") && diagram != null)
            {
                if (str.StartsWith("MiniCase.Shape."))
                {
                    CaseShape cs2 = new CaseShape();
                    cs2.ShapeType = str;
                    Point screenPoint = new Point(e.X, e.Y);
                    Point viewPoint = this.PointToClient(screenPoint);
                    cs2.Bounds = new RectangleD(viewPoint.X - 50, viewPoint.Y - 30, viewPoint.X + 50, viewPoint.Y + 30);
                    diagram.AddShape(cs2);
                    CalculateConnectionsCoordinates();
                    RedrawClientArea();
                }
            }
        }

        private void CaseDiagramView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

    }


    public class SelectedShapeEventArgs : EventArgs
    {
        public SelectedShapeEventArgs(CaseShape aShape)
        {
            shape = aShape;
        }
        private CaseShape shape;

        public CaseShape Shape
        {
            get { return shape; }
            set { shape = value; }
        }
    }

}
