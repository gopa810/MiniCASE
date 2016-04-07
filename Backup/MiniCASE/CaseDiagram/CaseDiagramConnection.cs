using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MiniCASE
{
    public class CaseDiagramConnection
    {
        public int startId;
        public int endId;
        public int type;
        public string startNote;
        public string centerNode;
        public string endNote;
        public bool validCoordinates = false;
        public DiagramPath path = null;
        public Point[] coordinates = null;
        public bool visible = true;
        public bool selected = false;
    }

}
