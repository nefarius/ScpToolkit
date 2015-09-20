using System;
using System.Windows;
using ScpDriverInstaller.View_Models;

namespace ScpDriverInstaller
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InstallationOptionsViewModel _viewModel = new InstallationOptionsViewModel();

        public MainWindow()
        {
            InitializeComponent();

            _viewModel.InstallButtonClicked += ViewModelOnInstallButtonClicked;
            _viewModel.UninstallButtonClicked += ViewModelOnUninstallButtonClicked;
            _viewModel.ExitButtonClicked += ViewModelOnExitButtonClicked;

            InstallGrid.DataContext = _viewModel;
        }

        private void ViewModelOnExitButtonClicked(object sender, EventArgs eventArgs)
        {
            Close();
        }

        private void ViewModelOnUninstallButtonClicked(object sender, EventArgs eventArgs)
        {
            
        }

        private void ViewModelOnInstallButtonClicked(object sender, EventArgs eventArgs)
        {
            
        }
    }
}
