using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeGraph.View
{
    [TemplatePart(Name = "PART_Port", Type = typeof(FrameworkElement))]
    public class NodePortView : ContentControl
    {
        #region Properties

        public NodePortViewModel ViewModel
        {
            get; private set;
        }

        public FrameworkElement PartPort
        {
            get; private set;
        }

        public TextBlock PartToolTip
        {
            get; private set;
        }

        public bool IsInput
        {
            get => (bool)this.GetValue(IsInputProperty);
            set => this.SetValue(IsInputProperty, value);
        }
        public static readonly DependencyProperty IsInputProperty =
            DependencyProperty.Register("IsInput", typeof(bool), typeof(NodePortView), new PropertyMetadata(false));

        public bool IsFilledPort
        {
            get => (bool)this.GetValue(IsFilledPortProperty);
            set => this.SetValue(IsFilledPortProperty, value);
        }
        public static readonly DependencyProperty IsFilledPortProperty =
            DependencyProperty.Register("IsFilledPort", typeof(bool), typeof(NodePortView), new PropertyMetadata(false));

        public bool IsPortEnabled
        {
            get => (bool)this.GetValue(IsPortEnabledProperty);
            set => this.SetValue(IsPortEnabledProperty, value);
        }
        public static readonly DependencyProperty IsPortEnabledProperty =
            DependencyProperty.Register("IsPortEnabled", typeof(bool), typeof(NodePortView), new PropertyMetadata(true));

        public bool ToolTipVisibility
        {
            get => (bool)this.GetValue(ToolTipVisibilityProperty);
            set => this.SetValue(ToolTipVisibilityProperty, value);
        }
        public static readonly DependencyProperty ToolTipVisibilityProperty =
            DependencyProperty.Register("ToolTipVisibility", typeof(bool), typeof(NodePortView), new PropertyMetadata(false));

        public string ToolTipText
        {
            get => (string)this.GetValue(ToolTipTextProperty);
            set => this.SetValue(ToolTipTextProperty, value);
        }
        public static readonly DependencyProperty ToolTipTextProperty =
            DependencyProperty.Register("ToolTipText", typeof(string), typeof(NodePortView), new PropertyMetadata(""));

        public bool IsConnectorMouseOver
        {
            get => (bool)this.GetValue(IsConnectorMouseOverProperty);
            set => this.SetValue(IsConnectorMouseOverProperty, value);
        }
        public static readonly DependencyProperty IsConnectorMouseOverProperty =
            DependencyProperty.Register("IsConnectorMouseOver", typeof(bool), typeof(NodePortView), new PropertyMetadata(false));

        #endregion // Properteis

        #region Template

        static NodePortView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodePortView), new FrameworkPropertyMetadata(typeof(NodePortView)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PartPort = this.Template.FindName("PART_Port", this) as FrameworkElement;
            if (null == this.PartPort)
            {
                throw new Exception("PART_Port is not instantiated in NodePortView");
            }
        }

        #endregion // Template

        #region Constructor

        public NodePortView(bool isInput)
        {
            this.IsInput = isInput;
            DataContextChanged += this.NodePortView_DataContextChanged;
            Loaded += this.NodePortView_Loaded;
        }

        #endregion // Constructor

        #region Events

        private void NodePortView_Loaded(object sender, RoutedEventArgs e)
        {
            this.SynchronizeProperties();
        }

        private void NodePortView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ViewModel = this.DataContext as NodePortViewModel;
            if (null == this.ViewModel)
            {
                throw new Exception("ViewModel must be bound as DataContext in NodePortView.");
            }

            this.ViewModel.View = this;
            this.ViewModel.PropertyChanged += this.ViewModelPropertyChanged;

            this.SynchronizeProperties();
        }

        protected virtual void SynchronizeProperties()
        {
            if (null == this.ViewModel)
            {
                return;
            }

            NodePort port = this.ViewModel.Model;
            this.IsInput = port.IsInput;
            this.IsFilledPort = (0 < port.Connectors.Count);
            this.IsPortEnabled = port.IsPortEnabled;
            this.IsEnabled = port.IsEnabled;
        }

        protected virtual void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.SynchronizeProperties();
        }

        #endregion // Events

        #region Mouse Events

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            Node node = this.ViewModel.Model.Owner;
            FlowChart flowChart = node.Owner;
            Keyboard.Focus(flowChart.ViewModel.View);

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                NodeGraphManager.DisconnectAll(this.ViewModel.Model);
            }
            else if (!NodeGraphManager.IsConnecting)
            {
                this.IsFilledPort = true;
                NodeGraphManager.BeginConnection(this.ViewModel.Model);
            }

            e.Handled = true;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (MouseButtonState.Pressed == e.LeftButton)
            {
                if (NodeGraphManager.IsConnecting)
                {
                    bool connectable = NodeGraphManager.CheckIfConnectable(this.ViewModel.Model, out string error);
                    if (connectable)
                    {
                        NodeGraphManager.SetOtherConnectionPort(this.ViewModel.Model);
                        this.ToolTipVisibility = false;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(error))
                        {
                            this.ToolTipVisibility = false;
                        }
                        else
                        {
                            this.ToolTipText = error;
                            this.ToolTipVisibility = true;
                        }
                    }
                }
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (NodeGraphManager.IsConnecting)
            {
                NodeGraphManager.SetOtherConnectionPort(null);
            }

            this.ToolTipVisibility = false;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (NodeGraphManager.IsConnecting)
            {
                NodeGraphManager.SetOtherConnectionPort(null);
            }

            this.ToolTipVisibility = false;
        }

        #endregion // Mouse Events

        #region HitTest

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (VisualTreeHelper.GetDescendantBounds(this).Contains(hitTestParameters.HitPoint))
            {
                return new PointHitTestResult(this, hitTestParameters.HitPoint);
            }

            return null;
        }

        #endregion // HitTest
    }
}
