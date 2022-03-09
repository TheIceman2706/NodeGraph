using System;

namespace NodeGraph.History
{
    public class DestroyConnectorCommand : NodeGraphCommand
    {
        #region Constructor

        public DestroyConnectorCommand(string name, object undoParams, object redoParams) : base(name, undoParams, redoParams)
        {

        }

        #endregion // Constructor

        #region Overrides NodeGraphCommand

        public override void Undo()
        {
            NodeGraphManager.DeserializeConnector(this.UndoParams as string);
        }

        public override void Redo()
        {
            Guid guid = (Guid)this.RedoParams;

            NodeGraphManager.DestroyConnector(guid);
        }

        #endregion // Overrides NodeGraphCommand
    }
}
