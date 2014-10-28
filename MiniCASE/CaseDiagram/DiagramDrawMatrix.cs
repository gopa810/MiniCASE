using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;

namespace MiniCASE
{
    public class DiagramDrawMatrix
    {
        public class PathFinderRecord
        {
            public int column = 0;
            public int row = 0;
            public PathFinderRecord next = null;
            public PathFinderRecord()
            {
            }
            public PathFinderRecord(int col, int rw)
            {
                column = col;
                row = rw;
            }
        }

        public class MatrixRange
        {
            public static readonly int MINX = -132767;
            public static readonly int MAXX = 132768;

            public int from = MINX;
            public int end = MAXX;

            public int Center
            {
                get
                {
                    int a = from;
                    int b = end;
                    ValidateRangeValues(ref a, ref b);
                    return (a + b) / 2;
                }
            }

            public float Width
            {
                get
                {
                    int a = from;
                    int b = end;
                    ValidateRangeValues(ref a, ref b);
                    return (a == b) ? 1 : (b - a);
                }
            }

            public static void ValidateRangeValues(ref int a, ref int b)
            {
                if (a == MINX && b == MAXX)
                {
                    a = -64;
                    b = 64;
                }
                else if (a == MINX)
                {
                    a = b - 80;
                }
                else if (b == MAXX)
                {
                    b = a + 80;
                }
            }
        }

        public class FoundPath
        {
            public List<MatrixArea> path = new List<MatrixArea>();

            public FoundPath()
            {
            }

            public FoundPath(MatrixArea area)
            {
                path.Add(area);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (MatrixArea ma in path)
                {
                    sb.AppendFormat(" {0},{1}", ma.column, ma.row);
                }

                sb.AppendFormat(" ==> {0}", Distance);
                return sb.ToString();
            }

            public FoundPath PathByAddingArea(MatrixArea area)
            {
                foreach (MatrixArea ma in path)
                {
                    if (ma.column == area.column && ma.row == area.row)
                        return null;
                }
                FoundPath newp = new FoundPath();
                newp.path.AddRange(this.path);
                newp.path.Add(area);
                return newp;
            }

            public FoundPath PathByInsertingArea(MatrixArea area)
            {
                foreach (MatrixArea ma in path)
                {
                    if (ma.column == area.column && ma.row == area.row)
                        return null;
                }
                FoundPath newp = new FoundPath();
                newp.path.Add(area);
                newp.path.AddRange(this.path);
                return newp;
            }

            public int GetDirection(MatrixArea from, MatrixArea end)
            {
                int x = end.column - from.column;
                int y = end.row - from.row;
                int dir = 0;

                if (x < 0)
                {
                    if (y < 0)
                    {
                        dir = 1;
                    }
                    else if (y > 0)
                    {
                        dir = 2;
                    }
                    else
                    {
                        dir = 3;
                    }
                }
                else if (x > 0)
                {
                    if (y < 0)
                    {
                        dir = 4;
                    }
                    else if (y > 0)
                    {
                        dir = 5;
                    }
                    else
                    {
                        dir = 6;
                    }
                }
                else
                {
                    if (y < 0)
                    {
                        dir = 7;
                    }
                    else if (y > 0)
                    {
                        dir = 8;
                    }
                    else
                    {
                        dir = 9;
                    }
                }

                return dir;
            }

            public float Distance
            {
                get
                {
                    float dist = 0;
                    int prevdir = 0;
                    int dir = 0;
                    for (int i = 1; i < path.Count; i++)
                    {
                        dist++;
                        dir = GetDirection(path[i - 1], path[i]);
                        if (dir != prevdir)
                            dist+=1;
                        prevdir = dir;
                    }
                    return dist;
                }
            }
        }

        public List<MatrixRange> hRanges = null;
        public List<MatrixRange> vRanges = null;
        public MatrixArea[,] aMatrix = null;
        public int aMatrixRows = 0;
        public int aMatrixCols = 0;

        public void InitStart()
        {
            hRanges = new List<MatrixRange>();
            vRanges = new List<MatrixRange>();
        }

        public void InsertDivider(List<MatrixRange> ranges, int x)
        {
            if (ranges.Count == 0)
            {
                ranges.Add(new MatrixRange());
            }

            for (int i = 0; i < ranges.Count; i++)
            {
                MatrixRange mr = ranges[i];
                if (mr.from == x || mr.end == x)
                    return;
                if (mr.from < x && mr.end > x)
                {
                    MatrixRange newr = new MatrixRange();
                    newr.from = mr.from;
                    newr.end = x;
                    mr.from = x;
                    ranges.Insert(i, newr);
                    break;
                }
            }
        }

        public void InsertHorizontalDivider(int x)
        {
            InsertDivider(hRanges, x);
        }

        public void InsertVerticalDivider(int x)
        {
            InsertDivider(vRanges, x);
        }

        public void CreateMatrix()
        {
            aMatrixRows = vRanges.Count;
            aMatrixCols = hRanges.Count;
            aMatrix = new MatrixArea[aMatrixCols, aMatrixRows];

            for (int c = 0; c < aMatrixCols; c++)
            {
                for (int r = 0; r < aMatrixRows; r++)
                {
                    aMatrix[c, r] = new MatrixArea();
                    aMatrix[c, r].column = c;
                    aMatrix[c, r].row = r;
                    aMatrix[c, r].prevArea = new List<MatrixArea>();
                    aMatrix[c, r].pathDistance = -1;
                }
            }
        }

        public void GetIndicesForRange(List<MatrixRange> ranges, int x1, int x2, out int index1, out int index2)
        {
            bool started = false;
            index1 = -1;
            index2 = -1;
            for (int i = 0; i < ranges.Count; i++)
            {
                MatrixRange mr = ranges[i];
                if (mr.from >= x1 && mr.end <= x2)
                {
                    if (started)
                    {
                        index2 = i;
                    }
                    else
                    {
                        index1 = i;
                        index2 = i;
                        started = true;
                    }
                }
                else
                {
                    if (started)
                        break;
                }
            }
        }

        public void StartMatrixPaths(List<PathFinderRecord> list, int startShapeId)
        {
            list.Clear();
            for (int c = 0; c < aMatrixCols; c++)
            {
                for (int r = 0; r < aMatrixRows; r++)
                {
                    MatrixArea ma = aMatrix[c, r];
                    ma.prevArea.Clear();
                    ma.pathDistance = -1;
                    if (ma != null && ma.shape != null && ma.shape.id == startShapeId)
                    {
                        list.Add(new PathFinderRecord(c, r));
                        ma.pathDistance = 0;
                    }
                }
            }
        }

        public void AddMatrixPath(List<PathFinderRecord> list, MatrixArea nexta)
        {
            PathFinderRecord newrec = new PathFinderRecord();
            newrec.column = nexta.column;
            newrec.row = nexta.row;
            bool pfrfound = false;
            foreach (PathFinderRecord pfr in list)
            {
                if (pfr.column == newrec.column && pfr.row == newrec.row)
                {
                    pfrfound = true;
                }
            }
            if (!pfrfound)
                list.Add(newrec);
        }

        public DiagramPath FindPath(int startId, int endId)
        {
            List<PathFinderRecord> list = new List<PathFinderRecord>();
            float targetDistance = 100000;
            int[,] steps = new int[4, 4] {
                    {-1, 0, 4, 2},
                    {0, -1, 1, 3},
                    {0,  1, 3, 1},
                    { 1, 0, 2, 4}
                };


            StartMatrixPaths(list, startId);

            while (list.Count > 0)
            {
                float newDistance = 0;
                PathFinderRecord rec = list[0];
                list.RemoveAt(0);

                //Debugger.Log(0, "", "## Processing area " + rec.column + "," + rec.row + "\n");

                MatrixArea ma = aMatrix[rec.column, rec.row];
                MatrixArea nexta;

                // trying progress in 4 directions
                for (int stepi = 0; stepi < 4; stepi++)
                {
                    float width = 0;
                    int newcol = rec.column + steps[stepi, 0];
                    int newrow = rec.row + steps[stepi, 1];
                    if (newcol >= 0 && newcol < aMatrixCols && newrow >= 0 && newrow < aMatrixRows)
                    {
                        //Debugger.Log(0, "", "  ## trying " + newcol + "," + newrow + "\n");
                        //LogPathCount(1);
                        nexta = aMatrix[newcol, newrow];
                        if (nexta.ShapeId == endId && ma.ShapeId == startId)
                            continue;
                        newDistance = ma.pathDistance + 1;
                        if (steps[stepi, 2] == 2 || steps[stepi, 2] == 4)
                            width = vRanges[newrow].Width;
                        else
                            width = hRanges[newcol].Width;
                        if (width < 12) newDistance++;
                        if (width < 8) newDistance++;
                        if (width < 4) newDistance++;
                        if (newDistance > targetDistance)
                            continue;
                        if ((nexta.pathDistance < 0 || nexta.pathDistance >= newDistance) && (nexta.shape == null || (nexta.shape != null && nexta.shape.id == endId)))
                        {
                            if (nexta.pathDistance > newDistance)
                                nexta.prevArea.Clear();
                            nexta.pathDistance = newDistance;
                            nexta.prevArea.Add(ma);
                            if (nexta.shape != null && nexta.shape.id == endId)
                            {
                                if (targetDistance > newDistance)
                                    targetDistance = newDistance;
                                //Debugger.Log(0, "", "      ## target achieved " + nexta.ToString() + "\n");
                            }
                            else
                            {
                                AddMatrixPath(list, nexta);
                            }
                        }
                    }
                }
            }

            //LogDistances();

            DiagramPath dpath = new DiagramPath();

            // creating list of cells of target shape
            List<MatrixArea> targetCells = new List<MatrixArea>();
            for (int c = 0; c < aMatrixCols; c++)
            {
                for (int r = 0; r < aMatrixRows; r++)
                {
                    MatrixArea ma = aMatrix[c, r];
                    if (ma.shape != null && ma.shape.id == endId && ma.prevArea != null && ma.prevArea.Count > 0)
                    {
                        targetCells.Add(ma);
                    }
                }
            }

            // create all paths by recursion
            List<FoundPath> foundPaths = new List<FoundPath>();
            foreach (MatrixArea ma in targetCells)
            {
                ma.CreatePaths(new FoundPath(ma), foundPaths);
            }

            // choosing shortest path
            FoundPath fpa = null;
            foreach (FoundPath fpath in foundPaths)
            {
                if (fpa == null || fpa.Distance > fpath.Distance)
                {
                    fpa = fpath;
                }
            }

            if (fpa != null)
            {
                // creating points from path
                Point[] points = new Point[fpa.path.Count];
                for (int j = 0; j < fpa.path.Count; j++)
                {
                    points[j] = new Point(fpa.path[j].column, fpa.path[j].row);
                }
                dpath.areaPath = points;
            }
            else
            {
                dpath.areaPath = new Point[]{};
            }
                
            return dpath;

        }

        public void ClearSlots()
        {
            for (int c = 0; c < aMatrixCols; c++)
            {
                for (int r = 0; r < aMatrixRows; r++)
                {
                    MatrixArea ma = aMatrix[c, r];
                    ma.ClearSlots();
                }
            }
        }

        public void AllocateSlots(Point[] points, CaseDiagramConnection conn)
        {
            if (points == null || points.Length < 2)
                return;
            int idx = 0;
            int next = 0;
            Point curr = points[idx];
            Point nextp;
            bool vert = false;

            while(idx < points.Length - 1)
            {
                next = EndOfDirection(points, idx);
                curr = points[idx];
                nextp = points[next];
                if (curr.X == nextp.X)
                    vert = true;
                else 
                    vert = false;
                if (next > idx)
                {
                    for (int slot = 0; slot < 100; slot++)
                    {
                        bool available = true;
                        for (int j = idx; j <= next; j++)
                        {
                            if (vert)
                            {
                                if (!aMatrix[points[j].X, points[j].Y].IsVSlotAvailable(slot))
                                {
                                    available = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (!aMatrix[points[j].X, points[j].Y].IsHSlotAvailable(slot))
                                {
                                    available = false;
                                    break;
                                }
                            }
                        }

                        if (available)
                        {
                            for (int j = idx; j <= next; j++)
                            {
                                if (vert)
                                {
                                    aMatrix[points[j].X, points[j].Y].SetVSlot(slot, conn);
                                }
                                else
                                {
                                    aMatrix[points[j].X, points[j].Y].SetHSlot(slot, conn);
                                }
                            }
                            break;
                        }
                    }
                    idx = next;
                }
                else
                {
                    break;
                }
            }


        }

        public int EndOfDirection(Point[] points, int startIndex)
        {
            int nextIndex = startIndex + 1;
            Point curr = points[startIndex];
            if (nextIndex >= points.Length)
            {
                return startIndex;
            }
            Point next = points[nextIndex];
            if (curr.X == next.X)
            {
                while (nextIndex < points.Length)
                {
                    if (points[nextIndex].X != curr.X)
                        return nextIndex - 1;
                    nextIndex++;
                }
                return points.Length - 1;
            }
            else
            {
                while (nextIndex < points.Length)
                {
                    if (points[nextIndex].Y != curr.Y)
                        return nextIndex - 1;
                    nextIndex++;
                }
                return points.Length - 1;
            }
        }

        public void RecalculateSlotsPositions()
        {
            for (int c = 0; c < aMatrixCols; c++)
            {
                MatrixRange rangec = hRanges[c] as MatrixRange;
                for (int r = 0; r < aMatrixRows; r++)
                {
                    MatrixRange ranger = vRanges[r] as MatrixRange;
                    MatrixArea ma = aMatrix[c, r];
                    if (ma.HorizontalSlotAllocatedCount > 0)
                    {
                        ma.RecalculateHorizontalSlots(ranger.from, ranger.end);
                    }
                    else
                    {
                        ma.vCenter = ranger.Center;
                    }

                    if (ma.VerticalSlotAllocatedCount > 0)
                    {
                        ma.RecalculateVerticalSlots(rangec.from, rangec.end);
                    }
                    else
                    {
                        ma.hCenter = rangec.Center;
                    }
                }
            }

        }

        public void CalculatePoints(CaseDiagramConnection conn)
        {
            MatrixArea ma;
            Point curr;
            Point[] trace = conn.path.areaPath;
            Point[] arr = new Point[trace.Length+2];
            for (int i = 1; i < trace.Length - 1; i++)
            {
                curr = trace[i];
                ma = aMatrix[curr.X,curr.Y];
                arr[i+1] = ma.GetPointForConnection(conn);
            }

            curr = trace[0];
            ma = aMatrix[curr.X, curr.Y];
            arr[1] = ma.GetAnchorPoint(trace[1], arr[2]);
            if (ma.shape != null)
                arr[0] = ma.shape.Bounds.CenterPoint;

            curr = trace[trace.Length - 1];
            ma = aMatrix[curr.X, curr.Y];
            arr[trace.Length] = ma.GetAnchorPoint(trace[trace.Length - 2], arr[trace.Length-1]);
            if (ma.shape != null)
                arr[trace.Length + 1] = ma.shape.Bounds.CenterPoint;

            conn.coordinates = arr;
            conn.validCoordinates = true;
        }

        public int GetColumnForPoint(int x, int y)
        {
            for (int i = 0; i < hRanges.Count; i++)
            {
                MatrixRange range = hRanges[i] as MatrixRange;
                if (range.from <= x && range.end >= x)
                    return i;
            }
            return -1;
        }

        public int GetRowForPoint(int x, int y)
        {
            for (int i = 0; i < vRanges.Count; i++)
            {
                MatrixRange range = vRanges[i] as MatrixRange;
                if (range.from <= y && range.end >= y)
                    return i;
            }
            return -1;
        }

        public CaseShape GetShapeAtPoint(int x, int y)
        {
            int column = GetColumnForPoint(x, y);
            int row = GetRowForPoint(x, y);

            MatrixArea ma = aMatrix[column, row];
            return ma.shape;
        }

        public CaseDiagramConnection GetConnectionAtPoint(int x, int y)
        {
            int column = GetColumnForPoint(x, y);
            int row = GetRowForPoint(x, y);

            MatrixArea ma = aMatrix[column, row];
            //Debugger.Log(0, "", string.Format("    position ({2},{3}) at matrix cell: {0}, {1}, hslots:", column, row, x, y));

            int slot = 0;
            CaseDiagramConnection conn = null;

            slot = ma.GetHSlotAt(y);
            conn = ma.GetConnectionAtHSlot(slot);
            if (conn != null)
                return conn;

            slot = ma.GetVSlotAt(x);
            conn = ma.GetConnectionAtVSlot(slot);
            return conn;
        }

        public void LogRanges()
        {
            Debugger.Log(0, "", "Horizontal ranges:\n");
            foreach (MatrixRange mr in hRanges)
            {
                Debugger.Log(0, "", string.Format("Range {0} - {1}\n", mr.from, mr.end));
            }
            Debugger.Log(0, "", "Vertical ranges:\n");
            foreach (MatrixRange mr in vRanges)
            {
                Debugger.Log(0, "", string.Format("Range {0} - {1}\n", mr.from, mr.end));
            }
            Debugger.Log(0, "", "------------------------------\n");
        }

        public void LogAreas()
        {
            Debugger.Log(0,"","Areas -------------------\n");
            for (int r = 0; r < aMatrixRows; r++)
            {
                for (int c = 0; c < aMatrixCols; c++)
                {
                    if (aMatrix[c,r].shape == null)
                        Debugger.Log(0,"","-,");
                    else
                        Debugger.Log(0,"", string.Format("{0},", aMatrix[c,r].shape.id));
                }
                Debugger.Log(0, "", "\n");
            }
        }

        public void LogDistances()
        {
            Debugger.Log(0, "", "Distances -------------------\n");
            for (int r = 0; r < aMatrixRows; r++)
            {
                for (int c = 0; c < aMatrixCols; c++)
                {
                    if (aMatrix[c, r].pathDistance < 0)
                        Debugger.Log(0, "", "-,");
                    else
                        Debugger.Log(0, "", string.Format("{0},", aMatrix[c, r].pathDistance));
                }
                Debugger.Log(0, "", "\n");
            }
        }
        public void LogPathCount(int i)
        {
            Debugger.Log(0, "", string.Format("Path Count --- {0} -------------------\n", i));
            for (int r = 0; r < aMatrixRows; r++)
            {
                for (int c = 0; c < aMatrixCols; c++)
                {
                    if (aMatrix[c, r].prevArea.Count != 0)
                        Debugger.Log(0, "", "-,");
                    else
                        Debugger.Log(0, "", string.Format("{0},", aMatrix[c, r].prevArea.Count));
                }
                Debugger.Log(0, "", "\n");
            }
        }
        public void LogSlots()
        {
            Debugger.Log(0, "", "Slots -------------------\n");
            for (int r = 0; r < aMatrixRows; r++)
            {
                for (int c = 0; c < aMatrixCols; c++)
                {
                    Debugger.Log(0, "", string.Format("{0}-{1},", aMatrix[c, r].VerticalSlotCount,aMatrix[c,r].HorizontalSlotCount));
                }
                Debugger.Log(0, "", "\n");
            }
            Debugger.Log(0, "", "VertSlots -------------------\n");
            for (int r = 0; r < aMatrixRows; r++)
            {
                for (int c = 0; c < aMatrixCols; c++)
                {
                    Debugger.Log(0, "", string.Format("{0}-{1},", aMatrix[c,r].vOffset, aMatrix[c,r].vStep));
                }
                Debugger.Log(0, "", "\n");
            }
            Debugger.Log(0, "", "HorzSlots -------------------\n");
            for (int r = 0; r < aMatrixRows; r++)
            {
                for (int c = 0; c < aMatrixCols; c++)
                {
                    Debugger.Log(0, "", string.Format("{0}-{1},", aMatrix[c, r].hOffset, aMatrix[c, r].hStep));
                }
                Debugger.Log(0, "", "\n");
            }
        }
    }

}
