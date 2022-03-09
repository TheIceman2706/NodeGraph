using NodeGraph.ViewModel;
using System;
using System.Xml;

namespace NodeGraph.Model
{
    [Connector()]
    public class Connector : ModelBase
    {
        #region Properties

        public FlowChart FlowChart
        {
            get; private set;
        }

        protected ConnectorViewModel _ViewModel;
        public ConnectorViewModel ViewModel
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

        protected NodePort _StartPort;
        public NodePort StartPort
        {
            get => this._StartPort;
            set
            {
                if (value != this._StartPort)
                {
                    this._StartPort = value;
                    this.RaisePropertyChanged("StartPort");
                }
            }
        }

        protected NodePort _EndPort;
        public NodePort EndPort
        {
            get => this._EndPort;
            set
            {
                if (value != this._EndPort)
                {
                    this._EndPort = value;
                    this.RaisePropertyChanged("EndPort");
                }
            }
        }

        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateConnector() method.
        /// </summary>
        protected internal Connector()
        {

        }

        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateConnector() method.
        /// </summary>
        public Connector(Guid guid, FlowChart flowChart) : base(guid)
        {
            this.FlowChart = flowChart;
        }

        #endregion // Constructor

        #region Methods

        public bool IsConnectedPort(NodePort port)
        {
            return (this.StartPort == port) || (this.EndPort == port);
        }

        #endregion // Methods

        #region Callbacks

        public virtual void OnCreate()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnCreate()");
            }

            this.IsInitialized = true;

            this.RaisePropertyChanged("Model");
        }

        public virtual void OnPreExecute()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnPreExecute()");
            }
        }

        public virtual void OnExecute()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnExecute()");
            }
        }

        public virtual void OnPostExecute()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnPostExecute()");
            }

            if (null != this.EndPort)
            {
                Node node = this.EndPort.Owner;
                node.OnPreExecute(this);
                node.OnExecute(this);
                node.OnPostExecute(this);
            }
        }

        public virtual void OnPreDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnPreDestroy()");
            }
        }

        public virtual void OnPostDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnPostDestroy()");
            }
        }

        public virtual void OnConnect(NodePort port)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnConnect()");
            }

            if (port is NodePropertyPort && !port.IsInput)
            {
                port.PropertyChanged += this.Port_PropertyChanged;
            }
            if (this.EndPort != null && this.EndPort.AllowMultipleInput)
            {
                port.PropertyChanged -= this.Port_PropertyChanged;
            }
        }

        private void Port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (sender == this.StartPort.Owner && this.StartPort.Name == e.PropertyName)
            //{

            //    var outPort = NodeGraphManager.FindNodePropertyPort(this.StartPort.Owner, e.PropertyName);
            //    NodeGraphManager.FindConnectedPorts(outPort, out var inPorts);
            //    foreach (var inPort in inPorts)
            //    {
            //        if (inPort is NodePropertyPort propertyPort)
            //            propertyPort.Value = outPort.Value;
            //    }
            //}

            //if (this.StartPort is NodePropertyPort startPropertyPort && this.EndPort is NodePropertyPort endPropertyPort)
            //{
            //    endPropertyPort.Value = startPropertyPort.Value;
            //}
        }

        public virtual void OnDisconnect(NodePort port)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnDisconnect()");
            }

            if (port is NodePropertyPort && !port.IsInput)
            {
                port.PropertyChanged -= this.Port_PropertyChanged;
            }
        }

        public virtual void OnDeserialize()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("Connector.OnDeserialize()");
            }

            NodeGraphManager.ConnectTo(this.StartPort, this);
            NodeGraphManager.ConnectTo(this.EndPort, this);
            this.IsInitialized = true;

            this.RaisePropertyChanged("Model");
        }

        #endregion // Callbacks

        #region Overrides IXmlSerializable

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            //{ Begin Creation info : You need not deserialize this block in ReadXml().
            // These are automatically serialized in Node.ReadXml().
            writer.WriteAttributeString("ViewModelType", this.ViewModel.GetType().AssemblyQualifiedName);
            writer.WriteAttributeString("Owner", this.FlowChart.Guid.ToString());
            //} End Creation Info.

            if (null != this.StartPort)
            {
                writer.WriteAttributeString("StartPort", this.StartPort.Guid.ToString());
            }

            if (null != this.EndPort)
            {
                writer.WriteAttributeString("EndPort", this.EndPort.Guid.ToString());
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            this.StartPort = NodeGraphManager.FindNodePort(Guid.Parse(reader.GetAttribute("StartPort")));
            if (null == this.StartPort)
            {
                throw new InvalidOperationException("StartPort can not be null in Connector.ReadXml().");
            }

            NodeGraphManager.ConnectTo(this.StartPort, this);
            this.EndPort = NodeGraphManager.FindNodePort(Guid.Parse(reader.GetAttribute("EndPort")));
            if (null == this.EndPort)
            {
                throw new InvalidOperationException("EndPort can not be null in Connector.ReadXml().");
            }

            NodeGraphManager.ConnectTo(this.EndPort, this);
        }

        #endregion // Overrides IXmlSerializable
    }
}
