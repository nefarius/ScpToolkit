using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ScpControl;
using ScpControl.Profiler;
using ScpControl.ScpCore;
using Xceed.Wpf.Toolkit;
using Ds3Button = ScpControl.Profiler.Ds3Button;
using Ds4Button = ScpControl.Profiler.Ds4Button;

namespace ScpProfiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ScpProxy _proxy = new ScpProxy();
        private DsPadId _currentPad;
        private readonly DualShockProfileViewModel _vm = new DualShockProfileViewModel();

        public MainWindow()
        {
            InitializeComponent();

            ProfilesCollectionControl.NewItemTypes = new List<Type>() {typeof (DualShockProfile)};
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            _proxy.NativeFeedReceived += ProxyOnNativeFeedReceived;
            _proxy.Start();

            MainGrid.DataContext = _vm;

            _vm.Profiles = _proxy.GetProfiles().ToList();
        }

        private void ProxyOnNativeFeedReceived(object sender, ScpHidReport report)
        {
            if (_vm.CurrentProfile == null) return;

            if(report.PadId != _currentPad) return;

            _vm.CurrentProfile.Model = report.Model;
            _vm.CurrentProfile.MacAddress = string.Join(":",
                (from z in report.PadMacAddress.GetAddressBytes() select z.ToString("X2")).ToArray());
            _vm.CurrentProfile.PadId = report.PadId;

            _vm.CurrentProfile.Remap(report);

            switch (report.Model)
            {
                case DsModel.DS3:
                    _vm.CurrentProfile.Ps.CurrentValue = report[Ds3Button.Ps].Value;
                    _vm.CurrentProfile.Circle.CurrentValue = report[Ds3Button.Circle].Value;
                    _vm.CurrentProfile.Cross.CurrentValue = report[Ds3Button.Cross].Value;
                    _vm.CurrentProfile.Square.CurrentValue = report[Ds3Button.Square].Value;
                    _vm.CurrentProfile.Triangle.CurrentValue = report[Ds3Button.Triangle].Value;
                    _vm.CurrentProfile.Select.CurrentValue = report[Ds3Button.Select].Value;
                    _vm.CurrentProfile.Start.CurrentValue = report[Ds3Button.Start].Value;
                    _vm.CurrentProfile.LeftShoulder.CurrentValue = report[Ds3Button.L1].Value;
                    _vm.CurrentProfile.RightShoulder.CurrentValue = report[Ds3Button.R1].Value;
                    _vm.CurrentProfile.LeftTrigger.CurrentValue = report[Ds3Button.L2].Value;
                    _vm.CurrentProfile.RightTrigger.CurrentValue = report[Ds3Button.R2].Value;
                    _vm.CurrentProfile.LeftThumb.CurrentValue = report[Ds3Button.L3].Value;
                    _vm.CurrentProfile.RightThumb.CurrentValue = report[Ds3Button.R3].Value;
                    _vm.CurrentProfile.Up.CurrentValue = report[Ds3Button.Up].Value;
                    _vm.CurrentProfile.Right.CurrentValue = report[Ds3Button.Right].Value;
                    _vm.CurrentProfile.Down.CurrentValue = report[Ds3Button.Down].Value;
                    _vm.CurrentProfile.Left.CurrentValue = report[Ds3Button.Left].Value;
                    break;
                case DsModel.DS4:
                    _vm.CurrentProfile.Ps.CurrentValue = report[Ds4Button.Ps].Value;
                    _vm.CurrentProfile.Circle.CurrentValue = report[Ds4Button.Circle].Value;
                    _vm.CurrentProfile.Cross.CurrentValue = report[Ds4Button.Cross].Value;
                    _vm.CurrentProfile.Square.CurrentValue = report[Ds4Button.Square].Value;
                    _vm.CurrentProfile.Triangle.CurrentValue = report[Ds4Button.Triangle].Value;
                    _vm.CurrentProfile.Select.CurrentValue = report[Ds4Button.Share].Value;
                    _vm.CurrentProfile.Start.CurrentValue = report[Ds4Button.Options].Value;
                    _vm.CurrentProfile.LeftShoulder.CurrentValue = report[Ds4Button.L1].Value;
                    _vm.CurrentProfile.RightShoulder.CurrentValue = report[Ds4Button.R1].Value;
                    _vm.CurrentProfile.LeftTrigger.CurrentValue = report[Ds4Button.L2].Value;
                    _vm.CurrentProfile.RightTrigger.CurrentValue = report[Ds4Button.R2].Value;
                    _vm.CurrentProfile.LeftThumb.CurrentValue = report[Ds4Button.L3].Value;
                    _vm.CurrentProfile.RightThumb.CurrentValue = report[Ds4Button.R3].Value;
                    _vm.CurrentProfile.Up.CurrentValue = report[Ds4Button.Up].Value;
                    _vm.CurrentProfile.Right.CurrentValue = report[Ds4Button.Right].Value;
                    _vm.CurrentProfile.Down.CurrentValue = report[Ds4Button.Down].Value;
                    _vm.CurrentProfile.Left.CurrentValue = report[Ds4Button.Left].Value;
                    break;
            }
        }

        private void CurrentPad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentPad = (DsPadId)((ComboBox)sender).SelectedItem;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _proxy.SubmitProfile(_vm.CurrentProfile);
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            EditProfileChildWindow.Show();
        }

        private void ProfilesCollectionControl_OnItemAdded(object sender, ItemEventArgs e)
        {
            _proxy.SubmitProfile(e.Item as DualShockProfile);
        }

        private void ProfilesCollectionControl_OnItemDeleted(object sender, ItemEventArgs e)
        {
            _proxy.RemoveProfile(e.Item as DualShockProfile);
        }
    }
}
