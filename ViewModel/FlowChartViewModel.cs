using NodeGraph.Model;
using NodeGraph.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace NodeGraph.ViewModel
{
    public class FlowChartViewModel : ViewModelBase
    {
        #region Fields

        public FlowChartView View;

        #endregion // Fields

        #region Properties

        private FlowChart _Model;
        public FlowChart Model
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

        protected ObservableCollection<NodeViewModel> _NodeViewModels = new ObservableCollection<NodeViewModel>();
        public ObservableCollection<NodeViewModel> NodeViewModels
        {
            get => this._NodeViewModels;
            set
            {
                if (value != this._NodeViewModels)
                {
                    this.RaisePropertyChanged("NodeViewModels");
                }
            }
        }

        protected ObservableCollection<ConnectorViewModel> _ConnectorViewModels = new ObservableCollection<ConnectorViewModel>();
        public ObservableCollection<ConnectorViewModel> ConnectorViewModels
        {
            get => this._ConnectorViewModels;
            set
            {
                if (value != this._ConnectorViewModels)
                {
                    this.RaisePropertyChanged("ConnectorViewModels");
                }
            }
        }

        private Visibility _SelectionVisibility = Visibility.Collapsed;
        public Visibility SelectionVisibility
        {
            get => this._SelectionVisibility;
            set
            {
                if (value != this._SelectionVisibility)
                {
                    this._SelectionVisibility = value;
                    this.RaisePropertyChanged("SelectionVisibility");
                }
            }
        }

        private double _SelectionStartX;
        public double SelectionStartX
        {
            get => this._SelectionStartX;
            set
            {
                if (value != this._SelectionStartX)
                {
                    this._SelectionStartX = value;
                    this.RaisePropertyChanged("SelectionStartX");
                }
            }
        }

        private double _SelectionStartY;
        public double SelectionStartY
        {
            get => this._SelectionStartY;
            set
            {
                if (value != this._SelectionStartY)
                {
                    this._SelectionStartY = value;
                    this.RaisePropertyChanged("SelectionStartY");
                }
            }
        }

        private double _SelectionWidth;
        public double SelectionWidth
        {
            get => this._SelectionWidth;
            set
            {
                if (value != this._SelectionWidth)
                {
                    this._SelectionWidth = value;
                    this.RaisePropertyChanged("SelectionWidth");
                }
            }
        }

        private double _SelectionHeight;
        public double SelectionHeight
        {
            get => this._SelectionHeight;
            set
            {
                if (value != this._SelectionHeight)
                {
                    this._SelectionHeight = value;
                    this.RaisePropertyChanged("SelectionHeight");
                }
            }
        }


        #endregion // Properties

        #region Constructor

        public FlowChartViewModel(FlowChart flowChart) : base(flowChart)
        {
            this.Model = flowChart;
        }

        #endregion // Constructor

        #region Events

        protected override void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ModelPropertyChanged(sender, e);

            this.RaisePropertyChanged(e.PropertyName);
        }

        #endregion // Events
    }
}
