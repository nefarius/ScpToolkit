using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using AutoDependencyPropertyMarker;
using ScpControl.Shared.Core;
using ScpControl.Utilities;

namespace ScpProfiler
{
    /// <summary>
    ///     Interaction logic for ButtonMappingEntryControl.xaml
    /// </summary>
    public partial class AxisMappingEntryControl : UserControl
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

        public AxisMappingEntryControl()
        {
            ButtonProfile = new DsButtonProfile();

            InitializeComponent();

            TargetCommandComboBox.ItemsSource = ValidKeys;
        }

        #endregion

        #region Private event handlers

        private void TargetTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (EnumMetaData) ((ComboBox) sender).SelectedItem;

            if (selectedItem == null) return;

            ButtonProfile.MappingTarget.CommandType =
                ((CommandType) selectedItem.Value);

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