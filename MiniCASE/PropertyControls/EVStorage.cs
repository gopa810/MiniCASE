using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MiniCASE
{
    public class EVStorage
    {

        public class UserControlEntry
        {
            public Type ControlType = null;
            public UserControl Control = null;
            public int index = 0;
        }

        private List<UserControlEntry> Controls = new List<UserControlEntry>();

        static EVStorage()
        {
        }

        public UserControl GetUserControl(Type t, int index)
        {
            if (t.IsSubclassOf(typeof(UserControl)))
            {
                foreach (UserControlEntry uce in Controls)
                {
                    if (uce.ControlType == t && uce.index == index)
                    {
                        return uce.Control;
                    }
                }
            }

            return null;
        }

        public UserControl GetSafeControl(Type t, int index)
        {
            UserControl emg = GetUserControl(t, index);
            if (emg == null)
            {
                emg = (UserControl)Activator.CreateInstance(t);
                AddUserControl(emg, index);
            }
            return emg;
        }


        public void AddUserControl(UserControl uc, int index)
        {
            UserControlEntry uce = new UserControlEntry();
            uce.Control = uc;
            uce.ControlType = uc.GetType();
            uce.index = index;
        }

    }

    public class LazyControl<T> where T : UserControl, new()
    {
        private T control = null;
        public T Instance
        {
            get
            {
                if (control == null)
                    control = new T();
                return control;
            }
        }
    }
}
