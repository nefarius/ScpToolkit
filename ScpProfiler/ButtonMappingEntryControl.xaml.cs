using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using ScpControl.Utilities;
using ComboBox = System.Windows.Controls.ComboBox;
using UserControl = System.Windows.Controls.UserControl;

namespace ScpProfiler
{
    /// <summary>
    ///     Interaction logic for ButtonMappingEntryControl.xaml
    /// </summary>
    public partial class ButtonMappingEntryControl : UserControl
    {
        public ButtonMappingEntryControl()
        {
            InitializeComponent();

            TargetCommandComboBox.ItemsSource = ValidKeys;
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
                    TargetCommandComboBox.ItemsSource = ValidKeys;
                    break;
                case CommandTypes.MouseAxis:
                    break;
                case CommandTypes.MouseButtons:
                    TargetCommandComboBox.ItemsSource =
                        Enum.GetValues(typeof (KbmPost.MouseButtons)).Cast<KbmPost.MouseButtons>();
                    break;
            }
        }

        private static readonly IEnumerable<Keys> ValidKeys = Enum.GetValues(typeof (Keys))
            .Cast<Keys>()
            .Where(k => k != Keys.None 
                && k != Keys.KeyCode 
                && k != Keys.Modifiers
                && k != Keys.Packet
                && k != Keys.NoName
                && k != Keys.LButton
                && k != Keys.RButton
                && k != Keys.MButton
                && k != Keys.XButton1
                && k != Keys.XButton2
                && k != Keys.HanguelMode
                && k != Keys.IMEAceept);

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