using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;

namespace MiniCASE
{
    public class CDShapeDefinition: CDObjectDefinition
    {
        public List<CDInstruction> cmds = new List<CDInstruction>();
        private Size default_size = new Size(100, 60);
        public ShapeBase ShapeBase = ShapeBase.Rectangle;
        public bool IsContainer = true;
        public bool UserVisible = true;
        public bool AutomaticBounds = false;
        private static CDShapeDefinition _def = null;

        public CDShapeDefinition(Guid oid): base(oid)
        {

        }

        public Size DefaultSize
        {
            get { return default_size; }
            set { default_size = value; }
        }

        public CDInstruction Add(CDInstruction instr)
        {
            cmds.Add(instr);
            return instr;
        }

        public CSParameterDef GetParam(string tag)
        {
            foreach (CSParameterDef p in parameters)
            {
                if (p.Name.Equals(tag))
                    return p;
            }
            return null;
        }

        public override void ReadFrom(XmlElement elem, CDReaderReferences refs)
        {
            base.ReadFrom(elem, refs);

            XmlElement head = elem["shape"];
            if (head != null)
            {
                XmlElement defsize = head["defaultSize"];
                if (defsize != null && defsize.HasAttribute("width") && defsize.HasAttribute("height"))
                {
                    int w, h;
                    if (int.TryParse(defsize.GetAttribute("width"), out w) && 
                        int.TryParse(defsize.GetAttribute("height"), out h))
                        default_size = new Size(w,h);
                }

                bool b;
                ShapeBase sb;

                if (head.HasAttribute("shapeBase") && Enum.TryParse(head.GetAttribute("shapeBase"), out sb))
                {
                    ShapeBase = sb;
                }
                if (head.HasAttribute("isContainer") && bool.TryParse(head.GetAttribute("isContainer"), out b))
                {
                    IsContainer = b;
                }
                if (head.HasAttribute("userVisible") && bool.TryParse(head.GetAttribute("userVisible"), out b))
                {
                    UserVisible = b;
                }
                if (head.HasAttribute("autoBounds") && bool.TryParse(head.GetAttribute("autoBounds"), out b))
                {
                    AutomaticBounds = b;
                }

                foreach(XmlElement E in CDXml.GetChildren(head, "instr"))
                {
                    CDInstruction instr = new CDInstruction();
                    instr.ReadFrom(E);
                    cmds.Add(instr);
                }
            }
        }

        public override void WriteTo(XmlElement elem, XmlDocument doc)
        {
            base.WriteTo(elem, doc);

            XmlElement head = doc.CreateElement("shape");
            elem.AppendChild(head);

            head.AppendChild(doc.CreateElement("defaultSize"));
            head["defaultSize"].SetAttribute("width", default_size.Width.ToString());
            head["defaultSize"].SetAttribute("height", default_size.Height.ToString());
            head.SetAttribute("shapeBase", ShapeBase.ToString());
            head.SetAttribute("isContainer", IsContainer.ToString());
            head.SetAttribute("userVisible", UserVisible.ToString());
            head.SetAttribute("autoBounds", AutomaticBounds.ToString());

            foreach(CDInstruction instr in cmds)
            {
                XmlElement E = doc.CreateElement("instr");
                head.AppendChild(E);
                instr.WriteTo(E, doc);
            }
        }
    }

    public enum ShapeAnchor
    {
        TopCenter, TopLeft, TopRight,
        Center, CenterLeft, CenterRight,
        BottomCenter, BottomLeft, BottomRight,
        Top, Left, Right, Bottom, Whole,
        None
    }

    public static class ShapeSide
    {
        public const int None = 0x00;
        public const int Top = 0x01;
        public const int Right = 0x02;
        public const int Bottom = 0x04;
        public const int Left = 0x08;
        public const int All = 0x0f;

        public static int GetSide(int index)
        {
            switch (index)
            {
                case 0: return Top;
                case 1: return Right;
                case 2: return Bottom;
                case 3: return Left;
                default: return None;
            }
        }
    }
}
