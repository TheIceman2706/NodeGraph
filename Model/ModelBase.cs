using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace NodeGraph.Model
{
    public class ModelBase : INotifyPropertyChanged, IXmlSerializable
    {
        #region Properties

        public bool IsInitialized { get; protected set; } = false;

        #endregion // Properties

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged

        #region IXmlSerializable

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Guid", this.Guid.ToString());
            writer.WriteAttributeString("Type", this.GetType().AssemblyQualifiedName);
        }

        public virtual void ReadXml(XmlReader reader)
        {
            // Reading "Guid" and "Type" tasks will be processed before a real instance created.
            // So, calls for this methods will be needless.
        }

        public virtual XmlSchema GetSchema()
        {
            return null;
        }

        #endregion IXmlSerializable

        #region Properties

        public Guid Guid
        {
            get; private set;
        }
        public bool IsDeserializedFromXml = false;

        #endregion // Properties

        #region Constructor

        public ModelBase()
        {
            this.IsDeserializedFromXml = true;
        }

        public ModelBase(Guid guid)
        {
            this.Guid = guid;
        }

        #endregion // Constructor
    }
}
