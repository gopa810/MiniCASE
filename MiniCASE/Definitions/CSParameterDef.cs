using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MiniCASE
{
    public class CSParameterDef
    {
        public string Name { get; set; }
        public CSParameterType ParameterType { get; set; }
        public object DefaultValue { get; set; }

        private static CSParameterDef p_empty_def;

        public static CSParameterDef Empty {  get { return p_empty_def; } }

        static CSParameterDef()
        {
            p_empty_def = new CSParameterDef() { DefaultValue = string.Empty,
                Name = string.Empty, ParameterType = CSParameterType.Empty };
        }

        public bool IsEmpty()
        {
            return ParameterType == CSParameterType.Empty;
        }

        public CSParameterDef()
        {
        }

        public CSParameterDef(string name, CSParameterType type, object defaultValue)
        {
            Name = name;
            ParameterType = type;
            DefaultValue = defaultValue;
        }

        public void WriteTo(XmlElement elem, XmlDocument doc)
        {
            elem.SetAttribute("name", Name);
            elem.SetAttribute("type", ParameterType.ToString());

            if (DefaultValue == null)
                return;

            XmlElement VE = doc.CreateElement("value");
            elem.AppendChild(VE);
            switch (ParameterType)
            {
                case CSParameterType.Color:
                    if (DefaultValue is Color)
                    {
                        VE.InnerText = ColorTranslator.ToHtml((Color)DefaultValue);
                    }
                    break;
                case CSParameterType.FontSize:
                case CSParameterType.Integer:
                case CSParameterType.LinePath:
                case CSParameterType.LinePattern:
                case CSParameterType.LineWidth:
                case CSParameterType.String:
                case CSParameterType.TextAlign:
                    VE.InnerText = DefaultValue.ToString();
                    break;
                case CSParameterType.LineCap:
                    if (DefaultValue is CDEndingDefinition)
                    {
                        VE.InnerText = ((CDEndingDefinition)DefaultValue).DefinitionId.ToString();
                    }
                    break;
                case CSParameterType.TextPadding:
                    if (DefaultValue is CSTextPadding)
                    {
                        CSTextPadding tp = (CSTextPadding)DefaultValue;
                        VE.InnerText = tp.ToString();
                    }
                    break;
                case CSParameterType.Image:
                    if (DefaultValue != null)
                    {
                        VE.InnerText = Convert.ToBase64String(CDGraphics.ImageToByteArray((Image)DefaultValue));
                    }
                    break;
                default:
                    break;
            }
        }

        public void ReadFrom(XmlElement elem, CDReaderReferences refs)
        {
            Name = elem.GetAttribute("name");
            ParameterType = (CSParameterType)Enum.Parse(typeof(CSParameterType), elem.GetAttribute("type"));
            DefaultValue = null;

            foreach (XmlElement VE in CDXml.GetChildren(elem, "value"))
            {
                switch (ParameterType)
                {
                    case CSParameterType.Color:
                        DefaultValue = ColorTranslator.FromHtml(VE.InnerText);
                        break;
                    case CSParameterType.FontSize:
                    case CSParameterType.LineWidth:
                        DefaultValue = float.Parse(VE.InnerText);
                        break;
                    case CSParameterType.Integer:
                        DefaultValue = int.Parse(VE.InnerText);
                        break;
                    case CSParameterType.LinePath:
                        DefaultValue = (ConnectionsMode)Enum.Parse(typeof(ConnectionsMode), VE.InnerText);
                        break;
                    case CSParameterType.LinePattern:
                        DefaultValue = (DashStyle)Enum.Parse(typeof(DashStyle), VE.InnerText);
                        break;
                    case CSParameterType.String:
                        DefaultValue = VE.InnerText;
                        break;
                    case CSParameterType.TextAlign:
                        DefaultValue = (CSTextAlign)Enum.Parse(typeof(CSTextAlign), VE.InnerText);
                        break;
                    case CSParameterType.LineCap:
                        refs.Add(CDLazyType.FindDefinitionForDefaultValue, new Guid(VE.InnerText), this);
                        break;
                    case CSParameterType.TextPadding:
                        DefaultValue = new CSTextPadding(VE.InnerText);
                        break;
                    case CSParameterType.Image:
                        if (!string.IsNullOrWhiteSpace(VE.InnerText))
                        {
                            DefaultValue = CDGraphics.ByteArrayToImage(Convert.FromBase64String(VE.InnerText));
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Storage in CSParameterDef.ReadFrom and CSParameterDef.WriteTo
    /// </summary>
    public enum CSParameterType
    {
        Empty,
        String,
        Color,
        Integer,
        FontSize,
        LineWidth,
        TextAlign,
        TextPadding,
        LineCap,
        LinePattern,
        LinePath,
        Image
    }
}
