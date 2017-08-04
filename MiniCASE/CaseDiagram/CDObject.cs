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
    public class CDObject
    {
        public CDProject Project { get; set; }
        public Guid ObjectId = Guid.Empty;
        public CDDiagram Decomposition = null;
        private ICDObjectDelegate p_cdObjectDelegate = null;
        public CDObjectDefinition Definition = null;

        public Dictionary<string, object> Params = new Dictionary<string, object>();

        public CDObject(CDProject prj, CDObjectDefinition def, Guid oid)
        {
            Project = prj;
            Definition = def;
            ObjectId = oid;
        }

        public void SetDelegate(ICDObjectDelegate delegateObject)
        {
            p_cdObjectDelegate = delegateObject;
        }

        // text align
        public virtual void SetTextAlign(string key, string value)
        {
            if (Enum.IsDefined(typeof(CSTextAlign), value))
            {
                Params[key] = Enum.Parse(typeof(CSTextAlign), value);
                if (p_cdObjectDelegate != null)
                {
                    p_cdObjectDelegate.CDObjectValueDidChange(this, key, value);
                }
            }
        }

        public virtual CSTextAlign GetTextAlign(string key)
        {
            object obj = GetObject(key);
            if (obj is CSTextAlign)
                return (CSTextAlign)obj;

            return CSTextAlign.Center;
        }

        public virtual void SetString(string key, string value)
        {
            Params[key] = value;
            if (p_cdObjectDelegate != null)
            {
                p_cdObjectDelegate.CDObjectValueDidChange(this, key, value);
            }
        }

        public virtual void SetFloat(string key, float floatValue)
        {
            Params[key] = floatValue;
            if (p_cdObjectDelegate != null)
            {
                p_cdObjectDelegate.CDObjectValueDidChange(this, key, floatValue);
            }
        }

        public virtual string GetString(string key)
        {
            if (Params.ContainsKey(key))
                return Params[key].ToString();

            CSParameterDef pd = Definition[key];
            return pd.DefaultValue.ToString();
        }

        public virtual float GetFloat(string key)
        {
            if (Params.ContainsKey(key))
            {
                object obj = Params[key];
                if (obj is float)
                    return (float)obj;
            }
            CSParameterDef pd = Definition[key];
            if (pd.DefaultValue is float)
                return (float)pd.DefaultValue;

            return 0f;
        }

        public virtual int GetInt(string key)
        {
            if (Params.ContainsKey(key))
            {
                object obj = Params[key];
                if (obj is int)
                    return (int)obj;
                if (obj is Enum)
                    return (int)obj;
            }

            CSParameterDef pd = Definition[key];
            if (pd.DefaultValue is int)
                return (int)pd.DefaultValue;

            return 0;
        }

        public virtual void SetObject(string key, object obj)
        {
            Params[key] = obj;
            if (p_cdObjectDelegate != null)
            {
                p_cdObjectDelegate.CDObjectValueDidChange(this, key, obj);
            }
        }

        public virtual object GetObject(string key)
        {
            if (Params.ContainsKey(key))
            {
                return Params[key];
            }
            CSParameterDef pd = Definition[key];
            return pd.DefaultValue;
        }

        public virtual void SetColor(string key, Color value)
        {
            Params[key] = value;
            if (p_cdObjectDelegate != null)
            {
                p_cdObjectDelegate.CDObjectValueDidChange(this, key, value);
            }
        }

        public virtual Color GetColor(string key)
        {
            object obj = GetObject(key);
            if (obj is Color)
                return (Color)obj;

            return Color.Empty;
        }


        public virtual void SetTextPadding(string key, CSTextPadding value)
        {
            CSTextPadding p;
            if (Params.ContainsKey(key))
            {
                object po = Params[key];
                if (po is CSTextPadding)
                {
                    p = (CSTextPadding)po;
                    p.Bottom = value.Bottom;
                    p.Left = value.Left;
                    p.Right = value.Right;
                    p.Top = value.Top;
                }
            }
            else
            {
                p = new CSTextPadding(value);
                Params[key] = p;
            }
            if (p_cdObjectDelegate != null)
            {
                p_cdObjectDelegate.CDObjectValueDidChange(this, key, value);
            }
        }

        public virtual CSTextPadding GetTextPadding(string key)
        {
            if (Params.ContainsKey(key))
            {
                object po = Params[key];
                if (po is CSTextPadding)
                {
                    return (CSTextPadding)po;
                }
            }

            CSParameterDef pd = Definition[key];
            if (pd.DefaultValue is CSTextPadding)
                return (CSTextPadding)pd.DefaultValue;

            return CSTextPadding.Default;
        }


        public virtual bool Load(XmlElement elem, CDReaderReferences refs)
        {
            if (elem.HasAttribute("ObjectId"))
                ObjectId = new Guid(elem.GetAttribute("ObjectId"));
            if (elem.HasAttribute("DecomposeId"))
            {
                Guid DecomposeId = new Guid(elem.GetAttribute("DecomposeId"));
                refs.Add(CDLazyType.FindDecompositionObject, DecomposeId, this);
                Decomposition = Project.FindDiagram(DecomposeId);
            }
            if (elem.HasAttribute("DefinitionId"))
            {
                refs.Add(CDLazyType.FindDefinition, new Guid(elem.GetAttribute("DefinitionId")), this);
            }
            foreach(XmlElement E in CDXml.GetChildren(elem, "Param"))
            {
                if (E.HasAttribute("Type") && E.HasAttribute("Name") && E.HasAttribute("Value"))
                {
                    string name = E.GetAttribute("Name");
                    switch(E.GetAttribute("Type"))
                    {
                        case "string":
                            Params[name] = E.GetAttribute("Value");
                            break;
                        case "int":
                            Params[name] = int.Parse(E.GetAttribute("Value"));
                            break;
                        case "float":
                            Params[name] = float.Parse(E.GetAttribute("Value"));
                            break;
                        case "double":
                            Params[name] = double.Parse(E.GetAttribute("Value"));
                            break;
                        case "textPadding":
                            Params[name] = new CSTextPadding(E.GetAttribute("Value"));
                            break;
                        case "color":
                            Params[name] = ColorTranslator.FromHtml(E.GetAttribute("Value"));
                            break;
                        case "textAlign":
                            Params[name] = (CSTextAlign)Enum.Parse(typeof(CSTextAlign), E.GetAttribute("Value"));
                            break;
                        case "connectionsMode":
                            Params[name] = (ConnectionsMode)Enum.Parse(typeof(ConnectionsMode), E.GetAttribute("Value"));
                            break;
                        case "dashStyle":
                            Params[name] = (DashStyle)Enum.Parse(typeof(DashStyle), E.GetAttribute("Value"));
                            break;
                        case "lineEnd":
                            refs.Add(CDLazyType.FindDefinitionForDictionaryValue, new Guid(E.GetAttribute("Value")), this.Params, name);
                            break;
                        case "image":
                            XmlElement de = E["data"];
                            if (de != null)
                            {
                                byte[] b = Convert.FromBase64String(de.InnerText);
                                Image img = CDGraphics.ByteArrayToImage(b);
                                if (img != null)
                                {
                                    Params[name] = img;
                                }
                            }
                            break;
                    }
                }
            }
            return true;
        }

        public virtual bool Save(XmlElement elem, XmlDocument doc)
        {
            if (!ObjectId.Equals(Guid.Empty))
                elem.SetAttribute("ObjectId", ObjectId.ToString());
            if (Decomposition != null)
                elem.SetAttribute("DecomposeId", Decomposition.ObjectId.ToString());
            if (Definition != null)
                elem.SetAttribute("DefinitionId", Definition.DefinitionId.ToString());
            foreach (KeyValuePair<string,object> entry in Params)
            {
                XmlElement E = doc.CreateElement("Param");
                elem.AppendChild(E);
                E.SetAttribute("Name", entry.Key);
                Type vt = entry.Value.GetType();
                if (entry.Value is string)
                {
                    E.SetAttribute("Type", "string");
                    E.SetAttribute("Value", entry.Value.ToString());
                }
                else if (entry.Value is CSTextAlign)
                {
                    E.SetAttribute("Type", "textAlign");
                    E.SetAttribute("Value", entry.Value.ToString());
                }
                else if (entry.Value is CSTextPadding)
                {
                    E.SetAttribute("Type", "textPadding");
                    E.SetAttribute("Value", entry.Value.ToString());
                }
                else if (entry.Value is float)
                {
                    E.SetAttribute("Type", "float");
                    E.SetAttribute("Value", entry.Value.ToString());
                }
                else if (entry.Value is int)
                {
                    E.SetAttribute("Type", "int");
                    E.SetAttribute("Value", entry.Value.ToString());
                }
                else if (entry.Value is Color)
                {
                    E.SetAttribute("Type", "color");
                    E.SetAttribute("Value", ColorTranslator.ToHtml((Color)entry.Value));
                }
                else if (entry.Value is double)
                {
                    E.SetAttribute("Type", "double");
                    E.SetAttribute("Value", entry.Value.ToString());
                }
                else if (entry.Value is ConnectionsMode)
                {
                    E.SetAttribute("Type", "connectionsMode");
                    E.SetAttribute("Value", entry.Value.ToString());
                }
                else if (entry.Value is CDEndingDefinition)
                {
                    E.SetAttribute("Type", "lineEnd");
                    E.SetAttribute("Value", ((CDEndingDefinition)entry.Value).DefinitionId.ToString());
                }
                else if (entry.Value is DashStyle)
                {
                    E.SetAttribute("Type", "dashStyle");
                    E.SetAttribute("Value", entry.Value.ToString());
                }
                else if (entry.Value is Image)
                {
                    E.SetAttribute("Type", "image");
                    byte[] b = CDGraphics.ImageToByteArray((Image)entry.Value);
                    XmlElement DE = doc.CreateElement("data");
                    E.AppendChild(DE);
                    DE.InnerText = Convert.ToBase64String(b);
                }
            }
            return true;
        }
    }

    public interface ICDObjectDelegate
    {
        void CDObjectValueDidChange(object obj, string key, object value);
    }
}
