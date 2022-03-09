using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace NodeGraph.Model
{
    public class NodePropertyPort : NodePort
    {
        #region Events

        public delegate void DynamicPropertyPortValueChangedDelegate(NodePropertyPort port, object prevValue, object newValue);
        public event DynamicPropertyPortValueChangedDelegate DynamicPropertyPortValueChanged;

        protected virtual void OnDynamicPropertyPortValueChanged(object prevValue, object newValue)
        {
            DynamicPropertyPortValueChanged?.Invoke(this, prevValue, newValue);
        }

        #endregion // Events

        #region Fields

        public readonly bool IsDynamic;
        public readonly bool IsPulling;
        public readonly bool HasEditor;
        protected FieldInfo _FieldInfo;
        protected PropertyInfo _PropertyInfo;

        #endregion // Fields

        #region Properties

        public object _Value;
        public object Value
        {
            get
            {
                //if possible, pull information through
                if (this.IsInput && this.Connectors.Count > 0 && this.Connectors[0] != null && this.Connectors[0].StartPort is NodePropertyPort startPort)
                {
                    return startPort.Value;
                }
                else
                {
                    if (this.IsDynamic)
                    {
                        return this._Value;
                    }
                    else
                    {
                        return (null != this._FieldInfo) ? this._FieldInfo.GetValue(this.Owner) : this._PropertyInfo.GetValue(this.Owner);
                    }
                }
            }
            set
            {
                object prevValue;
                if (this.IsDynamic)
                {
                    prevValue = this._Value;
                }
                else
                {
                    prevValue = (null != this._FieldInfo) ? this._FieldInfo.GetValue(this.Owner) : this._PropertyInfo.GetValue(this.Owner);
                }

                //if( value != prevValue )
                //{
                if (this.IsDynamic)
                {
                    this._Value = value;
                    this.OnDynamicPropertyPortValueChanged(prevValue, value);
                }
                else
                {
                    try
                    {
                        if (null != this._FieldInfo)
                        {
                            this._FieldInfo.SetValue(this.Owner, value);
                        }
                        else if (null != this._PropertyInfo)
                        {
                            this._PropertyInfo.SetValue(this.Owner, value);
                        }
                    }
                    catch (Exception) { }
                }

                this.RaisePropertyChanged("Value");
                //}
            }
        }

        public Type ValueType
        {
            get; private set;
        }

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// Never call this constructor directly. Use GraphManager.CreateNodePropertyPort() method.
        /// </summary>
        public NodePropertyPort(Guid guid, Node node, bool isInput, Type valueType, object value, string name, bool hasEditor) :
            base(guid, node, isInput)
        {
            this.Name = name;
            this.HasEditor = hasEditor;

            Type nodeType = node.GetType();
            this._FieldInfo = nodeType.GetField(this.Name);
            this._PropertyInfo = nodeType.GetProperty(this.Name);

            this.IsPulling = this._PropertyInfo != null && this._PropertyInfo.GetCustomAttribute<NodePropertyPortAttribute>().IsPulling;

            this.IsDynamic = (null == this._FieldInfo) && (null == this._PropertyInfo);

            this.ValueType = valueType;
            this.Value = value;

        }

        #endregion // Constructors

        #region Overrides IXmlSerializable

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);

            writer.WriteAttributeString("ValueType", this.ValueType.AssemblyQualifiedName);
            writer.WriteAttributeString("HasEditor", this.HasEditor.ToString());
            // writer.WriteAttributeString("IsPulling", IsPulling.ToString());

            Type realValueType = this.ValueType;
            if (null != this.Value)
            {
                realValueType = this.Value.GetType();
            }
            writer.WriteAttributeString("RealValueType", realValueType.AssemblyQualifiedName);

            XmlSerializer serializer = new XmlSerializer(realValueType);
            serializer.Serialize(writer, this.Value);
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            //IsPulling = bool.Parse(reader.GetAttribute("IsPulling"));
            Type realValueType = Type.GetType(reader.GetAttribute("RealValueType"));

            while (reader.Read())
            {
                if (XmlNodeType.Element == reader.NodeType)
                {
                    XmlSerializer serializer = new XmlSerializer(realValueType);
                    this.Value = serializer.Deserialize(reader);
                    break;
                }
            }
        }

        #endregion // Overrides IXmlSerializable

        #region Callbacks

        public override void OnCreate()
        {
            base.OnCreate();

            this.CheckValidity();
        }


        public override void OnDeserialize()
        {
            base.OnDeserialize();

            this.CheckValidity();
        }

        #endregion // Callbacks

        #region Methods

        public void CheckValidity()
        {
            if (null != this.Value)
            {
                if (!this.ValueType.IsAssignableFrom(this.Value.GetType()))
                {
                    throw new ArgumentException("Type of value is not same as typeOfvalue.");
                }
            }

            if ((!this.ValueType.IsClass && Nullable.GetUnderlyingType(this.ValueType) == null) && (null == this.Value))
            {
                throw new ArgumentNullException("If typeOfValue is not a class, you cannot specify value as null");
            }

            if (!this.IsDynamic)
            {
                Type nodeType = this.Owner.GetType();
                this._FieldInfo = nodeType.GetField(this.Name);
                this._PropertyInfo = nodeType.GetProperty(this.Name);

                Type propType = (null != this._FieldInfo) ? this._FieldInfo.FieldType : this._PropertyInfo.PropertyType;
                if (propType != this.ValueType)
                {
                    throw new ArgumentException(string.Format("ValueType( {0} ) is invalid, becasue a type of property or field is {1}.",
                        this.ValueType.Name, propType.Name));
                }
            }
        }


        #endregion // Methods
    }
}
