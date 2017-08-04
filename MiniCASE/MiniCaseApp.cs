using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MiniCASE
{
    public class MiniCaseApp
    {
        private static CDProject _project = null;
        public static CDProject Project
        {
            get
            {
                return _project;
            }
            set
            {
                if (_project != null)
                {
                    MainWindow.SaveProject();
                }
                _project = value;
            }
        }

        public static MainForm MainWindow { get; set; }

        public static CDShape SelectedShape = null;

        public static CDLibrary Library = null;

        public static CDConnection SelectedConnection = null;

        private static List<CDProjectBase> p_recentProjects = new List<CDProjectBase>();

        public static CDProjectBase[] RecentProjects
        {
            get
            {
                return p_recentProjects.ToArray<CDProjectBase>();
            }
        }

        public static void RemoveRecent(CDProjectBase rp)
        {

        }

        public static void PutRecentAtFirstPosition(CDProjectBase rp)
        {

        }

        public static string ConfigDir
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static void OnAppStarting()
        {
            string s = ConfigDir;
            string conf = Path.Combine(s, "library.xml");
            Library = new CDLibrary();
            if (File.Exists(conf))
            {
                Library.Load(conf);
            }
            else
            {
                Library.Initialize();
            }
        }

        public static void OnAppStopping()
        {
            string s = ConfigDir;
            string conf = Path.Combine(s, "library.xml");
            if (!File.Exists(conf))
            {
                Library.Save(conf);
            }
        }
    }

    public class MCA
    {
        public string Command;
        public object[] Args;
        private static object _def = null;

        public MCA()
        {
            Command = string.Empty;
            Args = null;
        }

        public MCA(string cmd)
        {
            Command = cmd;
            Args = null;
        }

        public MCA(string cmd, params object[] a)
        {
            Command = cmd;
            Args = a;
        }

        public object this[int index]
        {
            get
            {
                if (Args != null && Args.Length > index)
                {
                    return Args[index];
                }
                return _def;
            }
        }
    }
}
