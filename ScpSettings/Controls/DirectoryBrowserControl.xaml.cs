using System.Windows;
using System.Windows.Controls;
using AutoDependencyPropertyMarker;
using Ookii.Dialogs.Wpf;

namespace ScpSettings.Controls
{
    /// <summary>
    ///     Interaction logic for FileBrowserControl.xaml
    /// </summary>
    public partial class DirectoryBrowserControl : UserControl
    {
        public DirectoryBrowserControl()
        {
            InitializeComponent();

            MainGrid.DataContext = this;
        }

        [AutoDependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public string DirectoryPath { get; set; }

        [AutoDependencyProperty(Options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
        public string Description { get; set; }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new VistaFolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                Description = this.Description,
                UseDescriptionForTitle = true
            };

            if (folderBrowser.ShowDialog() != true) return;

            DirectoryPath = folderBrowser.SelectedPath;
        }
    }
}