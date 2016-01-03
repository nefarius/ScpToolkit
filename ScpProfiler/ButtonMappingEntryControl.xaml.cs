using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
    public partial class ButtonMappingEntryControl : UserControl
    {
        #region Ctor

        public ButtonMappingEntryControl()
        {
            ButtonProfile = new DsButtonProfile();

            InitializeComponent();

            CurrentCommandTypeView = new CollectionView(AvailableCommandTypes);
            CurrentCommandTargetView = new CollectionView(AvailableKeys);

            CurrentCommandTypeView.MoveCurrentTo(AvailableCommandTypes.First());
            CurrentCommandTargetView.MoveCurrentTo(AvailableKeys.First());

            CurrentCommandTypeView.CurrentChanged += CurrentCommandTypeOnCurrentChanged;
            CurrentCommandTargetView.CurrentChanged += CurrentCommandTargetOnCurrentChanged;
        }

        #endregion

        private void CurrentCommandTargetOnCurrentChanged(object sender, EventArgs eventArgs)
        {
            switch (ButtonProfile.MappingTarget.CommandType)
            {
                case CommandType.GamepadButton:
                    ButtonProfile.MappingTarget.CommandTarget = (Ds3Button) CurrentCommandTargetView.CurrentItem;
                    break;
                case CommandType.Keystrokes:
                    ButtonProfile.MappingTarget.CommandTarget =
                        (VirtualKeyCode) CurrentCommandTargetView.CurrentItem;
                    break;
                case CommandType.MouseButtons:
                    ButtonProfile.MappingTarget.CommandTarget =
                        (MouseButton) CurrentCommandTargetView.CurrentItem;
                    break;
            }
        }

        private void CurrentCommandTypeOnCurrentChanged(object sender, EventArgs eventArgs)
        {
            ButtonProfile.MappingTarget.CommandType =
                (CommandType)
                    Enum.ToObject(typeof (CommandType), ((EnumMetaData) CurrentCommandTypeView.CurrentItem).Value);

            switch (ButtonProfile.MappingTarget.CommandType)
            {
                case CommandType.GamepadButton:
                    CurrentCommandTargetView = new CollectionView(AvailableGamepadButtons);
                    break;
                case CommandType.Keystrokes:
                    CurrentCommandTargetView = new CollectionView(AvailableKeys);
                    break;
                case CommandType.MouseButtons:
                    CurrentCommandTargetView = new CollectionView(AvailableMouseButtons);
                    break;
            }

            CurrentCommandTargetView.MoveCurrentToFirst();
            CurrentCommandTargetView.CurrentChanged += CurrentCommandTargetOnCurrentChanged;
        }

        /// <summary>
        ///     Tries to convert an object value to a <see cref="VirtualKeyCode" />.
        /// </summary>
        /// <param name="o">An object containing the <see cref="VirtualKeyCode" /> index.</param>
        /// <returns>The corresponding <see cref="VirtualKeyCode" />.</returns>
        private static VirtualKeyCode ToVirtualKeyCode(object o)
        {
            return o != null
                ? (VirtualKeyCode) Enum.Parse(typeof (VirtualKeyCode), o.ToString())
                : AvailableKeys.First();
        }

        #region Private control events

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DependencyPropertyDescriptor
                .FromProperty(ButtonProfileProperty, typeof (ButtonMappingEntryControl))
                .AddValueChanged(this, (s, args) =>
                {
                    if (ButtonProfile == null) return;

                    CurrentCommandTypeView = new CollectionView(AvailableCommandTypes);
                    CurrentCommandTypeView.MoveCurrentToPosition((int) ButtonProfile.MappingTarget.CommandType);

                    switch (ButtonProfile.MappingTarget.CommandType)
                    {
                        case CommandType.GamepadButton:
                            CurrentCommandTargetView = new CollectionView(AvailableGamepadButtons);
                            CurrentCommandTargetView.MoveCurrentTo(ButtonProfile.MappingTarget.CommandTarget);
                            break;
                        case CommandType.Keystrokes:
                            CurrentCommandTargetView = new CollectionView(AvailableKeys);
                            CurrentCommandTargetView.MoveCurrentTo(
                                AvailableKeys.FirstOrDefault(
                                    k => k == ToVirtualKeyCode(ButtonProfile.MappingTarget.CommandTarget)));
                            break;
                            // TODO: implement!
                        case CommandType.MouseButtons:
                            CurrentCommandTargetView = new CollectionView(AvailableMouseButtons);
                            break;
                    }

                    CurrentCommandTypeView.CurrentChanged += CurrentCommandTypeOnCurrentChanged;
                    CurrentCommandTargetView.CurrentChanged += CurrentCommandTargetOnCurrentChanged;
                });
        }

        #endregion

        #region Private static fields

        private static readonly IList<EnumMetaData> AvailableCommandTypes =
            EnumExtensions.GetValuesAndDescriptions(typeof (CommandType)).ToList();

        private static readonly IList<VirtualKeyCode> AvailableKeys = Enum.GetValues(typeof (VirtualKeyCode))
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
                        && k != VirtualKeyCode.HANGUL).ToList();

        private static readonly IList<Ds3Button> AvailableGamepadButtons = Ds3Button.Buttons.ToList();

        private static readonly IList<MouseButton> AvailableMouseButtons =
            Enum.GetValues(typeof (MouseButton)).Cast<MouseButton>().ToList();

        #endregion

        #region Dependency properties

        [AutoDependencyProperty]
        public ImageSource IconSource { get; set; }

        [AutoDependencyProperty]
        public string IconToolTip { get; set; }

        public DsButtonProfile ButtonProfile
        {
            get { return (DsButtonProfile) GetValue(ButtonProfileProperty); }
            set { SetValue(ButtonProfileProperty, value); }
        }

        public static readonly DependencyProperty ButtonProfileProperty =
            DependencyProperty.Register("ButtonProfile", typeof (DsButtonProfile),
                typeof (ButtonMappingEntryControl));

        [AutoDependencyProperty]
        public ICollectionView CurrentCommandTypeView { get; set; }

        [AutoDependencyProperty]
        public ICollectionView CurrentCommandTargetView { get; set; }

        #endregion
    }
}
