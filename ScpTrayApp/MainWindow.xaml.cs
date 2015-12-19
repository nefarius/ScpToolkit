using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Plugin;
using log4net.Repository.Hierarchy;
using Libarius.System;
using Mantin.Controls.Wpf.Notification;

[assembly: XmlConfigurator]

namespace ScpTrayApp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IAppender
    {
        private static readonly LimitInstance ThisInstance = new LimitInstance(Assembly.GetExecutingAssembly().FullName);

        public MainWindow()
        {
            InitializeComponent();

            if (ThisInstance.IsOnlyInstance)
            {
                // Configure remoting. This loads the TCP channel as specified in the .config file.
                RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);

                // Publish the remote logging server. This is done using the log4net plugin.
                LogManager.GetRepository().PluginMap.Add(new RemoteLoggingServerPlugin("Log4netRemotingServerService"));

                //Get the logger repository hierarchy.  
                var repository = LogManager.GetRepository() as Hierarchy;

                //and add the appender to the root level  
                //of the logging hierarchy  
                repository.Root.AddAppender(this);

                //configure the logging at the root.  
                repository.Root.Level = Level.All;

                //mark repository as configured and  
                //notify that is has changed.  
                repository.Configured = true;
                repository.RaiseConfigurationChanged(EventArgs.Empty);
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            if (loggingEvent.Level == Level.Info)
            {
                ShowPopup("ScpToolkit Information", loggingEvent.RenderedMessage, NotificationType.Information);
            }

            if (loggingEvent.Level == Level.Warn)
            {
                ShowPopup("ScpToolkit Warning", loggingEvent.RenderedMessage, NotificationType.Warning);
            }

            if (loggingEvent.Level == Level.Error || loggingEvent.Level == Level.Fatal)
            {
                ShowPopup("ScpToolkit Error", loggingEvent.RenderedMessage, NotificationType.Error);
            }
        }

        private void ShowPopup(string title, string message, NotificationType type)
        {
            this.InvokeIfRequired(() =>
            {
                var popup = new ToastPopUp(title, message, type)
                {
                    Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1B, 0x1B, 0x1B)),
                    FontColor = Brushes.Bisque,
                    FontFamily = FontFamily
                };

                popup.Show();
            },
                DispatcherPriority.Normal);
        }

        private void MenuItemExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Called when Window was loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>http://stackoverflow.com/a/551847/490629</remarks>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        #region Window styles

        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        #endregion
    }
}
