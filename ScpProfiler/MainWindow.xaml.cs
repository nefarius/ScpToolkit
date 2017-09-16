using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HidReport.Contract.Enums;
using ScpControl;
using ScpControl.Shared.Core;
using Xceed.Wpf.Toolkit;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;


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

            IEnumerable<DualShockProfile> list = null;
            try
            {
                list = _proxy.GetProfiles();
            }
            catch (Exception err)
            {
                MessageBox.Show($"Can't load profiles. Error {err.Message}");
            }
            if (list == null)
            {
                list = new List<DualShockProfile>();
            }
            
           _vm.Profiles = list.ToList();
        }

        private void ProxyOnNativeFeedReceived(object sender, ScpHidReport scpHidReport)
        {
            if (_vm.CurrentProfile == null) return;

            if(scpHidReport.PadId != _currentPad) return;

            _vm.CurrentProfile.Model = scpHidReport.Model;
            _vm.CurrentProfile.MacAddress = string.Join(":",
                (from z in scpHidReport.PadMacAddress.GetAddressBytes() select z.ToString("X2")).ToArray());
            _vm.CurrentProfile.PadId = scpHidReport.PadId;

            _vm.CurrentProfile.Remap(scpHidReport);

            var report = scpHidReport.HidReport;
            switch (scpHidReport.Model)
            {
                case DsModel.DS3:
                    _vm.CurrentProfile.Ps.CurrentValue = report[ButtonsEnum.Ps].Value;
                    _vm.CurrentProfile.Circle.CurrentValue = report[ButtonsEnum.Circle].Value;
                    _vm.CurrentProfile.Cross.CurrentValue = report[ButtonsEnum.Cross].Value;
                    _vm.CurrentProfile.Square.CurrentValue = report[ButtonsEnum.Square].Value;
                    _vm.CurrentProfile.Triangle.CurrentValue = report[ButtonsEnum.Triangle].Value;
                    _vm.CurrentProfile.Select.CurrentValue = report[ButtonsEnum.Select].Value;
                    _vm.CurrentProfile.Start.CurrentValue = report[ButtonsEnum.Start].Value;
                    _vm.CurrentProfile.LeftShoulder.CurrentValue = report[ButtonsEnum.L1].Value;
                    _vm.CurrentProfile.RightShoulder.CurrentValue = report[ButtonsEnum.R1].Value;
                    _vm.CurrentProfile.LeftTrigger.CurrentValue = report[ButtonsEnum.L2].Value;
                    _vm.CurrentProfile.RightTrigger.CurrentValue = report[ButtonsEnum.R2].Value;
                    _vm.CurrentProfile.LeftThumb.CurrentValue = report[ButtonsEnum.L3].Value;
                    _vm.CurrentProfile.RightThumb.CurrentValue = report[ButtonsEnum.R3].Value;
                    _vm.CurrentProfile.Up.CurrentValue = report[ButtonsEnum.Up].Value;
                    _vm.CurrentProfile.Right.CurrentValue = report[ButtonsEnum.Right].Value;
                    _vm.CurrentProfile.Down.CurrentValue = report[ButtonsEnum.Down].Value;
                    _vm.CurrentProfile.Left.CurrentValue = report[ButtonsEnum.Left].Value;
                    break;
                case DsModel.DS4:
                    _vm.CurrentProfile.Ps.CurrentValue = report[ButtonsEnum.Ps].Value;
                    _vm.CurrentProfile.Circle.CurrentValue = report[ButtonsEnum.Circle].Value;
                    _vm.CurrentProfile.Cross.CurrentValue = report[ButtonsEnum.Cross].Value;
                    _vm.CurrentProfile.Square.CurrentValue = report[ButtonsEnum.Square].Value;
                    _vm.CurrentProfile.Triangle.CurrentValue = report[ButtonsEnum.Triangle].Value;
                    _vm.CurrentProfile.Select.CurrentValue = report[ButtonsEnum.Share].Value;
                    _vm.CurrentProfile.Start.CurrentValue = report[ButtonsEnum.Options].Value;
                    _vm.CurrentProfile.LeftShoulder.CurrentValue = report[ButtonsEnum.L1].Value;
                    _vm.CurrentProfile.RightShoulder.CurrentValue = report[ButtonsEnum.R1].Value;
                    _vm.CurrentProfile.LeftTrigger.CurrentValue = report[ButtonsEnum.L2].Value;
                    _vm.CurrentProfile.RightTrigger.CurrentValue = report[ButtonsEnum.R2].Value;
                    _vm.CurrentProfile.LeftThumb.CurrentValue = report[ButtonsEnum.L3].Value;
                    _vm.CurrentProfile.RightThumb.CurrentValue = report[ButtonsEnum.R3].Value;
                    _vm.CurrentProfile.Up.CurrentValue = report[ButtonsEnum.Up].Value;
                    _vm.CurrentProfile.Right.CurrentValue = report[ButtonsEnum.Right].Value;
                    _vm.CurrentProfile.Down.CurrentValue = report[ButtonsEnum.Down].Value;
                    _vm.CurrentProfile.Left.CurrentValue = report[ButtonsEnum.Left].Value;
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
