using NodeGraph.ViewModel;
using System;
using System.Windows.Media;

namespace NodeGraph.Model
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NodePropertyPortAttribute : NodePortAttribute
    {
        public Type ValueType;
        public Type ViewModelType = typeof(NodePropertyPortViewModel);
        public object DefaultValue;
        public bool HasEditor;

        public bool IsPulling
        {
            get; set;
        }

        public NodePropertyPortAttribute(string displayName, bool isInput, Type valueType, object defaultValue, bool hasEditor) : base(displayName, isInput)
        {
            if (null != defaultValue)
            {
                if ((valueType == typeof(Color)) && (defaultValue.GetType() == typeof(string)))
                {
                    defaultValue = ColorConverter.ConvertFromString(defaultValue as string);
                }
                else if (defaultValue.GetType() != valueType)
                {
                    throw new ArgumentException("Type of value is not same as ValueType.");
                }
            }

            if ((!valueType.IsClass && Nullable.GetUnderlyingType(valueType) == null) && (null == defaultValue))
            {
                throw new ArgumentException("If ValueType is not a class, you cannot specify value as null");
            }

            this.ValueType = valueType;
            this.IsInput = isInput;
            this.DefaultValue = defaultValue;
            this.AllowMultipleInput = false;
            this.AllowMultipleOutput = true;
            this.HasEditor = hasEditor;

            if (!typeof(NodePropertyPortViewModel).IsAssignableFrom(this.ViewModelType))
            {
                throw new ArgumentException("ViewModelType of NodePropertyAttribute must be subclass of NodePropertyPortViewModel");
            }
        }

    }
}
