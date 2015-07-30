using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ScpCustomHidProfiler
{
    /// <summary>
    ///     Interaction logic for ButtonMappingEntryControl.xaml
    /// </summary>
    public partial class ButtonMappingEntryControl : UserControl
    {
        public ButtonMappingEntryControl()
        {
            InitializeComponent();

            TargetCommandComboBox.ItemsSource = Enum.GetValues(typeof(Key)).Cast<Key>();
        }

        public Uri ImageSource
        {
            get { return (Uri) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        private static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var myUserControl = sender as ButtonMappingEntryControl;
            if (myUserControl != null)
            {
                myUserControl.ButtonImage.Source = new BitmapImage((Uri) e.NewValue);
            }
        }

        private void TargetTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = ((ComboBox) sender).SelectedItem as CommandTypes?;

            if (type == null || TargetCommandComboBox == null)
                return;

            switch (type)
            {
                case CommandTypes.GamepadButton:
                    break;
                case CommandTypes.Keystrokes:
                    TargetCommandComboBox.ItemsSource = Enum.GetValues(typeof (Key)).Cast<Key>();
                    break;
                case CommandTypes.MouseAxis:
                    break;
                case CommandTypes.MouseButtons:
                    break;
            }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register
                (
                    "ImageSource",
                    typeof (Uri),
                    typeof (ButtonMappingEntryControl),
                    new FrameworkPropertyMetadata(OnImageSourceChanged)
                );
    }
}