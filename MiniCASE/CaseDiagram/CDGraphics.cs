using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MiniCASE
{
    public class CDGraphics
    {
        public static Brush connectionsBrush = Brushes.Green;
        public static int connectionMarkWidth = 8;
        public static int connectionMarkHeight = 8;

        public static Dictionary<int, SolidBrush> brushes = new Dictionary<int, SolidBrush>();
        public static Dictionary<float, Dictionary<DashStyle, Dictionary<Color, Pen>>> pens = new Dictionary<float, Dictionary<DashStyle, Dictionary<Color, Pen>>>();
        public static Dictionary<string, Font> fonts = new Dictionary<string, Font>();

        public static void DrawShape(CDContext ctx, CDShape shape)
        {
            DrawShape(ctx, shape, shape.Bounds.Rectangle);
        }

        public static void DrawShape(CDContext ctx, CDShape shape, Rectangle bounds)
        {
            CDObject[] dataSources = { };
            CSParameterSource paramSource = new CSParameterSource();
            CDShapeDefinition def = shape.ShapeDefinition;
            if (shape.Decomposition != null)
            {
                CDDocumentDefinition dd = shape.Decomposition.DiagramDefinition;
                if (dd.RepresentShape != null)
                {
                    def = dd.RepresentShape;
                    paramSource.Add(def);
                    paramSource.Add(shape.Decomposition);
                    paramSource.Add(shape);
                    paramSource.Add(shape.Decomposition.DiagramDefinition);
                    paramSource.Add(shape.Definition);
                }
                else
                {
                    paramSource.Add(shape.Decomposition);
                    paramSource.Add(shape);
                    paramSource.Add(shape.Decomposition.DiagramDefinition);
                    paramSource.Add(shape.Definition);
                }
            }
            else
            {
                paramSource.Add(shape);
                paramSource.Add(shape.Definition);
            }

            if (def == null)
                def = CDLibrary.DefaultShape;

            Rectangle rect = Rectangle.Empty;
            Triangle triangle = Triangle.Empty;
            Point point = Point.Empty;
            Color clr = Color.White;
            float width = 1;
            string text = string.Empty;
            CSTextAlign textAlign;
            CSTextPadding padding = new CSTextPadding();
            Image img = null;

            foreach (CDInstruction instruction in def.cmds)
            {
                switch (instruction.Command)
                {
                    case CIKey.FillTriangle:
                        paramSource.TryExtractRelativeTriangle(bounds, ref triangle, instruction);
                        paramSource.TryExtractColor(ref clr, instruction);
                        CorrectBackColor(ctx, shape, ref clr);
                        ctx.Graphics.FillPolygon(GetBrush(clr), triangle.Polygon);
                        break;
                    case CIKey.DrawTriangle:
                        paramSource.TryExtractRelativeTriangle(bounds, ref triangle, instruction);
                        paramSource.TryExtractColor(ref clr, instruction);
                        paramSource.TryExtractWidth(def, ref width, instruction);
                        CorrectLineColor(ctx, shape, ref clr, ref width);
                        ctx.Graphics.DrawPolygon(GetPen(clr, width, DashStyle.Solid), triangle.Polygon);
                        break;
                    case CIKey.FillRectangle:
                        paramSource.TryExtractRelativeRect(bounds, ref rect, instruction);
                        paramSource.TryExtractColor(ref clr, instruction);
                        CorrectBackColor(ctx, shape, ref clr);
                        ctx.Graphics.FillRectangle(GetBrush(clr), rect);
                        break;
                    case CIKey.DrawRectangle:
                        paramSource.TryExtractRelativeRect(bounds, ref rect, instruction);
                        paramSource.TryExtractColor(ref clr, instruction);
                        paramSource.TryExtractWidth(def, ref width, instruction);
                        CorrectLineColor(ctx, shape, ref clr, ref width);
                        ctx.Graphics.DrawRectangle(GetPen(clr, width, DashStyle.Solid), rect);
                        break;
                    case CIKey.FillEllipse:
                        paramSource.TryExtractRelativeRect(bounds, ref rect, instruction);
                        paramSource.TryExtractColor(ref clr, instruction);
                        CorrectBackColor(ctx, shape, ref clr);
                        ctx.Graphics.FillEllipse(GetBrush(clr), rect);
                        break;
                    case CIKey.DrawEllipse:
                        paramSource.TryExtractRelativeRect(bounds, ref rect, instruction);
                        paramSource.TryExtractColor(ref clr, instruction);
                        paramSource.TryExtractWidth(def, ref width, instruction);
                        CorrectLineColor(ctx, shape, ref clr, ref width);
                        ctx.Graphics.DrawEllipse(GetPen(clr, width, DashStyle.Solid), rect);
                        break;
                    case CIKey.SetBoundsByFitImage:
                        paramSource.TryExtractImage(out img, instruction);
                        if (img != null)
                        {
                            double rat = Math.Min((double)img.Width / bounds.Width, (double)img.Height / bounds.Height);
                            if (rat < 0.5) rat = 0.5;
                            Rectangle imgRect = new Rectangle(0, 0, Convert.ToInt32(img.Width / rat), Convert.ToInt32(img.Height / rat));
                            imgRect.X = (bounds.Left + bounds.Right - imgRect.Width) / 2;
                            imgRect.Y = (bounds.Top + bounds.Bottom - imgRect.Height) / 2;
                            bounds = imgRect;
                        }
                        break;
                    case CIKey.SetBoundsByFillImage:
                        paramSource.TryExtractImage(out img, instruction);
                        if (img != null)
                        {
                            double rat = Math.Max((double)img.Width / bounds.Width, (double)img.Height / bounds.Height);
                            if (rat < 0.5) rat = 0.5;
                            Rectangle imgRect = new Rectangle(0, 0, Convert.ToInt32(img.Width / rat), Convert.ToInt32(img.Height / rat));
                            imgRect.X = (bounds.Left + bounds.Right - imgRect.Width) / 2;
                            imgRect.Y = (bounds.Top + bounds.Bottom - imgRect.Height) / 2;
                            bounds = imgRect;
                        }
                        break;
                    case CIKey.DrawImage:
                        paramSource.TryExtractImage(out img, instruction);
                        if (img != null)
                        {
                            ctx.Graphics.DrawImage(img, bounds);
                        }
                        break;
                    case CIKey.DrawText:
                        textAlign = CSTextAlign.Center;
                        width = 10;
                        bool byPoint = false;
                        bool byRect = false;
                        padding.Clear();
                        byPoint = paramSource.TryExtractRelativePoint(bounds, ref point, instruction);
                        if (!byPoint)
                            byRect = paramSource.TryExtractRelativeRect(bounds, ref rect, instruction);
                        else
                            rect = shape.Bounds.Rectangle;
                        paramSource.TryExtractColor(ref clr, instruction);
                        paramSource.TryExtractText(ref text, instruction);
                        paramSource.TryExtractFloat(def, ref width, instruction, CIParam.FontSizeId, CIParam.FontSize);
                        paramSource.TryExtractTextAlign(ref textAlign, instruction);
                        paramSource.TryExtractTextPadding(def, CIParam.TextPadding, ref padding, instruction);
                        width = Math.Max(width, 10f);

                        CorrectTextColor(ctx, shape, ref clr);
                        Font fnt = GetFont(FontFamily.GenericSansSerif, width);
                        SizeF textSize = ctx.Graphics.MeasureString(text, fnt);
                        StringFormat sf = new StringFormat();
                        Rectangle textRect = new Rectangle(0, 0, (int)textSize.Width + 2, (int)textSize.Height + 2);
                        if (byRect)
                        {
                            EvaluateTextRectangleAligned(ref rect, textAlign, padding, sf, ref textRect);
                        }
                        else if (byPoint)
                        {
                            EvaluateTextPointAligned(ref point, textAlign, sf, ref textRect);
                        }
                        ctx.Graphics.DrawString(text, GetFont(FontFamily.GenericSansSerif, width),
                            GetBrush(clr), textRect, sf);
                        break;
                }

            }


        }

        private static void EvaluateTextPointAligned(ref Point point, CSTextAlign textAlign, StringFormat sf, ref Rectangle textRect)
        {
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;
            switch (textAlign)
            {
                case CSTextAlign.Bottom:
                case CSTextAlign.BottomBottom:
                    textRect.X = point.X - textRect.Width / 2;
                    textRect.Y = point.Y + 4;
                    break;
                case CSTextAlign.BottomLeft:
                case CSTextAlign.BottomLeftBottom:
                case CSTextAlign.BottomLeftLeft:
                case CSTextAlign.BottomLeftOutside:
                    textRect.X = point.X - textRect.Width - 4;
                    textRect.Y = point.Y + 4;
                    break;
                case CSTextAlign.BottomRight:
                case CSTextAlign.BottomRightBottom:
                case CSTextAlign.BottomRightOutside:
                case CSTextAlign.BottomRightRight:
                    textRect.X = point.X + 4;
                    textRect.Y = point.Y + 4;
                    break;
                case CSTextAlign.Center:
                    textRect.X = point.X - textRect.Width / 2;
                    textRect.Y = point.Y - textRect.Height / 2;
                    break;
                case CSTextAlign.Left:
                case CSTextAlign.LeftLeft:
                    textRect.X = point.X - textRect.Width - 4;
                    textRect.Y = point.Y - textRect.Height / 2;
                    break;
                case CSTextAlign.Right:
                case CSTextAlign.RightRight:
                    textRect.X = point.X + 4;
                    textRect.Y = point.Y - textRect.Height / 2;
                    break;
                case CSTextAlign.Top:
                case CSTextAlign.TopTop:
                    textRect.X = point.X - textRect.Width / 2;
                    textRect.Y = point.Y - textRect.Height - 4;
                    break;
                case CSTextAlign.TopLeft:
                case CSTextAlign.TopLeftLeft:
                case CSTextAlign.TopLeftOutside:
                case CSTextAlign.TopLeftTop:
                    textRect.X = point.X - textRect.Width - 4;
                    textRect.Y = point.Y - textRect.Height - 4;
                    break;
                case CSTextAlign.TopRight:
                case CSTextAlign.TopRightOutside:
                case CSTextAlign.TopRightRight:
                case CSTextAlign.TopRightTop:
                    textRect.X = point.X + 4;
                    textRect.Y = point.Y - textRect.Height - 4;
                    break;
            }
        }

        private static void EvaluateTextRectangleAligned(ref Rectangle rect, CSTextAlign textAlign, CSTextPadding padding, StringFormat sf, ref Rectangle textRect)
        {
            sf.LineAlignment = StringAlignment.Center;

            // X - axis
            switch(textAlign)
            {
                case CSTextAlign.TopTop:
                case CSTextAlign.Top:
                case CSTextAlign.Center:
                case CSTextAlign.Bottom:
                case CSTextAlign.BottomBottom:
                    textRect.X = (rect.Left + rect.Right - textRect.Width) / 2;
                    sf.Alignment = StringAlignment.Center;
                    break;
                case CSTextAlign.BottomLeftBottom:
                case CSTextAlign.BottomLeft:
                case CSTextAlign.Left:
                case CSTextAlign.TopLeft:
                case CSTextAlign.TopLeftTop:
                    textRect.X = rect.Left + padding.Left;
                    sf.Alignment = StringAlignment.Near;
                    break;
                case CSTextAlign.BottomLeftOutside:
                case CSTextAlign.BottomLeftLeft:
                case CSTextAlign.LeftLeft:
                case CSTextAlign.TopLeftLeft:
                case CSTextAlign.TopLeftOutside:
                    textRect.X = rect.Left - textRect.Width - padding.Right;
                    sf.Alignment = StringAlignment.Far;
                    break;
                case CSTextAlign.TopRight:
                case CSTextAlign.TopRightTop:
                case CSTextAlign.Right:
                case CSTextAlign.BottomRight:
                case CSTextAlign.BottomRightBottom:
                    textRect.X = rect.Right - padding.Left - textRect.Width;
                    sf.Alignment = StringAlignment.Far;
                    break;
                case CSTextAlign.BottomRightOutside:
                case CSTextAlign.BottomRightRight:
                case CSTextAlign.RightRight:
                case CSTextAlign.TopRightOutside:
                case CSTextAlign.TopRightRight:
                    textRect.X = rect.Right + padding.Left;
                    sf.Alignment = StringAlignment.Near;
                    break;
            }

            // Y-axis
            switch (textAlign)
            {
                case CSTextAlign.TopLeftOutside:
                case CSTextAlign.TopLeftTop:
                case CSTextAlign.TopTop:
                case CSTextAlign.TopRightTop:
                case CSTextAlign.TopRightOutside:
                    textRect.Y = rect.Top - textRect.Height - padding.Bottom;
                    sf.LineAlignment = StringAlignment.Far;
                    break;
                case CSTextAlign.BottomBottom:
                case CSTextAlign.BottomLeftBottom:
                case CSTextAlign.BottomLeftOutside:
                case CSTextAlign.BottomRightBottom:
                case CSTextAlign.BottomRightOutside:
                    textRect.Y = rect.Bottom + padding.Top;
                    sf.LineAlignment = StringAlignment.Near;
                    break;
                case CSTextAlign.BottomRight:
                case CSTextAlign.BottomRightRight:
                case CSTextAlign.BottomLeftLeft:
                case CSTextAlign.BottomLeft:
                case CSTextAlign.Bottom:
                    textRect.Y = rect.Bottom - padding.Bottom - textRect.Height;
                    sf.LineAlignment = StringAlignment.Near;
                    break;
                case CSTextAlign.Center:
                case CSTextAlign.Left:
                case CSTextAlign.LeftLeft:
                case CSTextAlign.Right:
                case CSTextAlign.RightRight:
                    textRect.Y = (rect.Top + rect.Bottom - textRect.Height) / 2;
                    sf.LineAlignment = StringAlignment.Center;
                    break;
                case CSTextAlign.TopLeftLeft:
                case CSTextAlign.Top:
                case CSTextAlign.TopLeft:
                case CSTextAlign.TopRight:
                case CSTextAlign.TopRightRight:
                    textRect.Y = rect.Top + padding.Top;
                    sf.LineAlignment = StringAlignment.Near;
                    break;
            }

        }

        private static void CorrectTextColor(CDContext ctx, CDShape shape, ref Color clr)
        {
            if (ctx.TrackedItem == shape || shape.Highlighted)
                clr = ctx.ColorTextTracked;
            /*else if (shape.Selected)
                clr = ctx.ColorTextSelected;*/
        }

        private static void CorrectBackColor(CDContext ctx, CDShape shape, ref Color clr)
        {
            if (ctx.TrackedItem == shape || shape.Highlighted)
                clr = ctx.ColorBackTracked;
            /*else if (shape.Selected)
                clr = ctx.ColorBackSelected;*/
        }

        private static void CorrectLineColor(CDContext ctx, CDShape shape, ref Color clr, ref float width)
        {
            if (ctx.TrackedItem == shape || shape.Highlighted)
            {
                clr = ctx.ColorLineTracked;
                width = ctx.WidthLineTracked;
            }
            /*else if (shape.Selected)
            {
                clr = ctx.ColorLineSelected;
                width = ctx.WidthLineSelected;
            }*/
        }







        public static void DrawDefaultShape(CDContext ctx, CDShape shape)
        {
            Rectangle rect = shape.Bounds.Rectangle;
            rect.Offset(ctx.Offset);
            ctx.Graphics.FillRectangle(Brushes.WhiteSmoke, rect);
            ctx.Graphics.DrawRectangle(shape.Selected ? GetPen("Black", 2) : GetPen("Black", 1), rect);
        }

        public static void DrawSelectedMarks(CDContext ctx, CDShape shape)
        {
            if (!shape.Selected)
                return;

            Graphics g = ctx.Graphics;

            //g.DrawImageUnscaled(ctx.IconConnectionStart, shape.p_xl - 28, shape.p_yt + 4);

            /*g.DrawRectangle(GetPen(ctx.ColorLineSelected, ctx.WidthLineSelected, DashStyle.Solid), shape.Bounds.Left - 4,
                shape.Bounds.Top - 4, shape.Bounds.Width + 8, shape.Bounds.Height + 8);*/

            if (shape.ShapeDefinition.AutomaticBounds)
            {
                ctx.mouseAreaResizeTopLeft = Rectangle.Empty;
                ctx.mouseAreaResizeBottomRight = Rectangle.Empty;
            }
            else
            {
                ctx.mouseAreaResizeTopLeft.X = shape.Bounds.Left - 28;
                ctx.mouseAreaResizeTopLeft.Y = shape.Bounds.Top - 28;
                ctx.mouseAreaResizeTopLeft.Width = 24;
                ctx.mouseAreaResizeTopLeft.Height = 24;
                g.DrawImageUnscaled(ctx.IconResize, shape.Bounds.Left - 28, shape.Bounds.Top - 28);

                ctx.mouseAreaResizeBottomRight.X = shape.Bounds.Right + 4;
                ctx.mouseAreaResizeBottomRight.Y = shape.Bounds.Bottom + 4;
                ctx.mouseAreaResizeBottomRight.Width = 24;
                ctx.mouseAreaResizeBottomRight.Height = 24;
                g.DrawImageUnscaled(ctx.IconResize, shape.Bounds.Right + 4, shape.Bounds.Bottom + 4);
            }
        }


        public static Brush GetBrush(string name)
        {
            return GetBrush(Color.FromName(name));
        }

        public static Brush GetBrush(Color clr)
        {
            int key = clr.ToArgb();
            if (!brushes.ContainsKey(key))
            {
                brushes.Add(key, new SolidBrush(clr));
            }

            return brushes[key];
        }

        public static Pen GetPen(string name, float width)
        {
            return GetPen(Color.FromName(name), width, DashStyle.Solid);
        }

        public static Pen GetPen(Color clr, float width, DashStyle ds)
        {
            Dictionary<DashStyle, Dictionary<Color, Pen>> L2;
            Dictionary<Color, Pen> L3;

            Pen p;
            if (!pens.ContainsKey(width))
            {
                pens[width] = L2 = new Dictionary<DashStyle, Dictionary<Color, Pen>>();
            }
            else
            {
                L2 = pens[width];
            }

            if (!L2.ContainsKey(ds))
            {
                pens[width][ds] = L3 = new Dictionary<Color, Pen>();
            }
            else
            {
                L3 = pens[width][ds];
            }

            if (L3.ContainsKey(clr))
            {
                return L3[clr];
            }

            p = new Pen(clr, width);
            p.DashStyle = ds;
            L3[clr] = p;

            return p;
        }

        public static Font GetFont(FontFamily baseFont, float size)
        {
            string key = string.Format("{0}-{1}", baseFont.Name, size);
            if (!fonts.ContainsKey(key))
            {
                fonts.Add(key, new Font(baseFont, size));
            }
            return fonts[key];
        }

        public static byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public static Image ByteArrayToImage(byte[] b)
        {
            try
            {
                using (var ms = new System.IO.MemoryStream(b))
                {
                    Image img = Image.FromStream(ms);
                    return img;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
