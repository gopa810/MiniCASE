using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace MiniCASE
{
    /// <summary>
    /// contains and handles data dedicated to one shape in diagram
    /// </summary>
    public class CaseShape
    {
        private RectangleD bounds;
        private RectangleD boundsBack;
        public int id = 0;
        public ShapeBase ShapeBase = ShapeBase.Rectangle;
        public string ShapeReference = "MiniCase.Shape.Process";
        public ArrayList matrixAreas = new ArrayList();
        private bool selected = false;
        private bool highlighted = false;

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }

        public bool Highlighted
        {
            get
            {
                return highlighted;
            }
            set
            {
                highlighted = value;
            }
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
                bounds = value;
            }
        }

        public void SavePosition()
        {
            boundsBack = new RectangleD();
            boundsBack.Rectangle = bounds.Rectangle;
        }

        public void SetOffset(Size offset)
        {
            if (boundsBack == null)
                return;

            //Debugger.Log(0, "", string.Format("Moving {0} to {1}", boundsBack.Xa, boundsBack.Xa + offset.Width));
            bounds.Xa = boundsBack.Xa + offset.Width;
            bounds.Xb = boundsBack.Xb + offset.Width;
            bounds.Ya = boundsBack.Ya + offset.Height;
            bounds.Yb = boundsBack.Yb + offset.Height;
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
        public Point GetBorderPointForRectangle(double X, double Y)
        {
            Point cp = bounds.CenterPoint;
            Point intPoint = new Point();
            double Xc = X - cp.X;
            double Yc = Y - cp.Y;
            double rat = Convert.ToDouble(bounds.Height) / Convert.ToDouble(bounds.Width);
            Xc *= rat;

            if (Xc * Xc + Yc * Yc == 0)
                return cp;

            if (Xc + Yc > 0) // B or C
            {
                if (Xc < Yc) // C
                {
                    intPoint.X = cp.X + Convert.ToInt32((bounds.Yb - cp.Y) * Xc / (Yc * rat));
                    intPoint.Y = bounds.Yb;
                }
                else // D
                {
                    intPoint.X = bounds.Xb;
                    intPoint.Y = cp.Y + Convert.ToInt32((bounds.Xb - cp.X) * Yc * rat / Xc);
                }
            }
            else // A or D
            {
                if (Xc < Yc) // B
                {
                    intPoint.X = bounds.Xa;
                    intPoint.Y = cp.Y + Convert.ToInt32((bounds.Xa - cp.X) * Yc * rat / Xc);
                }
                else// A
                {
                    intPoint.X = cp.X + Convert.ToInt32((bounds.Ya - cp.Y) * Xc / (Yc * rat));
                    intPoint.Y = bounds.Ya;
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
            double a = (bounds.Xb - cp.X);
            double b = (bounds.Yb - cp.Y);
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
            double a = (bounds.Xb - cp.X);
            double b = (bounds.Yb - cp.Y);
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
            if (ShapeBase == MiniCASE.ShapeBase.Rectangle)
            {
                return GetBorderPointForRectangle(X, Y);
            }
            else if (ShapeBase == MiniCASE.ShapeBase.Ellipse)
            {
                return GetBorderPointForEllipsis(X, Y);
            }
            else if (ShapeBase == MiniCASE.ShapeBase.Pico)
            {
                return GetBorderPointForPico(X, Y);
            }
            else
            {
                return bounds.CenterPoint;
            }
        }

        /// <summary>
        /// Returns border point on current shape, which is used for connection 
        /// to the given shape 'sh'
        /// </summary>
        /// <param name="sh"></param>
        /// <returns></returns>
        public Point GetBorderPoint(CaseShape sh)
        {
            Point pt = sh.Bounds.CenterPoint;
            return GetBorderPoint(pt.X, pt.Y);
        }

        public bool ContainsPoint(Point pt)
        {
            if (ShapeBase == MiniCASE.ShapeBase.Rectangle)
            {
                return bounds.Rectangle.Contains(pt);
            }
            else if (ShapeBase == MiniCASE.ShapeBase.Ellipse)
            {
                Point cp = bounds.CenterPoint;
                double e1 = (pt.X - cp.X) / (bounds.Xb - cp.X);
                double e2 = (pt.Y - cp.Y) / (bounds.Yb - cp.Y);
                return (e1*e1 + e2*e2) < 1;
            }
            else if (ShapeBase == MiniCASE.ShapeBase.Pico)
            {
                Point cp = bounds.CenterPoint;
                double e1 = (pt.X - cp.X) / (bounds.Xb - cp.X);
                double e2 = (pt.Y - cp.Y) / (bounds.Yb - cp.Y);
                return (Math.Abs(e1) + Math.Abs(e2)) < 1;
            }
            else
            {
                return bounds.Rectangle.Contains(pt);
            }
        }

        public static double GetDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public enum ShapeBase
    {
        Ellipse,
        Rectangle,
        Pico
    }
}
