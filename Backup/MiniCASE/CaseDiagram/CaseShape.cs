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
    /// contains and handles data dedicated to one shape in diagram
    /// </summary>
    public class CaseShape
    {
        private RectangleD bounds;
        private RectangleD boundsBack;
        public int id = 0;
        public string ShapeType = "MiniCase.Shape.Process";
        public ArrayList matrixAreas = new ArrayList();
        private bool selected = false;
        private bool highlighted = false;

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }

        public bool Highlighted
        {
            get
            {
                return highlighted;
            }
            set
            {
                highlighted = value;
            }
        }

        public RectangleD Bounds
        {
            get
            {
                if (bounds == null)
                    bounds = new RectangleD();
                return bounds;
            }
            set
            {
                bounds = value;
            }
        }

        public void SavePosition()
        {
            boundsBack = new RectangleD();
            boundsBack.Rectangle = bounds.Rectangle;
        }

        public void SetOffset(Size offset)
        {
            if (boundsBack == null)
                return;

            //Debugger.Log(0, "", string.Format("Moving {0} to {1}", boundsBack.Xa, boundsBack.Xa + offset.Width));
            bounds.Xa = boundsBack.Xa + offset.Width;
            bounds.Xb = boundsBack.Xb + offset.Width;
            bounds.Ya = boundsBack.Ya + offset.Height;
            bounds.Yb = boundsBack.Yb + offset.Height;
        }

        public void ConfirmPosition()
        {
            boundsBack = null;
        }
    }
}
