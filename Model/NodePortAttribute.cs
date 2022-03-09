﻿using System;

namespace NodeGraph.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class NodePortAttribute : Attribute
    {
        public string Name = string.Empty;
        public string DisplayName = string.Empty;
        public bool IsInput = false;
        public bool AllowMultipleInput = false;
        public bool AllowMultipleOutput = false;
        public bool IsPortEnabled = true;
        public bool IsEnabled = true;

        public NodePortAttribute(string displayName, bool isInput)
        {
            this.DisplayName = displayName;
            this.IsInput = isInput;
        }
    }
}
