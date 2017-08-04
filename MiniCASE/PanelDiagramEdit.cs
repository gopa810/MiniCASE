using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MiniCASE
{
    public partial class PanelDiagramEdit : UserControl, ICDObjectDelegate
    {
        public EVStorage storage = new EVStorage();

        public List<CDDiagram> p_history = new List<CDDiagram>();

        public CDObject p_previouslyFedObject = null;

        public PanelDiagramEdit()
        {
            InitializeComponent();
            SetStatusInfo("Ready");
        }

        public void SetStatusInfo(string p)
        {
            statusLabel1.Text = p;
        }

        public void SetDiagram(CDDiagram d)
        {
            p_history.Add(d);
            SetDiagramWithoutHistory(d);
        }

        private void SetDiagramWithoutHistory(CDDiagram d)
        {
            caseDiagramView1.Diagram = d;
            caseDiagramView1.ClearSelection();
            toolStripLabel1.Text = d.GetString("Text");

            // initialise property panel

            evContainer1.ClearPanels();
            evContainer1.EditView = caseDiagramView1;
            FeedContainer(d, d);

        }

        private void caseDiagramView1_SelectedDiagramObjectsChanged(object sender, SelectedObjectsEventArgs e)
        {
            // in case of decomposed object, we show properties of decomposing diagram
            if (e.Shape != null && e.Shape.Decomposition != null)
            {
                e.Diagram = e.Shape.Decomposition;
                e.Shape = null;
            }

            if (e.Shape != null)
            {
                MiniCaseApp.SelectedShape = e.Shape;
                MiniCaseApp.SelectedConnection = null;
                //webBrowser1.DocumentText = CHShapePropertiesBuilder.GetHtml(e.Shape);
                evContainer1.ClearPanels();
                evContainer1.EditView = caseDiagramView1;
                FeedContainer(e.Shape, e.Shape.Diagram);
            }
            else if (e.Connection != null)
            {
                MiniCaseApp.SelectedShape = null;
                MiniCaseApp.SelectedConnection = e.Connection;

                evContainer1.ClearPanels();
                evContainer1.EditView = caseDiagramView1;
                FeedContainer(e.Connection, e.Connection.Diagram);
            }
            else if (e.Diagram != null)
            {
                MiniCaseApp.SelectedShape = null;
                MiniCaseApp.SelectedConnection = null;

                evContainer1.ClearPanels();
                evContainer1.EditView = caseDiagramView1;
                FeedContainer(e.Diagram, e.Diagram);
            }

        }

        public void FeedContainer(CDObject shape, CDDiagram diagram)
        {
            int index = 0;

            if (p_previouslyFedObject != null)
            {
                p_previouslyFedObject.SetDelegate(this);
                p_previouslyFedObject = shape;
            }
            shape.SetDelegate(this);

            foreach (CSParameterDef p in shape.Definition.parameters)
            {
                if (p.ParameterType == CSParameterType.String)
                {
                    EVStringEdit se = (EVStringEdit)storage.GetSafeControl(typeof(EVStringEdit), index);
                    se.SetName(p.Name, shape.GetString(p.Name));
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }
                else if (p.ParameterType == CSParameterType.Color)
                {
                    EVColorEdit se = (EVColorEdit)storage.GetSafeControl(typeof(EVColorEdit), index);
                    se.SetName(p.Name, shape.GetColor(p.Name));
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }
                else if (p.ParameterType == CSParameterType.FontSize)
                {
                    EVComboValue se = (EVComboValue)storage.GetSafeControl(typeof(EVComboValue), index);
                    se.SetOptions(p.ParameterType, 8f, 9f, 10f, 11f, 12f, 14f, 16f, 18f, 20f, 22f, 24f, 28f, 32f, 36f);
                    se.SetName(p.Name, shape.GetFloat(p.Name));
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }
                else if (p.ParameterType == CSParameterType.LineWidth)
                {
                    EVComboValue se = (EVComboValue)storage.GetSafeControl(typeof(EVComboValue), index);
                    se.SetOptions(p.ParameterType, 1f, 1.5f, 2f, 3f, 4f);
                    se.SetName(p.Name, shape.GetFloat(p.Name));
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }
                else if (p.ParameterType == CSParameterType.TextAlign)
                {
                    EVTextAlignEdit se = (EVTextAlignEdit)storage.GetSafeControl(typeof(EVTextAlignEdit), index);
                    se.SetName(p.Name, shape.GetTextAlign(p.Name));
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }
                else if (p.ParameterType == CSParameterType.TextPadding)
                {
                    EVTextPaddingEdit se = (EVTextPaddingEdit)storage.GetSafeControl(typeof(EVTextPaddingEdit), index);
                    se.SetName(p.Name, shape.GetTextPadding(p.Name));
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }
                else if (p.ParameterType == CSParameterType.LineCap)
                {
                    CDEndingDefinition edef = (CDEndingDefinition)shape.GetObject(p.Name);
                    EVComboValue se = (EVComboValue)storage.GetSafeControl(typeof(EVComboValue), index);
                    se.SetOptions(p.ParameterType);
                    foreach(CDEndingDefinition ed in diagram.DiagramDefinition.lineends)
                    {
                        se.AddOption(ed);
                    }
                    se.SetName(p.Name, edef);
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }
                else if (p.ParameterType == CSParameterType.LinePath)
                {
                    ConnectionsMode edef = (ConnectionsMode)shape.GetObject(p.Name);
                    EVComboValue se = (EVComboValue)storage.GetSafeControl(typeof(EVComboValue), index);
                    se.SetOptions(p.ParameterType);
                    se.AddOption(ConnectionsMode.Straight);
                    se.AddOption(ConnectionsMode.Rectangular);
                    se.SetName(p.Name, edef);
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }
                else if (p.ParameterType == CSParameterType.LinePattern)
                {
                    DashStyle edef = (DashStyle)shape.GetObject(p.Name);
                    EVComboValue se = (EVComboValue)storage.GetSafeControl(typeof(EVComboValue), index);
                    se.SetOptions(p.ParameterType);
                    se.AddOption(DashStyle.Solid);
                    se.AddOption(DashStyle.Dash);
                    se.AddOption(DashStyle.Dot);
                    se.AddOption(DashStyle.DashDot);
                    se.AddOption(DashStyle.DashDotDot);
                    se.SetName(p.Name, edef);
                    se.Object = shape;
                    se.Key = p.Name;
                    se.ContainerView = evContainer1;
                    evContainer1.AddPanel(se);
                }

                index++;
            }

            evContainer1.CommitPanels();
        }

        private void scheduleInvalidateDiagram()
        {
            timerInvalidateDiagram.Stop();
            timerInvalidateDiagram.Interval = 800;
            timerInvalidateDiagram.Start();
        }

        private void timerInvalidateDiagram_Tick(object sender, EventArgs e)
        {
            caseDiagramView1.Invalidate();
            timerInvalidateDiagram.Stop();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (p_history.Count > 1)
            {
                p_history.RemoveAt(p_history.Count - 1);
                CDDiagram d = p_history[p_history.Count - 1];
                SetDiagramWithoutHistory(d);
            }
        }


        public void CDObjectValueDidChange(object obj, string key, object value)
        {
            if (obj is CDDiagram)
            {
                if (key == "BackColor" && value is Color)
                {
                    caseDiagramView1.BackColor = (Color)value;
                    caseDiagramView1.Invalidate();
                }
                else if (key == "Text" && value is string)
                {
                    toolStripLabel1.Text = (string)value;
                }
            }
            else if (obj is CDConnection)
            {
                CDConnection conn = (CDConnection)obj;
                if (key == "PathStyle")
                {
                    conn.validCoordinates = false;
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SetDiagram(MiniCaseApp.Project.RootDiagram);
        }
    }
}
