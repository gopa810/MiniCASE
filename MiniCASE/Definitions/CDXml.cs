using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MiniCASE
{
    public class CDXml
    {
        public static List<XmlElement> GetChildren(XmlElement elem, string name)
        {
            List<XmlElement> EL = new List<XmlElement>();
            foreach(XmlElement e in elem.GetElementsByTagName(name))
            {
                if (e.ParentNode == elem)
                    EL.Add(e);
            }
            return EL;
        }
    }
}
