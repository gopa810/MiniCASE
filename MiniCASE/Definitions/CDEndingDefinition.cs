using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

namespace MiniCASE
{
    public class CDEndingDefinition: CDObjectDefinition
    {
        public List<CDEI> instructions = new List<CDEI>();


        public override string ToString()
        {
            return Name;
        }

        public enum CDT
        {
            DrawLine = 0,
            DrawRectangle = 1,
            DrawEllipse = 2,
            DrawTriangle = 3,
            FillRectangle = 4,
            FillEllipse = 5,
            FillTriangle = 6
        }

        public class CDEI
        {
            public CDT Command = CDT.DrawLine;
            public Color Color = Color.Empty;
            public PointF PA = PointF.Empty;
            public PointF PB = PointF.Empty;
            public PointF PC = PointF.Empty;

            public CDEI()
            {

            }

            public CDEI(CDT cmd, Color clr, PointF a1, PointF a2)
            {
                Command = cmd;
                Color = clr;
                PA = a1;
                PB = a2;
            }
            public CDEI(CDT cmd, Color clr, PointF a1, PointF a2, PointF a3)
            {
                Command = cmd;
                Color = clr;
                PA = a1;
                PB = a2;
                PC = a3;
            }
            public CDEI(CDT cmd, Color clr, float a1x, float a1y, float a2x, float a2y)
            {
                Command = cmd;
                Color = clr;
                PA = new PointF(a1x, a1y);
                PB = new PointF(a2x, a2y);
            }
            public CDEI(CDT cmd, Color clr, float a1x, float a1y, float a2x, float a2y, float a3x, float a3y)
            {
                Command = cmd;
                Color = clr;
                PA = new PointF(a1x, a1y);
                PB = new PointF(a2x, a2y);
                PC = new PointF(a3x, a3y);
            }
            public CDEI(CDT cmd, PointF a1, PointF a2)
            {
                Command = cmd;
                PA = a1;
                PB = a2;
            }
            public CDEI(CDT cmd, PointF a1, PointF a2, PointF a3)
            {
                Command = cmd;
                PA = a1;
                PB = a2;
                PC = a3;
            }
            public CDEI(CDT cmd, float a1x, float a1y, float a2x, float a2y)
            {
                Command = cmd;
                PA = new PointF(a1x, a1y);
                PB = new PointF(a2x, a2y);
            }
            public CDEI(CDT cmd, float a1x, float a1y, float a2x, float a2y, float a3x, float a3y)
            {
                Command = cmd;
                PA = new PointF(a1x, a1y);
                PB = new PointF(a2x, a2y);
                PC = new PointF(a3x, a3y);
            }

            public void WriteTo(XmlElement elem, XmlDocument doc)
            {
                elem.SetAttribute("cmd", Command.ToString());
                elem.SetAttribute("color", ColorTranslator.ToHtml(Color));
                if (!PA.IsEmpty)
                {
                    elem.SetAttribute("ax", PA.X.ToString());
                    elem.SetAttribute("ay", PA.Y.ToString());
                }
                if (!PB.IsEmpty)
                {
                    elem.SetAttribute("bx", PB.X.ToString());
                    elem.SetAttribute("by", PB.Y.ToString());
                }
                if (!PC.IsEmpty)
                {
                    elem.SetAttribute("cx", PC.X.ToString());
                    elem.SetAttribute("cy", PC.Y.ToString());
                }
            }

            public void ReadFrom(XmlElement elem)
            {
                Command = (CDT)Enum.Parse(typeof(CDT), elem.GetAttribute("cmd"));
                Color = ColorTranslator.FromHtml(elem.GetAttribute("color"));
                if (elem.HasAttribute("ax") && elem.HasAttribute("ay"))
                    PA = new PointF(float.Parse(elem.GetAttribute("ax")), float.Parse(elem.GetAttribute("ay")));
                if (elem.HasAttribute("bx") && elem.HasAttribute("by"))
                    PA = new PointF(float.Parse(elem.GetAttribute("bx")), float.Parse(elem.GetAttribute("by")));
                if (elem.HasAttribute("cx") && elem.HasAttribute("cy"))
                    PA = new PointF(float.Parse(elem.GetAttribute("cx")), float.Parse(elem.GetAttribute("cy")));
            }
        }

        public CDEndingDefinition(Guid oid): base(oid)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="a">This point is inside of connection line.</param>
        /// <param name="b">This point is outside of connection line, so it is 
        /// actually starting or ending point of connection.</param>
        public void Draw(CDContext ctx, Point a, Point b, Color lineColor, Color backColor, float lineWidth)
        {
            Point vectorX = new Point(a.X - b.X, a.Y - b.Y);
            Point vectorY = new Point();
            double dLength = Math.Sqrt(vectorX.X*vectorX.X + vectorX.Y*vectorX.Y);

            if (vectorX.X == 0 && vectorX.Y == 0)
                return;

            // normalize vector
            vectorX.X = Convert.ToInt32(vectorX.X * 32.0 / dLength);
            vectorX.Y = Convert.ToInt32(vectorX.Y * 32.0 / dLength);

            vectorY.X = -vectorX.Y;
            vectorY.Y = vectorX.X;

            Point[] pa;
            Pen pn;
            Brush br;

            foreach (CDEI ins in instructions)
            {
                switch(ins.Command)
                {
                    case CDT.DrawLine:
                        ctx.Graphics.DrawLine(CDGraphics.GetPen(ins.Color.IsEmpty ? lineColor : ins.Color, lineWidth, DashStyle.Solid), GetPoint(ins.PA, ref vectorX, ref vectorY, ref b),
                            GetPoint(ins.PB, ref vectorX, ref vectorY, ref b));
                        break;
                    case CDT.DrawTriangle:
                        pa = new Point[] { GetPoint(ins.PA, ref vectorX, ref vectorY, ref b),
                        GetPoint(ins.PB, ref vectorX, ref vectorY, ref b), GetPoint(ins.PC, ref vectorX, ref vectorY, ref b)};
                        pn = CDGraphics.GetPen(ins.Color.IsEmpty ? lineColor : ins.Color, lineWidth, DashStyle.Solid);
                        ctx.Graphics.DrawLine(pn, pa[0], pa[1]);
                        ctx.Graphics.DrawLine(pn, pa[1], pa[2]);
                        ctx.Graphics.DrawLine(pn, pa[2], pa[0]);
                        break;
                    case CDT.DrawRectangle:
                        pa = new Point[] { GetPoint(ins.PA.X, ins.PA.Y, ref vectorX, ref vectorY, ref b),
                            GetPoint(ins.PA.X, ins.PB.Y, ref vectorX, ref vectorY, ref b),
                            GetPoint(ins.PB.X, ins.PB.Y, ref vectorX, ref vectorY, ref b),
                            GetPoint(ins.PB.X, ins.PA.Y, ref vectorX, ref vectorY, ref b)};
                        pn = CDGraphics.GetPen(ins.Color.IsEmpty ? lineColor : ins.Color, lineWidth, DashStyle.Solid);
                        ctx.Graphics.DrawPolygon(pn, pa);
                        break;
                    case CDT.FillTriangle:
                        pa = new Point[] { GetPoint(ins.PA, ref vectorX, ref vectorY, ref b),
                        GetPoint(ins.PB, ref vectorX, ref vectorY, ref b), GetPoint(ins.PC, ref vectorX, ref vectorY, ref b)};
                        br = CDGraphics.GetBrush(ins.Color.IsEmpty ? backColor : ins.Color);
                        ctx.Graphics.FillPolygon(br, pa);
                        break;
                    case CDT.FillRectangle:
                        pa = new Point[] { GetPoint(ins.PA.X, ins.PA.Y, ref vectorX, ref vectorY, ref b),
                            GetPoint(ins.PA.X, ins.PB.Y, ref vectorX, ref vectorY, ref b),
                            GetPoint(ins.PB.X, ins.PB.Y, ref vectorX, ref vectorY, ref b),
                            GetPoint(ins.PB.X, ins.PA.Y, ref vectorX, ref vectorY, ref b)};
                        br = CDGraphics.GetBrush(ins.Color.IsEmpty ? backColor : ins.Color);
                        ctx.Graphics.FillPolygon(br, pa);
                        break;
                }
            }
        }

        private Point GetPoint(PointF a, ref Point vx, ref Point vy, ref Point origin)
        {
            return new Point((int)(a.X * vx.X + a.Y * vy.X + origin.X), (int)(a.X * vx.Y + a.Y * vy.Y + origin.Y));
        }

        private Point GetPoint(float aX, float aY, ref Point vx, ref Point vy, ref Point origin)
        {
            return new Point((int)(aX * vx.X + aY * vy.X + origin.X), (int)(aX * vx.Y + aY * vy.Y + origin.Y));
        }

        public override void ReadFrom(XmlElement elem, CDReaderReferences refs)
        {
            base.ReadFrom(elem, refs);
            instructions.Clear();

            foreach(XmlElement E in CDXml.GetChildren(elem, "instr"))
            {
                CDEI instr = new CDEI();
                instr.ReadFrom(E);
                instructions.Add(instr);
            }
        }

        public override void WriteTo(XmlElement elem, XmlDocument doc)
        {
            base.WriteTo(elem, doc);
            foreach (CDEI E in instructions)
            {
                XmlElement el = doc.CreateElement("instr");
                elem.AppendChild(el);
                E.WriteTo(el, doc);
            }
        }
    }
}
