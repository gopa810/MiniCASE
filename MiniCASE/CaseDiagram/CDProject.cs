using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MiniCASE
{
    public class CDProject: CDProjectBase
    {
        /// <summary>
        /// ID counter for objects
        /// </summary>
        private int CounterId = 1;

        /// <summary>
        /// List of diagrams
        /// </summary>
        public List<CDDiagram> diagrams = new List<CDDiagram>();

        private CDLibrary p_library = null;

        public CDLibrary Library
        {
            get
            {
                if (p_library == null) return MiniCaseApp.Library;
                return p_library;
            }
            set
            {
                p_library = value;
            }
        }

        public void InitProject()
        {
            CDDiagram diagram = new CDDiagram(this, Library.DefaultDocumentType, Guid.NewGuid());
            Library = MiniCaseApp.Library;

            CDShapeDefinition shd = diagram.DiagramDefinition.FindShapeDefinition("Shape.Process");

            CDShape cs = new CDShape(diagram, shd, Guid.NewGuid());
            cs.Bounds = new RectangleD(50, 50, 130, 100);
            diagram.AddShape(cs);

            CDShape cs2 = new CDShape(diagram, shd, Guid.NewGuid());
            cs2.Bounds = new RectangleD(150, 200, 230, 250);
            diagram.AddShape(cs2);

            CDShape cs3 = new CDShape(diagram, shd, Guid.NewGuid());
            cs3.Bounds = new RectangleD(250, 90, 340, 140);
            diagram.AddShape(cs3);

            CDShape cs4 = new CDShape(diagram, shd, Guid.NewGuid());
            cs4.Bounds = new RectangleD(20, 250, 220, 340);
            diagram.AddShape(cs4);

            //diagram.AddConnection(cs.id, cs2.id, 0);
            CDConnection conn = diagram.AddConnection(diagram.DiagramDefinition.DefaultConnectionType, cs, cs3);

            diagrams.Add(diagram);
        }

        public CDDiagram RootDiagram
        {
            get
            {
                if (diagrams.Count == 0)
                {
                    InitProject();
                }

                return diagrams[0];
            }
        }

        public CDDiagram FindDiagram(Guid diagramObjectId)
        {
            foreach(CDDiagram obj in diagrams)
            {
                if (obj.ObjectId.Equals(diagramObjectId))
                    return obj;
            }

            return null;
        }

        public CDShape FindShape(Guid shapeId)
        {
            foreach (CDDiagram obj in diagrams)
            {
                foreach (CDShape shp in obj.ShapeArray)
                {
                    if (shp.ObjectId.Equals(shapeId))
                        return shp;
                }
            }

            return null;
        }

        public bool Load(string filePath)
        {
            CDReaderReferences refs = new CDReaderReferences();
            diagrams.Clear();

            // TODO: implement loading data
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlElement RE = doc["document"];
            if (RE != null)
            {
                if (RE.HasAttribute("nextid"))
                    CounterId = int.Parse(RE.GetAttribute("nextid"));

                foreach(XmlElement E in CDXml.GetChildren(RE, "diagram"))
                {
                    CDDiagram diagram = new CDDiagram(this, null, Guid.Empty);
                    if (diagram.Load(E, refs))
                    {
                        diagrams.Add(diagram);
                    }
                }

                XmlElement libElem = RE["library"];
                if (libElem != null)
                {
                    Library = new CDLibrary();
                    Library.LoadFrom(libElem);
                }
                else
                    Library = MiniCaseApp.Library;
            }


            Type tdef = typeof(CDObjectDefinition);
            Type edef = typeof(CDEndingDefinition);

            foreach(CDLazyReference lz in refs)
            {
                if (lz.lazyType == CDLazyType.FindStartShape)
                {
                    ((CDConnection)lz.targetReference).StartShape = FindShape(lz.findObjectGuid);
                }
                else if (lz.lazyType == CDLazyType.FindEndShape)
                {
                    ((CDConnection)lz.targetReference).EndShape = FindShape(lz.findObjectGuid);
                }
                else if (lz.lazyType == CDLazyType.FindDefinition)
                {
                    if (lz.targetReference is CDObject)
                    {
                        ((CDObject)lz.targetReference).Definition = Library.FindDefinition(lz.findObjectGuid);
                    }
                }
                else if (lz.lazyType == CDLazyType.FindDefinitionForDictionaryValue)
                {
                    if (lz.targetReference is Dictionary<string,object>)
                    {
                        Dictionary<string,object> lpp = (Dictionary<string,object>)lz.targetReference;
                        lpp[lz.targetKey] = Library.FindDefinition(lz.findObjectGuid);
                    }
                }
                else if (lz.lazyType == CDLazyType.FindDecompositionObject)
                {
                    if (lz.targetReference is CDObject)
                    {
                        ((CDObject)lz.targetReference).Decomposition = FindDiagram(lz.findObjectGuid);
                    }
                }
            }
            // false is unsuccessful opening
            return true;
        }

        public bool Save(string filePath)
        {
            FilePath = filePath;

            // TODO: implement saving data
            XmlDocument doc = new XmlDocument();

            XmlElement root = doc.CreateElement("document");
            doc.AppendChild(root);

            root.SetAttribute("nextid", CounterId.ToString());

            foreach(CDDiagram diagram in diagrams)
            {
                XmlElement ediag = doc.CreateElement("diagram");
                root.AppendChild(ediag);

                diagram.Save(ediag, doc);
            }

            XmlElement libElem = doc.CreateElement("library");
            root.AppendChild(libElem);
            Library.WriteTo(doc, libElem);

            doc.Save(filePath);

            // false is unsuccessful saving
            // true is success
            return true;
        }
    }
}
