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
        public enum MouseMoveMode {
            None,
            SelectedShapeMove,
            OffsetMove,
            ConnectionMake,
            ReconnectStartPoint,
            ReconnectEndPoint,
            ResizeBorder,
            SelectArea
        };

        public event EventHandler<SelectedObjectsEventArgs> SelectedDiagramObjectsChanged;
        public event EventHandler<SelectedObjectsEventArgs> MouseOverShape;

        protected StringFormat centerStringFormat = new StringFormat();

        public CDContext mContext = new CDContext();

        private CDDiagram p_diagram = null;
        public CDDiagram Diagram 
        {
            get
            {
                return p_diagram;
            }
            set
            {
                if (p_diagram != null)
                    p_diagram.SetDelegate(null);
                p_diagram = value;
                if (p_diagram != null)
                {
                    BackColor = p_diagram.GetColor("BackColor");
                }
                else
                {
                    BackColor = SystemColors.Window;
                }
                Invalidate();
            }
        }
        public Point offset = new Point(0,0);
        public float scale = 1.0f;

        // tracking values
        private CDShape lastMouseOverShape = null;
        private CDShape lastSelectedShape = null;
        private CDConnection lastSelectedConnection = null;
        private Point mouseButtonDownStartOffset;
        private ShapeAnchor mouseObjectAnchorDown;
        private bool mouseButtonDown = false;
        private MouseMoveMode mouseMode = MouseMoveMode.None;
        private Point mouseLogicalConnStart = new Point();
        private Point mouseLogicalConnEnd = new Point();
        private Point mouseRelativeConnOffsetStart = new Point();
        private Point mouseRelativeConnOffsetEnd = new Point();

        private bool key_control = false;
        private bool key_shift = false;
        private bool key_c = false;
        private bool key_s = false;

        public CaseDiagramView()
        {
            // initialization of module helper objects
            centerStringFormat.Alignment = StringAlignment.Center;
            centerStringFormat.LineAlignment = StringAlignment.Center;

            // initialize ui
            InitializeComponent();

            mContext.IconConnectionStart = Properties.Resources.CDConnection;
            mContext.IconResize = Properties.Resources.CDResize;
            // ------------------------------------------
            // init of drawing helpers
            this.ClientSize = new Size(2000, 2000);

            this.MouseWheel += new MouseEventHandler(CaseDiagramView_MouseWheel);
        }

        public void ClearSelection()
        {
            MiniCaseApp.SelectedShape = null;
            MiniCaseApp.SelectedConnection = null;

            lastSelectedConnection = null;
            lastSelectedShape = null;
            if (p_diagram != null)
            {
                p_diagram.ClearSelection();
            }
        }

        public void CaseDiagramView_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                scale *= 0.8f;
                Invalidate();
            }
            else if (e.Delta > 0)
            {
                scale *= 1.25f;
                Invalidate();
            }
        }

        private void CaseDiagramView_Paint(object sender, PaintEventArgs e)
        {
            DrawToBuffer(e.Graphics);
        }

        private void CaseDiagramView_SizeChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void RedrawClientArea()
        {
            Invalidate();
        }

        private void DrawToBuffer(Graphics g)
        {
            mContext.Graphics = g;

            //grafx.Graphics.FillRectangle(SystemBrushes.Control, 0, 0, this.Width, this.Height);
            if (Diagram == null)
            {
                RectangleF rect = new RectangleF(new PointF(0, 0), new SizeF(this.Width, 48));

                g.DrawString("No Diagram", SystemFonts.MenuFont, Brushes.Black, rect, centerStringFormat);
                vScrollBar1.Visible = false;
                hScrollBar1.Visible = false;
            }
            else
            {
                Rectangle area = Diagram.DiagramRect;
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

            mContext.Offset = offset;
            mContext.Scale = scale;
            mContext.Diagram = Diagram;

            g.TranslateTransform(offset.X, offset.Y);
            g.ScaleTransform(scale, scale);

            //
            // check existence of groups
            //
            Diagram.CheckExistenceGroups();

            //
            // recalculate bounds of groups
            // (in reverse direction, because we need child group 
            // bounds for calculation of parent group bounds
            //
            Diagram.RecalculateChangedGroupBounds();

            //
            // recaklc changed connections due to move or creation
            //
            Diagram.RecalculateChangedConnections();

            //
            // drawing groups
            //
            foreach (CDShape grp in Diagram.GroupArray)
            {
                if (grp.Selected)
                    mContext.Graphics.DrawRectangle(mContext.SelectionPen, grp.Bounds.Rectangle);
                CDGraphics.DrawShape(mContext, grp);
            }

            //
            // draw selection background
            foreach (CDShape shape in Diagram.ShapeArray)
            {
                if (shape.Selected)
                    mContext.Graphics.DrawRectangle(mContext.SelectionPen, shape.Bounds.Rectangle);
            }

            foreach (CDConnection conn in Diagram.ConnArray)
            {
                if (conn.selected && conn.validCoordinates)
                    conn.DrawConnectionBackground(mContext);
            }

            //
            // draw connections
            //
            lastSelectedConnection = null;
            foreach (CDConnection conn in Diagram.ConnArray)
            {
                if (conn.selected)
                {
                    lastSelectedConnection = conn;
                }
                else
                {
                    if (conn.validCoordinates)
                    {
                        conn.DrawConnection(mContext);
                    }
                }
            }


            //
            // drawing shapes
            //
            foreach (CDShape shape in Diagram.ShapeArray)
            {
                CDGraphics.DrawShape(mContext, shape);

                if (shape.Selected)
                    CDGraphics.DrawSelectedMarks(mContext, shape);
            }

            if (mouseMode == MouseMoveMode.ConnectionMake)
            {
                g.DrawLine(Pens.Red, mouseLogicalConnStart.X, mouseLogicalConnStart.Y,
                    mouseLogicalConnEnd.X, mouseLogicalConnEnd.Y);
            }
            else if (mouseMode == MouseMoveMode.ReconnectStartPoint && lastSelectedConnection != null && !lastSelectedConnection.visible)
            {
                g.DrawLine(Pens.Red, mouseLogicalConnStart.X, mouseLogicalConnStart.Y,
                    mouseLogicalConnEnd.X, mouseLogicalConnEnd.Y);
            }
            else if (mouseMode == MouseMoveMode.ReconnectEndPoint && lastSelectedConnection != null && !lastSelectedConnection.visible)
            {
                g.DrawLine(Pens.Red, mouseLogicalConnStart.X, mouseLogicalConnStart.Y,
                    mouseLogicalConnEnd.X, mouseLogicalConnEnd.Y);
            }

            // drawing anchors for selected connection
            if (lastSelectedConnection != null && lastSelectedConnection.visible)
            {
                lastSelectedConnection.DrawConnection(mContext);
            }

            if (mouseMode == MouseMoveMode.SelectArea)
            {
                g.DrawRectangle(Pens.Gray, Math.Min(mContext.StartPoint.X, mContext.MovePoint.X),
                    Math.Min(mContext.StartPoint.Y, mContext.MovePoint.Y),
                    Math.Abs(mContext.StartPoint.X - mContext.MovePoint.X),
                    Math.Abs(mContext.MovePoint.Y - mContext.StartPoint.Y));
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
            if (Diagram == null)
                return;

            //Cursor cursorToSet = null;
            mContext.TrackedItem = null;
            mContext.MovePointP.X = e.X;
            mContext.MovePointP.Y = e.Y;
            mContext.MovePoint.X = e.X - offset.X;
            mContext.MovePoint.Y = e.Y - offset.Y;

            if (mouseMode == MouseMoveMode.ConnectionMake)
            {
                Diagram.ClearHighlight();
                if (lastSelectedShape != null)
                    lastSelectedShape.Highlighted = true;
                CDShape shape = Diagram.GetShapeAtPoint(mContext.MovePoint.X, mContext.MovePoint.Y);
                if (shape != null && shape != lastSelectedShape)
                {
                    mContext.TrackedItem = shape;
                    shape.Highlighted = true;
                }
                mouseLogicalConnEnd = mContext.MovePoint;
                Invalidate();
            }
            else if (mouseButtonDown)
            {
                //Debugger.Log(0, "", "Moved -- \n");
                if (mouseMode == MouseMoveMode.SelectArea)
                {
                    RectangleD rd = new RectangleD();
                    rd.Left = Math.Min(mContext.StartPoint.X, mContext.MovePoint.X);
                    rd.Right = Math.Max(mContext.StartPoint.X, mContext.MovePoint.X);
                    rd.Top = Math.Min(mContext.StartPoint.Y, mContext.MovePoint.Y);
                    rd.Bottom = Math.Max(mContext.StartPoint.Y, mContext.MovePoint.Y);
                    foreach (CDShape shape in Diagram.ShapeArray)
                    {
                        shape.Selected = shape.Bounds.IntersectsWith(rd);
                    }
                }
                else if (mouseMode == MouseMoveMode.SelectedShapeMove)
                {
                    Size moveOffset = new Size(mContext.MovePoint.X - mContext.StartPoint.X,
                        mContext.MovePoint.Y - mContext.StartPoint.Y);
                    if (lastSelectedShape != null && lastSelectedShape.ShapeDefinition.AutomaticBounds)
                    {
                        Diagram.SetChildrenOffset(lastSelectedShape.GetString("GroupName"), moveOffset);
                        lastSelectedShape.FlagNeedsRecalculateBounds = true;
                    }
                    else
                    {
                        Diagram.SetSelectionOffset(moveOffset);
                        Diagram.InvalidateSelectedShapesGroupBoundaries();
                    }
                    //CalculateConnectionsCoordinates();
                }
                else if (mouseMode == MouseMoveMode.OffsetMove)
                {
                    offset.X = mContext.mouseOffsetBackup.X + (mContext.MovePointP.X - mContext.StartPointP.X);
                    offset.Y = mContext.mouseOffsetBackup.Y + (mContext.MovePointP.Y - mContext.StartPointP.Y);
                }
                else if (mouseMode == MouseMoveMode.ReconnectStartPoint)
                {
                    if (lastSelectedConnection != null)
                    {
                        Diagram.ClearHighlight();
                        CDShape startShape = lastSelectedConnection.EndShape;
                        if (startShape != null)
                            startShape.Highlighted = true;
                        CDShape shape = Diagram.GetShapeAtPoint(mContext.MovePoint.X, mContext.MovePoint.Y);
                        if (shape != null)
                        {
                            mContext.TrackedItem = shape;
                            shape.Highlighted = true;
                            lastSelectedConnection.StartShape = shape;
                            lastSelectedConnection.visible = true;
                            lastSelectedConnection.startRelOffset = shape.Bounds.GetRelativePoint(mContext.MovePoint);
                            lastSelectedConnection.validCoordinates = false;
                        }
                        else
                        {
                            lastSelectedConnection.visible = false;
                        }
                        mouseLogicalConnEnd = mContext.MovePoint;
                    }
                }
                else if (mouseMode == MouseMoveMode.ReconnectEndPoint)
                {
                    if (lastSelectedConnection != null)
                    {
                        Diagram.ClearHighlight();
                        CDShape startShape = lastSelectedConnection.StartShape;
                        if (startShape != null)
                            startShape.Highlighted = true;
                        CDShape shape = Diagram.GetShapeAtPoint(mContext.MovePoint.X, mContext.MovePoint.Y);
                        if (shape != null)
                        {
                            mContext.TrackedItem = shape;
                            shape.Highlighted = true;
                            lastSelectedConnection.EndShape = shape;
                            lastSelectedConnection.visible = true;
                            lastSelectedConnection.endRelOffset = shape.Bounds.GetRelativePoint(mContext.MovePoint);
                            lastSelectedConnection.validCoordinates = false;
                        }
                        else
                        {
                            lastSelectedConnection.visible = false;
                        }
                        mouseLogicalConnEnd.X = e.X - offset.X;
                        mouseLogicalConnEnd.Y = e.Y - offset.Y;
                    }
                }
                else if (mouseMode == MouseMoveMode.ResizeBorder && lastSelectedShape != null)
                {
                    switch (mouseObjectAnchorDown)
                    {
                        case ShapeAnchor.Top:
                            lastSelectedShape.Bounds.Top = mContext.MovePoint.Y - mContext.hitOffset.Y;
                            break;
                        case ShapeAnchor.Bottom:
                            lastSelectedShape.Bounds.Bottom = mContext.MovePoint.Y - mContext.hitOffset.Y;
                            break;
                        case ShapeAnchor.Left:
                            lastSelectedShape.Bounds.Left = mContext.MovePoint.X - mContext.hitOffset.X;
                            break;
                        case ShapeAnchor.Right:
                            lastSelectedShape.Bounds.Right = mContext.MovePoint.X - mContext.hitOffset.X;
                            break;
                        case ShapeAnchor.TopLeft:
                            lastSelectedShape.Bounds.Top = mContext.MovePoint.Y - mContext.hitOffset.Y;
                            lastSelectedShape.Bounds.Left = mContext.MovePoint.X - mContext.hitOffset.X;
                            break;
                        case ShapeAnchor.TopRight:
                            lastSelectedShape.Bounds.Top = mContext.MovePoint.Y - mContext.hitOffset.Y;
                            lastSelectedShape.Bounds.Right = mContext.MovePoint.X - mContext.hitOffset.X;
                            break;
                        case ShapeAnchor.BottomLeft:
                            lastSelectedShape.Bounds.Bottom = mContext.MovePoint.Y - mContext.hitOffset.Y;
                            lastSelectedShape.Bounds.Left = mContext.MovePoint.X - mContext.hitOffset.X;
                            break;
                        case ShapeAnchor.BottomRight:
                            lastSelectedShape.Bounds.Bottom = mContext.MovePoint.Y - mContext.hitOffset.Y;
                            lastSelectedShape.Bounds.Right = mContext.MovePoint.X - mContext.hitOffset.X;
                            break;
                        default:
                            break;
                    }
                }
                RedrawClientArea();
            }
            else
            {
                /*
                cursorToSet = Cursors.Arrow;
                bool anchorStart;
                Point pt;
                CDShape shape = Diagram.GetShapeAtPoint(mContext.MovePoint.X, mContext.MovePoint.Y);
                if (shape != lastMouseOverShape)
                {
                    if (shape != null)
                        Debugger.Log(0, "", string.Format("Found shape: {0}\n", shape.id));
                    else
                        Debugger.Log(0, "", "No shape is under cursor\n");
                    EventHandler<SelectedObjectsEventArgs> handler = MouseOverShape;
                    if (handler != null)
                    {
                        handler(this, new SelectedObjectsEventArgs(shape));
                    }

                    lastMouseOverShape = shape;
                }

                if (shape == null)
                {
                    CDConnection conn = Diagram.GetConnectionAtPoint(mContext.MovePoint.X, mContext.MovePoint.Y);
                    if (conn != null)
                    {
                        Debugger.Log(0, "", string.Format("Found connection: {0} -> {1}\n", conn.startId, conn.endId));
                        cursorToSet = Cursors.Hand;
                    }
                    //else
                        //Debugger.Log(0, "", "Connection not found\n");
                }

                ShapeAnchor hitA;
                if (Diagram.SelectedCount == 1 && lastSelectedShape != null)
                {
                    hitA = TestHitConnectionAnchor(lastSelectedShape, mContext.MovePoint.X, mContext.MovePoint.Y);
                    switch (hitA)
                    {
                        case ShapeAnchor.Left:
                        case ShapeAnchor.Right:
                            cursorToSet = Cursors.SizeWE;
                            break;
                        case ShapeAnchor.Top:
                        case ShapeAnchor.Bottom:
                            cursorToSet = Cursors.SizeNS;
                            break;
                        case ShapeAnchor.TopLeft:
                        case ShapeAnchor.BottomRight:
                            cursorToSet = Cursors.SizeNWSE;
                            break;
                        case ShapeAnchor.TopRight:
                        case ShapeAnchor.BottomLeft:
                            cursorToSet = Cursors.SizeNESW;
                            break;
                        case ShapeAnchor.BottomCenter:
                        case ShapeAnchor.CenterLeft:
                        case ShapeAnchor.CenterRight:
                        case ShapeAnchor.TopCenter:
                            cursorToSet = Cursors.Cross;
                            break;
                        case ShapeAnchor.Whole:
                            cursorToSet = Cursors.SizeAll;
                            break;
                        default:
                            cursorToSet = Cursors.Default;
                            break;
                    }
                    mouseObjectAnchorDown = hitA;
                }
                else if (lastSelectedConnection != null && TestHitConnectionAnchor2(lastSelectedConnection.coordinates, mContext.MovePoint.X, mContext.MovePoint.Y, out anchorStart, out pt))
                {
                    cursorToSet = Cursors.Cross;
                }

                Cursor.Current = cursorToSet;*/
            }
        }

        private ShapeAnchor TestHitConnectionAnchor(CDShape shape, int eX, int eY)
        {
            if (mContext.mouseAreaResizeBottomRight.Contains(eX, eY))
            {
                mContext.hitOffset.X = eX - mContext.mouseAreaResizeBottomRight.X;
                mContext.hitOffset.Y = eY - mContext.mouseAreaResizeBottomRight.Y;
                return ShapeAnchor.BottomRight;
            }
            if (mContext.mouseAreaResizeTopLeft.Contains(eX, eY))
            {
                mContext.hitOffset.X = eX - mContext.mouseAreaResizeTopLeft.Right - 4;
                mContext.hitOffset.Y = eY - mContext.mouseAreaResizeTopLeft.Bottom - 4;
                return ShapeAnchor.TopLeft;
            }
            if (shape.ContainsPoint(eX, eY))
            {
                mContext.hitOffset.X = eX - shape.Bounds.Left;
                mContext.hitOffset.Y = eY - shape.Bounds.Top;
                return ShapeAnchor.Whole;
            }

            mContext.hitOffset.X = 0;
            mContext.hitOffset.Y = 0;
            return ShapeAnchor.None;
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
            if (e.Button == MouseButtons.Right)
            {
                return;
            }

            if (Diagram != null)
            {
                if (mouseMode == MouseMoveMode.OffsetMove)
                {
                    Diagram.ConfirmSelectionPosition();
                }
                else if (mouseMode == MouseMoveMode.SelectedShapeMove)
                {
                    Diagram.ConfirmSelectionPosition();
                }
                else if (mouseMode == MouseMoveMode.ConnectionMake)
                {
                    CDShape shape = Diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

                    if (shape != null && lastSelectedShape != null && shape != lastSelectedShape)
                    {
                        CDConnection conn = Diagram.AddConnection(Diagram.DiagramDefinition.DefaultConnectionType, lastSelectedShape, shape);
                        conn.startRelOffset = mouseRelativeConnOffsetStart;
                        conn.endRelOffset = shape.Bounds.GetRelativePoint(mContext.MovePoint);
                    }
                    ClearSelection();
                    Diagram.ClearHighlight();
                }
                else if (mouseMode == MouseMoveMode.ReconnectStartPoint)
                {
                    CDShape shape = Diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

                    if (lastSelectedConnection != null && shape != null)
                    {
                        if (lastSelectedConnection.EndShape != shape)
                        {
                            lastSelectedConnection.StartShape = shape;
                        }
                        lastSelectedConnection.visible = true;
                        lastSelectedConnection.validCoordinates = false;
                    }
                    Diagram.ClearHighlight();
                }
                else if (mouseMode == MouseMoveMode.ReconnectEndPoint)
                {
                    CDShape shape = Diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

                    if (lastSelectedConnection != null && shape != null)
                    {
                        if (lastSelectedConnection.StartShape != shape)
                        {
                            lastSelectedConnection.EndShape = shape;
                        }
                        lastSelectedConnection.visible = true;
                        lastSelectedConnection.validCoordinates = false;
                    }
                    Diagram.ClearHighlight();
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

        private bool PrepareContextMenu(ContextMenuStrip cmenu)
        {
            CDShape shape = Diagram.GetShapeAtPoint(mContext.StartPoint.X, mContext.StartPoint.Y);
            CDConnection conn = Diagram.GetConnectionAtPoint(mContext.StartPoint.X, mContext.StartPoint.Y);

            cmenu.Items.Clear();
            if (shape != null)
            {
                if (shape.Decomposition != null)
                {
                    AddMenuItem(cmenu, "Go to Decomposition", "GotoDiagram", shape.Decomposition);
                    AddMenuItem(cmenu, "Change Decomposition", "Decompose", shape);
                }
                else
                    AddMenuItem(cmenu, "Decompose", "Decompose", shape);

                if (!shape.Selected)
                    AddMenuItem(cmenu, "Select", "SelectObject", shape);

                AddMenuItem(cmenu, "Start Connection", "StartConn", shape);
                AddMenuSeparator(cmenu);
                AddMenuItem(cmenu, "Delete", "DeleteShape", shape);

            }
            else if (conn != null)
            {
                AddMenuSeparator(cmenu);
                AddMenuItem(cmenu, "Delete", "DeleteConnection", conn);
            }
            else
            {
                if (Diagram != null && Diagram.DiagramDefinition != null && Diagram.DiagramDefinition.shapes != null)
                {
                    foreach (CDShapeDefinition sd in Diagram.DiagramDefinition.shapes)
                    {
                        if (sd.UserVisible)
                            AddMenuItem(cmenu, "Insert \"" + sd.Description + "\"", "InsertShape", sd);
                    }
                }
            }

            AddMenuSeparator(cmenu);
            AddMenuItem(cmenu, "Find Diagram", "FindDiagram");

            return cmenu.Items.Count > 0;
        }

        private static void AddMenuSeparator(ContextMenuStrip cmenu)
        {
            if (cmenu.Items.Count > 0)
            {
                cmenu.Items.Add(new ToolStripSeparator());
            }
        }

        private void AddMenuItem(ContextMenuStrip cmenu, string userText, string cmd, params object[] args)
        {
            ToolStripItem tsi = cmenu.Items.Add(userText);
            tsi.Tag = new MCA(cmd, args);
            tsi.Click += Tsi_Click;
        }

        private void Tsi_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripItem)
            {
                ToolStripItem tsi = (ToolStripItem)sender;
                if (tsi.Tag is MCA)
                {
                    ExecuteCommand((MCA)tsi.Tag);
                }
            }
        }

        private void ExecuteCommand(MCA mca)
        {
            switch (mca.Command)
            {
                case "FindDiagram":
                    DialogFindDiagram dfd = new DialogFindDiagram();
                    if (dfd.ShowDialog() == DialogResult.OK)
                    {
                        if (dfd.SelectedDiagram != null)
                            MiniCaseApp.MainWindow.ShowPanel("diagram", dfd.SelectedDiagram);
                    }
                    break;
                case "GotoDiagram":
                    if (mca[0] is CDDiagram)
                    {
                        MiniCaseApp.MainWindow.ShowPanel("diagram", mca[0]);
                    }
                    break;
                case "Decompose":
                    if (mca[0] is CDShape)
                    {
                        CreateDecompositionForShape((CDShape)mca[0]);
                    }
                    break;
                case "SelectObject":
                    if (mca[0] is CDShape)
                    {
                        CDShape shape = (CDShape)mca[0];
                        shape.Diagram.ClearSelection();
                        shape.Selected = true;
                        lastSelectedShape = shape;
                        if (SelectedDiagramObjectsChanged != null)
                        {
                            SelectedObjectsEventArgs sse = new SelectedObjectsEventArgs(shape);
                            SelectedDiagramObjectsChanged(this, sse);
                        }
                        Invalidate();
                    }
                    break;
                case "SelectConnection":
                    if (mca[0] is CDConnection)
                    {
                        CDConnection conn = (CDConnection)mca[0];
                        Diagram.ClearSelection();
                        conn.selected = true;
                        SelectedObjectsEventArgs sse = new SelectedObjectsEventArgs(conn);
                        SelectedDiagramObjectsChanged(this, sse);
                    }
                    break;
                case "StartConn":
                    if (mca[0] is CDShape)
                    {
                        CDShape shape = (CDShape)mca[0];
                        mouseMode = MouseMoveMode.ConnectionMake;
                        mouseLogicalConnStart = mContext.StartPoint;
                        mouseRelativeConnOffsetStart = shape.Bounds.GetRelativePoint(mContext.StartPoint);
                        lastSelectedShape = shape;
                        Invalidate();
                    }
                    break;
                case "InsertShape":
                    if (mca[0] is CDShapeDefinition)
                    {
                        CDShapeDefinition shapeDef = (CDShapeDefinition)mca[0];
                        if (shapeDef != null)
                        {
                            CDShape cs2 = new CDShape(Diagram, shapeDef, Guid.NewGuid());
                            Point viewPoint = mContext.StartPoint;
                            Size defaultSize = shapeDef.DefaultSize;
                            cs2.Bounds = new RectangleD(viewPoint.X - defaultSize.Width / 2,
                                viewPoint.Y - defaultSize.Height / 2, viewPoint.X + defaultSize.Width / 2,
                                viewPoint.Y + defaultSize.Height / 2);
                            Diagram.AddShape(cs2);
                            Invalidate();
                        }
                    }
                    break;
                case "DeleteShape":
                    if (mca[0] is CDShape)
                    {
                        CDShape shape = (CDShape)mca[0];
                        shape.Diagram.DeleteShape(shape);
                        Invalidate();
                    }
                    break;
                case "DeleteConnection":
                    if (mca[0] is CDConnection)
                    {
                        CDConnection conn = (CDConnection)mca[0];
                        conn.Diagram.DeleteConnections(conn);
                        Invalidate();
                    }
                    break;
            }
        }

        private void CaseDiagramView_MouseDown(object sender, MouseEventArgs e)
        {
            //bool connAnchorStart = false;
            ShapeAnchor hitA = ShapeAnchor.None;

            // omit this mouseDown when we are creating connection
            if (mouseMode == MouseMoveMode.ConnectionMake)
            {
                return;
            }

            CDShape shapeHit = Diagram.GetShapeAtPoint(e.X - offset.X, e.Y - offset.Y);

            mouseButtonDown = true;
            mouseButtonDownStartOffset = new Point(offset.X, offset.Y);
            mouseMode = MouseMoveMode.None;
            mContext.StartPointP.X = e.X;
            mContext.StartPointP.Y = e.Y;
            mContext.StartPoint.X = e.X - offset.X;
            mContext.StartPoint.Y = e.Y - offset.Y;

            if (e.Button == MouseButtons.Right)
            {
                if (PrepareContextMenu(contextMenuStrip1))
                {
                    contextMenuStrip1.Show(this, mContext.StartPointP);
                }
                return;
            }

            if (key_s)
            {
                mouseMode = MouseMoveMode.SelectArea;
                Diagram.ClearSelection();
            }
            else if (key_c)
            {
                CDShape shape = shapeHit;
                if (shape != null)
                {
                    mouseMode = MouseMoveMode.ConnectionMake;
                    mouseLogicalConnStart = mContext.StartPoint;
                    mouseRelativeConnOffsetStart = shape.Bounds.GetRelativePoint(mContext.StartPoint);
                    lastSelectedShape = shape;
                    Debugger.Log(0, "", "MouseDown - connectionMake\n");
                }
                else
                {
                    mouseMode = MouseMoveMode.OffsetMove;
                    mContext.mouseOffsetBackup = mContext.Offset;
                }
            }
            else if (lastSelectedShape != null && lastSelectedShape == shapeHit && ((hitA = TestHitConnectionAnchor(lastSelectedShape, e.X - offset.X, e.Y - offset.Y)) != ShapeAnchor.None))
            {
                switch (hitA)
                {
                    case ShapeAnchor.Whole:
                        Diagram.SaveSelectionPosition();
                        mouseMode = MouseMoveMode.SelectedShapeMove;
                        RedrawClientArea();
                        break;
                    case ShapeAnchor.Bottom:
                    case ShapeAnchor.BottomLeft:
                    case ShapeAnchor.BottomRight:
                    case ShapeAnchor.Left:
                    case ShapeAnchor.Right:
                    case ShapeAnchor.Top:
                    case ShapeAnchor.TopLeft:
                    case ShapeAnchor.TopRight:
                        mouseMode = MouseMoveMode.ResizeBorder; break;
                    case ShapeAnchor.TopCenter:
                    case ShapeAnchor.CenterRight:
                    case ShapeAnchor.BottomCenter:
                    case ShapeAnchor.CenterLeft:
                        mouseMode = MouseMoveMode.ConnectionMake; break;
                    default:
                        mouseMode = MouseMoveMode.None; break;
                }
                mouseObjectAnchorDown = hitA;
                mouseLogicalConnStart.X = e.X - offset.X;
                mouseLogicalConnStart.Y = e.Y - offset.Y;
            }
            else if (lastSelectedConnection != null && mContext.mouseAreaReconnectStart.Contains(mContext.StartPoint))
            {
                mouseMode = MouseMoveMode.ReconnectStartPoint;
                CDShape startShape = lastSelectedConnection.EndShape;
                if (startShape != null)
                {
                    mouseLogicalConnStart = startShape.Bounds.GetLogicalPoint(lastSelectedConnection.endRelOffset);
                }
            }
            else if (lastSelectedConnection != null && mContext.mouseAreaReconnectEnd.Contains(mContext.StartPoint))
            {
                mouseMode = MouseMoveMode.ReconnectEndPoint;
                CDShape startShape = lastSelectedConnection.StartShape;
                if (startShape != null)
                {
                    mouseLogicalConnStart = startShape.Bounds.GetLogicalPoint(lastSelectedConnection.startRelOffset);
                }
            }
            else if (Diagram != null)
            {
                if (lastSelectedConnection != null)
                {
                    lastSelectedConnection.selected = false;
                    lastSelectedConnection = null;
                }

                CDShape shape = shapeHit;
                CDConnection conn = Diagram.GetConnectionAtPoint(e.X - offset.X, e.Y - offset.Y);
                lastSelectedShape = shape;

                if (shape == null)
                {
                    if (conn != null)
                    {
                        ExecuteCommand(new MCA("SelectConnection", conn));
                    }
                    else
                    {
                        ClearSelection();
                        // notify about selection change
                        SelectedDiagramObjectsChanged(this, new SelectedObjectsEventArgs(Diagram));
                    }
                }
                else
                {
                    if (shape.ShapeDefinition.AutomaticBounds)
                    {
                        if (conn != null)
                        {
                            lastSelectedConnection = conn;
                            lastSelectedShape = null;
                            ExecuteCommand(new MCA("SelectConnection", conn));
                        }
                        else
                        {
                            ExecuteCommand(new MCA("SelectObject", shape));
                        }
                    }
                    else if (key_control)
                    {
                        shape.Selected = !shape.Selected;
                    }
                    else
                    {
                        ExecuteCommand(new MCA("SelectObject", shape));
                    }
                }

                Diagram.SaveSelectionPosition();
                if (Diagram.SelectedCount > 0 && mouseMode == MouseMoveMode.None)
                    mouseMode = MouseMoveMode.SelectedShapeMove;
                else
                {
                    mouseMode = MouseMoveMode.OffsetMove;
                    mContext.mouseOffsetBackup = mContext.Offset;
                }

                Invalidate();
            }
        }

        private void CaseDiagramView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CDShape shape = Diagram.GetShapeAtPoint(mContext.StartPoint.X, mContext.StartPoint.Y);
                if (shape != null)
                {

                }
            }
        }

        private void CaseDiagramView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CDShape shape = Diagram.GetShapeAtPoint(mContext.StartPoint.X, mContext.StartPoint.Y);
            if (shape != null)
            {
                if (shape.Decomposition != null)
                {
                    MiniCaseApp.MainWindow.ShowPanel("diagram", shape.Decomposition);
                }
                else
                {
                    CreateDecompositionForShape(shape);
                }
            }
        }

        private static void CreateDecompositionForShape(CDShape shape)
        {
            DialogSelectDocumentType dlg = new DialogSelectDocumentType();
            dlg.SetProject(MiniCaseApp.Project);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.WillCreateNewDiagram)
                {
                    CDDocumentDefinition dd = dlg.SelectedDocumentType;
                    if (dd != null)
                    {
                        CDDiagram dg = new CDDiagram(MiniCaseApp.Project, dd, Guid.NewGuid());
                        dg.ObjectId = Guid.NewGuid();
                        shape.Decomposition = dg;
                        dg.SetString("Text", shape.GetString("Text"));
                        MiniCaseApp.Project.diagrams.Add(dg);
                        MiniCaseApp.MainWindow.ShowPanel("diagram", dg);
                    }
                }
                else
                {
                    CDDiagram dg = dlg.SelectedExistingDiagram;
                    shape.Decomposition = dg;
                    MiniCaseApp.MainWindow.ShowPanel("diagram", dg);
                }
            }
        }

        private void CaseDiagramView_MouseCaptureChanged(object sender, EventArgs e)
        {

        }

        private void CaseDiagramView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Control:
                    key_control = true;
                    break;
                case Keys.Shift:
                    key_shift = true;
                    break;
                case Keys.C:
                    key_c = true;
                    break;
                case Keys.S:
                    key_s = true;
                    break;
            }
        }

        private void CaseDiagramView_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Control:
                    key_control = false;
                    break;
                case Keys.Shift:
                    key_shift = false;
                    break;
                case Keys.C:
                    key_c = false;
                    break;
                case Keys.S:
                    key_s = false;
                    break;
                case Keys.Delete:
                    Diagram.DeleteSelection();
                    RedrawClientArea();
                    break;
                case Keys.Back:
                    Diagram.DeleteSelection();
                    RedrawClientArea();
                    break;
            }
        }

        private void CaseDiagramView_DragDrop(object sender, DragEventArgs e)
        {
            if (Diagram == null)
                return;

            string str = "";
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                str = (string)e.Data.GetData(DataFormats.Text);
                CDShapeDefinition shapeDef = Diagram.DiagramDefinition.FindShapeDefinition(str);
                if (shapeDef != null)
                {
                    CDShape cs2 = new CDShape(Diagram, shapeDef, Guid.NewGuid());
                    Point viewPoint = this.PointToClient(new Point(e.X, e.Y));
                    Size defaultSize = shapeDef.DefaultSize;
                    cs2.Bounds = new RectangleD(viewPoint.X - defaultSize.Width / 2,
                        viewPoint.Y - defaultSize.Height / 2, viewPoint.X + defaultSize.Width / 2,
                        viewPoint.Y + defaultSize.Height / 2);
                    Diagram.AddShape(cs2);
                    //CalculateConnectionsCoordinates();
                    RedrawClientArea();
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    Image img = null;
                    foreach (string fileName in files)
                    {
                        try
                        {
                            img = Image.FromFile(fileName);
                            break;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    if (img != null)
                    {
                        CDShape cs2 = new CDShape(Diagram, CDLibrary.ImageShape, Guid.NewGuid());
                        Point viewPoint = this.PointToClient(new Point(e.X, e.Y));
                        Size defaultSize = cs2.ShapeDefinition.DefaultSize;
                        cs2.Bounds = new RectangleD(viewPoint.X - defaultSize.Width / 2,
                            viewPoint.Y - defaultSize.Height / 2, viewPoint.X + defaultSize.Width / 2,
                            viewPoint.Y + defaultSize.Height / 2);
                        cs2.SetObject("Image", img);
                        Diagram.AddShape(cs2);
                        RedrawClientArea();
                    }
                }
            }

        }

        private void CaseDiagramView_DragEnter(object sender, DragEventArgs e)
        {
            if (Diagram == null)
            {
                e.Effect = DragDropEffects.None;
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

    }


    public class SelectedObjectsEventArgs : EventArgs
    {
        public SelectedObjectsEventArgs(CDShape aShape)
        {
            Shape = aShape;
        }

        public SelectedObjectsEventArgs(CDConnection aConn)
        {
            Connection = aConn;
        }

        public SelectedObjectsEventArgs(CDDiagram aDiag)
        {
            Diagram = aDiag;
        }

        public CDShape Shape { get; set; }

        public CDConnection Connection { get; set; }

        public CDDiagram Diagram { get; set; }

    }

}
