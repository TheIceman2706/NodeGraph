using ConnectorGraph.ViewModel;
using NodeGraph.Model;
using NodeGraph.View;
using System.ComponentModel;

namespace NodeGraph.ViewModel
{
    [ConnectorViewModel()]
    public class ConnectorViewModel : ViewModelBase
    {
        #region Fields

        public ConnectorView View;

        #endregion // Fields

        #region Properties

        private Connector _Model;
        public Connector Model
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

        public ConnectorViewModel(Connector connection) : base(connection)
        {
            this.Model = connection;
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
