using NodeGraph.Model;
using NodeGraph.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace NodeGraph.ViewModel
{
    [NodeViewModel()]
    public class NodeViewModel : ViewModelBase
    {
        #region Fields

        public NodeView View;

        public readonly Type NodeViewType;

        #endregion // Fields

        #region Properties

        private Node _Model;
        public Node Model
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

        public Visibility InputFlowPortsVisibility => (0 < this._InputFlowPortViewModels.Count) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility OutputFlowPortsVisibility => (0 < this._OutputFlowPortViewModels.Count) ? Visibility.Visible : Visibility.Collapsed;

        private ObservableCollection<NodeFlowPortViewModel> _InputFlowPortViewModels = new ObservableCollection<NodeFlowPortViewModel>();
        public ObservableCollection<NodeFlowPortViewModel> InputFlowPortViewModels
        {
            get => this._InputFlowPortViewModels;
            set
            {
                if (value != this._InputFlowPortViewModels)
                {
                    this._InputFlowPortViewModels = value;
                    this.RaisePropertyChanged("InputFlowPortViewModels");
                }
            }
        }

        private ObservableCollection<NodeFlowPortViewModel> _OutputFlowPortViewModels = new ObservableCollection<NodeFlowPortViewModel>();
        public ObservableCollection<NodeFlowPortViewModel> OutputFlowPortViewModels
        {
            get => this._OutputFlowPortViewModels;
            set
            {
                if (value != this._OutputFlowPortViewModels)
                {
                    this._OutputFlowPortViewModels = value;
                    this.RaisePropertyChanged("OutputFlowPortViewModels");
                }
            }
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get => this._IsSelected;
            set
            {
                if (value != this._IsSelected)
                {
                    this._IsSelected = value;
                    this.RaisePropertyChanged("IsSelected");
                }
            }
        }

        #endregion // Node Properties

        #region NodePropertyPorts

        public Visibility InputPropertyPortsVisibility => (0 < this._InputPropertyPortViewModels.Count) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility OutputPropertyPortsVisibility => (0 < this._OutputPropertyPortViewModels.Count) ? Visibility.Visible : Visibility.Collapsed;

        private ObservableCollection<NodePropertyPortViewModel> _InputPropertyPortViewModels = new ObservableCollection<NodePropertyPortViewModel>();
        public ObservableCollection<NodePropertyPortViewModel> InputPropertyPortViewModels
        {
            get => this._InputPropertyPortViewModels;
            set
            {
                if (value != this._InputPropertyPortViewModels)
                {
                    this._InputPropertyPortViewModels = value;
                    this.RaisePropertyChanged("InputPropertyPortViewModels");
                }
            }
        }

        private ObservableCollection<NodePropertyPortViewModel> _OutputPropertyPortViewModels = new ObservableCollection<NodePropertyPortViewModel>();
        public ObservableCollection<NodePropertyPortViewModel> OutputPropertyPortViewModels
        {
            get => this._OutputPropertyPortViewModels;
            set
            {
                if (value != this._OutputPropertyPortViewModels)
                {
                    this._OutputPropertyPortViewModels = value;
                    this.RaisePropertyChanged("OutputPropertyPortViewModels");
                }
            }
        }

        #endregion // NodePropertyPorts

        #region Constructors

        public NodeViewModel(Node node) : base(node)
        {
            this.Model = node ?? throw new ArgumentException("Node can not be null in NodeViewModel constructor");
        }

        #endregion // Constructors

        #region Events

        protected override void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ModelPropertyChanged(sender, e);

            this.RaisePropertyChanged(e.PropertyName);
        }

        #endregion // Events
    }
}
