using System;

namespace NodeGraph.History
{
    public class DestroyNodePortCommand : NodeGraphCommand
    {
        #region Constructor

        public DestroyNodePortCommand(string name, object undoParams, object redoParams) : base(name, undoParams, redoParams)
        {

        }

        #endregion // Constructor

        #region Overrides NodeGraphCommand

        public override void Undo()
        {
            NodeGraphManager.DeserializeNodePort(this.UndoParams as string);
        }

        public override void Redo()
        {
            Guid guid = (Guid)this.RedoParams;

            NodeGraphManager.DestroyNodePort(guid);
        }

        #endregion // Overrides NodeGraphCommand
    }
}
