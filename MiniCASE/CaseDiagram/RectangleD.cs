using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MiniCASE
{
    public class RectangleD
    {
        public int Xa = 0;
        public int Xb = 0;
        public int Ya = 0;
        public int Yb = 0;

        public RectangleD()
        {
        }

        public RectangleD(int left, int top, int right, int bottom)
        {
            Xa = left;
            Xb = right;
            Ya = top;
            Yb = bottom;
        }

        public Point CenterPoint
        {
            get
            {
                return new Point((Xa + Xb) / 2, (Ya + Yb) / 2);
            }
            set
            {
                int cx = (Xa + Xb) / 2;
                int cy = (Ya + Yb) / 2;
                int width = this.Width/2;
                int height = this.Height/2;

                Xa = Xa + (value.X - cx);
                Xb = Xb + (value.X - cx);
                Yb = Yb + (value.Y - cy);
                Ya = Ya + (value.Y - cy);
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(Math.Min(Xa, Xb), Math.Min(Ya, Yb), Math.Abs(Xa - Xb), Math.Abs(Ya - Yb));
            }
            set
            {
                Xa = value.Left;
                Ya = value.Top;
                Xb = value.Right;
                Yb = value.Bottom;
            }
        }

        public void Merge(RectangleD rect)
        {
            Xa = Math.Min(Xa, rect.Xa);
            Ya = Math.Min(Ya, rect.Ya);
            Xb = Math.Max(Xb, rect.Xb);
            Yb = Math.Max(Yb, rect.Yb);
        }

        public void Set(RectangleD rect)
        {
            Xa = rect.Xa;
            Xb = rect.Xb;
            Ya = rect.Ya;
            Yb = rect.Yb;
        }

        public int Width
        {
            get
            {
                return Math.Abs(Xa - Xb);
            }
            set
            {
                int currentWidth = Math.Abs(Xa - Xb);
                Xa = Xa - (value - currentWidth)/2;
                Xb = Xa + value;
            }
        }

        public int Height
        {
            get
            {
                return Math.Abs(Ya - Yb);
            }
            set
            {
                int currentHeight = Math.Abs(Ya - Yb);
                Ya = Ya - (value - currentHeight) / 2;
                Yb = Ya + value;
            }
        }

        public bool ContainsPoint(int px, int py)
        {
            return ((Xa <= px) && (Xb >= px) && (Ya <= py) && (Yb >= py)) ;
        }
    }
}
