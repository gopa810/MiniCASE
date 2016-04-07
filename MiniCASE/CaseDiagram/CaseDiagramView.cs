using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        //public DiagramDrawMatrix matrix = new DiagramDrawMatrix();
        public ConnectionsMode ConnectionDrawMode = ConnectionsMode.Path;

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
            cs2.ShapeBase = ShapeBase.Rectangle;
            cs2.Bounds = new RectangleD(150, 200, 230, 250);
            diagram.AddShape(cs2);

            CaseShape cs3 = new CaseShape();
            cs3.Bounds = new RectangleD(250, 90, 340, 140);
            diagram.AddShape(cs3);

            CaseShape cs4 = new CaseShape();
            cs4.Bounds = new RectangleD(20, 250, 220, 340);
            diagram.AddShape(cs4);

            //diagram.AddConnection(cs.id, cs2.id, 0);
            CaseDiagramConnection conn = diagram.AddConnection(cs.id, cs3.id);
            conn.EndCap = 2;
            //diagram.AddConnection(cs2.id, cs3.id, 0);

            // ------------------------------------------
            // init of drawing helpers
            context = BufferedGraphicsManager.Current;

            context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);

            grafx = context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));

            DrawToBuffer(grafx.Graphics);

            this.ClientSize = new Size(2000, 2000);

            cs2.ShapeBase = ShapeBase.Pico;
            Point tp = cs2.GetBorderPoint(110, 170);
            tp = cs2.GetBorderPoint(270, 170);
            tp = cs2.GetBorderPoint(270, 270);
            tp = cs2.GetBorderPoint(110, 270);
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

            foreach (CaseDiagramConnection conn in diagram.ConnArray)
            {
                if (!conn.validCoordinates)
                {
                    CalculateConnectionsCoordinates();
                }

                if (conn.validCoordinates)
                {
                    conn.DrawConnection(g, offset);
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

            if (mouseMode == MouseMoveMode.ConnectionMake 
                || mouseMode == MouseMoveMode.ReconnectStartPoint
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

        public class Electron
        {
            public double X;
            public double Y;

            public double dx;
            public double dy;

            public double dxop;
            public double dyop;

            public void ClearPower()
            {
                //dx = 0;
                //dy = 0;
                dxop = 0;
                dyop = 0;
            }

            public Point Point
            {
                get
                {
                    return new Point(Convert.ToInt32(X), Convert.ToInt32(Y));
                }
                set
                {
                    X = value.X;
                    Y = value.Y;
                }
            }

            /// <summary>
            /// Set target shape for this line
            /// </summary>
            /// <param name="sh"></param>
            public void AddTarget(CaseShape sh)
            {
                Point pt = sh.Bounds.CenterPoint;
                double diffx = pt.X - X;
                double diffy = pt.Y - Y;
                SetVectorLength(ref diffx, ref diffy, 1);
                dx = diffx;
                dy = diffy;
            }

            /// <summary>
            /// Add vector of opposing power from the opponent shape
            /// </summary>
            /// <param name="sh">Opponent shape</param>
            public void AddOpponent(CaseShape sh)
            {
                Point pt = sh.Bounds.CenterPoint;
                double A = dx;
                double B = dy;
                double x1 = X;
                double y1 = Y;
                double x2 = pt.X;
                double y2 = pt.Y;

                double w = (B * (x2 - x1) - A * (y2 - y1)) / (A * A + B * B);
                double v = (y2 - y1 + A * w) / B;

                // X3,Y3 is the point on the line between source and target object
                // which is nearest to the oponent object sh
                double x3 = x1 + A * v;
                double y3 = y1 + B * v;

                double V = GetSquareRoot(x3 - x2, y3 - y2);
                double R = GetSquareRoot(sh.Bounds.Width / 2, sh.Bounds.Height / 2) * 1.3;
                double S = GetSquareRoot(x3 - X, y3 - Y);

                // omega is strength of oposing power in the X3,Y3 point
                // <0, R)
                double omega = R - V;

                if (omega <= 0.0)
                    return;

                double sigma = 2 * omega;

                if (S > sigma || sigma < 0.1)
                    return;

                double mag = (Math.Cos(Math.PI / sigma * S) + 1) / 2 * omega;

                double diffx = x3 - x2;
                double diffy = y3 - y2;

                SetVectorLength(ref diffx, ref diffy, mag);

                dxop += diffx;
                dyop += diffy;
            }
            
            private void SetVectorLength(ref double ddx, ref double ddy, double length)
            {
                double d = GetSquareRoot(ddx, ddy);
                if (d != 0.0)
                {
                    ddx = ddx / d * length;
                    ddy = ddy / d * length;
                }
            }

            private double GetSquareRoot(double a, double b)
            {
                return Math.Sqrt(a * a + b * b);
            }

            public double DistanceFrom(Point point)
            {
                return GetSquareRoot(point.X - X, point.Y - Y);
            }

            public void Resolve(int i, int count)
            {
                double ret = (i < 5 ? i * 0.2 : 1.0) * (i >= count - 5 ? (count - i) * 0.2 : 1.0);

                dxop *= ret;
                dyop *= ret;

                X += dxop;
                Y += dyop;
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
                Rectangle rc = diagram.DiagramRect;

                // finding paths for connections
                foreach (CaseDiagramConnection conn in diagram.ConnArray)
                {
                    CaseShape ss = diagram.FindShape(conn.startId);
                    CaseShape es = diagram.FindShape(conn.endId);

                    List<Point> lp = CalculateConnectionPoints(ss, es);

                    conn.coordinates = lp.ToArray<Point>();
                    conn.validCoordinates = true;
                }
            }
        }

        private List<Point> CalculateConnectionPoints(CaseShape ss, CaseShape es)
        {
            Point p1 = ss.Bounds.CenterPoint;
            Point p2 = es.Bounds.CenterPoint;
            double length = CaseShape.GetDistance(p1, p2);
            int count = Convert.ToInt32(length / 4);
            double dx = Convert.ToDouble(p2.X - p1.X) / count;
            double dy = Convert.ToDouble(p2.Y - p1.Y) / count;

            List<Point> lp = new List<Point>();
            Electron e = new Electron();
            e.Point = p1;
            e.AddTarget(es);

            //Debugger.Log(0, "", "-- line --\n");
            for (int i = 0; i < count; i++)
            {
                e.X = p1.X + i * dx;
                e.Y = p1.Y + i * dy;
                e.ClearPower();
                foreach (CaseShape sh in diagram.ShapeArray)
                {
                    if (sh.id != ss.id && sh.id != es.id)
                    {
                        e.AddOpponent(sh);
                    }
                }
                e.Resolve(i, count);
                Point np = e.Point;
                if (lp.Count > 0)
                {
                    if (!es.ContainsPoint(np))
                        lp.Add(np);
                    else
                        break;
                }
                else if (!ss.ContainsPoint(np))
                {
                    lp.Add(np);
                }
                //lp.Add(e.Point);
            }
            return lp;
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
                    CaseShape shape = diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
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
                        CaseShape shape = diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
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
                    CaseShape shape = diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
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
                CaseShape shape = diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
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
                    CaseDiagramConnection conn = diagram.GetConnectionAtPoint(e.X - offset.X, e.Y - offset.Y);
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
                    CaseShape shape = diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

                    if (shape != null && lastSelectedShape != null)
                    {
                        CaseDiagramConnection conn = diagram.AddConnection(lastSelectedShape.id, shape.id);
                        conn.EndCap = 2;
                    }
                    diagram.ClearHighlight();
                }
                else if (mouseMode == MouseMoveMode.ReconnectStartPoint)
                {
                    CaseShape shape = diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

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
                    CaseShape shape = diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

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
                CaseShape shape = diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);
                lastSelectedShape = shape;

                if (shape == null)
                {
                    if (!key_control)
                    {
                        diagram.ClearSelection();
                    }

                    CaseDiagramConnection conn = diagram.GetConnectionAtPoint(e.X - offset.X, e.Y - offset.Y);
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
                    cs2.ShapeReference = str;
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
