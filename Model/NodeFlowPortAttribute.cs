using NodeGraph.ViewModel;
using System;

namespace NodeGraph.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = true)]
    public class NodeFlowPortAttribute : NodePortAttribute
    {
        public Type ViewModelType = typeof(NodeFlowPortViewModel);

        public NodeFlowPortAttribute(string name, string displayName, bool isInput) : base(displayName, isInput)
        {
            this.Name = name;
            this.AllowMultipleInput = true;
            this.AllowMultipleOutput = false;

            if (!typeof(NodeFlowPortViewModel).IsAssignableFrom(this.ViewModelType))
            {
                throw new ArgumentException("ViewModelType of NodeFlowPortAttribute must be subclass of NodeFlowPortViewModel");
            }
        }
    }
}
