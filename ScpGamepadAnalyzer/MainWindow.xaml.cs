using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using HidSharp;
using Ookii.Dialogs.Wpf;
using ScpControl.Usb.Gamepads;

namespace ScpGamepadAnalyzer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private fields

        private readonly SortedList<CaptureType, TaskDialog> _interpreterDiags =
            new SortedList<CaptureType, TaskDialog>();

        private UsbBlankGamepad _device;

        #endregion

        #region Ctor

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                // don't look! =D
                Close();
            };
        }

        #endregion

        #region Properties

        public HidDevice SelectedHidDevice { get; set; }

        #endregion

        #region Window events

        private void Window_Initialized(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Hold it right there! You don't need this tool if you have a DualShock controller! Seek help on the forums if your're not sure what to do next.",
                "Wait a sec'", MessageBoxButton.OK, MessageBoxImage.Exclamation);

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

        private void OpenDeviceButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedHidDevice == null)
            {
                new TaskDialog
                {
                    Buttons = { new TaskDialogButton(ButtonType.Ok) },
                    WindowTitle = "Hey!",
                    Content =
                        "Please select a device first :)",
                    MainIcon = TaskDialogIcon.Error
                }.ShowDialog(this);

                return;
            }

            var targetFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                string.Format("{0}_hid-report.dump.txt", SelectedHidDevice.ProductName));

            _device = new UsbBlankGamepad(SelectedHidDevice, SelectedHidDevice.DevicePath,
                targetFile);

            if (_device.Open(SelectedHidDevice.DevicePath) && _device.Start())
            {
                InterpretButton.IsEnabled = true;
                CloseButton.IsEnabled = true;

                new TaskDialog
                {
                    Buttons = {new TaskDialogButton(ButtonType.Ok)},
                    WindowTitle = "Well that worked!",
                    Content = "All fine, proceed with the next step now.",
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

        private void CloseDeviceButton_OnClick(object sender, RoutedEventArgs e)
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

            InterpretButton.IsEnabled = false;
            CloseButton.IsEnabled = false;

            new TaskDialog
            {
                Buttons = {new TaskDialogButton(ButtonType.Ok)},
                WindowTitle = "Yay!",
                Content = "Device free'd, have a nice day!",
                MainIcon = TaskDialogIcon.Information
            }.ShowDialog(this);
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion
    }
}