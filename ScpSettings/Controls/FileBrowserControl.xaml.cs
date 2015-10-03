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
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register
                (
                    "FilePath",
                    typeof(string),
                    typeof(FileBrowserControl),
                    new FrameworkPropertyMetadata(OnFilePathChanged)
                );

        public static readonly DependencyProperty IsSoundEnabledProperty =
            DependencyProperty.Register
                (
                    "IsSoundEnabled",
                    typeof(bool),
                    typeof(FileBrowserControl),
                    new FrameworkPropertyMetadata(OnIsSoundEnabledChanged)
                );

        public FileBrowserControl()
        {
            InitializeComponent();

            IsSoundEnabledCheckBox.DataContext = this;
        }

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public bool IsSoundEnabled
        {
            get { return (bool)GetValue(IsSoundEnabledProperty); }
            set { SetValue(IsSoundEnabledProperty, value); }
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

        private static void OnIsSoundEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var myUserControl = sender as FileBrowserControl;
            if (myUserControl != null)
            {
                myUserControl.IsSoundEnabledCheckBox.IsChecked = e.NewValue as bool?;
            }
        }

        private void OnPropertyChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("FilePath"));
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
    }
}