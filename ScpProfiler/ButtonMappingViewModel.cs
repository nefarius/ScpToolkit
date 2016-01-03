using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using WindowsInput;
using WindowsInput.Native;
using AutoDependencyPropertyMarker;
using PropertyChanged;
using ScpControl.Shared.Core;
using ScpControl.Utilities;
using CommandType = ScpControl.Shared.Core.CommandType;

namespace ScpProfiler
{
    [ImplementPropertyChanged]
    public class ButtonMappingViewModel : DependencyObject
    {
        private static readonly IList<EnumMetaData> AvailableCommandTypes =
            EnumExtensions.GetValuesAndDescriptions(typeof (CommandType)).ToList();

        private static readonly IList<VirtualKeyCode> AvailableKeys = Enum.GetValues(typeof(VirtualKeyCode))
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

        private static readonly IList<MouseButton> AvailableMouseButtons = Enum.GetValues(typeof (MouseButton)).Cast<MouseButton>().ToList();

        public ICollectionView CurrentCommandTypeView { get; set; }

        public ICollectionView CurrentCommandTargetView { get; set; }

        [AutoDependencyProperty]
        public DsButtonProfile CurrentButtonProfile { get; set; }

        public ButtonMappingViewModel() : this(null) { }

        public ButtonMappingViewModel(DsButtonProfile profile)
        {
            CurrentButtonProfile = profile;

            CurrentCommandTypeView = new CollectionView(AvailableCommandTypes);
            CurrentCommandTargetView = new CollectionView(AvailableKeys);

            CurrentCommandTypeView.MoveCurrentTo(AvailableCommandTypes.First());
            CurrentCommandTargetView.MoveCurrentTo(AvailableKeys.First());

            if (CurrentButtonProfile != null)
            {
                CurrentCommandTypeView.MoveCurrentTo(profile.MappingTarget.CommandType);
                CurrentCommandTargetView.MoveCurrentTo(profile.MappingTarget.CommandTarget);
            }
            else
            {
                CurrentButtonProfile = new DsButtonProfile();
            }

            CurrentCommandTypeView.CurrentChanged += CurrentCommandTypeOnCurrentChanged;
            CurrentCommandTargetView.CurrentChanged += CurrentCommandTargetOnCurrentChanged;
        }

        private void CurrentCommandTargetOnCurrentChanged(object sender, EventArgs eventArgs)
        {
            if (CurrentButtonProfile == null) return;

            switch (CurrentButtonProfile.MappingTarget.CommandType)
            {
                case CommandType.GamepadButton:
                    CurrentButtonProfile.MappingTarget.CommandTarget = (Ds3Button) CurrentCommandTargetView.CurrentItem;
                    break;
                case CommandType.Keystrokes:
                    CurrentButtonProfile.MappingTarget.CommandTarget =
                        (VirtualKeyCode) CurrentCommandTargetView.CurrentItem;
                    break;
                case CommandType.MouseButtons:
                    CurrentButtonProfile.MappingTarget.CommandTarget =
                        (MouseButton) CurrentCommandTargetView.CurrentItem;
                    break;
            }
        }

        private void CurrentCommandTypeOnCurrentChanged(object sender, EventArgs eventArgs)
        {
            if (CurrentButtonProfile == null) return;

            CurrentButtonProfile.MappingTarget.CommandType =
                (CommandType)
                    Enum.ToObject(typeof (CommandType), ((EnumMetaData) CurrentCommandTypeView.CurrentItem).Value);

            switch (CurrentButtonProfile.MappingTarget.CommandType)
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
        }
    }
}
