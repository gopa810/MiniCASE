using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MiniCASE
{
    public class ShapesLibrary
    {
        public static Brush connectionsBrush = Brushes.Green;
        public static int connectionMarkWidth = 8;
        public static int connectionMarkHeight = 8;

        public static Dictionary<string, ShapeDefinition> shapes = new Dictionary<string,ShapeDefinition>();
        public static Color[] colors =  new Color[] {
                Color.AliceBlue,	Color.DarkSlateGray,	Color.LightSalmon,	Color.PaleVioletRed,
                Color.AntiqueWhite,	Color.DarkTurquoise,	Color.LightSeaGreen,	Color.PapayaWhip,
                Color.Aqua,	Color.DarkViolet,	Color.LightSkyBlue,	Color.PeachPuff,
                Color.Aquamarine,	Color.DeepPink,	Color.LightSlateGray,	Color.Peru,
                Color.Azure,	Color.DeepSkyBlue,	Color.LightSteelBlue,	Color.Pink,
                Color.Beige,	Color.DimGray,	Color.LightYellow,	Color.Plum,
                Color.Bisque,	Color.DodgerBlue,	Color.Lime,	Color.PowderBlue,
                Color.Black,	Color.Firebrick,	Color.LimeGreen,	Color.Purple,
                Color.BlanchedAlmond,	Color.FloralWhite,	Color.Linen,	Color.Red,
                Color.Blue,	Color.ForestGreen,	Color.Magenta,	Color.RosyBrown,
                Color.BlueViolet,	Color.Fuchsia,	Color.Maroon,	Color.RoyalBlue,
                Color.Brown,	Color.Gainsboro,	Color.MediumAquamarine,	Color.SaddleBrown,
                Color.BurlyWood,	Color.GhostWhite,	Color.MediumBlue,	Color.Salmon,
                Color.CadetBlue,	Color.Gold,	Color.MediumOrchid,	Color.SandyBrown,
                Color.Chartreuse,	Color.Goldenrod,	Color.MediumPurple,	Color.SeaGreen,
                Color.Chocolate,	Color.Gray,	Color.MediumSeaGreen,	Color.SeaShell,
                Color.Coral,	Color.Green,	Color.MediumSlateBlue,	Color.Sienna,
                Color.CornflowerBlue,	Color.GreenYellow,	Color.MediumSpringGreen,	Color.Silver,
                Color.Cornsilk,	Color.Honeydew,	Color.MediumTurquoise,	Color.SkyBlue,
                Color.Crimson,	Color.HotPink,	Color.MediumVioletRed,	Color.SlateBlue,
                Color.Cyan,	Color.IndianRed,	Color.MidnightBlue,	Color.SlateGray,
                Color.DarkBlue,	Color.Indigo,	Color.MintCream,	Color.Snow,
                Color.DarkCyan,	Color.Ivory,	Color.MistyRose,	Color.SpringGreen,
                Color.DarkGoldenrod,	Color.Khaki,	Color.Moccasin,	Color.SteelBlue,
                Color.DarkGray,	Color.Lavender,	Color.NavajoWhite,	Color.Tan,
                Color.DarkGreen,	Color.LavenderBlush,	Color.Navy,	Color.Teal,
                Color.DarkKhaki,	Color.LawnGreen,	Color.OldLace,	Color.Thistle,
                Color.DarkMagenta,	Color.LemonChiffon,	Color.Olive,	Color.Tomato,
                Color.DarkOliveGreen,	Color.LightBlue,	Color.OliveDrab,	Color.Turquoise,
                Color.DarkOrange,	Color.LightCoral,	Color.Orange,	Color.Violet,
                Color.DarkOrchid,	Color.LightCyan,	Color.OrangeRed,	Color.Wheat,
                Color.DarkRed,	Color.LightGoldenrodYellow,	Color.Orchid,	Color.White,
                Color.DarkSalmon,	Color.LightGreen,	Color.PaleGoldenrod,	Color.WhiteSmoke,
                Color.DarkSeaGreen,	Color.LightGray,	Color.PaleGreen,	Color.Yellow,
                Color.DarkSlateBlue,	Color.LightPink,	Color.PaleTurquoise,	Color.YellowGreen
            };
        public static Dictionary<string,SolidBrush> brushes = new Dictionary<string,SolidBrush>();
        public static Dictionary<string, Pen> pens = new Dictionary<string, Pen>();

        public static bool CanUseCommand(bool shapeHigh, bool shapeSel, ShapeDrawCommandCondition commandCond)
        {
            if (commandCond == ShapeDrawCommandCondition.Allways)
                return true;

            switch (commandCond)
            {
                case ShapeDrawCommandCondition.Highlighted:
                    return shapeHigh;
                case ShapeDrawCommandCondition.NotHighlighted:
                    return !shapeHigh;
                case ShapeDrawCommandCondition.Selected:
                    return shapeSel;
                case ShapeDrawCommandCondition.NotSelected:
                    return !shapeSel;
            }

            return true;
        }

        public static void DrawShape(Graphics g, CaseShape shape, Point offset)
        {
            if (shapes.Count == 0)
            {
                InitializeShapes();
            }

            if (!shapes.ContainsKey(shape.ShapeReference))
            {
                DrawDefaultShape(g, shape, offset);
            }
            else
            {
                ShapeDefinition def = shapes[shape.ShapeReference];

                if (def == null)
                {
                    DrawDefaultShape(g, shape, offset);
                }
                else
                {
                    string currentFillColor = "White";
                    string currentPenColor = "Black";
                    int currentPenWidth = 1;
                    foreach (ShapeDrawCommand cmd in def.commands)
                    {
                        if (!CanUseCommand(shape.Highlighted, shape.Selected, cmd.conditions))
                            continue;
                        if (cmd.command == ShapeDrawCommand.SetFillColor)
                        {
                            currentFillColor = cmd.text;
                        }
                        else if (cmd.command == ShapeDrawCommand.SetPenColor)
                        {
                            currentPenColor = cmd.text;
                        }
                        else if (cmd.command == ShapeDrawCommand.SetPenWidth)
                        {
                            currentPenWidth = cmd.A;
                        }
                        else if (cmd.command == ShapeDrawCommand.Fill)
                        {
                            Rectangle rect = shape.Bounds.Rectangle;
                            rect.Offset(offset);
                            g.FillRectangle(GetBrush(currentFillColor), MakeRelativeRect(rect, cmd));
                        }
                        else if (cmd.command == ShapeDrawCommand.Rectangle)
                        {
                            Rectangle rect = shape.Bounds.Rectangle;
                            rect.Offset(offset);
                            g.DrawRectangle(GetPen(currentPenColor,currentPenWidth), MakeRelativeRect(rect, cmd));
                        }
                    }
                }
            }

            DrawSelectedMarks(g, shape, offset);

        }

        public static Rectangle MakeRelativeRect(Rectangle rect, ShapeDrawCommand cmd)
        {
            return new Rectangle(rect.Left + rect.Width * cmd.A / 100,
                                rect.Top + rect.Height * cmd.B / 100, 
                                rect.Width * (cmd.C - cmd.A) / 100,
                                rect.Height * (cmd.D - cmd.B) / 100);
        }

        public static void DrawDefaultShape(Graphics g, CaseShape shape, Point offset)
        {
            Rectangle rect = shape.Bounds.Rectangle;
            rect.Offset(offset);
            g.FillRectangle(Brushes.WhiteSmoke, rect);
            g.DrawRectangle(shape.Selected ? GetPen("Black", 2) : GetPen("Black", 1), rect);

            DrawSelectedMarks(g, shape, offset);
        }

        public static void DrawSelectedMarks(Graphics g, CaseShape shape, Point offset)
        {
            if (!shape.Selected)
                return;
            g.FillRectangle(Brushes.Yellow, shape.Bounds.Xa - connectionMarkWidth / 2 + offset.X,
                (shape.Bounds.Ya + shape.Bounds.Yb - connectionMarkHeight) / 2 + offset.Y, connectionMarkWidth, connectionMarkHeight);
            g.FillRectangle(Brushes.Yellow, shape.Bounds.Xb - connectionMarkWidth / 2 + offset.X,
                (shape.Bounds.Ya + shape.Bounds.Yb - connectionMarkHeight) / 2 + offset.Y, connectionMarkWidth, connectionMarkHeight);
            g.FillRectangle(Brushes.Yellow, (shape.Bounds.Xb + shape.Bounds.Xa - connectionMarkWidth) / 2 + offset.X,
                shape.Bounds.Ya - connectionMarkHeight / 2 + offset.Y, connectionMarkWidth, connectionMarkHeight);
            g.FillRectangle(Brushes.Yellow, (shape.Bounds.Xb + shape.Bounds.Xa - connectionMarkWidth) / 2 + offset.X,
                shape.Bounds.Yb - connectionMarkHeight / 2 + offset.Y, connectionMarkWidth, connectionMarkHeight);
        }

        public static string Description(string shapeType)
        {
            if (shapes.ContainsKey(shapeType))
                return shapes[shapeType].Description;
            return shapeType;
        }

        public static void InitializeShapes()
        {
            ShapeDefinition sd = new ShapeDefinition();

            sd.Name = "MiniCase.Shape.Process";
            sd.Description = "Process entity";

            sd.AddCommand(ShapeDrawCommand.SetFillColor, "Gray");
            sd.AddCommand(ShapeDrawCommand.Fill, 0, 0, 100, 20);
            sd.AddCommand(ShapeDrawCommand.SetFillColor, "White");
            sd.AddCommand(ShapeDrawCommand.Fill, 0, 20, 100, 100);
            sd.AddCommand(ShapeDrawCommand.SetPenWidth, 1, ShapeDrawCommandCondition.NotHighlighted);
            sd.AddCommand(ShapeDrawCommand.SetPenWidth, 2, ShapeDrawCommandCondition.Highlighted);
            sd.AddCommand(ShapeDrawCommand.SetPenColor, "Black");
            sd.AddCommand(ShapeDrawCommand.Rectangle, 0, 0, 100, 100);

            shapes.Add(sd.Name, sd);

        }

        public static Color GetColor(string name)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].Name == name)
                    return colors[i];
            }
            return Color.Black;
        }

        public static int GetColorIndex(string name)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].Name == name)
                    return i;
            }
            return 0;
        }

        public static Brush GetBrush(string name)
        {
            if (!brushes.ContainsKey(name))
            {
                brushes.Add(name, new SolidBrush(GetColor(name)));
            }

            return brushes[name];
        }

        public static Pen GetPen(string name, int width)
        {
            string key = string.Format("{0}-{1}", name, width);
            if (!pens.ContainsKey(key))
            {
                pens.Add(key, new Pen(GetColor(name), width));
            }
            return pens[key];
        }

        public static CaseShape[] CreateAllShapes()
        {
            int i = 0;
            CaseShape[] retval = new CaseShape[shapes.Count];
            foreach (string shape in shapes.Keys)
            {
                retval[i] = new CaseShape();
                retval[i].ShapeReference = shape;
                i++;
            }
            return retval;
        }
    }
}
