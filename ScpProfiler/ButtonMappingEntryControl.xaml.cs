using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using AutoDependencyPropertyMarker;
using ScpControl.Profiler;
using ScpControl.Utilities;

namespace ScpProfiler
{
    /// <summary>
    ///     Interaction logic for ButtonMappingEntryControl.xaml
    /// </summary>
    public partial class ButtonMappingEntryControl : UserControl
    {
        #region Private fields

        private static readonly IEnumerable<VirtualKeyCode> ValidKeys = Enum.GetValues(typeof (VirtualKeyCode))
            .Cast<VirtualKeyCode>()
            .Where(k => k != VirtualKeyCode.MODECHANGE
                        && k != VirtualKeyCode.PACKET
                        && k != VirtualKeyCode.NONAME
                        && k != VirtualKeyCode.LBUTTON
                        && k != VirtualKeyCode.RBUTTON
                        && k != VirtualKeyCode.MBUTTON
                        && k != VirtualKeyCode.XBUTTON1
                        && k != VirtualKeyCode.XBUTTON2
                        && k != VirtualKeyCode.HANGEUL
                        && k != VirtualKeyCode.HANGUL);

        #endregion

        #region Ctor

        public ButtonMappingEntryControl()
        {
            ButtonProfile = new DsButtonProfile();

            InitializeComponent();

            TargetCommandComboBox.ItemsSource = ValidKeys;
        }

        #endregion

        #region Private event handlers

        private void TargetTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonProfile.MappingTarget.CommandType =
                ((CommandType) ((EnumMetaData) ((ComboBox) sender).SelectedItem).Value);

            if (TargetCommandComboBox == null)
                return;

            switch (ButtonProfile.MappingTarget.CommandType)
            {
                case CommandType.GamepadButton:
                    TargetCommandComboBox.SelectedItem = ButtonProfile.MappingTarget.CommandTarget;
                    TargetCommandComboBox.ItemsSource = Ds3Button.Buttons;
                    break;
                case CommandType.Keystrokes:
                    TargetCommandComboBox.ItemsSource = ValidKeys;
                    break;
                case CommandType.MouseAxis:
                    break;
                case CommandType.MouseButtons:
                    TargetCommandComboBox.ItemsSource =
                        Enum.GetValues(typeof (MouseButton)).Cast<MouseButton>();
                    break;
            }
        }

        private void TargetCommandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonProfile.MappingTarget.CommandTarget = ((ComboBox) sender).SelectedItem;
        }

        #endregion

        #region Dependency properties

        [AutoDependencyProperty]
        public ImageSource IconSource { get; set; }

        [AutoDependencyProperty]
        public string IconToolTip { get; set; }

        [AutoDependencyProperty]
        public DsButtonProfile ButtonProfile { get; set; }

        #endregion
    }
}