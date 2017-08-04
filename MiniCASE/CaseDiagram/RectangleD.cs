using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;

namespace MiniCASE
{
    public class RectangleD
    {
        public int Left = 0;
        public int Right = 0;
        public int Top = 0;
        public int Bottom = 0;

        public RectangleD()
        {
        }

        public RectangleD(int left, int top, int right, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public Point CenterPoint
        {
            get
            {
                return new Point((Left + Right) / 2, (Top + Bottom) / 2);
            }
            set
            {
                int cx = (Left + Right) / 2;
                int cy = (Top + Bottom) / 2;
                int width = this.Width/2;
                int height = this.Height/2;

                Left = Left + (value.X - cx);
                Right = Right + (value.X - cx);
                Bottom = Bottom + (value.Y - cy);
                Top = Top + (value.Y - cy);
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(Math.Min(Left, Right), Math.Min(Top, Bottom), Math.Abs(Left - Right), Math.Abs(Top - Bottom));
            }
            set
            {
                Left = value.Left;
                Top = value.Top;
                Right = value.Right;
                Bottom = value.Bottom;
            }
        }

        public void Merge(RectangleD rect)
        {
            Left = Math.Min(Left, rect.Left);
            Top = Math.Min(Top, rect.Top);
            Right = Math.Max(Right, rect.Right);
            Bottom = Math.Max(Bottom, rect.Bottom);
        }

        public void Set(RectangleD rect)
        {
            Left = rect.Left;
            Right = rect.Right;
            Top = rect.Top;
            Bottom = rect.Bottom;
        }

        public int Width
        {
            get
            {
                return Math.Abs(Left - Right);
            }
            set
            {
                int currentWidth = Math.Abs(Left - Right);
                Left = Left - (value - currentWidth)/2;
                Right = Left + value;
            }
        }

        public int Height
        {
            get
            {
                return Math.Abs(Top - Bottom);
            }
            set
            {
                int currentHeight = Math.Abs(Top - Bottom);
                Top = Top - (value - currentHeight) / 2;
                Bottom = Top + value;
            }
        }

        public bool ContainsPoint(int px, int py)
        {
            return ((Left <= px) && (Right >= px) && (Top <= py) && (Bottom >= py)) ;
        }

        public bool IntersectsWith(RectangleD rd)
        {
            return ((Left <= rd.Right) && (Right >= rd.Left) && (Top <= rd.Bottom) && (Bottom >= rd.Top));
        }

        public Point GetRelativePoint(Point point)
        {
            if (Right == Left || Top == Bottom)
                return Point.Empty;
            return new Point(100*(point.X - Left) / (Right - Left), 100 * (point.Y - Top) / (Bottom - Top) );
        }

        public Point GetLogicalPoint(Point point)
        {
            return new Point(point.X * Width / 100 + Left, point.Y * Height / 100 + Top);
        }

        public Point TopLeft
        {
            get { return new Point(Left, Top); }
        }
        public Point TopRight
        {
            get { return new Point(Right, Top); }
        }
        public Point BottomLeft
        {
            get { return new Point(Left, Bottom); }
        }
        public Point BottomRight
        {
            get { return new Point(Right, Bottom); }
        }
        public Point TopCenter
        {
            get { return new Point((Left + Right)/2, Top); }
        }
        public Point CenterRight
        {
            get { return new Point(Right, (Top + Bottom)/2); }
        }
        public Point BottomCenter
        {
            get { return new Point((Left + Right)/2, Bottom); }
        }
        public Point CenterLeft
        {
            get { return new Point(Left, (Top + Bottom)/2); }
        }

        public void Save(XmlElement elem)
        {
            elem.SetAttribute("Top", Top.ToString());
            elem.SetAttribute("Right", Right.ToString());
            elem.SetAttribute("Bottom", Bottom.ToString());
            elem.SetAttribute("Left", Left.ToString());
        }

        public void Load(XmlElement elem)
        {
            if (elem.HasAttribute("Top"))
                Top = int.Parse(elem.GetAttribute("Top"));
            if (elem.HasAttribute("Bottom"))
                Bottom = int.Parse(elem.GetAttribute("Bottom"));
            if (elem.HasAttribute("Left"))
                Left = int.Parse(elem.GetAttribute("Left"));
            if (elem.HasAttribute("Right"))
                Right = int.Parse(elem.GetAttribute("Right"));
        }




    }
}
