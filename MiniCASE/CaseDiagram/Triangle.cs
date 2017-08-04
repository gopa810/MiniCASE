using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MiniCASE
{
    public struct Triangle
    {
        private bool is_empty;
        public int AX, AY, BX, BY, CX, CY;

        public Triangle(int ax, int ay, int bx, int by, int cx, int cy)
        {
            is_empty = false;
            AX = ax;
            AY = ay;
            BX = bx;
            BY = by;
            CX = cx;
            CY = cy;
        }

        private static Triangle p_empty = new Triangle(0, 0, 0, 0, 0, 0);
        public static Triangle Empty
        {
            get
            {
                p_empty.is_empty = true;
                return p_empty;
            }
        }

        public bool IsEmpty
        {
            get { return is_empty; }
        }

        public Point A
        {
            get { return new Point(AX, AY); }
            set { AX = value.X; AY = value.Y; }
        }
        public Point B
        {
            get { return new Point(BX, BY); }
            set { BX = value.X; BY = value.Y; }
        }

        public Point C
        {
            get { return new Point(CX, CY); }
            set { CX = value.X; CY = value.Y; }
        }

        public Point[] Polygon
        {
            get { return new Point[] { A, B, C }; }
        }
    }
}
