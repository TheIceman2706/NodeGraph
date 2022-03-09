using NodeGraph.Model;

namespace NodeGraph.ViewModel
{
    [NodePropertyPortViewModel()]
    public class NodePropertyPortViewModel : NodePortViewModel
    {
        #region Constructor

        public NodePropertyPortViewModel(NodePropertyPort nodeProperty) : base(nodeProperty)
        {
            this.Model = nodeProperty;
        }

        #endregion // Constructor
    }
}
