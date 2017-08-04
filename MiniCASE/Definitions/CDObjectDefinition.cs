using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MiniCASE
{
    public class CDObjectDefinition
    {
        public Guid DefinitionId = Guid.Empty;
        public string Name = string.Empty;
        public string Description = string.Empty;
        public List<CSParameterDef> parameters = new List<CSParameterDef>();



        public CDObjectDefinition(Guid oid)
        {
            DefinitionId = oid;
        }

        public void Add(CSParameterDef parameterDefinition)
        {
            parameters.Add(parameterDefinition);
        }

        public void AddParameter(string name, CSParameterType type, object defaultValue)
        {
            parameters.Add(new CSParameterDef(name, type, defaultValue));
        }

        public bool ContainsKey(string key)
        {
            foreach (CSParameterDef pd in parameters)
            {
                if (pd.Name.Equals(key))
                    return true;
            }
            return false;
        }

        public CSParameterDef this[string key]
        {
            get
            {
                foreach(CSParameterDef pd in parameters)
                {
                    if (pd.Name.Equals(key))
                        return pd;
                }
                return CSParameterDef.Empty;
            }
        }

        public virtual void WriteTo(XmlElement elem, XmlDocument doc)
        {
            elem.SetAttribute("name", Name);
            elem.SetAttribute("description", Description);
            elem.SetAttribute("guid", DefinitionId.ToString());

            foreach(CSParameterDef pd in parameters)
            {
                XmlElement E = doc.CreateElement("parameterDefinition");
                elem.AppendChild(E);
                pd.WriteTo(E, doc);
            }
        }

        public virtual void ReadFrom(XmlElement elem, CDReaderReferences refs)
        {
            Name = elem.GetAttribute("name");
            Description = elem.GetAttribute("description");
            if (elem.HasAttribute("guid"))
                DefinitionId = new Guid(elem.GetAttribute("guid"));
            else
                DefinitionId = Guid.NewGuid();
            parameters.Clear();

            foreach(XmlElement E in CDXml.GetChildren(elem, "parameterDefinition"))
            {
                if (E.ParentNode != elem)
                    continue;
                CSParameterDef pd = new CSParameterDef();
                pd.ReadFrom(E, refs);
                parameters.Add(pd);
            }
        }
    }
}
