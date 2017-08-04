using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MiniCASE
{
    public class CSParameterSource
    {
        public List<object> sources = new List<object>();

        public void Add(object src)
        {
            sources.Add(src);
        }

        public bool TryExtractRelativeRect(Rectangle bounds, ref Rectangle rect, CDInstruction instruction)
        {
            if (instruction.ContainsKey(CIParam.RelativeRect))
            {
                rect = (Rectangle)instruction[CIParam.RelativeRect];
                rect = MakeRelativeRect(bounds, rect);
                return true;
            }
            return false;
        }

        public bool TryExtractImage(out Image image, CDInstruction instruction)
        {
            image = null;

            if (instruction.ContainsKey(CIParam.Image))
            {
                image = (Image)instruction[CIParam.Image];
                return true;
            }

            if (instruction.ContainsKey(CIParam.ImageId))
            {
                string key = (string)instruction[CIParam.ImageId];
                object value = RetrieveObjectValue(key);
                if (value != null && value is Image)
                {
                    image = (Image)value;
                }
            }

            return false;
        }

        public bool TryExtractRelativeTriangle(Rectangle bounds, ref Triangle rect, CDInstruction instruction)
        {
            if (instruction.ContainsKey(CIParam.RelativeTriangle))
            {
                rect = (Triangle)instruction[CIParam.RelativeTriangle];
                rect = MakeRelativeTriangle(bounds, rect);
                return true;
            }
            return false;
        }

        public bool TryExtractRelativePoint(Rectangle bounds, ref Point point, CDInstruction instruction)
        {
            if (instruction.ContainsKey(CIParam.RelativePoint))
            {
                point = (Point)instruction[CIParam.RelativePoint];
                point = MakeRelativePoint(bounds, point.X, point.Y);
                return true;
            }
            return false;
        }


        public bool TryExtractColor(ref Color clr, CDInstruction instruction)
        {
            if (instruction.ContainsKey(CIParam.Color))
            {
                clr = (Color)instruction[CIParam.Color];
                return true;
            }

            if (instruction.ContainsKey(CIParam.ColorId))
            {
                string key = (string)instruction[CIParam.ColorId];
                object value = RetrieveObjectValue(key);
                if (value != null && value is Color)
                {
                    clr = (Color)value;
                }
            }

            return false;
        }

        public bool TryExtractWidth(CDShapeDefinition def, ref float width, CDInstruction instruction)
        {
            return TryExtractFloat(def, ref width, instruction, CIParam.WidthId, CIParam.Width);
        }

        public bool TryExtractFloat(CDShapeDefinition def, ref float width, CDInstruction instruction,
            CIParam keyId, CIParam key)
        {
            if (instruction.ContainsKey(key))
            {
                if (instruction[key] is int)
                {
                    width = (int)instruction[key];
                }
                else if (instruction[key] is float)
                {
                    width = (float)instruction[key];
                }
                return true;
            }

            if (instruction.ContainsKey(keyId))
            {
                string keys = (string)instruction[keyId];
                object value = RetrieveObjectValue(keys);
                if (value != null && value is float)
                {
                    width = (float)value;
                    return true;
                }
                else if (value != null && value is int)
                {
                    width = (int)value;
                    return true;
                }

            }
            return false;
        }


        public bool TryExtractTextPadding(CDShapeDefinition def, CIParam param, ref CSTextPadding tp, CDInstruction instruction)
        {
            if (instruction.ContainsKey(param))
            {
                string key = (string)instruction[param];
                object value = RetrieveObjectValue(key);
                if (value != null && value is CSTextPadding)
                {
                    tp = (CSTextPadding)value;
                    return true;
                }
            }
            return false;
        }


        public bool TryExtractText(ref string txt, CDInstruction instruction)
        {
            if (instruction.ContainsKey(CIParam.TextId))
            {
                string key = (string)instruction[CIParam.TextId];
                object value = RetrieveObjectValue(key);
                if (value != null && value is string)
                {
                    txt = (string)value;
                    return true;
                }
            }
            if (instruction.ContainsKey(CIParam.Text))
            {
                txt = (string)instruction[CIParam.Text];
                return true;
            }

            return false;
        }

        public bool TryExtractTextAlign(ref CSTextAlign textAlign, CDInstruction instruction)
        {
            if (instruction.ContainsKey(CIParam.TextAlignId))
            {
                string key = (string)instruction[CIParam.TextAlignId];
                object value = RetrieveObjectValue(key);
                if (value != null && value is CSTextAlign)
                {
                    textAlign = (CSTextAlign)value;
                    return true;
                }
            }
            if (instruction.ContainsKey(CIParam.TextAlign))
            {
                textAlign = (CSTextAlign)instruction[CIParam.TextAlign];
                return true;
            }

            return false;
        }



        private object RetrieveObjectValue(string key)
        {
            foreach (object obj in sources)
            {
                if (obj is CDObject)
                {
                    CDObject cdo = (CDObject)obj;
                    if (cdo.Params.ContainsKey(key))
                    {
                        if (cdo.Params[key] != null)
                            return cdo.Params[key];
                    }
                }
                else if (obj is CDObjectDefinition)
                {
                    CDObjectDefinition cdd = (CDObjectDefinition)obj;
                    if (cdd.ContainsKey(key))
                    {
                        CSParameterDef pad = cdd[key];
                        if (pad.DefaultValue != null)
                            return pad.DefaultValue;
                    }
                }
            }

            return null;
        }

        public static Rectangle MakeRelativeRect(Rectangle rect, Rectangle cmd)
        {
            return new Rectangle(rect.Left + rect.Width * cmd.Left / 100,
                                rect.Top + rect.Height * cmd.Top / 100,
                                rect.Width * cmd.Width / 100,
                                rect.Height * cmd.Height / 100);
        }

        public static Point MakeRelativePoint(Rectangle rect, int cmdX, int cmdY)
        {
            return new Point(rect.Left + rect.Width * cmdX / 100,
                                rect.Top + rect.Height * cmdY / 100);
        }

        public static Triangle MakeRelativeTriangle(Rectangle rect, Triangle cmd)
        {
            return new Triangle(rect.Left + rect.Width * cmd.AX / 100,
                                rect.Top + rect.Height * cmd.AY / 100,
                                rect.Left + rect.Width * cmd.BX / 100,
                                rect.Top + rect.Height * cmd.BY / 100,
                                rect.Left + rect.Width * cmd.CX / 100,
                                rect.Top + rect.Height * cmd.CY / 100);
        }
    }
}
