using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml;
using System.Drawing.Drawing2D;

namespace MiniCASE
{
    public class CDDocumentDefinition: CDObjectDefinition
    {
        public List<CDShapeDefinition> shapes = new List<CDShapeDefinition>();
        public List<CDConnectionDefinition> connections = new List<CDConnectionDefinition>();
        public List<CDEndingDefinition> lineends = new List<CDEndingDefinition>();

        public CDShapeDefinition RepresentShape = null;


        public override string ToString()
        {
            return Name;
        }

        public CDDocumentDefinition(Guid oid): base(oid)
        {

        }

        public void InitializeDefaultShapes(CDLibrary lib)
        {
            shapes.Clear();

            CDShapeDefinition sd;


            sd = new CDShapeDefinition(lib.NextId);
            sd.Name = "%Represent";
            sd.Description = "Representative shape";
            sd.ShapeBase = ShapeBase.Rectangle;
            sd.UserVisible = false;

            sd.parameters.Add(new CSParameterDef("Text", CSParameterType.String, null));
            sd.parameters.Add(new CSParameterDef("ParentGroup", CSParameterType.String, ""));
            sd.parameters.Add(new CSParameterDef("Background", CSParameterType.Color, Color.LightCyan));
            sd.parameters.Add(new CSParameterDef("BackgroundTitle", CSParameterType.Color, Color.SteelBlue));
            sd.parameters.Add(new CSParameterDef("LineColor", CSParameterType.Color, Color.Black));
            sd.parameters.Add(new CSParameterDef("TextColor", CSParameterType.Color, Color.Black));

            sd.Add(new CDInstruction(CIKey.FillRectangle,
                CIParam.RelativeRect, new Rectangle(0, 20, 100, 80),
                CIParam.ColorId, "Background"));
            sd.Add(new CDInstruction(CIKey.FillRectangle,
                CIParam.RelativeRect, new Rectangle(0, 0, 100, 20),
                CIParam.ColorId, "BackgroundTitle"));
            sd.Add(new CDInstruction(CIKey.FillTriangle,
                CIParam.RelativeTriangle, new Triangle(100, 80, 100, 100, 80, 100),
                CIParam.ColorId, "BackgroundTitle"));
            sd.Add(new CDInstruction(CIKey.DrawRectangle,
                CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                CIParam.ColorId, "LineColor",
                CIParam.Width, 2));
            sd.Add(new CDInstruction(CIKey.DrawText,
                CIParam.RelativeRect, new Rectangle(5, 25, 90, 70),
                CIParam.TextId, "Text",
                CIParam.ColorId, "TextColor"));

            // add to shapes
            shapes.Add(sd);


            sd = new CDShapeDefinition(lib.NextId);
            sd.Name = "Shape.Process";
            sd.Description = "Process entity";
            sd.ShapeBase = ShapeBase.Rectangle;

            sd.parameters.Add(new CSParameterDef("Text", CSParameterType.String, ""));
            sd.parameters.Add(new CSParameterDef("ParentGroup", CSParameterType.String, ""));
            sd.parameters.Add(new CSParameterDef("Background", CSParameterType.Color, Color.White));
            sd.parameters.Add(new CSParameterDef("BackgroundTitle", CSParameterType.Color, Color.Gray));
            sd.parameters.Add(new CSParameterDef("LineColor", CSParameterType.Color, Color.Black));
            sd.parameters.Add(new CSParameterDef("TextColor", CSParameterType.Color, Color.Black));

            sd.Add(new CDInstruction(CIKey.FillRectangle,
                CIParam.RelativeRect, new Rectangle(0, 20, 100, 80),
                CIParam.ColorId, "Background"));
            sd.Add(new CDInstruction(CIKey.FillRectangle,
                CIParam.RelativeRect, new Rectangle(0, 0, 100, 20),
                CIParam.ColorId, "BackgroundTitle"));
            sd.Add(new CDInstruction(CIKey.DrawRectangle,
                CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                CIParam.ColorId, "LineColor",
                CIParam.Width, 1));
            sd.Add(new CDInstruction(CIKey.DrawText,
                CIParam.RelativeRect, new Rectangle(5, 25, 90, 70),
                CIParam.TextId, "Text",
                CIParam.ColorId, "TextColor"));

            // add to shapes
            shapes.Add(sd);


            sd = new CDShapeDefinition(lib.NextId);
            sd.Name = "Shape.Rect";
            sd.Description = "Rectangular general shape";
            sd.ShapeBase = ShapeBase.Rectangle;

            sd.parameters.Add(new CSParameterDef("Text", CSParameterType.String, ""));
            sd.parameters.Add(new CSParameterDef("ParentGroup", CSParameterType.String, ""));
            sd.parameters.Add(new CSParameterDef("Background", CSParameterType.Color, Color.White));
            sd.parameters.Add(new CSParameterDef("LineColor", CSParameterType.Color, Color.Black));
            sd.parameters.Add(new CSParameterDef("TextColor", CSParameterType.Color, Color.Black));
            sd.parameters.Add(new CSParameterDef("LineWidth", CSParameterType.LineWidth, 1f));
            sd.parameters.Add(new CSParameterDef("FontSize", CSParameterType.FontSize, 12f));
            sd.parameters.Add(new CSParameterDef("TextAlign", CSParameterType.TextAlign, CSTextAlign.Center));

            sd.Add(new CDInstruction(CIKey.FillRectangle,
                CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                CIParam.ColorId, "Background"));
            sd.Add(new CDInstruction(CIKey.DrawRectangle,
                CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                CIParam.ColorId, "LineColor",
                CIParam.WidthId, "LineWidth"));
            sd.Add(new CDInstruction(CIKey.DrawText,
                CIParam.RelativePoint, new Point(50, 50),
                CIParam.TextId, "Text",
                CIParam.ColorId, "TextColor",
                CIParam.FontSizeId, "FontSize",
                CIParam.TextAlignId, "TextAlign"));

            shapes.Add(sd);

            sd = new CDShapeDefinition(lib.NextId);
            sd.Name = "Shape.Circle";
            sd.Description = "Circular general shape";
            sd.ShapeBase = ShapeBase.Ellipse;
            sd.parameters.Add(new CSParameterDef("Text", CSParameterType.String, ""));
            sd.parameters.Add(new CSParameterDef("ParentGroup", CSParameterType.String, ""));
            sd.parameters.Add(new CSParameterDef("Background", CSParameterType.Color, Color.White));
            sd.parameters.Add(new CSParameterDef("LineColor", CSParameterType.Color, Color.Black));
            sd.parameters.Add(new CSParameterDef("TextColor", CSParameterType.Color, Color.Black));
            sd.parameters.Add(new CSParameterDef("LineWidth", CSParameterType.LineWidth, 1f));
            sd.parameters.Add(new CSParameterDef("FontSize", CSParameterType.FontSize, 12f));
            sd.parameters.Add(new CSParameterDef("TextAlign", CSParameterType.TextAlign, CSTextAlign.Center));

            sd.Add(new CDInstruction(CIKey.FillEllipse,
                CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                CIParam.ColorId, "Background"));
            sd.Add(new CDInstruction(CIKey.DrawEllipse,
                CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                CIParam.ColorId, "LineColor",
                CIParam.WidthId, "LineWidth"));
            sd.Add(new CDInstruction(CIKey.DrawText,
                CIParam.RelativePoint, new Point(50, 50),
                CIParam.TextId, "Text",
                CIParam.ColorId, "TextColor",
                CIParam.FontSizeId, "FontSize",
                CIParam.TextAlignId, "TextAlign"));
            shapes.Add(sd);

        }


        public void InitializeDefaultConnections(CDLibrary lib)
        {
            CDEndingDefinition _defPlain = new CDEndingDefinition(lib.NextId);
            _defPlain.Name = "Default Plain";

            CDEndingDefinition _defArrow = new CDEndingDefinition(lib.NextId);
            _defArrow.Name = "Default Arrow";

            _defArrow.instructions.Add(new CDEndingDefinition.CDEI(CDEndingDefinition.CDT.DrawLine, Color.Empty, 0, 0, 0.3f, 0.1f));
            _defArrow.instructions.Add(new CDEndingDefinition.CDEI(CDEndingDefinition.CDT.DrawLine, Color.Empty, 0, 0, 0.3f, -0.1f));

            CDConnectionDefinition _def = new CDConnectionDefinition(lib.NextId);
            _def.Name = "Default";
            _def.AddParameter("Text", CSParameterType.String, "");
            _def.AddParameter("StartNote", CSParameterType.String, "");
            _def.AddParameter("EndNote", CSParameterType.String, "");
            _def.AddParameter("LineColor", CSParameterType.Color, Color.Black);
            _def.AddParameter("LineWidth", CSParameterType.LineWidth, 1f);
            _def.AddParameter("StartCap", CSParameterType.LineCap, _defPlain);
            _def.AddParameter("EndCap", CSParameterType.LineCap, _defArrow);
            _def.AddParameter("LinePattern", CSParameterType.LinePattern, DashStyle.Solid);
            _def.AddParameter("PathStyle", CSParameterType.LinePath, ConnectionsMode.Straight);
            connections.Add(_def);


            lineends.Add(_defPlain);
            lineends.Add(_defArrow);
        }

        public CDConnectionDefinition DefaultConnectionType
        {
            get
            {
                return connections[0];
            }
        }

        public CDShapeDefinition FindShapeDefinition(string str)
        {
            foreach (CDShapeDefinition dd in shapes)
            {
                if (dd.Name.Equals(str))
                    return dd;
            }
            return null;
        }

        public CDEndingDefinition FindLineEndingDefinition(Guid lineEndID)
        {
            foreach (CDEndingDefinition ed in lineends)
            {
                if (ed.DefinitionId.Equals(lineEndID))
                    return ed;
            }

            return null;
        }

        public CDShapeDefinition FindShapeDefinition(Guid shapeDefID)
        {
            if (shapeDefID.Equals(CDLibrary.GroupShape.DefinitionId))
                return CDLibrary.GroupShape;

            foreach (CDShapeDefinition ed in shapes)
            {
                if (ed.DefinitionId.Equals(shapeDefID))
                    return ed;
            }

            return null;
        }

        public CDConnectionDefinition FindConnectionDefinition(Guid connDefID)
        {
            foreach (CDConnectionDefinition ed in connections)
            {
                if (ed.DefinitionId.Equals(connDefID))
                    return ed;
            }

            return null;
        }

        public override void ReadFrom(XmlElement elem, CDReaderReferences refs)
        {
            CDReaderReferences references = new CDReaderReferences();

            base.ReadFrom(elem, references);

            shapes.Clear();
            connections.Clear();
            lineends.Clear();

            foreach (XmlElement node in CDXml.GetChildren(elem, "shape"))
            {
                CDShapeDefinition sd = new CDShapeDefinition(Guid.Empty);
                sd.ReadFrom(node, references);
                shapes.Add(sd);
            }

            foreach(XmlElement E in CDXml.GetChildren(elem, "connection"))
            {
                CDConnectionDefinition cd = new CDConnectionDefinition(Guid.Empty);
                cd.ReadFrom(E, references);
                connections.Add(cd);
            }

            foreach (XmlElement E in CDXml.GetChildren(elem, "lineend"))
            {
                CDEndingDefinition ed = new CDEndingDefinition(Guid.Empty);
                ed.ReadFrom(E, references);
                lineends.Add(ed);
            }

            // this is evaluated after reading of all shape definitions
            if (elem.HasAttribute("RepresentativeShapeName"))
            {
                RepresentShape = FindShapeDefinition(elem.GetAttribute("RepresentativeShapeName"));
                if (RepresentShape == null)
                    RepresentShape = CDLibrary.DefaultShape;
            }


            //
            // resolve lazy references
            //
            Type edType = typeof(CDEndingDefinition);

            foreach(CDLazyReference lz in references)
            {
                if (lz.lazyType == CDLazyType.FindDefinitionForDefaultValue)
                {
                    CDEndingDefinition ed = FindLineEndingDefinition(lz.findObjectGuid);
                    if (ed != null && lz.targetReference is CSParameterDef)
                    {
                        CSParameterDef pd = (CSParameterDef)lz.targetReference;
                        pd.DefaultValue = ed;
                    }
                }
            }
        }

        public override void WriteTo(XmlElement elem, XmlDocument doc)
        {
            base.WriteTo(elem, doc);

            if (RepresentShape != null)
            {
                elem.SetAttribute("RepresentativeShapeName", RepresentShape.Name);
            }

            foreach (CDShapeDefinition sd in shapes)
            {
                XmlElement E = doc.CreateElement("shape");
                elem.AppendChild(E);
                sd.WriteTo(E, doc);
            }

            foreach(CDConnectionDefinition cd in connections)
            {
                XmlElement E = doc.CreateElement("connection");
                elem.AppendChild(E);
                cd.WriteTo(E, doc);
            }

            foreach(CDEndingDefinition ed in lineends)
            {
                XmlElement E = doc.CreateElement("lineend");
                elem.AppendChild(E);
                ed.WriteTo(E, doc);
            }
        }
    }
}
