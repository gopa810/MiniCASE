using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml;

namespace MiniCASE
{
    public class CDLibrary
    {

        public List<CDDocumentDefinition> DocumentTypes = new List<CDDocumentDefinition>();

        private static CDShapeDefinition p_groupShape = null;
        private static CDShapeDefinition p_defaultShape = null;

        public static CDShapeDefinition DefaultShape
        {
            get
            {
                if (p_defaultShape == null)
                {
                    CDShapeDefinition sd = new CDShapeDefinition(new Guid("627fbfd9-0f35-4fd2-9db0-ea9731b9c2c3"));
                    sd.Name = "Shape.Default";
                    sd.Description = "Default object shape";
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

                    p_defaultShape = sd;
                }
                return p_defaultShape;
            }
        }

        private static CDShapeDefinition p_imageShape = null;

        public static CDShapeDefinition ImageShape
        {
            get
            {
                if (p_imageShape == null)
                {
                    CDShapeDefinition sd = new CDShapeDefinition(new Guid("49933950-d75e-4ec9-82aa-5aeeb8253961"));
                    sd.Name = "%Image";
                    sd.Description = "Image";
                    sd.ShapeBase = ShapeBase.Rectangle;
                    sd.IsContainer = false;
                    sd.UserVisible = true;
                    sd.AutomaticBounds = false;
                    sd.parameters.Add(new CSParameterDef("Text", CSParameterType.String, ""));
                    sd.parameters.Add(new CSParameterDef("TextColor", CSParameterType.Color, Color.Black));
                    sd.parameters.Add(new CSParameterDef("Image", CSParameterType.Image, null));
                    sd.parameters.Add(new CSParameterDef("TextAlign", CSParameterType.TextAlign, CSTextAlign.TopLeft));
                    sd.parameters.Add(new CSParameterDef("Padding", CSParameterType.TextPadding, CSTextPadding.Default));
                    sd.parameters.Add(new CSParameterDef("FontSize", CSParameterType.FontSize, 12f));
                    sd.Add(new CDInstruction(CIKey.SetBoundsByFitImage, CIParam.ImageId, "Image"));
                    sd.Add(new CDInstruction(CIKey.DrawImage, CIParam.ImageId, "Image"));
                    sd.Add(new CDInstruction(CIKey.DrawText,
                        CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                        CIParam.TextId, "Text",
                        CIParam.ColorId, "TextColor",
                        CIParam.FontSizeId, "FontSize",
                        CIParam.TextAlignId, "TextAlign",
                        CIParam.TextPadding, "Padding"));

                    p_imageShape = sd;
                }

                return p_imageShape;
            }
        }

        public static CDShapeDefinition GroupShape
        {
            get
            {
                if (p_groupShape == null)
                {
                    CDShapeDefinition sd = new CDShapeDefinition(new Guid("fc2b65bc-1671-4f6c-851e-63a40a9f56b1"));

                    sd.Name = "%MiniCase.Group";
                    sd.Description = "Group of items";
                    sd.ShapeBase = ShapeBase.Rectangle;
                    sd.IsContainer = true;
                    sd.UserVisible = false;
                    sd.AutomaticBounds = true;
                    sd.parameters.Add(new CSParameterDef("Text", CSParameterType.String, ""));
                    sd.parameters.Add(new CSParameterDef("GroupName", CSParameterType.String, ""));
                    sd.parameters.Add(new CSParameterDef("ParentGroup", CSParameterType.String, ""));
                    sd.parameters.Add(new CSParameterDef("Background", CSParameterType.Color, Color.White));
                    sd.parameters.Add(new CSParameterDef("LineColor", CSParameterType.Color, Color.Black));
                    sd.parameters.Add(new CSParameterDef("TextColor", CSParameterType.Color, Color.Black));
                    sd.parameters.Add(new CSParameterDef("LineWidth", CSParameterType.LineWidth, 1f));
                    sd.parameters.Add(new CSParameterDef("FontSize", CSParameterType.FontSize, 12f));
                    sd.parameters.Add(new CSParameterDef("TextAlign", CSParameterType.TextAlign, CSTextAlign.TopLeft));
                    sd.parameters.Add(new CSParameterDef("Padding", CSParameterType.TextPadding, CSTextPadding.Default16));
                    sd.parameters.Add(new CSParameterDef("Margin", CSParameterType.TextPadding, new CSTextPadding(32, 64, 32, 32)));


                    sd.Add(new CDInstruction(CIKey.FillRectangle,
                        CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                        CIParam.ColorId, "Background"));
                    sd.Add(new CDInstruction(CIKey.DrawRectangle,
                        CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                        CIParam.ColorId, "LineColor",
                        CIParam.WidthId, "LineWidth"));
                    sd.Add(new CDInstruction(CIKey.DrawText,
                        CIParam.RelativeRect, new Rectangle(0, 0, 100, 100),
                        CIParam.TextId, "Text",
                        CIParam.ColorId, "TextColor",
                        CIParam.FontSizeId, "FontSize",
                        CIParam.TextAlignId, "TextAlign",
                        CIParam.TextPadding, "Padding"));

                    p_groupShape = sd;
                }

                return p_groupShape;
            }
        }

        public Guid NextId
        {
            get
            {
                return Guid.NewGuid();
            }
        }

        private static CDDocumentDefinition p_defaultDocumentType = null;


        public CDDocumentDefinition DefaultDocumentType
        {
            get
            {
                if (p_defaultDocumentType == null)
                {
                    p_defaultDocumentType = new CDDocumentDefinition(NextId);
                    p_defaultDocumentType.Name = "General Diagram";

                    // these parameters are also for representative shape
                    p_defaultDocumentType.AddParameter("Text", CSParameterType.String, "diagram");
                    p_defaultDocumentType.AddParameter("Background", CSParameterType.Color, Color.White);

                    p_defaultDocumentType.InitializeDefaultShapes(this);
                    p_defaultDocumentType.InitializeDefaultConnections(this);

                    p_defaultDocumentType.RepresentShape = p_defaultDocumentType.FindShapeDefinition("%Represent");
                }
                return p_defaultDocumentType;
            }
        }

        public CDObjectDefinition FindDefinition(Guid findObjectId)
        {
            foreach(CDDocumentDefinition dd in DocumentTypes)
            {
                CDObjectDefinition objDef = dd.FindLineEndingDefinition(findObjectId);
                if (objDef != null) return objDef;

                objDef = dd.FindShapeDefinition(findObjectId);
                if (objDef != null) return objDef;

                objDef = dd.FindConnectionDefinition(findObjectId);
                if (objDef != null) return objDef;
            }

            return null;
        }


        public void Initialize()
        {
            string fileName = "";

            if (File.Exists(fileName))
            {

            }
            else
            {
                // init docu types
                DocumentTypes.Add(DefaultDocumentType);

            }
        }


        public void Save(string fileName)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement root = doc.CreateElement("library");
            doc.AppendChild(root);
            WriteTo(doc, root);

            doc.Save(fileName);
        }

        public void WriteTo(XmlDocument doc, XmlElement root)
        {
            foreach (CDDocumentDefinition dt in DocumentTypes)
            {
                XmlElement elem = doc.CreateElement("document");
                root.AppendChild(elem);

                dt.WriteTo(elem, doc);
            }
        }

        public void Load(string fileName)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(fileName);

            LoadFrom(doc["library"]);
        }

        public void LoadFrom(XmlElement root)
        {
            foreach (XmlElement E in CDXml.GetChildren(root, "document"))
            {
                CDDocumentDefinition docDef = new CDDocumentDefinition(Guid.Empty);
                docDef.ReadFrom(E, null);
                DocumentTypes.Add(docDef);
            }
        }
    }
}
