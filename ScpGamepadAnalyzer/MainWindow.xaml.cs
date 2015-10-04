using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using Ookii.Dialogs.Wpf;
using ScpControl.Driver;
using ScpControl.Usb;
using ScpControl.Usb.Gamepads;

namespace ScpGamepadAnalyzer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SortedList<CaptureType, TaskDialog> _interpreterDiags =
            new SortedList<CaptureType, TaskDialog>();

        private UsbBlankGamepad _device;
        private IntPtr _hwnd;
        private WdiUsbDevice _wdiCurrent;

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                // don't look! =D
            };
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(this).Handle;

            const string messageTitle = "Press button or engage axis";
            const string messageTemplate = "Please press and hold {0} now and click Capture";

            foreach (CaptureType type in Enum.GetValues(typeof (CaptureType)))
            {
                if (type == CaptureType.Default)
                    continue;

                var diag = new TaskDialog
                {
                    Buttons =
                    {
                        new TaskDialogButton("Capture"),
                        new TaskDialogButton("I don't have this button/axis, skip it")
                    },
                    WindowTitle = messageTitle,
                    Content = string.Format(messageTemplate, type),
                    MainIcon = TaskDialogIcon.Information
                };

                _interpreterDiags.Add(type, diag);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_device != null)
            {
                _device.Stop();
                _device.Close();
            }
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedDevice = DevicesComboBox.SelectedItem as WdiUsbDevice;

            if (selectedDevice == null)
                return;

            var msgBox = new TaskDialog
            {
                Buttons = {new TaskDialogButton(ButtonType.Yes), new TaskDialogButton(ButtonType.No)},
                WindowTitle = "I'm about to change the device driver!",
                Content = string.Format("You selected the device {1}{0}{0}Want me to change the driver now?",
                    Environment.NewLine,
                    selectedDevice),
                MainIcon = TaskDialogIcon.Information
            };

            var msgResult = msgBox.ShowDialog(this);

            var tmpPath = Path.Combine(Path.GetTempPath(), "ScpGamepadAnalyzer");

            if (msgResult.ButtonType != ButtonType.Yes) return;

            var result = WdiWrapper.Instance.InstallLibusbKDriver(selectedDevice.HardwareId,
                UsbBlankGamepad.DeviceClassGuid, tmpPath,
                string.Format("{0}.inf", selectedDevice.Description),
                _hwnd, true);

            if (result == WdiErrorCode.WDI_SUCCESS)
            {
                _wdiCurrent = selectedDevice;
                OpenDeviceButton.IsEnabled = true;

                new TaskDialog
                {
                    Buttons = {new TaskDialogButton(ButtonType.Ok)},
                    WindowTitle = "Yay!",
                    Content = "Driver changed successfully, proceed with the next step now.",
                    MainIcon = TaskDialogIcon.Information
                }.ShowDialog(this);
            }
            else
            {
                new TaskDialog
                {
                    Buttons = {new TaskDialogButton(ButtonType.Ok)},
                    WindowTitle = "Ohnoes!",
                    Content =
                        "It didn't work! What a shame :( Please reboot your machine, cross your fingers and try again.",
                    MainIcon = TaskDialogIcon.Error
                }.ShowDialog(this);
            }
        }

        private void OpenDeviceButton_OnClick(object sender, RoutedEventArgs e)
        {
            _device = new UsbBlankGamepad(_wdiCurrent.HardwareId,
                string.Format("{0}_hid-report.dump.txt", _wdiCurrent.Description));

            if (_device.Open() && _device.Start())
            {
                InterpretButton.IsEnabled = true;
                RevertButton.IsEnabled = true;

                new TaskDialog
                {
                    Buttons = {new TaskDialogButton(ButtonType.Ok)},
                    WindowTitle = "Yay!",
                    Content = "Device opened successfully, proceed with the next step now.",
                    MainIcon = TaskDialogIcon.Information
                }.ShowDialog(this);
            }
            else
            {
                new TaskDialog
                {
                    Buttons = {new TaskDialogButton(ButtonType.Ok)},
                    WindowTitle = "Ohnoes!",
                    Content =
                        "It didn't work! What a shame :( Please reboot your machine, cross your fingers and try again.",
                    MainIcon = TaskDialogIcon.Error
                }.ShowDialog(this);
            }
        }

        private void InterpretButton_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (
                var dialog in
                    _interpreterDiags.Where(dialog => dialog.Value.ShowDialog(this).Text.Equals("Capture")))
            {
                _device.Capture = dialog.Key;
            }
        }

        private void RevertDriverButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.Stop();
                _device.Close();
            }
            else
            {
                return;
            }

            if (Devcon.Remove(UsbBlankGamepad.DeviceClassGuid, _device.Path, null))
            {
                new TaskDialog
                {
                    Buttons = {new TaskDialogButton(ButtonType.Ok)},
                    WindowTitle = "Yay!",
                    Content = "Device driver reverted successfully, have a nice day!",
                    MainIcon = TaskDialogIcon.Information
                }.ShowDialog(this);
            }
            else
            {
                new TaskDialog
                {
                    Buttons = {new TaskDialogButton(ButtonType.Ok)},
                    WindowTitle = "Ohnoes!",
                    Content =
                        "It didn't work! What a shame :( Please manually revert the driver in Windows Device Manager.",
                    MainIcon = TaskDialogIcon.Error
                }.ShowDialog(this);
            }
        }
    }
}