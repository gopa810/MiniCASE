using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;

namespace MiniCASE
{
    public class CDInstruction: Dictionary<CIParam, object>
    {
        public CIKey Command = CIKey.None;

        public CDInstruction()
        {
        }

        public CDInstruction(CIKey cmd)
        {
            Command = cmd;
        }

        public CDInstruction(CIKey cmd, params object[] args)
        {
            Command = cmd;
            for (int i = 0; i < args.Length - 1; i += 2)
            {
                if (args[i] is CIParam)
                {
                    this.Add((CIParam)args[i], args[i + 1]);
                }
            }
        }

        public void ReadFrom(XmlElement E)
        {
            CIKey cmd;
            if (E.HasAttribute("Command") && Enum.TryParse(E.GetAttribute("Command"), out cmd))
            {
                Command = cmd;
                foreach(XmlElement B in CDXml.GetChildren(E, "arg"))
                {
                    CIParam key;
                    string[] p;
                    if (Enum.TryParse(B.GetAttribute("Key"), out key))
                    {
                        switch(key)
                        {
                            case CIParam.Color:
                                this[key] = ColorTranslator.FromHtml(B.GetAttribute("ColorHtml"));
                                break;
                            case CIParam.ColorId: case CIParam.FontSizeId: case CIParam.HeightId:
                            case CIParam.Text: case CIParam.TextId: case CIParam.WidthId:
                            case CIParam.ImageId:
                                this[key] = B.GetAttribute("String");
                                break;
                            case CIParam.FontSize: case CIParam.Height: case CIParam.Width:
                                this[key] = float.Parse(B.GetAttribute("Float"));
                                break;
                            case CIParam.RelativePoint:
                                p = B.GetAttribute("Point").Split('|');
                                if (p.Length == 2)
                                    this[key] = new Point(int.Parse(p[0]), int.Parse(p[1]));
                                break;
                            case CIParam.RelativeRect:
                                p = B.GetAttribute("Rectangle").Split('|');
                                if (p.Length == 4)
                                    this[key] = new Rectangle(int.Parse(p[0]), int.Parse(p[1]),
                                        int.Parse(p[2]), int.Parse(p[3]));
                                break;
                            case CIParam.RelativeTriangle:
                                p = B.GetAttribute("Triangle").Split('|');
                                if (p.Length == 6)
                                    this[key] = new Triangle(int.Parse(p[0]), int.Parse(p[1]),
                                        int.Parse(p[2]), int.Parse(p[3]), int.Parse(p[4]), int.Parse(p[5]));
                                break;
                            case CIParam.TextAlign:
                                this[key] = Enum.Parse(typeof(CSTextAlign), B.GetAttribute("TextAlign"));
                                break;
                            case CIParam.TextPadding:
                                this[key] = new CSTextPadding(B.GetAttribute("TextPadding"));
                                break;
                            case CIParam.Image:
                                this[key] = CDGraphics.ByteArrayToImage(Convert.FromBase64String(B.InnerText));
                                break;
                        }
                    }
                }
            }
        }

        public void WriteTo(XmlElement E, XmlDocument doc)
        {
            E.SetAttribute("Command", Command.ToString());
            foreach(KeyValuePair<CIParam,object> pair in this)
            {
                XmlElement B = doc.CreateElement("arg");
                E.AppendChild(B);
                B.SetAttribute("Key", pair.Key.ToString());
                if (pair.Key == CIParam.Color)
                {
                    Color clr = (Color)pair.Value;
                    B.SetAttribute("ColorHtml", ColorTranslator.ToHtml(clr));
                }
                else if (pair.Key == CIParam.ColorId)
                {
                    B.SetAttribute("String", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.Image)
                {
                    B.InnerText = Convert.ToBase64String(CDGraphics.ImageToByteArray((Image)pair.Value));
                }
                else if (pair.Key == CIParam.ImageId)
                {
                    B.SetAttribute("String", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.RelativeRect)
                {
                    Rectangle rc = (Rectangle)pair.Value;
                    B.SetAttribute("Rectangle", string.Format("{0}|{1}|{2}|{3}", rc.X, rc.Y, rc.Width, rc.Height));
                }
                else if (pair.Key == CIParam.RelativePoint)
                {
                    Point pt = (Point)pair.Value;
                    B.SetAttribute("Point", string.Format("{0}|{1}", pt.X, pt.Y));
                }
                else if (pair.Key == CIParam.RelativeTriangle)
                {
                    Triangle tr = (Triangle)pair.Value;
                    B.SetAttribute("Triangle", string.Format("{0}|{1}|{2}|{3}|{4}|{5}", tr.AX, tr.AY, tr.BX, tr.BY, tr.CX, tr.CY));
                }
                else if (pair.Key == CIParam.FontSize)
                {
                    B.SetAttribute("Float", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.FontSizeId)
                {
                    B.SetAttribute("String", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.Height)
                {
                    B.SetAttribute("Float", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.HeightId)
                {
                    B.SetAttribute("String", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.Text)
                {
                    B.SetAttribute("String", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.TextAlign)
                {
                    CSTextAlign ta = (CSTextAlign)pair.Value;
                    B.SetAttribute("TextAlign", ta.ToString());
                }
                else if (pair.Key == CIParam.TextAlignId)
                {
                    B.SetAttribute("String", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.TextId)
                {
                    B.SetAttribute("String", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.TextPadding)
                {
                    CSTextPadding ta = (CSTextPadding)pair.Value;
                    B.SetAttribute("TextPadding", ta.ToString());
                }
                else if (pair.Key == CIParam.Width)
                {
                    B.SetAttribute("Float", pair.Value.ToString());
                }
                else if (pair.Key == CIParam.WidthId)
                {
                    B.SetAttribute("String", pair.Value.ToString());
                }
            }
        }



    }
}
