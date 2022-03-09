using NodeGraph.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace NodeGraph.Model
{
    public class NodePort : ModelBase
    {
        #region Fields

        public readonly bool IsInput;

        public readonly Node Owner;

        #endregion // Fields

        #region Properties

        public NodePortViewModel ViewModel
        {
            get; set;
        }

        private string _Name;
        public string Name
        {
            get => this._Name;
            set
            {
                if (value != this._Name)
                {
                    this._Name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        private bool _AllowMultipleInput;
        public bool AllowMultipleInput
        {
            get => this._AllowMultipleInput;
            set
            {
                if (value != this._AllowMultipleInput)
                {
                    this._AllowMultipleInput = value;
                    this.RaisePropertyChanged("AllowMultipleInput");
                }
            }
        }

        private bool _AllowMultipleOutput;
        public bool AllowMultipleOutput
        {
            get => this._AllowMultipleOutput;
            set
            {
                if (value != this._AllowMultipleOutput)
                {
                    this._AllowMultipleOutput = value;
                    this.RaisePropertyChanged("AllowMultipleOutput");
                }
            }
        }

        private string _DisplayName;
        public string DisplayName
        {
            get => this._DisplayName;
            set
            {
                if (value != this._DisplayName)
                {
                    this._DisplayName = value;
                    this.RaisePropertyChanged("DisplayName");
                }
            }
        }

        private ObservableCollection<Connector> _Connectors = new ObservableCollection<Connector>();
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

        private bool _IsPortEnabled = true;
        public bool IsPortEnabled
        {
            get => this._IsPortEnabled;
            set
            {
                if (value != this._IsPortEnabled)
                {
                    this._IsPortEnabled = value;
                    this.RaisePropertyChanged("IsPortEnabled");
                }
            }
        }

        private bool _IsEnabled = true;
        public bool IsEnabled
        {
            get => this._IsEnabled;
            set
            {
                if (value != this._IsEnabled)
                {
                    this._IsEnabled = value;
                    this.RaisePropertyChanged("IsEnabled");
                }
            }
        }

        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateNodeFlowPort() or GraphManager.CreateNodePropertyPort() method.
        /// </summary>
        protected NodePort(Guid guid, Node node, bool isInput) : base(guid)
        {
            this.Owner = node;
            this.IsInput = isInput;
        }

        #endregion // Constructor

        public event EventHandler<Connector> Connected, Disconnected;

        #region Destructor

        ~NodePort()
        {
        }

        #endregion // Destructor

        #region Methods

        public virtual bool IsConnectable(NodePort otherPort, out string error)
        {
            error = "";
            return true;
        }

        #endregion // Methods

        #region Callbacks

        public virtual void OnCreate()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("NodePort.OnCreate()");
            }

            this.IsInitialized = true;

            this.RaisePropertyChanged("Model");
        }

        public virtual void OnPreDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("NodePort.OnPreDestroy()");
            }
        }

        public virtual void OnPostDestroy()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("NodePort.OnPostDestroy()");
            }
        }

        public virtual void OnConnect(Connector connector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("NodePort.OnConnect()");
            }

            Connected?.Invoke(this, connector);
        }

        public virtual void OnDisconnect(Connector connector)
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("NodePort.OnDisconnect()");
            }

            Disconnected?.Invoke(this, connector);
        }

        public virtual void OnDeserialize()
        {
            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine("NodePort.OnDeserialize()");
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
            // These are automatically serialized in Node.ReadXml().
            writer.WriteAttributeString("ViewModelType", this.ViewModel.GetType().AssemblyQualifiedName);
            writer.WriteAttributeString("Owner", this.Owner.Guid.ToString());
            writer.WriteAttributeString("IsInput", this.IsInput.ToString());
            //} End Creation Info.

            writer.WriteAttributeString("Name", this.Name);
            writer.WriteAttributeString("DisplayName", this.DisplayName);
            writer.WriteAttributeString("AllowMultipleInput", this.AllowMultipleInput.ToString());
            writer.WriteAttributeString("AllowMultipleOutput", this.AllowMultipleOutput.ToString());
            writer.WriteAttributeString("IsPortEnabled", this.IsPortEnabled.ToString());
            writer.WriteAttributeString("IsEnabled", this.IsEnabled.ToString());
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            this.Name = reader.GetAttribute("Name");
            this.DisplayName = reader.GetAttribute("DisplayName");
            this.AllowMultipleInput = bool.Parse(reader.GetAttribute("AllowMultipleInput"));
            this.AllowMultipleOutput = bool.Parse(reader.GetAttribute("AllowMultipleOutput"));
            this.IsPortEnabled = bool.Parse(reader.GetAttribute("IsPortEnabled"));
            this.IsEnabled = bool.Parse(reader.GetAttribute("IsEnabled"));
        }

        #endregion // Overrides IXmlSerializable
    }
}
