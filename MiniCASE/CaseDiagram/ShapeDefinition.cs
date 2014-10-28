using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniCASE
{
    public class ShapeDefinition
    {
        private string name = string.Empty;
        private string description = string.Empty;
        public List<ShapeDrawCommand> commands = new List<ShapeDrawCommand>();


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public ShapeDrawCommand AddCommand(int aCommand)
        {
            ShapeDrawCommand cmd = new ShapeDrawCommand();
            cmd.command = aCommand;
            commands.Add(cmd);
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, ShapeDrawCommandCondition cond)
        {
            ShapeDrawCommand cmd = new ShapeDrawCommand();
            cmd.command = aCommand;
            cmd.conditions = cond;
            commands.Add(cmd);
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand);
            cmd.A = a;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, string a)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand);
            cmd.text = a;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, int b)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand, a);
            cmd.B = b;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, int b, string value)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand);
            cmd.A = a;
            cmd.B = b;
            cmd.text = value;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, int b, int c, int d)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand, a, b);
            cmd.C = c;
            cmd.D = d;

            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, int b, int c, int d, string text)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand, a, b, c, d);
            cmd.text = text;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, ShapeDrawCommandCondition cond)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand);
            cmd.A = a;
            cmd.conditions = cond;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, string a, ShapeDrawCommandCondition cond)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand);
            cmd.text = a;
            cmd.conditions = cond;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, int b, ShapeDrawCommandCondition cond)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand, a);
            cmd.B = b;
            cmd.conditions = cond;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, int b, string value, ShapeDrawCommandCondition cond)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand);
            cmd.A = a;
            cmd.B = b;
            cmd.text = value;
            cmd.conditions = cond;
            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, int b, int c, int d, ShapeDrawCommandCondition cond)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand, a, b);
            cmd.C = c;
            cmd.D = d;
            cmd.conditions = cond;

            return cmd;
        }

        public ShapeDrawCommand AddCommand(int aCommand, int a, int b, int c, int d, string text, ShapeDrawCommandCondition cond)
        {
            ShapeDrawCommand cmd = AddCommand(aCommand, a, b, c, d);
            cmd.text = text;
            cmd.conditions = cond;
            return cmd;
        }

    }

    public enum ShapeDrawCommandCondition
    {
        Selected = 0x1,
        NotSelected = 0x10,
        Highlighted = 0x100,
        NotHighlighted = 0x1000,
        Allways = 0x11111111
    }

    public class ShapeDrawCommand
    {
        public const int Fill = 1;
        public const int Rectangle = 2;
        public const int SetPenColor = 3;
        public const int SetPenWidth = 4;
        public const int SetFillColor = 5;

        public int command = 0;
        public int A = 0;
        public int B = 0;
        public int C = 0;
        public int D = 0;
        public ShapeDrawCommandCondition conditions = ShapeDrawCommandCondition.Allways;
        public string text = string.Empty;
    }
}
