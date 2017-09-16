using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ookii.Dialogs.Wpf;
using ColorPickerControls.Dialogs;
using Bindables;

namespace ScpSettings.Controls
{
    /// <summary>
    ///     Interaction logic for ColorChooserControl.xaml
    /// </summary>
    public partial class ColorChooserControl : UserControl
    {
        public ColorChooserControl()
        {
            InitializeComponent();

            MainGrid.DataContext = this;
        }

        [DependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public uint Ds4BatteryColor { get; set; }

        public string ColorText
        {
            get
            {
                return lblText.Content.ToString();
            }
            set
            {
                lblText.Content = value;
            }
        }

        private uint BrushToUInt(Brush brush)
        {
            Color color = ((SolidColorBrush) brush).Color;
            return (uint) System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B).ToArgb();
        }

        private Brush UIntToBrush(uint integer)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb((int)integer);
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // https://www.codeproject.com/Articles/131708/WPF-Color-Picker-Construction-Kit
            ColorPickerStandardDialog colorDialog = new ColorPickerStandardDialog();
            colorDialog.InitialColor = ((SolidColorBrush)this.lblColor.Background).Color;

            if (colorDialog.ShowDialog().GetValueOrDefault())
            {
                lblColor.Background = new SolidColorBrush(colorDialog.SelectedColor);
                Ds4BatteryColor = BrushToUInt(lblColor.Background);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lblColor.Background = UIntToBrush(Ds4BatteryColor);
        }
    }
}