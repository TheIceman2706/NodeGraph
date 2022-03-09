using NodeGraph.Model;
using NodeGraph.ViewModel;
using PropertyTools.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NodeGraph.View
{
    [TemplatePart(Name = "PART_Header", Type = typeof(EditableTextBlock))]
    public class NodeView : ContentControl
    {
        #region Fields

        private EditableTextBlock _Part_Header;
        private readonly DispatcherTimer _ClickTimer = new DispatcherTimer();
        private int _ClickCount = 0;

        private readonly BitmapImage[] _ExecutionStateImages;

        #endregion // Fields

        #region Properties

        public NodeViewModel ViewModel
        {
            get; private set;
        }

        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(NodeView), new PropertyMetadata(false));

        public bool HasConnection
        {
            get => (bool)this.GetValue(HasConnectionProperty);
            set => this.SetValue(HasConnectionProperty, value);
        }
        public static readonly DependencyProperty HasConnectionProperty =
            DependencyProperty.Register("HasConnection", typeof(bool), typeof(NodeView), new PropertyMetadata(false));

        public Thickness SelectionThickness
        {
            get => (Thickness)this.GetValue(SelectionThicknessProperty);
            set => this.SetValue(SelectionThicknessProperty, value);
        }
        public static readonly DependencyProperty SelectionThicknessProperty =
            DependencyProperty.Register("SelectionThickness", typeof(Thickness), typeof(NodeView), new PropertyMetadata(new Thickness(2.0)));

        public double CornerRadius
        {
            get => (double)this.GetValue(CornerRadiusProperty);
            set => this.SetValue(CornerRadiusProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(NodeView), new PropertyMetadata(8.0));

        public BitmapImage ExecutionStateImage
        {
            get => (BitmapImage)this.GetValue(ExecutionStateImageProperty);
            set => this.SetValue(ExecutionStateImageProperty, value);
        }
        public static readonly DependencyProperty ExecutionStateImageProperty =
            DependencyProperty.Register("ExecutionStateImage", typeof(BitmapImage), typeof(NodeView),
                new PropertyMetadata(null));

        #endregion // Properties

        #region Constructors

        private BitmapImage LoadBitmapImage(Uri uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        public NodeView()
        {
            DataContextChanged += this.NodeView_DataContextChanged;
            Loaded += this.NodeView_Loaded;
            Unloaded += this.NodeView_Unloaded;

            this._ExecutionStateImages = new BitmapImage[4];

            this._ExecutionStateImages[0] = new BitmapImage();

            this._ExecutionStateImages[1] = this.LoadBitmapImage(
                new Uri("pack://application:,,,/NodeGraph;component/Resources/Images/Executing.png"));

            this._ExecutionStateImages[2] = this.LoadBitmapImage(
                new Uri("pack://application:,,,/NodeGraph;component/Resources/Images/Executed.png"));

            this._ExecutionStateImages[3] = this.LoadBitmapImage(
                new Uri("pack://application:,,,/NodeGraph;component/Resources/Images/Failed.png"));
            ;
        }

        #endregion // Constructors

        #region Events

        private void NodeView_Loaded(object sender, RoutedEventArgs e)
        {
            this.SynchronizeProperties();
            this.OnCanvasRenderTransformChanged();

            this._ClickTimer.Interval = TimeSpan.FromMilliseconds(300);
            this._ClickTimer.Tick += this._ClickTimer_Tick;
        }

        private void _ClickTimer_Tick(object sender, EventArgs e)
        {
            this._ClickCount = 0;
            this._ClickTimer.Stop();
        }

        private void NodeView_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void NodeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ViewModel = this.DataContext as NodeViewModel;
            if (null == this.ViewModel)
            {
                throw new Exception("ViewModel must be bound as DataContext in NodeView.");
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

            this.IsSelected = this.ViewModel.IsSelected;
            this.HasConnection = (0 < this.ViewModel.InputFlowPortViewModels.Count) ||
                (0 < this.ViewModel.OutputFlowPortViewModels.Count) ||
                (0 < this.ViewModel.InputPropertyPortViewModels.Count) ||
                (0 < this.ViewModel.OutputPropertyPortViewModels.Count);

            this.ExecutionStateImage = this._ExecutionStateImages[(int)this.ViewModel.Model.ExecutionState];
        }

        protected virtual void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.SynchronizeProperties();
        }

        #endregion // Events

        #region Template Events

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._Part_Header = this.Template.FindName("PART_Header", this) as EditableTextBlock;
            if (null != this._Part_Header)
            {
                this._Part_Header.MouseDown += this._Part_Header_MouseDown;
            }
        }

        #endregion // Template Events

        #region Header Events

        private void _Part_Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this._Part_Header);

            if (0 == this._ClickCount)
            {
                this._ClickTimer.Start();
                this._ClickCount++;
            }
            else if (1 == this._ClickCount)
            {
                this._Part_Header.IsEditing = true;
                Keyboard.Focus(this._Part_Header);
                this._ClickCount = 0;
                this._ClickTimer.Stop();

                e.Handled = true;
            }
        }

        #endregion // Header Events

        #region Mouse Events

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            FlowChart flowChart = this.ViewModel.Model.Owner;

            if (NodeGraphManager.IsConnecting)
            {
                bool bConnected;
                flowChart.History.BeginTransaction("Creating Connection");
                {
                    bConnected = NodeGraphManager.EndConnection();
                }
                flowChart.History.EndTransaction(!bConnected);
            }

            if (NodeGraphManager.IsSelecting)
            {
                bool bChanged = false;
                flowChart.History.BeginTransaction("Selecting");
                {
                    bChanged = NodeGraphManager.EndDragSelection(false);
                }
                flowChart.History.EndTransaction(!bChanged);
            }

            if (!NodeGraphManager.AreNodesReallyDragged &&
                NodeGraphManager.MouseLeftDownNode == this.ViewModel.Model)
            {
                flowChart.History.BeginTransaction("Selection");
                {
                    NodeGraphManager.TrySelection(flowChart, this.ViewModel.Model,
                        Keyboard.IsKeyDown(Key.LeftCtrl),
                        Keyboard.IsKeyDown(Key.LeftShift),
                        Keyboard.IsKeyDown(Key.LeftAlt));
                }
                flowChart.History.EndTransaction(false);
            }

            NodeGraphManager.EndDragNode();

            NodeGraphManager.MouseLeftDownNode = null;

            e.Handled = true;
        }

        private Point _DraggingStartPos;
        private Matrix _ZoomAndPanStartMatrix;
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            FlowChart flowChart = this.ViewModel.Model.Owner;
            FlowChartView flowChartView = flowChart.ViewModel.View;
            Keyboard.Focus(flowChartView);

            NodeGraphManager.EndConnection();
            NodeGraphManager.EndDragNode();
            NodeGraphManager.EndDragSelection(false);

            NodeGraphManager.MouseLeftDownNode = this.ViewModel.Model;

            NodeGraphManager.BeginDragNode(flowChart);

            Node node = this.ViewModel.Model;
            this._DraggingStartPos = new Point(node.X, node.Y);

            flowChart.History.BeginTransaction("Moving node");

            this._ZoomAndPanStartMatrix = flowChartView.ZoomAndPan.Matrix;

            e.Handled = true;
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (NodeGraphManager.IsNodeDragging)
            {
                FlowChart flowChart = this.ViewModel.Model.Owner;

                Node node = this.ViewModel.Model;
                Point delta = new Point(node.X - this._DraggingStartPos.X, node.Y - this._DraggingStartPos.Y);

                if ((0 != (int)delta.X) &&
                    (0 != (int)delta.Y))
                {
                    ObservableCollection<Guid> selectionList = NodeGraphManager.GetSelectionList(node.Owner);
                    foreach (Guid guid in selectionList)
                    {
                        Node currentNode = NodeGraphManager.FindNode(guid);

                        flowChart.History.AddCommand(new History.NodePropertyCommand(
                            "Node.X", currentNode.Guid, "X", currentNode.X - delta.X, currentNode.X));
                        flowChart.History.AddCommand(new History.NodePropertyCommand(
                            "Node.Y", currentNode.Guid, "Y", currentNode.Y - delta.Y, currentNode.Y));
                    }

                    flowChart.History.AddCommand(new History.ZoomAndPanCommand(
                        "ZoomAndPan", flowChart, this._ZoomAndPanStartMatrix, flowChart.ViewModel.View.ZoomAndPan.Matrix));

                    flowChart.History.EndTransaction(false);
                }
                else
                {
                    flowChart.History.EndTransaction(true);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (NodeGraphManager.IsNodeDragging &&
                (NodeGraphManager.MouseLeftDownNode == this.ViewModel.Model) &&
                !this.IsSelected)
            {
                Node node = this.ViewModel.Model;
                FlowChart flowChart = node.Owner;
                NodeGraphManager.TrySelection(flowChart, node, false, false, false);
            }
        }

        #endregion // Mouse Events

        #region RenderTrasnform

        public void OnCanvasRenderTransformChanged()
        {
            Matrix matrix = (this.VisualParent as Canvas).RenderTransform.Value;
            double scale = matrix.M11;

            this.SelectionThickness = new Thickness(2.0 / scale);
            this.CornerRadius = 8.0 / scale;
        }

        #endregion // RenderTransform
    }
}
