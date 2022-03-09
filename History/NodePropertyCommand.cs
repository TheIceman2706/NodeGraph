using NodeGraph.Model;
using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace NodeGraph.History
{
    public class NodePropertyCommand : NodeGraphCommand
    {
        #region Additional information

        public Guid Guid
        {
            get; private set;
        }
        public string PropertyName
        {
            get; private set;
        }

        #endregion // additional information

        #region Constructor

        public NodePropertyCommand(string name, Guid nodeGuid, string propertyName, object undoParams, object redoParams) : base(name, undoParams, redoParams)
        {
            this.Guid = nodeGuid;
            this.PropertyName = propertyName;
        }

        #endregion // Constructor

        #region Overrides NodeGraphCommand

        public override void Undo()
        {
            Node node = NodeGraphManager.FindNode(this.Guid);
            if (null == node)
            {
                throw new InvalidOperationException("Node does not exist.");
            }

            if ("IsSelected" == this.PropertyName)
            {
                this.UpdateSelection((bool)this.UndoParams);
            }
            else
            {
                Type type = node.GetType();
                PropertyInfo propInfo = type.GetProperty(this.PropertyName);
                propInfo.SetValue(node, this.UndoParams);
            }
        }

        public override void Redo()
        {
            Node node = NodeGraphManager.FindNode(this.Guid);
            if (null == node)
            {
                throw new InvalidOperationException("Node does not exist.");
            }

            if ("IsSelected" == this.PropertyName)
            {
                this.UpdateSelection((bool)this.RedoParams);
            }
            else
            {
                Type type = node.GetType();
                PropertyInfo propInfo = type.GetProperty(this.PropertyName);
                propInfo.SetValue(node, this.RedoParams);
            }
        }

        #endregion // Overrides NodeGraphCommand

        #region Private Methods

        private void UpdateSelection(bool isSelected)
        {
            Node node = NodeGraphManager.FindNode(this.Guid);

            ObservableCollection<Guid> selectionList = NodeGraphManager.GetSelectionList(node.Owner);

            node.ViewModel.IsSelected = isSelected;

            if (node.ViewModel.IsSelected)
            {
                System.Diagnostics.Debug.WriteLine("True");
                if (!selectionList.Contains(this.Guid))
                {
                    selectionList.Add(this.Guid);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("False");
                if (selectionList.Contains(this.Guid))
                {
                    selectionList.Remove(this.Guid);
                }
            }
        }

        #endregion // Private Methods
    }
}
