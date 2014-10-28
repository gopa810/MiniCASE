using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace MiniCASE
{
    public class MatrixArea
    {
        public int row = 0;
        public int column = 0;
        public CaseShape shape = null;

        public List<MatrixArea> prevArea = new List<MatrixArea>();
        public float pathDistance = -1;

        private List<CaseDiagramConnection> hSlots = null;
        private List<CaseDiagramConnection> vSlots = null;
        public int hOffset = 0;
        public int hCenter = 0;
        public int hStep = 0;
        public int vOffset = 0;
        public int vCenter = 0;
        public int vStep = 0;

        public override string ToString()
        {
            return string.Format("{0},{1}", column, row);
        }
        public int ShapeId
        {
            get
            {
                if (shape != null)
                    return shape.id;
                return -1;
            }
        }
        public bool IsHSlotAvailable(int a)
        {
            if (shape != null)
                return true;
            if (hSlots == null)
                return true;
            if (hSlots.Count <= a)
                return true;
            return hSlots[a] == null ? true : false;
        }
        public bool IsVSlotAvailable(int a)
        {
            if (shape != null)
                return true;
            if (vSlots == null)
                return true;
            if (vSlots.Count <= a)
                return true;
            return vSlots[a] == null ? true : false;
        }
        public void SetHSlot(int a, CaseDiagramConnection conn)
        {
            if (shape != null)
                return;
            if (hSlots == null)
                hSlots = new List<CaseDiagramConnection>();
            while (hSlots.Count <= a)
            {
                hSlots.Add(null);
            }
            hSlots[a] = conn;
        }
        public void SetVSlot(int a, CaseDiagramConnection conn)
        {
            if (shape != null)
                return;
            if (vSlots == null)
                vSlots = new List<CaseDiagramConnection>();
            while (vSlots.Count <= a)
            {
                vSlots.Add(null);
            }
            vSlots[a] = conn;
        }
        public int FindHSlot(CaseDiagramConnection conn)
        {
            if (shape != null)
                return -1;
            if (hSlots == null)
                return -1;
            for (int i = 0; i < hSlots.Count; i++)
            {
                CaseDiagramConnection cone = hSlots[i] as CaseDiagramConnection;
                if (cone != null && conn == cone)
                    return i;
            }
            return -1;
        }
        public int FindVSlot(CaseDiagramConnection conn)
        {
            if (shape != null)
                return -1;
            if (vSlots == null)
                return -1;
            for (int i = 0; i < vSlots.Count; i++)
            {
                CaseDiagramConnection cone = vSlots[i] as CaseDiagramConnection;
                if (cone != null && conn == cone)
                    return i;
            }
            return -1;
        }
        public int VerticalSlotCount
        {
            get
            {
                int cnt = 0;
                if (vSlots != null)
                {
                    foreach (CaseDiagramConnection cdc in vSlots)
                    {
                        if (cdc != null)
                            cnt++;
                    }
                }
                return cnt;
            }
        }
        public int HorizontalSlotCount
        {
            get
            {
                int cnt = 0;
                if (hSlots != null)
                {
                    foreach (CaseDiagramConnection cdc in hSlots)
                    {
                        if (cdc != null)
                            cnt++;
                    }
                }
                return cnt;
            }
        }
        public int SlotCount
        {
            get
            {
                return VerticalSlotCount + HorizontalSlotCount;
            }
        }
        public int HorizontalSlotAllocatedCount
        {
            get
            {
                if (hSlots == null)
                    return 0;
                return hSlots.Count;
            }
        }
        public int VerticalSlotAllocatedCount
        {
            get
            {
                if (vSlots == null)
                    return 0;
                return vSlots.Count;
            }
        }

        public void RecalculateHorizontalSlots(int from, int to)
        {
            int count = HorizontalSlotAllocatedCount + 2;
            vStep = 8;
            DiagramDrawMatrix.MatrixRange.ValidateRangeValues(ref from, ref to);
            if (Math.Abs(to - from) < vStep * count)
            {
                vStep = Math.Abs(to - from) / count;
            }
            vCenter = (from + to) / 2;
            vOffset = (from + to - count * vStep) / 2 + vStep;
        }

        public void RecalculateVerticalSlots(int from, int to)
        {
            int count = VerticalSlotAllocatedCount + 2;
            hStep = 8;
            DiagramDrawMatrix.MatrixRange.ValidateRangeValues(ref from, ref to);
            if (Math.Abs(to - from) < hStep * count)
            {
                hStep = Math.Abs(to - from) / count;
            }
            hCenter = (from + to) / 2;
            hOffset = (from + to - count * hStep) / 2 + hStep;
        }

        public Point GetPointForConnection(CaseDiagramConnection conn)
        {
            int x, y;
            int idx = 0;

            idx = FindHSlot(conn);
            if (idx < 0)
                y = vCenter;
            else
                y = vOffset + idx * vStep;

            idx = FindVSlot(conn);
            if (idx < 0)
                x = hCenter;
            else
                x = hOffset + idx * hStep;

            return new Point(x, y);
        }

        public void ClearSlots()
        {
            hSlots = null;
            vSlots = null;
        }

        public Point GetAnchorPoint(Point logicalCoord, Point physCoord)
        {
            Point ret = new Point();
            if (shape == null)
                return ret;
            if (logicalCoord.X < column)
            {
                ret.Y = physCoord.Y;
                ret.X = shape.Bounds.Xa;
            }
            else if (logicalCoord.X > column)
            {
                ret.Y = physCoord.Y;
                ret.X = shape.Bounds.Xb;
            }
            else if (logicalCoord.Y < row)
            {
                ret.X = physCoord.X;
                ret.Y = shape.Bounds.Ya;
            }
            else
            {
                ret.X = physCoord.X;
                ret.Y = shape.Bounds.Yb;
            }

            return ret;
        }

        public void CreatePaths(DiagramDrawMatrix.FoundPath list, List<DiagramDrawMatrix.FoundPath> paths)
        {
            foreach (MatrixArea ma in prevArea)
            {
                ma.CreatePaths(list.PathByInsertingArea(ma), paths);
            }
            if (pathDistance < 1)
                paths.Add(list);
        }

        public int GetHSlotAt(int y)
        {
            int position = y - vOffset;
            int curr = 0;

            for (int i = 0; i < 20; i++)
            {
                if (curr >= position - 2)
                {
                    if (curr <= position + 2)
                        return i;
                    else
                        return -1;
                }
                curr += vStep;
            }

            return -1;
        }

        public int GetVSlotAt(int x)
        {
            int position = x - hOffset;
            int curr = 0;

            for (int i = 0; i < 20; i++)
            {
                if (curr >= position - 2)
                {
                    if (curr <= position + 2)
                        return i;
                    else
                        return -1;
                }
                curr += hStep;
            }

            return -1;
        }

        public CaseDiagramConnection GetConnectionAtHSlot(int hslot)
        {
            if (hSlots == null || hslot < 0 || hslot >= hSlots.Count)
                return null;

            return hSlots[hslot];
        }

        public CaseDiagramConnection GetConnectionAtVSlot(int vslot)
        {
            if (vSlots == null || vslot < 0 || vslot >= vSlots.Count)
                return null;

            return vSlots[vslot];
        }

    }
}
