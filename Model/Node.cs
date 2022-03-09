using NodeGraph.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Media;
using System.Xml;

namespace NodeGraph.Model
{
    [Node()]
    public class Node : ModelBase
    {
        #region Fields

        public readonly FlowChart Owner;

        #endregion // Fields

        #region Properties

        protected NodeViewModel _ViewModel;
        public NodeViewModel ViewModel
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

        protected string _Header;
        public string Header
        {
            get => this._Header;
            set
            {
                if (value != this._Header)
                {
                    this.AddNodePropertyCommand("Header", this._Header, value);
                    this._Header = value;
                    this.RaisePropertyChanged("Header");
                }
            }
        }

        protected SolidColorBrush _HeaderBackgroundColor = Brushes.Black;
        public SolidColorBrush HeaderBackgroundColor
        {
            get => this._HeaderBackgroundColor;
            set
            {
                if (value != this._HeaderBackgroundColor)
                {
                    this.AddNodePropertyCommand("HeaderBackgroundColor", this._HeaderBackgroundColor, value);
                    this._HeaderBackgroundColor = value;
                    this.RaisePropertyChanged("HeaderBackgroundColor");
                }
            }
        }

        protected SolidColorBrush _HeaderFontColor = Brushes.White;
        public SolidColorBrush HeaderFontColor
        {
            get => this._HeaderFontColor;
            set
            {
                if (value != this._HeaderFontColor)
                {
                    this.AddNodePropertyCommand("HeaderFontColor", this._HeaderFontColor, value);
                    this._HeaderFontColor = value;
                    this.RaisePropertyChanged("HeaderFontColor");
                }
            }
        }

        private bool _AllowEditingHeader = true;
        public bool AllowEditingHeader
        {
            get => this._AllowEditingHeader;
            set
            {
                if (value != this._AllowEditingHeader)
                {
                    this.AddNodePropertyCommand("AllowEditingHeader", this._AllowEditingHeader, value);
                    this._AllowEditingHeader = value;
                    this.RaisePropertyChanged("AllowEditingHeader");
                }
            }
        }

        private bool _AllowCircularConnection = false;
        public bool AllowCircularConnection
        {
            get => this._AllowCircularConnection;
            set
            {
                if (value != this._AllowCircularConnection)
                {
                    this.AddNodePropertyCommand("AllowCircularConnection", this._AllowCircularConnection, value);
                    this._AllowCircularConnection = value;
                    this.RaisePropertyChanged("AllowCircularConnection");
                }
            }
        }

        protected double _X = 0.0;
        public double X
        {
            get => this._X;
            set
            {
                if (value != this._X)
                {
                    this._X = value;
                    this.RaisePropertyChanged("X");
                }
            }
        }

        protected double _Y = 0.0;
        public double Y
        {
            get => this._Y;
            set
            {
                if (value != this._Y)
                {
                    this._Y = value;
                    this.RaisePropertyChanged("Y");
                }
            }
        }

        protected int _ZIndex = 1;
        public int ZIndex
        {
            get => this._ZIndex;
            set
            {
                if (value != this._ZIndex)
                {
                    this._ZIndex = value;
                    this.RaisePropertyChanged("ZIndex");
                }
            }
        }

        protected ObservableCollection<NodeFlowPort> _InputFlowPorts = new ObservableCollection<NodeFlowPort>();
        public ObservableCollection<NodeFlowPort> InputFlowPorts
        {
            get => this._InputFlowPorts;
            set
            {
                if (value != this._InputFlowPorts)
                {
                    this._InputFlowPorts = value;
                    this.RaisePropertyChanged("InputFlowPorts");
                }
            }
        }

        protected ObservableCollection<NodeFlowPort> _OutputFlowPorts = new ObservableCollection<NodeFlowPort>();
        public ObservableCollection<NodeFlowPort> OutputFlowPorts
        {
            get => this._OutputFlowPorts;
            set
            {
                if (value != this._OutputFlowPorts)
                {
                    this._OutputFlowPorts = value;
                    this.RaisePropertyChanged("OutputFlowPorts");
                }
            }
        }

        protected ObservableCollection<NodePropertyPort> _InputPropertyPorts = new ObservableCollection<NodePropertyPort>();
        public ObservableCollection<NodePropertyPort> InputPropertyPorts
        {
            get => this._InputPropertyPorts;
            set
            {
                if (value != this._InputPropertyPorts)
                {
                    this._InputPropertyPorts = value;
                    this.RaisePropertyChanged("InputPropertyPorts");
                }
            }
        }

        protected ObservableCollection<NodePropertyPort> _OutputPropertyPorts = new ObservableCollection<NodePropertyPort>();
        public ObservableCollection<NodePropertyPort> OutputPropertyPorts
        {
            get => this._OutputPropertyPorts;
            set
            {
                if (value != this._OutputPropertyPorts)
                {
                    this._OutputPropertyPorts = value;
                    this.RaisePropertyChanged("OutputPropertyPorts");
                }
            }
        }

        private NodeExecutionState _ExecutionState;
        public NodeExecutionState ExecutionState
        {
            get => this._ExecutionState;
            set
            {
                if (value != this._ExecutionState)
                {
                    this._ExecutionState = value;
                    this.RaisePropertyChanged("ExecutionState");
                }
            }
        }


        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateNode() method.
        /// </summary>
        public Node(Guid guid, FlowChart flowChart) : base(guid)
        {
            this.Owner = flowChart;
        }

        #endregion // Constructor

        #region Destructor

        ~Node()
        {

        }

        #endregion // Destructor

        #region Callbacks

        public virtual void OnCreate()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Node.OnPreExecute()");
            }

            this.IsInitialized = true;

            this.RaisePropertyChanged("Model");
        }

        public virtual void OnPreExecute(Connector prevConnector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Node.OnPreExecute()");
            }

            this.ExecutionState = NodeExecutionState.Executing;
        }

        public virtual void OnExecute(Connector prevConnector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Node.OnExecute()");
            }
        }

        public virtual void OnPostExecute(Connector prevConnector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Node.OnPostExecute()");
            }

            this.ExecutionState = NodeExecutionState.Executed;
        }

        public virtual void OnPreDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Node.OnPreDestroy()");
            }
        }

        public virtual void OnPostDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Node.OnPostDestroy()");
            }
        }

        public virtual void OnDeserialize()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Node.OnDeserialize()");
            }

            foreach (NodeFlowPort port in this.InputFlowPorts)
            {
                port.OnDeserialize();
            }

            foreach (NodeFlowPort port in this.OutputFlowPorts)
            {
                port.OnDeserialize();
            }

            foreach (NodePropertyPort port in this.InputPropertyPorts)
            {
                port.OnDeserialize();
            }

            foreach (NodePropertyPort port in this.OutputPropertyPorts)
            {
                port.OnDeserialize();
            }

            this.IsInitialized = true;

            this.RaisePropertyChanged("Model");
        }

        #endregion // Callbacks

        #region Overrides IXmlSerializable

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            //{ Begin Creation info : You need not deserialize this block in ReadXml().
            // These are automatically serialized in FlowChart.ReadXml().
            writer.WriteAttributeString("ViewModelType", this.ViewModel.GetType().FullName);
            writer.WriteAttributeString("Owner", this.Owner.Guid.ToString());
            //} End creation info.

            writer.WriteAttributeString("Header", this.Header);
            writer.WriteAttributeString("HeaderBackgroundColor", this.HeaderBackgroundColor.ToString());
            writer.WriteAttributeString("HeaderFontColor", this.HeaderFontColor.ToString());
            writer.WriteAttributeString("AllowEditingHeader", this.AllowEditingHeader.ToString());

            writer.WriteAttributeString("AllowCircularConnection", this.AllowCircularConnection.ToString());

            writer.WriteAttributeString("X", this.X.ToString());
            writer.WriteAttributeString("Y", this.Y.ToString());
            writer.WriteAttributeString("ZIndex", this.ZIndex.ToString());

            writer.WriteStartElement("InputFlowPorts");
            foreach (NodeFlowPort port in this.InputFlowPorts)
            {
                writer.WriteStartElement("FlowPort");
                port.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("OutputFlowPorts");
            foreach (NodeFlowPort port in this.OutputFlowPorts)
            {
                writer.WriteStartElement("FlowPort");
                port.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("InputPropertyPorts");
            foreach (NodePropertyPort port in this.InputPropertyPorts)
            {
                writer.WriteStartElement("PropertyPort");
                port.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("OutputPropertyPorts");
            foreach (NodePropertyPort port in this.OutputPropertyPorts)
            {
                writer.WriteStartElement("PropertyPort");
                port.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            this.Header = reader.GetAttribute("Header");
            this.HeaderBackgroundColor = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(reader.GetAttribute("HeaderBackgroundColor")));
            this.HeaderFontColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                reader.GetAttribute("HeaderFontColor")));
            this.AllowEditingHeader = bool.Parse(reader.GetAttribute("AllowEditingHeader"));

            this.AllowCircularConnection = bool.Parse(reader.GetAttribute("AllowCircularConnection"));

            this.X = double.Parse(reader.GetAttribute("X"));
            this.Y = double.Parse(reader.GetAttribute("Y"));
            this.ZIndex = int.Parse(reader.GetAttribute("ZIndex"));

            bool isInputFlowPortsEnd = false;
            bool isOutputFlowPortsEnd = false;
            bool isInputPropertyPortsEnd = false;
            bool isOutputPropertyPortsEnd = false;
            while (reader.Read())
            {
                if (XmlNodeType.Element == reader.NodeType)
                {
                    if (("PropertyPort" == reader.Name) ||
                        ("FlowPort" == reader.Name))
                    {
                        string prevReaderName = reader.Name;

                        Guid guid = Guid.Parse(reader.GetAttribute("Guid"));
                        Type type = Type.GetType(reader.GetAttribute("Type"));
                        Type vmType = Type.GetType(reader.GetAttribute("ViewModelType"));
                        bool isInput = bool.Parse(reader.GetAttribute("IsInput"));

                        string ownerGuidString = reader.GetAttribute("Owner");
                        Node node = NodeGraphManager.FindNode(Guid.Parse(ownerGuidString));

                        if ("PropertyPort" == prevReaderName)
                        {
                            string name = reader.GetAttribute("Name");
                            Type valueType = Type.GetType(reader.GetAttribute("ValueType"));
                            bool hasEditor = bool.Parse(reader.GetAttribute("HasEditor"));

                            NodePropertyPort port = NodeGraphManager.CreateNodePropertyPort(
                                true, guid, node, isInput, valueType, null, name, hasEditor, vmType);
                            port.ReadXml(reader);
                        }
                        else
                        {
                            NodeFlowPort port = NodeGraphManager.CreateNodeFlowPort(
                                true, guid, node, isInput, vmType);
                            port.ReadXml(reader);
                        }
                    }

                }

                if (reader.IsEmptyElement || (XmlNodeType.EndElement == reader.NodeType))
                {
                    if ("InputFlowPorts" == reader.Name)
                    {
                        isInputFlowPortsEnd = true;
                    }
                    else if ("OutputFlowPorts" == reader.Name)
                    {
                        isOutputFlowPortsEnd = true;
                    }
                    else if ("InputPropertyPorts" == reader.Name)
                    {
                        isInputPropertyPortsEnd = true;
                    }
                    else if ("OutputPropertyPorts" == reader.Name)
                    {
                        isOutputPropertyPortsEnd = true;
                    }
                    else if ("Node" == reader.Name)
                    {
                        break;
                    }
                }

                if (isInputFlowPortsEnd && isOutputFlowPortsEnd &&
                    isInputPropertyPortsEnd && isOutputPropertyPortsEnd)
                {
                    break;
                }
            }
        }

        #endregion // Overrides IXmlSerializable

        #region History

        protected virtual void AddNodePropertyCommand(string propertyName, object prevValue, object newValue)
        {
            if (!this.IsInitialized)
            {
                return;
            }

            this.Owner.History.BeginTransaction("Setting Property");

            this.Owner.History.AddCommand(new History.NodePropertyCommand(
                propertyName, this.Guid, propertyName, prevValue, newValue));

            this.Owner.History.EndTransaction(false);
        }

        #endregion // History

        #region PropertyChanged

        public override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);

            NodePropertyPort port = NodeGraphManager.FindNodePropertyPort(this, propertyName);
            if (null != port)
            {
                Type nodeType = this.GetType();

                PropertyInfo propertyInfo = nodeType.GetProperty(propertyName);
                if (null != propertyInfo)
                {
                    port.Value = propertyInfo.GetValue(this);
                }
                else
                {
                    FieldInfo fieldInfo = nodeType.GetField(propertyName);
                    if (null != fieldInfo)
                    {
                        port.Value = fieldInfo.GetValue(this);
                    }
                }
            }
        }

        #endregion // PropertyChanged
    }

    public enum NodeExecutionState
    {
        None,
        Executing,
        Executed,
        Failed,
    }
}
