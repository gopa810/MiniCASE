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

        //
        // new fields
        //
        public int freeSpaceAround = 64;
        public RectangleD rectangleOfMatrix;
        public double[,] mainField = null;
        public double[,] subField = null;
        public bool[,] visitedField = null;
        public int fieldWidth = 0;
        public int fieldHeight = 0;
        public int gridStep = 4;

        public void InitStart(Rectangle rc)
        {
            // calculates bigger rectangle aligned to 4points
            RectangleD rd = new RectangleD();
            rd.Xa = (rc.Left / gridStep) * gridStep;
            rd.Xb = (rc.Right / gridStep) * gridStep + gridStep;
            rd.Ya = (rc.Top / gridStep) * gridStep;
            rd.Yb = (rc.Bottom / gridStep) * gridStep + gridStep;

            // calculates bigger with some space around
            rd.Xa -= freeSpaceAround;
            rd.Xb += freeSpaceAround;
            rd.Ya -= freeSpaceAround;
            rd.Yb += freeSpaceAround;

            // calculates conversion constants
            // logical matrix should start at indexes 0,0
            rectangleOfMatrix = rd;

            int mw = rd.Width / gridStep;
            int mh = rd.Height / gridStep;

            if (mw > fieldWidth || mh > fieldHeight)
            {
                mainField = null;
                subField = null;
                fieldWidth = mw;
                fieldHeight = mh;
            }


            // initializes 2 big arrays
            mainField = new double[fieldWidth, fieldHeight];
            subField = new double[fieldWidth, fieldHeight];
            visitedField = new bool[fieldWidth, fieldHeight];

            for (int i = 0; i < fieldWidth; i++)
            {
                for (int j = 0; j < fieldHeight; j++)
                {
                    mainField[i, j] = 0.0;
                    subField[i, j] = 0.0;
                    visitedField[i, j] = false;
                }
            }


            hRanges = new List<MatrixRange>();
            vRanges = new List<MatrixRange>();
        }

        /// <summary>
        /// Modifies main field array with new object
        /// For main field:
        /// Adds new potentials to all nodes according the position of object
        /// For sub field:
        /// Sets new potentials to the nodes.
        /// </summary>
        /// <param name="pt"></param>
        public void InsertPeak(Point pt, bool bMain)
        {
            Point sp = ConvertCoordinatesViewToMatrix(pt);
            double baseValue = mainField[sp.X,sp.Y];

            for (int i = 0; i < fieldWidth; i++)
            {
                for (int j = 0; j < fieldHeight; j++)
                {
                    double d = getDistance(i, j, sp.X, sp.Y);
                    double pw = Math.Pow(Math.E, -d/50);

                    if (bMain)
                        mainField[i, j] += pw;
                    else
                        subField[i, j] = -2 * pw * baseValue;
                }
            }

            int a = 0;
        }

        public double getDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public Point ConvertCoordinatesViewToMatrix(Point pt)
        {
            Point np = new Point();
            np.X = Convert.ToInt32(Math.Round((pt.X - rectangleOfMatrix.Xa) / Convert.ToDouble(gridStep)));
            np.Y = Convert.ToInt32(Math.Round((pt.Y - rectangleOfMatrix.Ya) / Convert.ToDouble(gridStep)));
            return np;
        }

        public void ClearVisitedField()
        {
            for (int i = 0; i < fieldWidth; i++)
            {
                for (int j = 0; j < fieldHeight; j++)
                {
                    visitedField[i, j] = false;
                }
            }
        }

        static int[] checkOffsX = { -1, 0, 1, 1, 1, 0, -1, -1 };
        static int[] checkOffsY = { -1, -1, -1, 0, 1, 1, 1, 0 };

        public bool IsValidMatrixPoint(Point p)
        {
            if (p.X < 0 || p.X >= fieldWidth)
                return false;
            if (p.Y < 0 || p.Y >= fieldHeight)
                return false;
            return true;
        }

        public bool MoveToNextPoint(ref Point current, ref Point target)
        {
            int foundIndex = -1;
            Point tries = new Point();
            double foundMinimum = 1000.0;
            double value = 0.0;

            for (int i = 0; i < 8; i++)
            {
                tries.X = current.X + checkOffsX[i];
                tries.Y = current.Y + checkOffsY[i];
                if (!IsValidMatrixPoint(tries))
                    continue;
                value = mainField[tries.X, tries.Y] + subField[tries.X, tries.Y];
                if (foundMinimum > value)
                {
                    foundIndex = i;
                    foundMinimum = value;
                }
            }

            // if nothing was found or that one found was already visited, then find direct
            // way to the target
            if (foundIndex < 0 || visitedField[current.X + checkOffsX[foundIndex], current.Y + checkOffsY[foundIndex]] == true)
            {
                double minDist = 1000.0;

                for (int i = 0; i < 8; i++)
                {
                    tries.X = current.X + checkOffsX[i];
                    tries.Y = current.Y + checkOffsY[i];
                    if (!IsValidMatrixPoint(tries))
                        continue;
                    double d = getDistance(tries.X, tries.Y, target.X, target.Y);
                    if (d < minDist)
                    {
                        foundIndex = i;
                        minDist = 0;
                    }
                }
            }

            current.X += checkOffsX[foundIndex];
            current.Y += checkOffsY[foundIndex];
            visitedField[current.X, current.Y] = true;

            return ((current.X == target.X) && (current.Y == target.Y));
        }

        public DiagramPath FindPath(CaseShape ss, CaseShape es)
        {
            Point sp = ConvertCoordinatesViewToMatrix(ss.Bounds.CenterPoint);
            Point ep = ConvertCoordinatesViewToMatrix(es.Bounds.CenterPoint);

            // starts on sp and tries to reach ep
            // in every step
            // - gets all surrounding gradients and select the lowest gradient
            // - if that field was already visited, then tries to select that field around
            //   current, which is nearest to target field
            Point current = new Point(sp.X, sp.Y);
            List<Point> track = new List<Point>();
            bool finished = false;

            track.Add(sp);
            finished = MoveToNextPoint(ref current, ref ep);
            while (!finished)
            {
                track.Add(current);
                finished = MoveToNextPoint(ref current, ref ep);
            }
            track.Add(ep);


            DiagramPath dpath = new DiagramPath();
            dpath.areaPath = track.ToArray<Point>();

                
            return dpath;

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

        public int GetColumnForPoint(int x, int y)
        {
            return 0;
        }

        public int GetRowForPoint(int x, int y)
        {
            return 0;

        }

        public CaseShape GetShapeAtPoint(int x, int y)
        {
            return null;
        }

        public CaseDiagramConnection GetConnectionAtPoint(int x, int y)
        {
            return null;
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
