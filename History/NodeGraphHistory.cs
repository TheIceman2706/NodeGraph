using System;
using System.Collections.Generic;

namespace NodeGraph.History
{
    /// <summary>
    /// History stack for NodeGraph. 
    /// </summary>
    public class NodeGraphHistory
    {
        #region Transaction

        public class Transaction
        {
            public string Name
            {
                get; private set;
            }

            public List<NodeGraphCommand> Commands = new List<NodeGraphCommand>();

            public Transaction(string name)
            {
                this.Name = name;
            }

            public void Add(NodeGraphCommand command)
            {
                this.Commands.Add(command);
            }

            internal void Undo()
            {
                for (int i = this.Commands.Count - 1; i >= 0; --i)
                {
                    this.Commands[i].Undo();
                }
            }

            internal void Redo()
            {
                for (int i = 0; i < this.Commands.Count; ++i)
                {
                    this.Commands[i].Redo();
                }
            }
        };

        #endregion // Transaction

        #region Fields

        private List<Transaction> _Transactions = new List<Transaction>();

        private bool _IsProcessingHistory = false;

        private int _CurrentPos = -1;

        #endregion // Fields

        #region Properties

        public object Owner
        {
            get; private set;
        }

        #endregion Properites

        #region Constructor

        public NodeGraphHistory(object owner, int numTransactions = 100)
        {
            this.Owner = owner;

            this.SetNumberOfTransactions(numTransactions);
        }

        #endregion // Constructor

        #region Private Methods

        #endregion // Private Methods

        #region Public Methods

        public void Clear()
        {
            this._Transactions.Clear();
            this._CurrentPos = -1;
        }

        public int GetNumberOfTransactions()
        {
            return this._Transactions.Count;
        }

        public void SetNumberOfTransactions(int numTransactions, bool bClear = false)
        {
            if (bClear)
            {
                this._Transactions.Clear();
                this._CurrentPos = -1;
            }

            int prevNumTransactions = this._Transactions.Count;

            if (prevNumTransactions < numTransactions)
            {
                List<Transaction> newTransactions = new List<Transaction>();
                for (int i = 0; i < numTransactions; ++i)
                {
                    if (i < prevNumTransactions)
                    {
                        newTransactions.Add(this._Transactions[i]);
                    }
                    else
                    {
                        newTransactions.Add(null);
                    }
                }
                this._Transactions = newTransactions;
            }
            else if (prevNumTransactions > numTransactions)
            {
                List<Transaction> newTransactions = new List<Transaction>();

                int start = 0;
                if (this._CurrentPos >= numTransactions)
                {
                    start = this._CurrentPos - numTransactions;
                }
                else
                {
                    start = prevNumTransactions - 1 - numTransactions;
                }

                int count = 0;
                for (int i = start; i <= this._CurrentPos; ++i, ++count)
                {
                    newTransactions.Add(this._Transactions[i]);
                }

                for (int i = 0; i < numTransactions - count; ++i)
                {
                    newTransactions.Add(null);
                }

                this._Transactions = newTransactions;
            }
        }

        public List<Transaction> GetTransactions()
        {
            return this._Transactions;
        }

        public void GetTransactionList(out List<Transaction> undoTransactionList,
            out Transaction currentTransaction,
            out List<Transaction> redoTransactionList)
        {
            undoTransactionList = new List<Transaction>();
            currentTransaction = null;
            redoTransactionList = new List<Transaction>();

            if (-1 == this._CurrentPos)
            {
                return;
            }

            for (int i = 0; i < this._CurrentPos; ++i)
            {
                undoTransactionList.Add(this._Transactions[i]);
            }

            currentTransaction = this._Transactions[this._CurrentPos];

            for (int i = this._CurrentPos + 1; i < this._Transactions.Count; ++i)
            {
                redoTransactionList.Add(this._Transactions[i]);
            }
        }

        private Transaction _TransactionAdding = null;
        public void BeginTransaction(string name)
        {
            if (this._IsProcessingHistory)
            {
                return;
            }

            if (null != this._TransactionAdding)
            {
                this.EndTransaction(true);
            }

            this._TransactionAdding = new Transaction(name);

            if (NodeGraphManager.OutputDebugInfo)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("BeginTransaction {0}", this._TransactionAdding.Name));
            }
        }

        public void AddCommand(NodeGraphCommand command)
        {
            if (this._IsProcessingHistory)
            {
                return;
            }

            if (null != this._TransactionAdding)
            {
                this._TransactionAdding.Add(command);

                if (NodeGraphManager.OutputDebugInfo)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Add command to transaction {0} : {1}", this._TransactionAdding.Name, command.Name));
                }
            }
        }

        public void EndTransaction(bool bCancel)
        {
            if (this._IsProcessingHistory)
            {
                return;
            }

            if (null == this._TransactionAdding)
            {
                return;
            }

            if (!bCancel && (0 != this._TransactionAdding.Commands.Count))
            {
                int nextPos = this._CurrentPos + 1;
                if (nextPos >= this._Transactions.Count) // shift
                {
                    this._Transactions.RemoveAt(0);
                    this._Transactions.Add(null);
                }
                else
                {
                    this._CurrentPos = nextPos;
                }

                this._Transactions[this._CurrentPos] = this._TransactionAdding;

                for (int i = this._CurrentPos + 1; i < this._Transactions.Count; ++i)
                {
                    this._Transactions[i] = null;
                }

            }

            if (NodeGraphManager.OutputDebugInfo)
            {
                if (bCancel || (0 == this._TransactionAdding.Commands.Count))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("CancelTransaction {0}", this._TransactionAdding.Name));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("EndTransaction {0}", this._TransactionAdding.Name));
                }
            }

            this._TransactionAdding = null;
        }

        public void Undo()
        {
            this.MoveTo(this._CurrentPos - 1);
        }

        public void Redo()
        {
            this.MoveTo(this._CurrentPos + 1);
        }

        public void MoveTo(int pos)
        {
            this._IsProcessingHistory = true;

            int realPos = Math.Min(this._Transactions.Count - 1, Math.Max(-1, pos));

            // redo.
            if (this._CurrentPos < realPos)
            {
                int lastPos = -1000;
                for (int i = this._CurrentPos + 1; i <= realPos; ++i)
                {
                    if (null == this._Transactions[i])
                    {
                        break;
                    }

                    this._Transactions[i].Redo();
                    lastPos = i;

                    if (NodeGraphManager.OutputDebugInfo)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Redo {0} : {1}", i, this._Transactions[i].Name));
                    }
                }

                this._CurrentPos = (-1000 != lastPos) ? lastPos : this._CurrentPos;
            }
            // undo
            else if (this._CurrentPos > realPos)
            {
                for (int i = this._CurrentPos; i > realPos; --i)
                {
                    this._Transactions[i].Undo();

                    if (NodeGraphManager.OutputDebugInfo)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Undo {0} : {1}", i, this._Transactions[i].Name));
                    }
                }
                this._CurrentPos = realPos;
            }
            // nothing.
            else
            {

            }

            this._IsProcessingHistory = false;
        }

        #endregion // Public Methods
    }
}
