using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MiniCASE
{
    public class CDContext
    {
        public Graphics Graphics { get; set; }

        public CDDiagram Diagram { get; set; }

        public Point Offset { get; set; }
        public float Scale { get; set; }
        public CDShape SelectedItem { get; set; }
        public CDShape TrackedItem { get; set; }

        public Image IconConnectionStart { get; set; }
        public Image IconResize { get; set; }

        public Pen SelectionPen { get; set; }

        public Color ColorBackSelected { get; set; }
        public Color ColorLineSelected { get; set; }
        public Color ColorTextSelected { get; set; }
        public int WidthLineSelected { get; set; }
        public Color ColorBackTracked { get; set; }
        public Color ColorLineTracked { get; set; }
        public Color ColorTextTracked { get; set; }
        public int WidthLineTracked { get; set; }

        public Point StartPoint = new Point();
        public Point MovePoint = new Point();

        public Point StartPointP = new Point();
        public Point MovePointP = new Point();
        public Point mouseOffsetBackup = new Point();

        public Rectangle mouseAreaResizeBottomRight = Rectangle.Empty;
        public Rectangle mouseAreaResizeTopLeft = Rectangle.Empty;
        public Rectangle mouseAreaReconnectStart = Rectangle.Empty;
        public Rectangle mouseAreaReconnectEnd = Rectangle.Empty;
        public Point hitOffset = Point.Empty;

        public CDContext()
        {
            Graphics = null;
            Offset = Point.Empty;
            SelectedItem = null;

            ColorBackSelected = Color.LightYellow;
            ColorLineSelected = Color.Brown;
            ColorTextSelected = Color.Brown;
            WidthLineSelected = 2;

            ColorBackTracked = Color.LightBlue;
            ColorLineTracked = Color.BlueViolet;
            ColorTextTracked = Color.BlueViolet;
            WidthLineTracked = 2;

            SelectionPen = new Pen(Color.FromArgb(190, Color.LightYellow), 20f);
            SelectionPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            SelectionPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            SelectionPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
        }
    }
}
