using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;

namespace ScpSettings.Controls
{
    /// <summary>
    ///     Interaction logic for FileBrowserControl.xaml
    /// </summary>
    public partial class FileBrowserControl : UserControl, INotifyPropertyChanged
    {
        public FileBrowserControl()
        {
            InitializeComponent();
        }

        public string FilePath
        {
            get { return (string) GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnFilePathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var myUserControl = sender as FileBrowserControl;
            if (myUserControl != null)
            {
                myUserControl.FilePathTextBox.Text = e.NewValue as string;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var fileBrowser = new VistaOpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = Properties.Resources.SupportedAudioFilesFilter
            };

            if (fileBrowser.ShowDialog() != true) return;

            FilePath = fileBrowser.FileName;
            OnPropertyChanged();
        }

        private void OnPropertyChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("FilePath"));
        }

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register
                (
                    "FilePath",
                    typeof (string),
                    typeof (FileBrowserControl),
                    new FrameworkPropertyMetadata(OnFilePathChanged)
                );
    }
}
