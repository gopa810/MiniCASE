using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCASE
{
    public class CSTextPadding
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public static CSTextPadding Default = new CSTextPadding();
        public static CSTextPadding Default16 = new CSTextPadding(16, 16, 16, 16);
        public static CSTextPadding Default32 = new CSTextPadding(32, 32, 32, 32);

        public void Clear()
        {
            Left = Top = Bottom = Right = 0;
        }

        public CSTextPadding()
        {
        }

        public CSTextPadding(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
        public CSTextPadding(CSTextPadding tp)
        {
            Left = tp.Left;
            Top = tp.Top;
            Right = tp.Right;
            Bottom = tp.Bottom;
        }

        public CSTextPadding(string s)
        {
            string[] p = s.Split('|');
            if (p.Length == 4)
            {
                int.TryParse(p[0], out Left);
                int.TryParse(p[1], out Top);
                int.TryParse(p[2], out Right);
                int.TryParse(p[3], out Bottom);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}|{3}", Left, Top, Right, Bottom);
        }
    }
}
