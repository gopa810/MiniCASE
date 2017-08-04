using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

namespace MiniCASE
{
    public class CDConnection: CDObject
    {
        public CDShape StartShape;
        public CDShape EndShape;


        public CDDiagram Diagram { get; set; }
        /// <summary>
        /// Type of connection
        /// bit 0-7   - arrow type start
        /// bit 8-15  - arrow type end
        ///      value of type:
        ///          0 - no arrow
        ///          1 - simple arrow
        ///          2 - filled arrow
        /// bit 16-23 - line type
        ///      value of line type:
        ///          0 - black solid
        ///          1 - black dotted
        ///          2 - black dashed
        /// </summary>
        //public ConnectionsMode PathMode = ConnectionsMode.Straight;

        public Point startRelOffset = new Point(50, 50);
        public Point endRelOffset = new Point(50, 50);
        public bool validCoordinates = false;
        public Point[] coordinates = null;
        public bool visible = true;
        public bool selected = false;

        public CDConnectionDefinition ConnectionDefinition
        {
            get { return (CDConnectionDefinition)Definition; }
        }

        public CDConnection(CDDiagram diag, CDConnectionDefinition def, Guid oid): base(diag.Project, def, oid)
        {
            Diagram = diag;
        }


        public Pen GetConnectionPen(Color lineColor)
        {
            float width = selected ? 2f : 1f;
            DashStyle cp = (DashStyle)GetInt("LinePattern");

            return CDGraphics.GetPen(lineColor, width, cp);
        }

        public void DrawConnectionBackground(CDContext ctx)
        {
            if (selected)
                ctx.Graphics.DrawLines(ctx.SelectionPen, coordinates);
        }

        public void DrawConnection(CDContext ctx)
        {
            Point p1;
            Point p2 = new Point(0, 0);

            Color clrBack = GetColor("BackColor");
            Color clrLine = GetColor("LineColor");
            CDEndingDefinition startCap = (CDEndingDefinition)GetObject("StartCap");
            CDEndingDefinition endCap = (CDEndingDefinition)GetObject("EndCap");

            float width = (float)GetFloat("LineWidth");
            DashStyle cp = (DashStyle)GetInt("LinePattern");

            Pen pen = CDGraphics.GetPen(clrLine, width, cp);

            ctx.Graphics.DrawLines(pen, coordinates);

            if (coordinates.Length > 1 && startCap != null)
            {
                startCap.Draw(ctx, coordinates[1], coordinates[0], clrLine, clrBack, width);
            }

            if (coordinates.Length > 1 && endCap != null)
            {
                endCap.Draw(ctx, coordinates[coordinates.Length - 2],
                    coordinates[coordinates.Length - 1], clrLine, clrBack, width);
            }

            if (selected)
            {
                Point a = StartShape.Bounds.GetLogicalPoint(startRelOffset);
                ctx.mouseAreaReconnectStart.X = a.X - 8;
                ctx.mouseAreaReconnectStart.Y = a.Y - 8;
                ctx.mouseAreaReconnectStart.Width = 16;
                ctx.mouseAreaReconnectStart.Height = 16;
                ctx.Graphics.FillRectangle(Brushes.Green, ctx.mouseAreaReconnectStart);

                a = EndShape.Bounds.GetLogicalPoint(endRelOffset);
                ctx.mouseAreaReconnectEnd.X = a.X - 8;
                ctx.mouseAreaReconnectEnd.Y = a.Y - 8;
                ctx.mouseAreaReconnectEnd.Width = 16;
                ctx.mouseAreaReconnectEnd.Height = 16;
                ctx.Graphics.FillRectangle(Brushes.Green, ctx.mouseAreaReconnectEnd);
            }

        }


        public override bool Save(XmlElement elem, XmlDocument doc)
        {
            elem.SetAttribute("startId", StartShape.ObjectId.ToString());
            elem.SetAttribute("startX", startRelOffset.X.ToString());
            elem.SetAttribute("startY", startRelOffset.Y.ToString());
            elem.SetAttribute("endId", EndShape.ObjectId.ToString());
            elem.SetAttribute("endX", endRelOffset.X.ToString());
            elem.SetAttribute("endY", endRelOffset.Y.ToString());
            return base.Save(elem, doc);
        }

        public override bool Load(XmlElement elem, CDReaderReferences refs)
        {
            if (elem.HasAttribute("startId"))
            {
                refs.Add(CDLazyType.FindStartShape, new Guid(elem.GetAttribute("startId")), this);
            }
            if (elem.HasAttribute("startX") && elem.HasAttribute("startY"))
                startRelOffset = new Point(int.Parse(elem.GetAttribute("startX")), int.Parse(elem.GetAttribute("startY")));
            if (elem.HasAttribute("endId"))
            {
                refs.Add(CDLazyType.FindEndShape, new Guid(elem.GetAttribute("endId")), this);
            }
            if (elem.HasAttribute("endX") && elem.HasAttribute("endY"))
                endRelOffset = new Point(int.Parse(elem.GetAttribute("endX")), int.Parse(elem.GetAttribute("endY")));
            return base.Load(elem, refs);
        }

    }

    public enum ConnectionsMode
    {
        Straight = 0,
        Rectangular = 1,
        Path = 2
    }

}
