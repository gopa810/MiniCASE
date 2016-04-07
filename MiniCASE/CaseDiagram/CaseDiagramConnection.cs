using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MiniCASE
{
    public class CaseDiagramConnection
    {
        public int startId;
        public int endId;


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
        public int StartCap = 0;
        public int EndCap = 0;
        public int LineStyle = 0;

        public string startNote;
        public string centerNode;
        public string endNote;
        public bool validCoordinates = false;
        public DiagramPath path = null;
        public Point[] coordinates = null;
        public bool visible = true;
        public bool selected = false;


        private static Dictionary<int, Pen> connPensMap = new Dictionary<int, Pen>();
        private static Dictionary<int, Pen> connSelPensMap = new Dictionary<int, Pen>();


        public static LineCap GetCapFromStyle(int i)
        {
            switch (i)
            {
                case 0:
                    return LineCap.Flat;
                case 1:
                    return LineCap.ArrowAnchor;
                case 2:
                    return LineCap.Triangle;
                default:
                    return LineCap.Flat;
            }
        }

        public static DashStyle GetDashFromStyle(int i)
        {
            switch (i)
            {
                case 0:
                    return DashStyle.Solid;
                case 1:
                    return DashStyle.Dash;
                case 2:
                    return DashStyle.Dot;
                case 3:
                    return DashStyle.DashDot;
                case 4:
                    return DashStyle.DashDotDot;
                default:
                    return DashStyle.Solid;
            }
        }

        public static Pen GetConnectionPen(CaseDiagramConnection conn)
        {
            Dictionary<int, Pen> map = conn.selected ? connSelPensMap : connPensMap;

            if (!map.ContainsKey(conn.LineStyle))
            {
                Pen pen = new Pen(conn.selected ? Color.DarkGreen : Color.Black);
                //pen.StartCap = GetCapFromStyle(conn.type & 0xff);
                //pen.EndCap = GetCapFromStyle((conn.type & 0xff00) >> 8);
                pen.DashStyle = GetDashFromStyle(conn.LineStyle);
                map.Add(conn.LineStyle, pen);
            }

            return map[conn.LineStyle];
        }

        /// <summary>
        /// Drawing of line cap
        /// </summary>
        /// <param name="g">graphics context</param>
        /// <param name="offset">offset of diagram within view</param>
        /// <param name="p">pen for drawing a cap</param>
        /// <param name="p1">start point of line</param>
        /// <param name="p2">end point of line (cap is drawn for this point)</param>
        /// <param name="opt">Type of cap</param>
        public static void DrawArrowEnd(Graphics g, Point offset, Pen p, Point p1, Point p2, int opt)
        {
            if (opt == 0)
                return;

            Point ps = p1;
            Point[] vtx = new Point[3];
            vtx[1] = p2;
            vtx[1].Offset(offset);
            ps.Offset(offset);
            double dx = ps.X - vtx[1].X;
            double dy = ps.Y - vtx[1].Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist == 0.0)
                dist = 1.0;
            else
                dist = 10 / dist;
            dx *= dist;
            dy *= dist;
            vtx[0].X = Convert.ToInt32(vtx[1].X + dx - dy * 0.3);
            vtx[0].Y = Convert.ToInt32(vtx[1].Y + dy + dx * 0.3);
            vtx[2].X = Convert.ToInt32(vtx[1].X + dx + dy * 0.3);
            vtx[2].Y = Convert.ToInt32(vtx[1].Y + dy - dx * 0.3);

            if (opt == 1)
            {
                g.DrawLine(p, vtx[0], vtx[1]);
                g.DrawLine(p, vtx[1], vtx[2]);
            }
            else if (opt == 2)
            {
                g.FillPolygon(Brushes.Black, vtx);
                //g.DrawPolygon(p, vtx);
            }
        }

        public void DrawConnection(Graphics g, Point offset)
        {
            Point p1;
            Point p2 = new Point(0, 0);
            int count = 0;
            Pen pen = GetConnectionPen(this);

            if (coordinates.Length > 1)
            {
                DrawArrowEnd(g, offset, Pens.Black, coordinates[1], coordinates[0], StartCap);
            }

            foreach (Point p in coordinates)
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

            if (coordinates.Length > 1)
            {
                DrawArrowEnd(g, offset, Pens.Black, coordinates[coordinates.Length - 2], 
                    coordinates[coordinates.Length - 1], EndCap);
            }
        }
    }

}
