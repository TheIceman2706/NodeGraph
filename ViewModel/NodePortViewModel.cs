using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.ComponentModel;

namespace NodeGraph.ViewModel
{
    public class NodePortViewModel : ViewModelBase
    {
        #region Fields

        public NodePortView View;

        #endregion // Fields

        #region Properties

        private NodePort _Model;
        public NodePort Model
        {
            get => this._Model;
            set
            {
                if (value != this._Model)
                {
                    this._Model = value;
                    this.RaisePropertyChanged("Model");
                }
            }
        }

        #endregion // Properties

        #region Constructor

        public NodePortViewModel(NodePort nodePort) : base(nodePort)
        {
            this.Model = nodePort ?? throw new ArgumentNullException("NodePort can not be null in NodePortViewModel constructor.");
            this.Model.Connectors.CollectionChanged += this._ConnectorViewModels_CollectionChanged;
        }

        #endregion // Constructor

        #region Collection Events

        private void _ConnectorViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Node node = this.Model.Owner;

            if (null != e.OldItems)
            {
                foreach (object item in e.OldItems)
                {
                    node.ViewModel.RaisePropertyChanged("Connectors");
                }
            }

            if (null != e.NewItems)
            {
                foreach (object item in e.NewItems)
                {
                    Connector addedConnector = item as Connector;
                    node.ViewModel.RaisePropertyChanged("Connectors");
                }
            }

            if (null != this.View)
            {
                this.RaisePropertyChanged("Connectors");
            }
        }

        #endregion // Collection Events

        #region Events

        protected override void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ModelPropertyChanged(sender, e);

            this.RaisePropertyChanged(e.PropertyName);
        }

        #endregion // Events
    }
}
