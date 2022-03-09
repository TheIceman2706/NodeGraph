using NodeGraph.Controls;
using NodeGraph.Converters;
using NodeGraph.Model;
using PropertyTools.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NodeGraph.View
{
    [TemplatePart(Name = "PART_Header", Type = typeof(EditableTextBlock))]
    public class NodePropertyPortView : NodePortView
    {
        #region Fields

        private EditableTextBlock _Part_Header;
        private readonly DispatcherTimer _ClickTimer = new DispatcherTimer();
        private int _ClickCount = 0;

        #endregion // Fields

        #region Properites

        public Visibility PropertyEditorVisibility
        {
            get => (Visibility)this.GetValue(PropertyEditorVisibilityProperty);
            set => this.SetValue(PropertyEditorVisibilityProperty, value);
        }
        public static readonly DependencyProperty PropertyEditorVisibilityProperty =
            DependencyProperty.Register("PropertyEditorVisibility", typeof(Visibility), typeof(NodePropertyPortView), new PropertyMetadata(Visibility.Hidden));

        public FrameworkElement PropertyEditor
        {
            get => (FrameworkElement)this.GetValue(PropertyEditorProperty);
            set => this.SetValue(PropertyEditorProperty, value);
        }
        public static readonly DependencyProperty PropertyEditorProperty =
            DependencyProperty.Register("PropertyEditor", typeof(FrameworkElement), typeof(NodePropertyPortView), new PropertyMetadata(null));

        #endregion // Properties

        #region Constructor

        public NodePropertyPortView(bool isInput) : base(isInput)
        {
            Loaded += this.NodePropertyPortView_Loaded;
        }

        #endregion // Constructor

        #region Template Events

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._Part_Header = this.Template.FindName("PART_Header", this) as EditableTextBlock;
            if (null != this._Part_Header)
            {
                this._Part_Header.MouseDown += this._Part_Header_MouseDown;
                ;
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

        #region Events

        private void NodePropertyPortView_Loaded(object sender, RoutedEventArgs e)
        {
            this.CreatePropertyEditor();
            this.SynchronizeProperties();

            this._ClickTimer.Interval = TimeSpan.FromMilliseconds(300);
            this._ClickTimer.Tick += this._ClickTimer_Tick;
            ;
        }

        private void _ClickTimer_Tick(object sender, EventArgs e)
        {
            this._ClickCount = 0;
            this._ClickTimer.Stop();
        }

        protected override void SynchronizeProperties()
        {
            base.SynchronizeProperties();

            if (this.IsInput)
            {
                this.PropertyEditorVisibility = this.IsFilledPort ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #endregion // Events

        #region Property Editors

        private void CreatePropertyEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;
            if (port.HasEditor)
            {
                Type type = port.ValueType;

                if (typeof(bool) == type)
                {
                    this.PropertyEditor = this.CreateBoolEditor();
                }
                else if (typeof(string) == type)
                {
                    this.PropertyEditor = this.CreateStringEditor();
                }
                else if (typeof(byte) == type)
                {
                    this.PropertyEditor = this.CreateByteEditor();
                }
                else if (typeof(short) == type)
                {
                    this.PropertyEditor = this.CreateShortEditor();
                }
                else if (typeof(int) == type)
                {
                    this.PropertyEditor = this.CreateIntEditor();
                }
                else if (typeof(long) == type)
                {
                    this.PropertyEditor = this.CreateLongEditor();
                }
                else if (typeof(float) == type)
                {
                    this.PropertyEditor = this.CreateFloatEditor();
                }
                else if (typeof(double) == type)
                {
                    this.PropertyEditor = this.CreateDoubleEditor();
                }
                else if (type.IsEnum)
                {
                    this.PropertyEditor = this.CreateEnumEditor();
                }
                else if (typeof(Color) == type)
                {
                    this.PropertyEditor = this.CreateColorEditor();
                }

                if (null != this.PropertyEditor)
                {
                    this.PropertyEditorVisibility = Visibility.Visible;
                }
            }
        }

        public FrameworkElement CreateEnumEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            Array array = Enum.GetValues(port.ValueType);
            int selectedIndex = -1;
            for (int i = 0; i < array.Length; ++i)
            {
                if (port.Value.ToString() == array.GetValue(i).ToString())
                {
                    selectedIndex = i;
                    break;
                }
            }

            ComboBox comboBox = new ComboBox();
            comboBox.SelectionChanged += this.EnumComboBox_SelectionChanged;
            comboBox.ItemsSource = array;
            comboBox.SelectedIndex = selectedIndex;
            return comboBox;
        }

        private void EnumComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            ComboBox comboBox = this.PropertyEditor as ComboBox;
            if (null != comboBox)
            {
                port.Value = comboBox.SelectedItem;
            }
        }

        public FrameworkElement CreateBoolEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            CheckBox textBox = new CheckBox
            {
                IsChecked = (bool)port.Value
            };
            textBox.SetBinding(CheckBox.IsCheckedProperty, this.CreateBinding(port, "Value", null));
            return textBox;
        }

        public FrameworkElement CreateStringEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            TextBoxEx textBox = new TextBoxEx
            {
                Text = (port.Value ?? "").ToString()
            };
            textBox.SetBinding(TextBox.TextProperty, this.CreateBinding(port, "Value", null));
            return textBox;
        }

        public FrameworkElement CreateByteEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            NumericTextBox textBox = new NumericTextBox
            {
                IsInteger = true,
                Text = port.Value.ToString()
            };
            textBox.SetBinding(TextBox.TextProperty, this.CreateBinding(port, "Value", new ByteToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateShortEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            NumericTextBox textBox = new NumericTextBox
            {
                IsInteger = true,
                Text = port.Value.ToString()
            };
            textBox.SetBinding(TextBox.TextProperty, this.CreateBinding(port, "Value", new ShortToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateIntEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            NumericTextBox textBox = new NumericTextBox
            {
                IsInteger = true,
                Text = port.Value.ToString()
            };
            textBox.SetBinding(TextBox.TextProperty, this.CreateBinding(port, "Value", new IntToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateLongEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            NumericTextBox textBox = new NumericTextBox
            {
                IsInteger = true,
                Text = port.Value.ToString()
            };
            textBox.SetBinding(TextBox.TextProperty, this.CreateBinding(port, "Value", new LongToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateFloatEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            NumericTextBox textBox = new NumericTextBox
            {
                IsInteger = false,
                Text = port.Value.ToString()
            };
            textBox.SetBinding(TextBox.TextProperty, this.CreateBinding(port, "Value", new FloatToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateDoubleEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            NumericTextBox textBox = new NumericTextBox
            {
                IsInteger = false,
                Text = port.Value.ToString()
            };
            textBox.SetBinding(TextBox.TextProperty, this.CreateBinding(port, "Value", new DoubleToStringConverter()));
            return textBox;
        }

        public FrameworkElement CreateColorEditor()
        {
            NodePropertyPort port = this.ViewModel.Model as NodePropertyPort;

            ColorPicker picker = new ColorPicker
            {
                SelectedColor = (Color)port.Value
            };
            picker.SetBinding(ColorPicker.SelectedColorProperty, this.CreateBinding(port, "Value", null));
            return picker;
        }

        public Binding CreateBinding(NodePropertyPort port, string propertyName, IValueConverter converter)
        {
            Binding binding = new Binding(propertyName)
            {
                Source = port,
                Mode = BindingMode.TwoWay,
                Converter = converter,
                UpdateSourceTrigger = UpdateSourceTrigger.Default,
                ValidatesOnDataErrors = true,
                ValidatesOnExceptions = true
            };

            return binding;
        }

        #endregion // Property Editors.
    }
}
