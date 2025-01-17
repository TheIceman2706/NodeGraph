﻿using NodeGraph.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NodeGraph.View
{
    public class NodeFlowPortViewsContainer : ItemsControl
    {
        #region Fields

        private Type _ViewType = null;

        #endregion // Fields

        #region Properties

        public bool IsInput
        {
            get => (bool)this.GetValue(IsInputProperty);
            set => this.SetValue(IsInputProperty, value);
        }
        public static readonly DependencyProperty IsInputProperty =
            DependencyProperty.Register("IsInput", typeof(bool), typeof(NodeFlowPortViewsContainer), new PropertyMetadata(false));

        #endregion // Properties

        #region Overrides ItemsControl

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            NodeFlowPortViewModel viewModel = item as NodeFlowPortViewModel;

            NodeFlowPortViewModelAttribute[] attrs = item.GetType().GetCustomAttributes(typeof(NodeFlowPortViewModelAttribute), false) as NodeFlowPortViewModelAttribute[];

            if (0 == attrs.Length)
            {
                throw new Exception("A NodeFlowPortViewModelAttribute must exist for NodeFlowPortViewModel class.");
            }
            else if (1 < attrs.Length)
            {
                throw new Exception("A NodeFlowPortViewModelAttribute must exist only one.");
            }

            this._ViewType = attrs[0].ViewType;

            return base.IsItemItsOwnContainerOverride(item);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            NodeFlowPortViewModelAttribute[] attrs = item.GetType().GetCustomAttributes(typeof(NodeFlowPortViewModelAttribute), false) as NodeFlowPortViewModelAttribute[];
            if (1 != attrs.Length)
            {
                throw new Exception("A NodeFlowPortViewModelAttribute must exist for NodeFlowPortViewModel class");
            }

            FrameworkElement fe = element as FrameworkElement;

            ResourceDictionary resourceDictionary = new ResourceDictionary
            {
                Source = new Uri("/NodeGraph;component/Themes/generic.xaml", UriKind.RelativeOrAbsolute)
            };

            Style style = resourceDictionary[attrs[0].ViewStyleName] as Style;
            if (null == style)
            {
                style = Application.Current.TryFindResource(attrs[0].ViewStyleName) as Style;
            }
            fe.Style = style;

            if (null == fe.Style)
            {
                throw new Exception(String.Format("{0} does not exist", attrs[0].ViewStyleName));
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return Activator.CreateInstance(this._ViewType, new object[] { this.IsInput }) as DependencyObject;
        }

        #endregion // Overrides ItemsControl
    }
}
