using NodeGraph.Model;
using NodeGraph.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NodeGraph.View
{
    [TemplatePart(Name = "PART_ConnectorViewsContainer", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_NodeViewsContainer", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_DragAndSelectionCanvas", Type = typeof(FrameworkElement))]
    public class FlowChartView : ContentControl
    {
        #region Fields

        protected DispatcherTimer _Timer = new DispatcherTimer();
        protected double _CurrentTime = 0.0;

        #endregion // Fields

        #region Properties

        public FlowChartViewModel ViewModel
        {
            get; private set;
        }

        private readonly ZoomAndPan _ZoomAndPan = new ZoomAndPan();
        public ZoomAndPan ZoomAndPan => this._ZoomAndPan;

        protected FrameworkElement _NodeCanvas;
        public FrameworkElement NodeCanvas => this._NodeCanvas;

        protected FrameworkElement _ConnectorCanvas;
        public FrameworkElement ConnectorCanvas => this._ConnectorCanvas;

        protected FrameworkElement _PartConnectorViewsContainer;
        public FrameworkElement PartConnectorViewsContainer => this._PartConnectorViewsContainer;

        protected FrameworkElement _PartNodeViewsContainer;
        public FrameworkElement PartNodeViewsContainer => this._PartNodeViewsContainer;

        protected FrameworkElement _PartDragAndSelectionCanvas;
        public FrameworkElement PartDragAndSelectionCanvas => this._PartDragAndSelectionCanvas;

        public ObservableCollection<string> Logs
        {
            get => (ObservableCollection<string>)this.GetValue(LogsProperty);
            set => this.SetValue(LogsProperty, value);
        }
        public static readonly DependencyProperty LogsProperty =
            DependencyProperty.Register("Logs", typeof(ObservableCollection<string>), typeof(FlowChartView), new PropertyMetadata(new ObservableCollection<string>()));

        #endregion // Properties

        #region Constructors

        static FlowChartView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlowChartView), new FrameworkPropertyMetadata(typeof(FlowChartView)));
        }

        public FlowChartView()
        {
            this.Focusable = true;
            DataContextChanged += this.FlowChartView_DataContextChanged;

            SizeChanged += this.FlowChartView_SizeChanged;

            this._Timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
            this._Timer.Tick += this.Timer_Tick;
            this._Timer.Start();

            this.AllowDrop = true;

            DragEnter += this.FlowChartView_DragEnter;
            DragLeave += this.FlowChartView_DragLeave;
            DragOver += this.FlowChartView_DragOver;
            Drop += this.FlowChartView_Drop;
        }

        #endregion // Constructors

        #region Public Methods

        public ModelBase FindModelUnderMouse(Point mousePos, out Point viewSpacePos, out Point modelSpacePos, out ModelType modelType)
        {
            ModelBase model = this.ViewModel.Model;

            viewSpacePos = mousePos;
            modelSpacePos = this._ZoomAndPan.MatrixInv.Transform(mousePos);
            modelType = ModelType.FlowChart;

            HitTestResult hitResult = VisualTreeHelper.HitTest(this, mousePos);
            if ((null != hitResult) && (null != hitResult.VisualHit))
            {

                DependencyObject hit = hitResult.VisualHit;
                NodePortView portView = ViewUtil.FindFirstParent<NodePortView>(hit);
                if (null != portView)
                {
                    model = portView.ViewModel.Model;
                    if (model is NodeFlowPort)
                    {
                        modelType = ModelType.PropertyPort;
                    }
                    else if (typeof(NodeFlowPort).IsAssignableFrom(portView.ViewModel.Model.GetType()))
                    {
                        modelType = ModelType.FlowPort;
                    }
                }
                else
                {
                    NodeView nodeView = ViewUtil.FindFirstParent<NodeView>(hit);
                    if (null != nodeView)
                    {
                        model = nodeView.ViewModel.Model;
                        modelType = ModelType.Node;
                    }
                    else
                    {
                        model = this.ViewModel.Model;
                        modelType = ModelType.FlowChart;
                    }
                }
            }

            return model;
        }

        public void AddLog(string log)
        {
            this.Logs.Add(log);
        }

        public void RemoveLog(string log)
        {
            this.Logs.Remove(log);
        }

        public void ClearLogs()
        {
            this.Logs.Clear();
        }

        #endregion // Public Methods

        #region Template

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._PartConnectorViewsContainer = this.GetTemplateChild("PART_ConnectorViewsContainer") as FrameworkElement;
            if (null == this._PartConnectorViewsContainer)
            {
                throw new Exception("PART_ConnectorViewsContainer can not be null in FlowChartView");
            }

            this._PartNodeViewsContainer = this.GetTemplateChild("PART_NodeViewsContainer") as FrameworkElement;
            if (null == this._PartNodeViewsContainer)
            {
                throw new Exception("PART_NodeViewsContainer can not be null in FlowChartView");
            }

            this._PartDragAndSelectionCanvas = this.GetTemplateChild("PART_DragAndSelectionCanvas") as FrameworkElement;
            if (null == this._PartDragAndSelectionCanvas)
            {
                throw new Exception("PART_DragAndSelectionCanvas can not be null in FlowChartView");
            }
        }

        #endregion // Template

        #region Events

        private void FlowChartView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this._ZoomAndPan.ViewWidth = this.ActualWidth;
            this._ZoomAndPan.ViewHeight = this.ActualHeight;
        }

        private void FlowChartView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.ViewModel = this.DataContext as FlowChartViewModel;
            if (null == this.ViewModel)
            {
                return;
            }

            this.ViewModel.View = this;
            this.ViewModel.PropertyChanged += this.ViewModelPropertyChanged;

            if (null == this._ConnectorCanvas)
            {
                this._ConnectorCanvas = ViewUtil.FindChild<Canvas>(this._PartNodeViewsContainer);
                if (null == this._PartDragAndSelectionCanvas)
                {
                    throw new Exception("Canvas can not be null in PART_ConnectorViewsContainer");
                }
            }

            if (null == this._NodeCanvas)
            {
                this._NodeCanvas = ViewUtil.FindChild<Canvas>(this._PartNodeViewsContainer);
                if (null == this._PartDragAndSelectionCanvas)
                {
                    throw new Exception("Canvas can not be null in PART_NodeViewsContainer");
                }
            }

            this._ZoomAndPan.UpdateTransform += this._ZoomAndPan_UpdateTransform;
        }

        protected virtual void SynchronizeProperties()
        {
            if (null == this.ViewModel)
            {
                return;
            }
        }

        protected virtual void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.SynchronizeProperties();
        }

        #endregion // Events

        #region Mouse Events

        private void _ZoomAndPan_UpdateTransform()
        {
            this._NodeCanvas.RenderTransform = new MatrixTransform(this._ZoomAndPan.Matrix);

            foreach (System.Collections.Generic.KeyValuePair<Guid, Node> pair in NodeGraphManager.Nodes)
            {
                NodeView nodeView = pair.Value.ViewModel.View;
                nodeView.OnCanvasRenderTransformChanged();
            }
        }

        protected Point _RightButtonDownPos;
        protected Point _LeftButtonDownPos;
        protected Point _PrevMousePos;
        protected bool _IsDraggingCanvas;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (null == this.ViewModel)
            {
                return;
            }

            Keyboard.Focus(this);

            this._ZoomAndPanStartMatrix = this.ZoomAndPan.Matrix;

            this._LeftButtonDownPos = e.GetPosition(this);
            this._PrevMousePos = this._LeftButtonDownPos;

            if (!NodeGraphManager.IsNodeDragging &&
                !NodeGraphManager.IsConnecting &&
                !NodeGraphManager.IsSelecting)
            {
                Point mousePos = e.GetPosition(this);

                NodeGraphManager.BeginDragSelection(this.ViewModel.Model,
                    this._ZoomAndPan.MatrixInv.Transform(mousePos));

                this.ViewModel.SelectionStartX = mousePos.X;
                this.ViewModel.SelectionWidth = 0;
                this.ViewModel.SelectionStartY = mousePos.Y;
                this.ViewModel.SelectionHeight = 0;

                bool bCtrl = Keyboard.IsKeyDown(Key.LeftCtrl);
                bool bShift = Keyboard.IsKeyDown(Key.LeftShift);
                bool bAlt = Keyboard.IsKeyDown(Key.LeftAlt);

                if (!bCtrl && !bShift && !bAlt)
                {
                    NodeGraphManager.DeselectAllNodes(this.ViewModel.Model);
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (null == this.ViewModel)
            {
                return;
            }

            FlowChart flowChart = this.ViewModel.Model;

            NodeGraphManager.EndConnection();
            NodeGraphManager.EndDragNode();

            if (NodeGraphManager.IsSelecting)
            {
                bool bChanged = false;
                flowChart.History.BeginTransaction("Selecting");
                {
                    bChanged = NodeGraphManager.EndDragSelection(false);
                }

                Point mousePos = e.GetPosition(this);

                if ((0 != (int)(mousePos.X - this._LeftButtonDownPos.X)) ||
                    (0 != (int)(mousePos.Y - this._LeftButtonDownPos.Y)))
                {
                    flowChart.History.AddCommand(new History.ZoomAndPanCommand(
                        "ZoomAndPan", this.ViewModel.Model, this._ZoomAndPanStartMatrix, this.ZoomAndPan.Matrix));
                    bChanged = true;
                }

                flowChart.History.EndTransaction(!bChanged);
            }

        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (null == this.ViewModel)
            {
                return;
            }

            Keyboard.Focus(this);

            this._RightButtonDownPos = e.GetPosition(this);

            this._ZoomAndPanStartMatrix = this.ZoomAndPan.Matrix;

            if (!NodeGraphManager.IsDragging)
            {
                this._IsDraggingCanvas = true;

                Mouse.Capture(this, CaptureMode.SubTree);

                History.NodeGraphHistory history = this.ViewModel.Model.History;
                history.BeginTransaction("Panning");
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);

            if (null == this.ViewModel)
            {
                return;
            }

            NodeGraphManager.EndConnection();
            NodeGraphManager.EndDragNode();
            NodeGraphManager.EndDragSelection(true);

            Point mousePos = e.GetPosition(this);
            Point diff = new Point(
                Math.Abs(this._RightButtonDownPos.X - mousePos.X),
                Math.Abs(this._RightButtonDownPos.Y - mousePos.Y));

            bool wasDraggingCanvas = (5.0 < diff.X) || (5.0 < diff.Y);

            if (this._IsDraggingCanvas)
            {
                this._IsDraggingCanvas = false;
                Mouse.Capture(null);

                History.NodeGraphHistory history = this.ViewModel.Model.History;
                if (wasDraggingCanvas)
                {
                    history.AddCommand(new History.ZoomAndPanCommand(
                        "ZoomAndPan", this.ViewModel.Model, this._ZoomAndPanStartMatrix, this.ZoomAndPan.Matrix));

                    history.EndTransaction(false);
                }
                else
                {
                    history.EndTransaction(true);
                }
            }

            if (!wasDraggingCanvas)
            {
                ModelBase model = this.FindModelUnderMouse(mousePos, out Point viewSpacePos, out Point modelSpacePos, out ModelType modelType);

                if (null != model)
                {
                    BuildContextMenuArgs args = new BuildContextMenuArgs
                    {
                        ViewSpaceMouseLocation = viewSpacePos,
                        ModelSpaceMouseLocation = modelSpacePos,
                        ModelType = modelType
                    };
                    this.ContextMenu = new ContextMenu();
                    this.ContextMenu.Closed += this.ContextMenu_Closed;
                    args.ContextMenu = this.ContextMenu;

                    if (!NodeGraphManager.InvokeBuildContextMenu(model, args))
                    {
                        this.ContextMenu = null;
                    }
                }
            }
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            this.ContextMenu = null;
        }

        private void UpdateDragging(Point mousePos, Point delta)
        {
            if (NodeGraphManager.IsConnecting)
            {
                NodeGraphManager.UpdateConnection(mousePos);
            }
            else if (NodeGraphManager.IsNodeDragging)
            {
                double invScale = 1.0f / this._ZoomAndPan.Scale;
                NodeGraphManager.DragNode(new Point(delta.X * invScale, delta.Y * invScale));
            }
            else if (NodeGraphManager.IsSelecting)
            {
                // gather nodes in area.

                bool bCtrl = Keyboard.IsKeyDown(Key.LeftCtrl);
                bool bShift = Keyboard.IsKeyDown(Key.LeftShift);
                bool bAlt = Keyboard.IsKeyDown(Key.LeftAlt);

                NodeGraphManager.UpdateDragSelection(this.ViewModel.Model,
                    this._ZoomAndPan.MatrixInv.Transform(mousePos), bCtrl, bShift, bAlt);

                Point startPos = this._ZoomAndPan.Matrix.Transform(NodeGraphManager.SelectingStartPoint);

                Point selectionStart = new Point(Math.Min(startPos.X, mousePos.X), Math.Min(startPos.Y, mousePos.Y));
                Point selectionEnd = new Point(Math.Max(startPos.X, mousePos.X), Math.Max(startPos.Y, mousePos.Y));

                this.ViewModel.SelectionStartX = selectionStart.X;
                this.ViewModel.SelectionStartY = selectionStart.Y;
                this.ViewModel.SelectionWidth = selectionEnd.X - selectionStart.X;
                this.ViewModel.SelectionHeight = selectionEnd.Y - selectionStart.Y;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (null == this.ViewModel)
            {
                return;
            }

            Point mousePos = e.GetPosition(this);

            MouseArea area = this.CheckMouseArea();
            Point delta = new Point(mousePos.X - this._PrevMousePos.X,
                mousePos.Y - this._PrevMousePos.Y);

            if (NodeGraphManager.IsDragging)
            {
                this.UpdateDragging(mousePos, delta);
            }
            else
            {
                if (this._IsDraggingCanvas)
                {
                    this._ZoomAndPan.StartX -= delta.X;
                    this._ZoomAndPan.StartY -= delta.Y;
                }
            }

            this._PrevMousePos = mousePos;

            e.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (null == this.ViewModel)
            {
                return;
            }

            // This case does not occur because of ClipCursor.
            // Becuase this event is invoked when mouse is on port-view tooltip context,
            // consequentially, connection will be broken by EndConnection() call.
            // So, below lines are commented.
            //NodeGraphManager.EndConnection();
            //NodeGraphManager.EndDragNode();
            //NodeGraphManager.EndDragSelection( true );
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (null == this.ViewModel)
            {
                return;
            }

            NodeGraphManager.EndConnection();
            NodeGraphManager.EndDragNode();
            NodeGraphManager.EndDragSelection(true);

            if (this._IsDraggingCanvas)
            {
                this._IsDraggingCanvas = false;
                Mouse.Capture(null);

                History.NodeGraphHistory history = this.ViewModel.Model.History;
                history.EndTransaction(true);
            }
        }

        private bool _IsWheeling = false;
        private double _WheelStartTime = 0.0;
        private Matrix _ZoomAndPanStartMatrix;
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (null == this.ViewModel)
            {
                return;
            }

            if (!this._IsWheeling)
            {
                History.NodeGraphHistory history = this.ViewModel.Model.History;
                history.BeginTransaction("Zooming");
                this._ZoomAndPanStartMatrix = this.ZoomAndPan.Matrix;
            }

            this._WheelStartTime = this._CurrentTime;
            this._IsWheeling = true;

            double newScale = this._ZoomAndPan.Scale;
            newScale += (0.0 > e.Delta) ? -0.05 : 0.05;
            newScale = Math.Max(0.1, Math.Min(1.0, newScale));

            Point vsZoomCenter = e.GetPosition(this);
            Point zoomCenter = this._ZoomAndPan.MatrixInv.Transform(vsZoomCenter);

            this._ZoomAndPan.Scale = newScale;

            Point vsNextZoomCenter = this._ZoomAndPan.Matrix.Transform(zoomCenter);
            Point vsDelta = new Point(vsZoomCenter.X - vsNextZoomCenter.X, vsZoomCenter.Y - vsNextZoomCenter.Y);

            this._ZoomAndPan.StartX -= vsDelta.X;
            this._ZoomAndPan.StartY -= vsDelta.Y;
        }

        #endregion // Mouse Events

        #region Timer Events

        private void Timer_Tick(object sender, EventArgs e)
        {
            this._CurrentTime += this._Timer.Interval.Milliseconds;

            if (null == this.ViewModel)
            {
                return;
            }

            if (NodeGraphManager.IsDragging)
            {
                MouseArea area = this.CheckMouseArea();

                if (MouseArea.None != area)
                {
                    Point delta = new Point(0.0, 0.0);
                    if (MouseArea.Left == (area & MouseArea.Left))
                    {
                        delta.X = -10.0;
                    }

                    if (MouseArea.Right == (area & MouseArea.Right))
                    {
                        delta.X = 10.0;
                    }

                    if (MouseArea.Top == (area & MouseArea.Top))
                    {
                        delta.Y = -10.0;
                    }

                    if (MouseArea.Bottom == (area & MouseArea.Bottom))
                    {
                        delta.Y = 10.0;
                    }

                    Point mousePos = Mouse.GetPosition(this);
                    this.UpdateDragging(
                        new Point(mousePos.X + delta.X, mousePos.Y + delta.Y), // virtual mouse-position.
                        delta); // virtual delta.

                    this._ZoomAndPan.StartX += delta.X;
                    this._ZoomAndPan.StartY += delta.Y;
                }
            }
            else if (this._IsWheeling)
            {
                if (200 < (this._CurrentTime - this._WheelStartTime))
                {
                    this._IsWheeling = false;

                    History.NodeGraphHistory history = this.ViewModel.Model.History;

                    history.AddCommand(new History.ZoomAndPanCommand(
                        "ZoomAndPan", this.ViewModel.Model, this._ZoomAndPanStartMatrix, this.ZoomAndPan.Matrix));

                    history.EndTransaction(false);
                }
            }
            else
            {
                this._CurrentTime = 0.0;
            }
        }

        #endregion // Timer Events

        #region Keyboard Events

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (null == this.ViewModel)
            {
                return;
            }

            if (this.IsFocused)
            {
                if (Key.Delete == e.Key)
                {
                    FlowChart flowChart = this.ViewModel.Model;
                    flowChart.History.BeginTransaction("Destroy Selected Nodes");
                    {
                        NodeGraphManager.DestroySelectedNodes(this.ViewModel.Model);
                    }
                    flowChart.History.EndTransaction(false);
                }
                else if (Key.Escape == e.Key)
                {
                    FlowChart flowChart = this.ViewModel.Model;
                    flowChart.History.BeginTransaction("Destroy Selected Nodes");
                    {
                        NodeGraphManager.DeselectAllNodes(this.ViewModel.Model);
                    }
                    flowChart.History.EndTransaction(false);
                }
                else if (Key.A == e.Key)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        NodeGraphManager.SelectAllNodes(this.ViewModel.Model);
                    }
                    else
                    {
                        this.FitNodesToView(false);
                    }
                }
                else if (Key.F == e.Key)
                {
                    this.FitNodesToView(true);
                }
                else if (Key.Z == e.Key)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        History.NodeGraphHistory history = this.ViewModel.Model.History;
                        history.Undo();
                    }
                }
                else if (Key.Y == e.Key)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        History.NodeGraphHistory history = this.ViewModel.Model.History;
                        history.Redo();
                    }
                }
            }
        }

        #endregion // Keyboard Events

        #region Area

        public enum MouseArea : uint
        {
            None = 0x00000000,
            Left = 0x00000001,
            Right = 0x00000002,
            Top = 0x00000004,
            Bottom = 0x00000008,
        }

        public MouseArea CheckMouseArea()
        {
            Point absPosition = Mouse.GetPosition(this);
            Point absTopLeft = new Point(0.0, 0.0);
            Point absBottomRight = new Point(this.ActualWidth, this.ActualHeight);

            MouseArea area = MouseArea.None;

            if (absPosition.X < (absTopLeft.X + 4.0))
            {
                area |= MouseArea.Left;
            }

            if (absPosition.X > (absBottomRight.X - 4.0))
            {
                area |= MouseArea.Right;
            }

            if (absPosition.Y < (absTopLeft.Y + 4.0))
            {
                area |= MouseArea.Top;
            }

            if (absPosition.Y > (absBottomRight.Y - 4.0))
            {
                area |= MouseArea.Bottom;
            }

            return area;
        }

        #endregion // Area

        #region Fitting.

        public void FitNodesToView(bool bOnlySelected)
        {
            NodeGraphManager.CalculateContentSize(this.ViewModel.Model, bOnlySelected, out double minX, out double maxX, out double minY, out double maxY);
            if ((minX == maxX) || (minY == maxY))
            {
                return;
            }

            FlowChart flowChart = this.ViewModel.Model;
            flowChart.History.BeginTransaction("Destroy Selected Nodes");
            {
                this._ZoomAndPanStartMatrix = this.ZoomAndPan.Matrix;

                double vsWidth = this._ZoomAndPan.ViewWidth;
                double vsHeight = this._ZoomAndPan.ViewHeight;

                Point margin = new Point(vsWidth * 0.05, vsHeight * 0.05);
                minX -= margin.X;
                minY -= margin.Y;
                maxX += margin.X;
                maxY += margin.Y;

                double contentWidth = maxX - minX;
                double contentHeight = maxY - minY;

                this._ZoomAndPan.StartX = (minX + maxX - vsWidth) * 0.5;
                this._ZoomAndPan.StartY = (minY + maxY - vsHeight) * 0.5;
                this._ZoomAndPan.Scale = 1.0;

                Point vsZoomCenter = new Point(vsWidth * 0.5, vsHeight * 0.5);
                Point zoomCenter = this._ZoomAndPan.MatrixInv.Transform(vsZoomCenter);

                double newScale = Math.Min(vsWidth / contentWidth, vsHeight / contentHeight);
                this._ZoomAndPan.Scale = Math.Max(0.1, Math.Min(1.0, newScale));

                Point vsNextZoomCenter = this._ZoomAndPan.Matrix.Transform(zoomCenter);
                Point vsDelta = new Point(vsZoomCenter.X - vsNextZoomCenter.X, vsZoomCenter.Y - vsNextZoomCenter.Y);

                this._ZoomAndPan.StartX -= vsDelta.X;
                this._ZoomAndPan.StartY -= vsDelta.Y;

                if (0 != (int)(this._ZoomAndPan.Matrix.OffsetX - this._ZoomAndPanStartMatrix.OffsetX) ||
                    0 != (int)(this._ZoomAndPan.Matrix.OffsetX - this._ZoomAndPanStartMatrix.OffsetX))
                {
                    flowChart.History.AddCommand(new History.ZoomAndPanCommand(
                        "ZoomAndPan", this.ViewModel.Model, this._ZoomAndPanStartMatrix, this.ZoomAndPan.Matrix));
                }
            }
            flowChart.History.EndTransaction(false);
        }

        #endregion // Fitting.

        #region Drag & Drop Events

        private ModelBase BuidNodeGraphDragEventArgs(DragEventArgs args, out NodeGraphDragEventArgs eventArgs)
        {
            ModelBase model = this.FindModelUnderMouse(args.GetPosition(this), out Point viewSpacePos, out Point modelSpacePos, out ModelType modelType);

            eventArgs = null;

            if (null != model)
            {
                eventArgs = new NodeGraphDragEventArgs
                {
                    ViewSpaceMouseLocation = viewSpacePos,
                    ModelSpaceMouseLocation = modelSpacePos,
                    ModelType = modelType,
                    DragEventArgs = args
                };
            }

            return model;
        }

        private void FlowChartView_Drop(object sender, DragEventArgs args)
        {
            ModelBase model = this.BuidNodeGraphDragEventArgs(args, out NodeGraphDragEventArgs eventArgs);
            if (null != model)
            {
                NodeGraphManager.InvokeDrop(model, eventArgs);
            }
        }

        private void FlowChartView_DragOver(object sender, DragEventArgs args)
        {
            ModelBase model = this.BuidNodeGraphDragEventArgs(args, out NodeGraphDragEventArgs eventArgs);
            if (null != model)
            {
                NodeGraphManager.InvokeDragOver(model, eventArgs);
            }
        }

        private void FlowChartView_DragLeave(object sender, DragEventArgs args)
        {
            ModelBase model = this.BuidNodeGraphDragEventArgs(args, out NodeGraphDragEventArgs eventArgs);
            if (null != model)
            {
                NodeGraphManager.InvokeDragLeave(model, eventArgs);
            }
        }

        private void FlowChartView_DragEnter(object sender, DragEventArgs args)
        {
            ModelBase model = this.BuidNodeGraphDragEventArgs(args, out NodeGraphDragEventArgs eventArgs);
            if (null != model)
            {
                NodeGraphManager.InvokeDragEnter(model, eventArgs);
            }
        }

        #endregion // Drag & Drop Events
    }

    public class ZoomAndPan
    {
        #region Properties

        private double _ViewWidth;
        public double ViewWidth
        {
            get => this._ViewWidth;
            set
            {
                if (value != this._ViewWidth)
                {
                    this._ViewWidth = value;
                }
            }
        }

        private double _ViewHeight;
        public double ViewHeight
        {
            get => this._ViewHeight;
            set
            {
                if (value != this._ViewHeight)
                {
                    this._ViewHeight = value;
                }
            }
        }

        private double _StartX = 0.0;
        public double StartX
        {
            get => this._StartX;
            set
            {
                if (value != this._StartX)
                {
                    this._StartX = value;
                    this._UpdateTransform();
                }
            }
        }

        private double _StartY = 0.0;
        public double StartY
        {
            get => this._StartY;
            set
            {
                if (value != this._StartY)
                {
                    this._StartY = value;
                    this._UpdateTransform();
                }
            }
        }

        private double _Scale = 1.0;
        public double Scale
        {
            get => this._Scale;
            set
            {
                if (value != this._Scale)
                {
                    this._Scale = value;
                    this._UpdateTransform();
                }
            }
        }

        private Matrix _Matrix = Matrix.Identity;
        public Matrix Matrix
        {
            get => this._Matrix;
            set
            {
                if (value != this._Matrix)
                {
                    this._Matrix = value;
                    this._MatrixInv = value;
                    this._MatrixInv.Invert();
                }
            }
        }

        private Matrix _MatrixInv = Matrix.Identity;
        public Matrix MatrixInv => this._MatrixInv;


        #endregion // Properties

        #region Methdos

        private void _UpdateTransform()
        {
            Matrix newMatrix = Matrix.Identity;
            newMatrix.Scale(this._Scale, this._Scale);
            newMatrix.Translate(-this._StartX, -this._StartY);

            this.Matrix = newMatrix;

            UpdateTransform?.Invoke();
        }

        #endregion // Methods

        #region Events

        public delegate void UpdateTransformDelegate();
        public event UpdateTransformDelegate UpdateTransform;

        #endregion // Events
    }
}
