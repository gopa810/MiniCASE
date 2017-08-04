using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Xml;

namespace MiniCASE
{
    /// <summary>
    /// contains and handles data dedicated to one shape in diagram
    /// </summary>
    public class CDShape : CDObject
    {
        public CDDiagram Diagram { get; set; }
        private RectangleD bounds;
        private RectangleD boundsBack;
        public CDShapeDefinition ShapeDefinition
        {
            get
            {
                return (CDShapeDefinition)Definition;
            }
        }

        private bool selected = false;
        private bool highlighted = false;
        public bool FlagNeedsRecalculateBounds = false;


        public CDShape(CDDiagram diag, CDShapeDefinition shapeRef, Guid objectId): base(diag==null ? null : diag.Project, shapeRef, objectId)
        {
            Diagram = diag;
        }

        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public bool Highlighted
        {
            get { return highlighted; }
            set { highlighted = value; }
        }

        public RectangleD Bounds
        {
            get
            {
                if (bounds == null)
                    bounds = new RectangleD();
                return bounds;
            }
            set
            {
                bounds = new RectangleD();
                bounds.Set(value);
            }
        }

        public void SavePosition()
        {
            boundsBack = new RectangleD();
            boundsBack.Left = bounds.Left;
            boundsBack.Right = bounds.Right;
            boundsBack.Top = bounds.Top;
            boundsBack.Bottom = bounds.Bottom;
        }

        public void SetOffset(Size offset)
        {
            if (boundsBack == null)
                return;

            //Debugger.Log(0, "", string.Format("Moving {0} to {1}", boundsBack.Xa, boundsBack.Xa + offset.Width));
            bounds.Left = boundsBack.Left + offset.Width;
            bounds.Right = boundsBack.Right + offset.Width;
            bounds.Top = boundsBack.Top + offset.Height;
            bounds.Bottom = boundsBack.Bottom + offset.Height;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="side">Constant provided by class ShapeSide</param>
        /// <returns></returns>
        public Point GetBorderPointOnSide(int side)
        {
            switch (side)
            {
                case ShapeSide.Top: return new Point((bounds.Left + bounds.Right) / 2, bounds.Top);
                case ShapeSide.Bottom: return new Point((bounds.Left + bounds.Right) / 2, bounds.Bottom);
                case ShapeSide.Left: return new Point(bounds.Left, (bounds.Top + bounds.Bottom) / 2);
                case ShapeSide.Right: return new Point(bounds.Right, (bounds.Top + bounds.Bottom) / 2);
                default: return Point.Empty;
            }
        }

        public void ConfirmPosition()
        {
            boundsBack = null;
        }

        /// <summary>
        /// Returns point located at the border of entity, that is intersection between border and
        /// connection line.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public Point GetBorderPointForRectangle(double X, double Y, Point internalPoint)
        {
            Point intPoint = new Point();
            double Xc = X - internalPoint.X;
            double Yc = Y - internalPoint.Y;
            double rat = Convert.ToDouble(bounds.Height) / Convert.ToDouble(bounds.Width);
            Xc *= rat;

            if (Xc * Xc + Yc * Yc == 0)
                return internalPoint;

            if (Xc + Yc > 0) // B or C
            {
                if (Xc < Yc) // C
                {
                    intPoint.X = internalPoint.X + Convert.ToInt32((bounds.Bottom - internalPoint.Y) * Xc / (Yc * rat));
                    intPoint.Y = bounds.Bottom;
                }
                else // D
                {
                    intPoint.X = bounds.Right;
                    intPoint.Y = internalPoint.Y + Convert.ToInt32((bounds.Right - internalPoint.X) * Yc * rat / Xc);
                }
            }
            else // A or D
            {
                if (Xc < Yc) // B
                {
                    intPoint.X = bounds.Left;
                    intPoint.Y = internalPoint.Y + Convert.ToInt32((bounds.Left - internalPoint.X) * Yc * rat / Xc);
                }
                else// A
                {
                    intPoint.X = internalPoint.X + Convert.ToInt32((bounds.Top - internalPoint.Y) * Xc / (Yc * rat));
                    intPoint.Y = bounds.Top;
                }
            }

            return intPoint;
        }

        public Point GetBorderPointForEllipsis(double X, double Y)
        {
            Point cp = bounds.CenterPoint;
            Point intPoint = new Point();
            double Xc = X - cp.X;
            double Yc = Y - cp.Y;
            double a = (bounds.Right - cp.X);
            double b = (bounds.Bottom - cp.Y);
            double c = Xc;
            double d = Yc;

            if (Math.Abs(Xc) + Math.Abs(Yc) == 0)
                return cp;

            double yt = Math.Sqrt(1 / (1 / (b * b) + c * c / (d * d * a * a)));
            double xt = Math.Sqrt(1 / (1 / (a * a) + d * d / (c * c * b * b)));

            if (Xc > 0)
            {
                if (Yc > 0)
                {
                    intPoint.X = Convert.ToInt32(cp.X + xt);
                    intPoint.Y = Convert.ToInt32(cp.Y + yt);
                }
                else
                {
                    intPoint.X = Convert.ToInt32(cp.X + xt);
                    intPoint.Y = Convert.ToInt32(cp.Y - yt);
                }
            }
            else
            {
                if (Yc > 0)
                {
                    intPoint.X = Convert.ToInt32(cp.X - xt);
                    intPoint.Y = Convert.ToInt32(cp.Y + yt);
                }
                else
                {
                    intPoint.X = Convert.ToInt32(cp.X - xt);
                    intPoint.Y = Convert.ToInt32(cp.Y - yt);
                }
            }

            return intPoint;
        }

        public Point GetBorderPointForPico(double X, double Y)
        {
            Point cp = bounds.CenterPoint;
            Point intPoint = new Point();
            double Xc = X - cp.X;
            double Yc = Y - cp.Y;
            double a = (bounds.Right - cp.X);
            double b = (bounds.Bottom - cp.Y);
            double c = Math.Abs(Xc);
            double d = Math.Abs(Yc);

            if (c + d == 0)
                return cp;

            double yt = 1 / (1 / b + c / (d * a));
            double xt = 1 / (1 / a + d / (c * b));

            if (Xc > 0)
            {
                if (Yc > 0)
                {
                    intPoint.X = Convert.ToInt32(cp.X + xt);
                    intPoint.Y = Convert.ToInt32(cp.Y + yt);
                }
                else
                {
                    intPoint.X = Convert.ToInt32(cp.X + xt);
                    intPoint.Y = Convert.ToInt32(cp.Y - yt);
                }
            }
            else
            {
                if (Yc > 0)
                {
                    intPoint.X = Convert.ToInt32(cp.X - xt);
                    intPoint.Y = Convert.ToInt32(cp.Y + yt);
                }
                else
                {
                    intPoint.X = Convert.ToInt32(cp.X - xt);
                    intPoint.Y = Convert.ToInt32(cp.Y - yt);
                }
            }

            return intPoint;
        }

        public Point GetBorderPoint(double X, double Y)
        {
            if (ShapeDefinition.ShapeBase == ShapeBase.Rectangle)
            {
                return GetBorderPointForRectangle(X, Y, bounds.CenterPoint);
            }
            else if (ShapeDefinition.ShapeBase == ShapeBase.Ellipse)
            {
                return GetBorderPointForEllipsis(X, Y);
            }
            else if (ShapeDefinition.ShapeBase == ShapeBase.Pico)
            {
                return GetBorderPointForPico(X, Y);
            }
            else
            {
                return bounds.CenterPoint;
            }
        }

        public bool GetBorderPoint(Point startPoint, Point endPoint, out Point borderPoint, out Point anchorPoint, out ShapeAnchor anchor)
        {
            int length = 20;
            if (ShapeDefinition.ShapeBase == ShapeBase.Rectangle)
            {
                if (GetIntersect(startPoint, endPoint, Bounds.TopLeft, Bounds.TopRight, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchorPoint.Offset(0, -length);
                    anchor = ShapeAnchor.Top;
                    return true;
                }
                if (GetIntersect(startPoint, endPoint, Bounds.TopLeft, Bounds.BottomLeft, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchorPoint.Offset(-length, 0);
                    anchor = ShapeAnchor.Left;
                    return true;
                }
                if (GetIntersect(startPoint, endPoint, Bounds.TopRight, Bounds.BottomRight, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchorPoint.Offset(length, 0);
                    anchor = ShapeAnchor.Right;
                    return true;
                }
                if (GetIntersect(startPoint, endPoint, Bounds.BottomLeft, Bounds.BottomRight, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchorPoint.Offset(0, length);
                    anchor = ShapeAnchor.Bottom;
                    return true;
                }

                anchorPoint = borderPoint;
                anchorPoint.Offset(0, 0);
                anchor = ShapeAnchor.None;
                return false;
            }
            else if (ShapeDefinition.ShapeBase == ShapeBase.Ellipse)
            {
                if (GetIntersectEllipse(startPoint, endPoint, Bounds.Rectangle, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchor = ResolveAnchorDirection(ref borderPoint, ref anchorPoint, length);
                    return true;
                }
            }
            else if (ShapeDefinition.ShapeBase == ShapeBase.Pico)
            {
                if (GetIntersect(startPoint, endPoint, Bounds.TopCenter, Bounds.CenterRight, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchor = ResolveAnchorDirection(ref borderPoint, ref anchorPoint, length);
                    return true;
                }
                if (GetIntersect(startPoint, endPoint, Bounds.CenterRight, Bounds.BottomCenter, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchor = ResolveAnchorDirection(ref borderPoint, ref anchorPoint, length);
                    return true;
                }
                if (GetIntersect(startPoint, endPoint, Bounds.CenterLeft, Bounds.BottomCenter, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchor = ResolveAnchorDirection(ref borderPoint, ref anchorPoint, length);
                    return true;
                }
                if (GetIntersect(startPoint, endPoint, Bounds.CenterLeft, Bounds.TopCenter, out borderPoint))
                {
                    anchorPoint = borderPoint;
                    anchor = ResolveAnchorDirection(ref borderPoint, ref anchorPoint, length);
                    return true;
                }

                anchorPoint = Point.Empty;
                anchor = ShapeAnchor.None;
                return false;
            }

            borderPoint = Point.Empty;
            anchorPoint = borderPoint;
            anchor = ShapeAnchor.None;
            return false;
        }

        private ShapeAnchor ResolveAnchorDirection(ref Point borderPoint, ref Point anchorPoint, int length)
        {
            ShapeAnchor anchor;
            double angle = Math.Atan2(borderPoint.Y - (Bounds.Top + Bounds.Bottom) / 2, borderPoint.X - (Bounds.Left + Bounds.Right) / 2);
            double sinus = Math.Sin(angle);
            double cosinus = Math.Cos(angle);
            double half = Math.Sqrt(2) / 2;
            anchorPoint.Offset(Convert.ToInt32(length * cosinus), Convert.ToInt32(length * sinus));
            if (Math.Abs(sinus) <= half)
                anchor = (cosinus >= 0 ? ShapeAnchor.Right : ShapeAnchor.Left);
            else
                anchor = (sinus >= 0 ? ShapeAnchor.Top : ShapeAnchor.Bottom);
            return anchor;
        }

        public bool GetIntersect(Point s1, Point e1, Point s2, Point e2, out Point point)
        {
            int A = s1.X;
            int B = e1.X - s1.X;
            int C = s2.X;
            int D = e2.X - s2.X;
            int M = s1.Y;
            int N = e1.Y - s1.Y;
            int O = s2.Y;
            int P = e2.Y - s2.Y;

            int d = B * P - N * D;
            if (d != 0)
            {
                double div = d;

                double u = (B * (M - O) + N * (C - A)) / div;
                double t = (B == 0 ? (O + P * u - M) / N : (C - A + D * u) / B);

                if (u >= 0.0 && u <= 1.0 && t >= 0.0 && t <= 1.0)
                {
                    point = new Point(Convert.ToInt32(A + B * t), Convert.ToInt32(M + N * t));
                    return true;
                }
            }

            point = Point.Empty;
            return false;
        }

        public static bool GetIntersectEllipse(Point s1, Point e1, Rectangle r1, out Point point)
        {
            int startLineX = s1.X;
            int startLineY = s1.Y;
            int vectorLineX = e1.X - s1.X;
            int vectorLineY = e1.Y - s1.Y;

            double m = vectorLineX == 0 ? 0 : 1.0 * vectorLineY / vectorLineX;
            double c = vectorLineX == 0 ? startLineY : startLineY - 1.0 * vectorLineY * startLineX / vectorLineX;

            int h = r1.Left + r1.Width / 2;
            int k = r1.Top + r1.Height / 2;
            int a = r1.Width / 2;
            int b = r1.Height / 2;

            double eps = c - k;
            double delta = c + m * h;

            double a2m2b2 = a * a * m * m + b * b;

            double sq = a2m2b2 - delta * delta - k * k + 2 * delta * k;

            if (sq < 0.0)
            {
                point = Point.Empty;
                return false;
            }

            double x1, y1;
            double x2 = 0, y2 = 0;
            double t1, t2;

            if (sq < 0.0001)
            {
                x1 = (h * b * b - m * a * a * eps) / a2m2b2;
                y1 = (b * b * delta + k * a * a * m * m) / a2m2b2;

                t1 = vectorLineX == 0 ? (y1 - startLineY) / vectorLineY : (x1 - startLineX) / vectorLineX;

                if (t1 >= 0 && t1 <= 1.0)
                {
                    point = new Point(Convert.ToInt32(startLineX + vectorLineX * t1), Convert.ToInt32(startLineY + vectorLineY * t1));
                    return true;
                }
                else
                {
                    point = Point.Empty;
                    return false;
                }
            }
            else
            {
                x1 = (h * b * b - m * a * a * eps + a * b * Math.Sqrt(sq)) / a2m2b2;
                y1 = (b * b * delta + k * a * a * m * m + a * b * m * Math.Sqrt(sq)) / a2m2b2;
                x2 = (h * b * b - m * a * a * eps - a * b * Math.Sqrt(sq)) / a2m2b2;
                y2 = (b * b * delta + k * a * a * m * m - a * b * m * Math.Sqrt(sq)) / a2m2b2;

                t1 = vectorLineX == 0 ? (y1 - startLineY) / vectorLineY : (x1 - startLineX) / vectorLineX;
                t2 = vectorLineX == 0 ? (y2 - startLineY) / vectorLineY : (x2 - startLineX) / vectorLineX;

                if (t1 >= 0 && t1 <= 1.0)
                {
                    point = new Point(Convert.ToInt32(startLineX + vectorLineX * t1), Convert.ToInt32(startLineY + vectorLineY * t1));
                    return true;
                }
                else if (t2 >= 0 && t2 <= 1.0)
                {
                    point = new Point(Convert.ToInt32(startLineX + vectorLineX * t2), Convert.ToInt32(startLineY + vectorLineY * t2));
                    return true;
                }
                else
                {
                    point = Point.Empty;
                    return false;
                }
            }

        }

        /// <summary>
        /// Returns border point on current shape, which is used for connection 
        /// to the given shape 'sh'
        /// </summary>
        /// <param name="sh"></param>
        /// <returns></returns>
        public Point GetBorderPoint(CDShape sh)
        {
            Point pt = sh.Bounds.CenterPoint;
            return GetBorderPoint(pt.X, pt.Y);
        }

        public bool ContainsPoint(Point pt)
        {
            return ContainsPoint(pt.X, pt.Y);
        }

        public bool ContainsPoint(int px, int py)
        {
            if (ShapeDefinition.ShapeBase == ShapeBase.Rectangle)
            {
                return bounds.Rectangle.Contains(px, py);
            }
            else if (ShapeDefinition.ShapeBase == ShapeBase.Ellipse)
            {
                Point cp = bounds.CenterPoint;
                double e1 = (px - cp.X) / (bounds.Right - cp.X);
                double e2 = (py - cp.Y) / (bounds.Bottom - cp.Y);
                return (e1 * e1 + e2 * e2) < 1;
            }
            else if (ShapeDefinition.ShapeBase == ShapeBase.Pico)
            {
                Point cp = bounds.CenterPoint;
                double e1 = (px - cp.X) / (bounds.Right - cp.X);
                double e2 = (py - cp.Y) / (bounds.Bottom - cp.Y);
                return (Math.Abs(e1) + Math.Abs(e2)) < 1;
            }
            else
            {
                return bounds.Rectangle.Contains(px, py);
            }
        }

        public static double GetDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }


        public override bool Load(XmlElement elem, CDReaderReferences refs)
        {
            foreach (XmlElement e in CDXml.GetChildren(elem, "bounds"))
                bounds.Load(e);

            return base.Load(elem, refs);
        }

        public override bool Save(XmlElement elem, XmlDocument doc)
        {
            XmlElement bndNode = doc.CreateElement("bounds");

            elem.AppendChild(bndNode);
            bounds.Save(bndNode);


            return base.Save(elem, doc);
        }
    }

    public enum ShapeBase
    {
        Ellipse,
        Rectangle,
        Pico
    }
}
