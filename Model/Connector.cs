﻿using NodeGraph.ViewModel;
using System;
using System.Xml;

namespace NodeGraph.Model
{
	[Connector()]
	public class Connector : ModelBase
	{
		#region Properties

		public FlowChart FlowChart { get; private set; }

		protected ConnectorViewModel _ViewModel;
		public ConnectorViewModel ViewModel
		{
			get { return _ViewModel; }
			set
			{
				if( value != _ViewModel )
				{
					_ViewModel = value;
					RaisePropertyChanged( "ViewModel" );
				}
			}
		}

		protected NodePort _StartPort;
		public NodePort StartPort
		{
			get { return _StartPort; }
			set
			{
				if( value != _StartPort )
				{
					_StartPort = value;
					RaisePropertyChanged( "StartPort" );
				}
			}
		}

		protected NodePort _EndPort;
		public NodePort EndPort
		{
			get { return _EndPort; }
			set
			{
				if( value != _EndPort )
				{
					_EndPort = value;
					RaisePropertyChanged( "EndPort" );
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
		public Connector( Guid guid, FlowChart flowChart ) : base( guid )
		{
			FlowChart = flowChart;
		}

		#endregion // Constructor

		#region Methods

		public bool IsConnectedPort( NodePort port )
		{
			return ( StartPort == port ) || ( EndPort == port );
		}

		#endregion // Methods

		#region Callbacks

		public virtual void OnCreate()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnCreate()" );
			IsInitialized = true;

			RaisePropertyChanged( "Model" );
		}

		public virtual void OnPreExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnPreExecute()" );
		}

		public virtual void OnExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnExecute()" );
		}

		public virtual void OnPostExecute()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnPostExecute()" );

			if( null != EndPort )
			{
				Node node = EndPort.Owner;
				node.OnPreExecute( this );
				node.OnExecute( this );
				node.OnPostExecute( this );
			}
		}

		public virtual void OnPreDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnPreDestroy()" );
		}

		public virtual void OnPostDestroy()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnPostDestroy()" );
		}

		public virtual void OnConnect( NodePort port )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnConnect()" );
			if (port is NodePropertyPort && !port.IsInput)
			{
                port.PropertyChanged += this.Port_PropertyChanged;
			}
			if (EndPort is NodePropertyPort endPort && StartPort is NodePropertyPort startPort)
			{
				endPort.Value = startPort.Value;
			}
		}

        private void Port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
			if(e.PropertyName == "Value")
			if (EndPort is NodePropertyPort endPort && StartPort is NodePropertyPort startPort)
			{
				//endPort.Value = startPort.Value;
			}
        }

        public virtual void OnDisconnect( NodePort port )
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnDisconnect()" );
			if (port is NodePropertyPort && !port.IsInput)
			{
				port.PropertyChanged -= this.Port_PropertyChanged;
			}
		}

		public virtual void OnDeserialize()
		{
			if( NodeGraphManager.OutputDebugInfo )
				System.Diagnostics.Debug.WriteLine( "Connector.OnDeserialize()" );

			NodeGraphManager.ConnectTo( StartPort, this );
			NodeGraphManager.ConnectTo( EndPort, this );
			IsInitialized = true;

			RaisePropertyChanged( "Model" );
		}

		#endregion // Callbacks

		#region Overrides IXmlSerializable

		public override void WriteXml( XmlWriter writer )
		{
			base.WriteXml( writer );

			//{ Begin Creation info : You need not deserialize this block in ReadXml().
			// These are automatically serialized in Node.ReadXml().
			writer.WriteAttributeString( "ViewModelType", ViewModel.GetType().AssemblyQualifiedName );
			writer.WriteAttributeString( "Owner", FlowChart.Guid.ToString() );
			//} End Creation Info.

			if( null != StartPort )
				writer.WriteAttributeString( "StartPort", StartPort.Guid.ToString() );
			if( null != EndPort )
				writer.WriteAttributeString( "EndPort", EndPort.Guid.ToString() );
		}

		public override void ReadXml( XmlReader reader )
		{
			base.ReadXml( reader );

			StartPort = NodeGraphManager.FindNodePort( Guid.Parse( reader.GetAttribute( "StartPort" ) ) );
			if( null == StartPort )
				throw new InvalidOperationException( "StartPort can not be null in Connector.ReadXml()." );
			NodeGraphManager.ConnectTo(StartPort, this );	
			EndPort = NodeGraphManager.FindNodePort( Guid.Parse( reader.GetAttribute( "EndPort" ) ) );
			if( null == EndPort )
				throw new InvalidOperationException( "EndPort can not be null in Connector.ReadXml()." );
			NodeGraphManager.ConnectTo(EndPort, this);
		}

		#endregion // Overrides IXmlSerializable
	}
}
