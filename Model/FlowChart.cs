using NodeGraph.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace NodeGraph.Model
{
    [FlowChart()]
    public class FlowChart : ModelBase
    {
        #region Properties

        protected FlowChartViewModel _ViewModel;
        public FlowChartViewModel ViewModel
        {
            get => this._ViewModel;
            set
            {
                if (value != this._ViewModel)
                {
                    this._ViewModel = value;
                    this.RaisePropertyChanged("ViewModel");
                }
            }
        }

        protected ObservableCollection<Node> _Nodes = new ObservableCollection<Node>();
        public ObservableCollection<Node> Nodes
        {
            get => this._Nodes;
            set
            {
                if (value != this._Nodes)
                {
                    this._Nodes = value;
                    this.RaisePropertyChanged("Nodes");
                }
            }
        }

        protected ObservableCollection<Connector> _Connectors = new ObservableCollection<Connector>();
        public ObservableCollection<Connector> Connectors
        {
            get => this._Connectors;
            set
            {
                if (value != this._Connectors)
                {
                    this._Connectors = value;
                    this.RaisePropertyChanged("Connectors");
                }
            }
        }

        public History.NodeGraphHistory History
        {
            get; private set;
        }

        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateFlowChart() method.
        /// </summary>
        public FlowChart(Guid guid) : base(guid)
        {
            this.History = new History.NodeGraphHistory(this, 100);
        }

        #endregion // Constructor

        #region Callbacks

        public virtual void OnCreate()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("FlowChart.OnCreate()");
            }

            this.IsInitialized = true;

            this.RaisePropertyChanged("Model");
        }

        public virtual void OnPreExecute()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("FlowChart.OnPreExecute()");
            }
        }

        public virtual void OnExecute()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("FlowChart.OnExecute()");
            }
        }

        public virtual void OnPostExecute()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("FlowChart.OnPostExecute()");
            }
        }

        public virtual void OnPreDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("FlowChart.OnPreDestroy()");
            }
        }

        public virtual void OnPostDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("FlowChart.OnPostDestroy()");
            }
        }

        public virtual void OnDeserialize()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("FlowChart.OnDeserialize()");
            }

            foreach (Node node in this.Nodes)
            {
                node.OnDeserialize();
            }

            foreach (Connector connector in this.Connectors)
            {
                connector.OnDeserialize();
            }

            this.IsInitialized = true;

            this.RaisePropertyChanged("Model");
        }

        #endregion // Callbacks

        #region Overrides IXmlSerializable

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("Flow");
            base.WriteXml(writer);

            writer.WriteStartElement("Nodes");
            foreach (Node node in this.Nodes)
            {
                writer.WriteStartElement("Node");
                node.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Connectors");
            foreach (Connector connector in this.Connectors)
            {
                writer.WriteStartElement("Connector");
                connector.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public void ReadXml(XmlReader reader, FlowChart chart = null)
        {
            base.ReadXml(reader);

            bool isNodesEnd = false;
            bool isConnectorsEnd = false;

            while (reader.Read())
            {
                if (XmlNodeType.Element == reader.NodeType)
                {
                    if (("Node" == reader.Name) ||
                        ("Connector" == reader.Name))
                    {
                        string prevReaderName = reader.Name;

                        Guid guid = Guid.Parse(reader.GetAttribute("Guid"));
                        Type type = Type.GetType(reader.GetAttribute("Type"));
                        FlowChart flowChart = chart ?? NodeGraphManager.FindFlowChart(
                            Guid.Parse(reader.GetAttribute("Owner")));

                        if ("Node" == prevReaderName)
                        {
                            Type vmType = Type.GetType(reader.GetAttribute("ViewModelType"));
                            if (NodeGraphManager.FindNode(guid) != null)
                            {
                                NodeGraphManager.DestroyNode(guid);
                            }

                            Node node = NodeGraphManager.CreateNode(true, guid, flowChart, type, 0.0, 0.0, 0, vmType);
                            node.ReadXml(reader);
                        }
                        else
                        {
                            if (NodeGraphManager.FindConnector(guid) != null)
                            {
                                NodeGraphManager.DestroyConnector(guid);
                            }

                            Connector connector = NodeGraphManager.CreateConnector(false, guid, flowChart, type);
                            connector.ReadXml(reader);
                        }
                    }
                }

                if (reader.IsEmptyElement || XmlNodeType.EndElement == reader.NodeType)
                {
                    if ("Nodes" == reader.Name)
                    {
                        isNodesEnd = true;
                    }
                    else if ("Connectors" == reader.Name)
                    {
                        isConnectorsEnd = true;
                    }
                }

                if (isNodesEnd && isConnectorsEnd)
                {
                    break;
                }
            }
        }

        #endregion // Overrides IXmlSerializable
    }
}
