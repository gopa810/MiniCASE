using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Xml;

namespace MiniCASE
{
    /// <summary>
    /// Model object containing data required for one diagram.
    /// Diagram consists of shapes, links, notes, etc...
    /// </summary>
    public class CDDiagram: CDObject
    {
        private int selectedCount = 0;
        public List<CDShape> ShapeArray = new List<CDShape>();
        public List<CDConnection> ConnArray = new List<CDConnection>();
        public List<CDShape> GroupArray = new List<CDShape>();

        public CDDocumentDefinition DiagramDefinition
        {
            get
            {
                return (CDDocumentDefinition)Definition;
            }
        }

        public CDDiagram(CDProject prj, CDDocumentDefinition def, Guid oid): base(prj, def, oid)
        {
        }

        public Rectangle DiagramRect
        {
            get
            {
                bool inits = false;
                RectangleD rect = new RectangleD();

                foreach (CDShape shape in ShapeArray)
                {
                    if (inits)
                    {
                        rect.Merge(shape.Bounds);
                    }
                    else
                    {
                        rect.Set(shape.Bounds);
                        inits = true;
                    }
                }

                foreach (CDShape shape in GroupArray)
                {
                    if (inits)
                    {
                        rect.Merge(shape.Bounds);
                    }
                    else
                    {
                        rect.Set(shape.Bounds);
                        inits = true;
                    }
                }

                return rect.Rectangle;
            }
        }

        public void ClearHighlight()
        {
            foreach (CDShape shape in ShapeArray)
            {
                shape.Highlighted = false;
            }
            foreach (CDShape shape in GroupArray)
            {
                shape.Highlighted = false;
            }
        }

        public int SelectedCount
        {
            get
            {
                return selectedCount;
            }
        }

        public void AddShape(CDShape shape)
        {
            ShapeArray.Add(shape);
        }

        public CDShape FindShape(Guid shapeId)
        {
            foreach (CDShape sh in ShapeArray)
            {
                if (sh.ObjectId.Equals(shapeId))
                    return sh;
            }
            return null;
        }

        public CDConnection AddConnection(CDConnectionDefinition cd, CDShape aStartId, CDShape anEndId)
        {
            CDConnection conn = new CDConnection(this, cd, Guid.NewGuid());

            conn.StartShape = aStartId;
            conn.EndShape = anEndId;
            conn.validCoordinates = false;

            ConnArray.Add(conn);

            return conn;
        }

        public void ClearSelection()
        {
            foreach (CDShape shape in ShapeArray)
            {
                shape.Selected = false;
            }
            foreach (CDShape shape in GroupArray)
            {
                shape.Selected = false;
            }
            foreach (CDConnection conn in ConnArray)
            {
                conn.selected = false;
            }
        }

        public void SaveSelectionPosition()
        {
            selectedCount = 0;
            foreach (CDShape shape in ShapeArray)
            {
                //if (shape.Selected)
                {
                    selectedCount++;
                    shape.SavePosition();
                }
            }
        }

        public void SetSelectionOffset(Size offset)
        {
            foreach (CDShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    shape.SetOffset(offset);
                }
            }
        }

        public void ConfirmSelectionPosition()
        {
            foreach (CDShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    shape.ConfirmPosition();
                }
            }
        }

        public void DeleteSelection()
        {
            List<CDShape> arr = new List<CDShape>();

            foreach (CDShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    DeleteConnectionsStartingIn(shape);
                }
                else
                {
                    arr.Add(shape);
                }
            }

            ShapeArray.Clear();
            ShapeArray.AddRange(arr);

            List<CDConnection> arrc = new List<CDConnection>();
            foreach (CDConnection conn in ConnArray)
            {
                conn.validCoordinates = false;
                if (!conn.selected)
                {
                    arrc.Add(conn);
                }
            }
            ConnArray.Clear();
            ConnArray.AddRange(arrc);
        }

        public void DeleteConnectionsStartingIn(CDShape shapeId)
        {
            List<CDConnection> arr = new List<CDConnection>();
            foreach (CDConnection conn in ConnArray)
            {
                if (conn.StartShape != shapeId && conn.EndShape != shapeId)
                {
                    arr.Add(conn);
                }
            }
            ConnArray.Clear();
            ConnArray.AddRange(arr);
        }

        public void InvalidateSelectedShapesGroupBoundaries()
        {
            foreach (CDShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    string groupName = shape.GetString("ParentGroup");
                    
                    foreach (CDConnection conn in ConnArray)
                    {
                        if (conn.EndShape == shape || conn.StartShape == shape)
                            conn.validCoordinates = false;
                    }
                    
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        foreach (CDShape grp in GroupArray)
                        {
                            if (groupName.Equals(grp.GetString("GroupName")))
                            {
                                grp.FlagNeedsRecalculateBounds = true;
                            }
                        }
                    }
                }
            }
        }

        public void RecalculateChangedConnections()
        {
            foreach (CDConnection conn in ConnArray)
            {
                if (conn.validCoordinates)
                    continue;

                object obj = conn.GetObject("PathStyle");
                ConnectionsMode cm = (obj is ConnectionsMode) ? (ConnectionsMode)obj : ConnectionsMode.Straight;

                if (cm == ConnectionsMode.Straight)
                {
                    CDShape shape1 = conn.StartShape;
                    CDShape shape2 = conn.EndShape;

                    Point startPoint = shape1.Bounds.GetLogicalPoint(conn.startRelOffset);
                    Point endPoint = shape2.Bounds.GetLogicalPoint(conn.endRelOffset);
                    Point c1, c2;
                    Point a1, a2;
                    ShapeAnchor n1, n2;

                    if (shape1.GetBorderPoint(startPoint, endPoint, out c1, out a1, out n1) &&
                        shape2.GetBorderPoint(startPoint, endPoint, out c2, out a2, out n2))
                    {
                        conn.coordinates = new Point[] { c1, c2 };
                        conn.validCoordinates = true;
                    }
                }
                else if (cm == ConnectionsMode.Rectangular)
                {
                    CDShape shape1 = conn.StartShape;
                    CDShape shape2 = conn.EndShape;

                    Point startPoint = shape1.Bounds.GetLogicalPoint(conn.startRelOffset);
                    Point endPoint = shape2.Bounds.GetLogicalPoint(conn.endRelOffset);
                    Point c1, c2;
                    Point a1, a2;
                    Point middle;
                    ShapeAnchor n1, n2;

                    if (shape1.GetBorderPoint(startPoint, endPoint, out c1, out a1, out n1) &&
                        shape2.GetBorderPoint(startPoint, endPoint, out c2, out a2, out n2))
                    {
                        switch (n1)
                        {
                            case ShapeAnchor.Bottom:
                            case ShapeAnchor.Top: c1.X = a1.X = startPoint.X; break;
                            case ShapeAnchor.Right:
                            case ShapeAnchor.Left: c1.Y = a1.Y = startPoint.Y; break;
                        }
                        switch (n2)
                        {
                            case ShapeAnchor.Bottom:
                            case ShapeAnchor.Top: c2.X = a2.X = endPoint.X; break;
                            case ShapeAnchor.Right:
                            case ShapeAnchor.Left: c2.Y = a2.Y = endPoint.Y; break;
                        }
                        middle = new Point((a1.X + a2.X) / 2, (a1.Y + a2.Y) / 2);
                        switch (n1)
                        {
                            case ShapeAnchor.Top:
                                switch (n2)
                                {
                                    case ShapeAnchor.Bottom:
                                        conn.coordinates = new Point[] { c1, a1, new Point(a1.X, middle.Y), new Point(a2.X, middle.Y), a2, c2 };
                                        conn.validCoordinates = true;
                                        break;
                                    case ShapeAnchor.Right:
                                    case ShapeAnchor.Left:
                                        conn.coordinates = new Point[] { c1, a1, new Point(a1.X, a2.Y), a2, c2 };
                                        conn.validCoordinates = true;
                                        break;
                                }
                                break;
                            case ShapeAnchor.Right:
                                switch (n2)
                                {
                                    case ShapeAnchor.Top:
                                    case ShapeAnchor.Bottom:
                                        conn.coordinates = new Point[] { c1, a1, new Point(a2.X, a1.Y), a2, c2 };
                                        conn.validCoordinates = true;
                                        break;
                                    case ShapeAnchor.Left:
                                        conn.coordinates = new Point[] { c1, a1, new Point(middle.X, a1.Y), new Point(middle.X, a2.Y), a2, c2 };
                                        conn.validCoordinates = true;
                                        break;
                                }
                                break;
                            case ShapeAnchor.Bottom:
                                switch (n2)
                                {
                                    case ShapeAnchor.Top:
                                        conn.coordinates = new Point[] { c1, a1, new Point(a1.X, middle.Y), new Point(a2.X, middle.Y), a2, c2 };
                                        conn.validCoordinates = true;
                                        break;
                                    case ShapeAnchor.Right:
                                    case ShapeAnchor.Left:
                                        conn.coordinates = new Point[] { c1, a1, new Point(a1.X, a2.Y), a2, c2 };
                                        conn.validCoordinates = true;
                                        break;
                                }
                                break;
                            case ShapeAnchor.Left:
                                switch (n2)
                                {
                                    case ShapeAnchor.Right:
                                        conn.coordinates = new Point[] { c1, a1, new Point(middle.X, a1.Y), new Point(middle.X, a2.Y), a2, c2 };
                                        conn.validCoordinates = true;
                                        break;
                                    case ShapeAnchor.Top:
                                    case ShapeAnchor.Bottom:
                                        conn.coordinates = new Point[] { c1, a1, new Point(a2.X, a1.Y), a2, c2 };
                                        conn.validCoordinates = true;
                                        break;
                                }
                                break;
                        }
                        if (!conn.validCoordinates)
                        {
                            conn.coordinates = new Point[] { c1, c2 };
                            conn.validCoordinates = true;
                        }
                    }
                }
            }
        }

        public void SetChildrenOffset(string v, Size moveOffset)
        {
            if (string.IsNullOrWhiteSpace(v))
                return;

            foreach(CDShape shape in ShapeArray)
            {
                if (shape.GetString("ParentGroup") == v)
                {
                    shape.SetOffset(moveOffset);
                    foreach (CDConnection conn in ConnArray)
                    {
                        if (conn.EndShape == shape || conn.StartShape == shape)
                            conn.validCoordinates = false;
                    }
                }
            }

            foreach(CDShape group in GroupArray)
            {
                if (group.GetString("ParentGroup") == v)
                {
                    SetChildrenOffset(group.GetString("GroupName"), moveOffset);
                    group.FlagNeedsRecalculateBounds = true;
                }
            }
        }

        public CDShape GetShapeAtPoint(int px, int py)
        {
            foreach (CDShape sh in ShapeArray)
            {
                if (sh.Bounds.ContainsPoint(px, py))
                    return sh;
            }

            foreach (CDShape sh in GroupArray)
            {
                if (sh.Bounds.ContainsPoint(px, py))
                    return sh;
            }

            return null;
        }

        public CDConnection GetConnectionAtPoint(int px, int py)
        {
            int dist = 10;
            foreach(CDConnection con in ConnArray)
            {
                if (con.validCoordinates)
                {
                    Point[] C = con.coordinates;
                    for (int i = 0; i < C.Length - 1; i++)
                    {
                        if (C[i].X == C[i + 1].X)
                        {
                            if (C[i].Y < C[i + 1].Y)
                            {
                                if (Math.Abs(px - C[i].X) <= dist && C[i].Y - dist <= py && C[i + 1].Y + dist >= py)
                                    return con;
                            }
                            else
                            {
                                if (Math.Abs(px - C[i].X) <= dist && C[i].Y - dist >= py && C[i + 1].Y + dist <= py)
                                    return con;
                            }
                        }
                        else if (C[i].Y == C[i+1].Y)
                        {
                            if (C[i].X < C[i + 1].X)
                            {
                                if (Math.Abs(py - C[i].Y) <= dist && C[i].X - dist <= px && C[i + 1].X + dist >= px)
                                    return con;
                            }
                            else
                            {
                                if (Math.Abs(py - C[i].Y) <= dist && C[i].X - dist >= px && C[i + 1].X + dist <= px)
                                    return con;
                            }
                        }
                        else
                        {
                            if (DistanceLineAndPoint(C[i].X, C[i].Y, C[i+1].X, C[i+1].Y, px, py, dist))
                                return con;
                        }
                    }
                }
            }
            return null;
        }

        public bool DistanceLineAndPoint(int line1X, int line1Y, int line2X, int line2Y, int pointX, int pointY, double dist)
        {
            double a, b;
            double d1, d2;

            a = Math.Abs((line2Y - line1Y)*pointX - (line2X - line1X)*pointY + line2X*line1Y - line2Y*line1X);
            b = Math.Sqrt(Math.Pow(line2Y - line1Y, 2) + Math.Pow(line2X - line1X, 2));

            d1 = Math.Sqrt(Math.Pow(pointY - line1Y, 2) + Math.Pow(pointX - line1X, 2));
            d2 = Math.Sqrt(Math.Pow(pointY - line2Y, 2) + Math.Pow(pointX - line2X, 2));

            if (dist < (d1 + d2 - b))
                return false;

            return (a / b) < dist;
        }

        public CDShape FindGroup(string p)
        {
            if (string.IsNullOrEmpty(p))
                return null;
            foreach (CDShape grp in GroupArray)
            {
                if (p.Equals(grp.GetString("GroupName")))
                    return grp;
            }

            return null;
        }


        public void CheckExistenceGroups()
        {
            HashSet<CDShape> groups = new HashSet<CDShape>();
            foreach (CDShape g in GroupArray)
                groups.Add(g);

            foreach (CDShape shape in ShapeArray)
            {
                string grpName = shape.GetString("ParentGroup");
                if (!string.IsNullOrEmpty(grpName))
                {
                    CDShape grp = FindGroup(grpName);
                    groups.Remove(grp);
                    if (grp == null)
                    {
                        grp = new CDShape(this, CDLibrary.GroupShape, Guid.NewGuid());
                        grp.FlagNeedsRecalculateBounds = true;
                        grp.SetString("GroupName", grpName);
                        GroupArray.Add(grp);
                    }
                }
            }

            // remove unused groups
            foreach (CDShape g in groups)
            {
                GroupArray.Remove(g);
            }
        }

        public void RecalculateChangedGroupBounds()
        {
            string gi, gj;
            // sort groups
            for (int i = 0; i < GroupArray.Count - 1; i++)
            {
                gi = GroupArray[i].GetString("ParentGroup");
                if (string.IsNullOrEmpty(gi))
                    continue;
                for (int j = i + 1; j < GroupArray.Count; j++)
                {
                    gj = GroupArray[j].GetString("GroupName");
                    if (gi.Equals(gj))
                    {
                        CDShape s = GroupArray[i];
                        GroupArray[i] = GroupArray[j];
                        GroupArray[j] = s;
                    }
                }
            }

            for (int i = GroupArray.Count - 1; i >= 0; i--)
            {
                string grpName;
                CDShape grp = GroupArray[i];
                if (grp.FlagNeedsRecalculateBounds)
                {
                    grpName = grp.GetString("GroupName");
                    bool finit = false;
                    foreach (CDShape shape in ShapeArray)
                    {
                        if (grpName.Equals(shape.GetString("ParentGroup")))
                        {
                            if (finit)
                                grp.Bounds.Merge(shape.Bounds);
                            else
                            {
                                grp.Bounds.Set(shape.Bounds);
                                finit = true;
                            }
                        }
                    }
                    foreach (CDShape shape in GroupArray)
                    {
                        if (grpName.Equals(shape.GetString("ParentGroup")))
                        {
                            if (finit)
                                grp.Bounds.Merge(shape.Bounds);
                            else
                            {
                                grp.Bounds.Set(shape.Bounds);
                                finit = true;
                            }
                        }
                    }

                    CSTextPadding padding = grp.GetTextPadding("Margin");
                    grp.Bounds.Left -= padding.Left;
                    grp.Bounds.Top -= padding.Top;
                    grp.Bounds.Right += padding.Right;
                    grp.Bounds.Bottom += padding.Bottom;

                }
            }
        }


        public override bool Load(XmlElement elem, CDReaderReferences refs)
        {
            if (!base.Load(elem, refs))
                return false;

            foreach(XmlElement E in CDXml.GetChildren(elem, "shape"))
            {
                CDShape shape = new CDShape(this, null, Guid.Empty);
                if (shape.Load(E, refs))
                    ShapeArray.Add(shape);
            }

            foreach(XmlElement E in CDXml.GetChildren(elem, "connection"))
            {
                CDConnection conn = new CDConnection(this, null, Guid.Empty);
                if (conn.Load(E, refs))
                    ConnArray.Add(conn);
            }

            foreach(XmlElement E in CDXml.GetChildren(elem, "group"))
            {
                CDShape shape = new CDShape(this, null, Guid.Empty);
                if (shape.Load(E, refs))
                    GroupArray.Add(shape);
            }

            return true;
        }

        public override bool Save(XmlElement elem, XmlDocument doc)
        {
            foreach(CDShape shape in ShapeArray)
            {
                XmlElement E = doc.CreateElement("shape");
                elem.AppendChild(E);
                shape.Save(E, doc);
            }

            foreach(CDConnection conn in ConnArray)
            {
                XmlElement E = doc.CreateElement("connection");
                elem.AppendChild(E);
                conn.Save(E, doc);
            }

            foreach (CDShape shape in GroupArray)
            {
                XmlElement E = doc.CreateElement("group");
                elem.AppendChild(E);
                shape.Save(E, doc);
            }

            return base.Save(elem, doc);
        }

        public void DeleteShape(CDShape shape)
        {
            for(int i = ConnArray.Count - 1; i >= 0; i --)
            {
                CDConnection conn = ConnArray[i];
                if (conn.StartShape == shape || conn.EndShape == shape)
                {
                    ConnArray.RemoveAt(i);
                }
            }

            for(int i = ShapeArray.Count - 1; i >= 0; i--)
            {
                if (ShapeArray[i] == shape)
                {
                    ShapeArray.RemoveAt(i);
                }
            }
        }

        public void DeleteConnections(CDConnection conn)
        {
            for (int i = ConnArray.Count - 1; i >= 0; i--)
            {
                if (ConnArray[i] == conn)
                {
                    ConnArray.RemoveAt(i);
                }
            }
        }
    }

}
