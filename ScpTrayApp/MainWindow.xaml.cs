using System;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Plugin;
using log4net.Repository.Hierarchy;
using Mantin.Controls.Wpf.Notification;

[assembly: XmlConfigurator]

namespace ScpTrayApp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IAppender
    {
        public MainWindow()
        {
            InitializeComponent();

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
    }
}