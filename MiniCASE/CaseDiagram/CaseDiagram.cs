using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace MiniCASE
{
    /// <summary>
    /// Model object containing data required for one diagram.
    /// Diagram consists of shapes, links, notes, etc...
    /// </summary>
    public class CaseDiagram
    {
        public int DiagramId = 0;
        private int selectedCount = 0;
        public List<CaseShape> ShapeArray = new List<CaseShape>();
        public List<CaseDiagramConnection> ConnArray = new List<CaseDiagramConnection>();

        public Rectangle DiagramRect
        {
            get
            {
                bool inits = false;
                RectangleD rect = new RectangleD();

                foreach (CaseShape shape in ShapeArray)
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
            foreach (CaseShape shape in ShapeArray)
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

        public void AddShape(CaseShape shape)
        {
            int maxid = 0;
            foreach (CaseShape sh in ShapeArray)
            {
                maxid = Math.Max(maxid, sh.id);
            }
            shape.id = maxid + 1;
            ShapeArray.Add(shape);

        }

        public CaseShape FindShape(int shapeId)
        {
            foreach (CaseShape sh in ShapeArray)
            {
                if (sh.id == shapeId)
                    return sh;
            }
            return null;
        }

        public CaseDiagramConnection AddConnection(int aStartId, int anEndId)
        {
            CaseDiagramConnection conn = new CaseDiagramConnection();

            conn.startId = aStartId;
            conn.endId = anEndId;
            conn.validCoordinates = false;

            ConnArray.Add(conn);

            return conn;
        }

        public void ClearSelection()
        {
            foreach (CaseShape shape in ShapeArray)
            {
                shape.Selected = false;
            }
            foreach (CaseDiagramConnection conn in ConnArray)
            {
                conn.selected = false;
            }
        }

        public void SaveSelectionPosition()
        {
            selectedCount = 0;
            foreach (CaseShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    selectedCount++;
                    shape.SavePosition();
                }
            }
        }

        public void SetSelectionOffset(Size offset)
        {
            foreach (CaseShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    shape.SetOffset(offset);
                }
            }
        }

        public void ConfirmSelectionPosition()
        {
            foreach (CaseShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    shape.ConfirmPosition();
                }
            }
        }

        public void DeleteSelection()
        {
            List<CaseShape> arr = new List<CaseShape>();

            foreach (CaseShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    DeleteConnectionsStartingIn(shape.id);
                }
                else
                {
                    arr.Add(shape);
                }
            }

            ShapeArray.Clear();
            ShapeArray.AddRange(arr);

            List<CaseDiagramConnection> arrc = new List<CaseDiagramConnection>();
            foreach (CaseDiagramConnection conn in ConnArray)
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

        public void DeleteConnectionsStartingIn(int shapeId)
        {
            List<CaseDiagramConnection> arr = new List<CaseDiagramConnection>();
            foreach (CaseDiagramConnection conn in ConnArray)
            {
                if (conn.startId != shapeId && conn.endId != shapeId)
                {
                    arr.Add(conn);
                }
            }
            ConnArray.Clear();
            ConnArray.AddRange(arr);
        }

        public void InvalidateMovedShapesConnections()
        {
            foreach (CaseShape shape in ShapeArray)
            {
                if (shape.Selected)
                {
                    foreach (CaseDiagramConnection conn in ConnArray)
                    {
                        if (conn.endId == shape.id || conn.startId == shape.id)
                            conn.validCoordinates = false;
                    }
                }
            }
        }

        public CaseShape GetShapeAtPoint(int px, int py)
        {
            foreach (CaseShape sh in ShapeArray)
            {
                if (sh.Bounds.ContainsPoint(px, py))
                    return sh;
            }

            return null;
        }

        public CaseDiagramConnection GetConnectionAtPoint(int px, int py)
        {
            return null;
        }
    }

}
