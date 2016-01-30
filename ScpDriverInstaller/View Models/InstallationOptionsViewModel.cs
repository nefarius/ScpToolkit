using System;
using System.Windows.Input;
using PropertyChanged;
using ScpControl.Utilities;

namespace ScpDriverInstaller.View_Models
{
    public delegate void InstallButtonClickedEventHandler(object sender, EventArgs e);
    public delegate void UninstallButtonClickedEventHandler(object sender, EventArgs e);
    public delegate void ExitButtonClickedEventHandler(object sender, EventArgs e);

    [ImplementPropertyChanged]
    public class InstallationOptionsViewModel
    {
        private readonly bool _canExecute;

        public bool InstallWindowsService { get; set; }

        public bool InstallBluetoothDriver { get; set; }

        public bool InstallDualShock3Driver { get; set; }

        public bool InstallDualShock4Driver { get; set; }

        public bool InstallXbox360Driver { get; set; }

        public bool IsXbox360DriverNeeded
        {
            get { return !(OsInfoHelper.OsParse(OsInfoHelper.OsInfo) >= OsType.Win8); }
        }

        public bool ForceDriverInstallation { get; set; }

        public bool InstallDs3ButtonEnabled { get; set; }

        public bool InstallBthButtonEnabled { get; set; }

        public InstallationOptionsViewModel()
        {
            InstallWindowsService = true;
            InstallBluetoothDriver = true;
            InstallDualShock3Driver = true;
            InstallDualShock4Driver = true;
            InstallXbox360Driver = IsXbox360DriverNeeded;
            InstallDs3ButtonEnabled = false;

            _canExecute = true;
        }

        public event InstallButtonClickedEventHandler InstallButtonClicked;
        public event UninstallButtonClickedEventHandler UninstallButtonClicked;
        public event ExitButtonClickedEventHandler ExitButtonClicked;

        private ICommand _installCommand;
        private ICommand _uninstallCommand;
        private ICommand _exitCommand;

        public ICommand InstallClickCommand
        {
            get
            {
                return _installCommand ?? (_installCommand = new CommandHandler(InstallAction, _canExecute));
            }
        }

        public ICommand UninstallClickCommand
        {
            get
            {
                return _uninstallCommand ?? (_uninstallCommand = new CommandHandler(UninstallAction, _canExecute));
            }
        }

        public ICommand ExitClickCommand
        {
            get
            {
                return _exitCommand ?? (_exitCommand = new CommandHandler(ExitAction, _canExecute));
            }
        }

        private void InstallAction()
        {
            if (InstallButtonClicked != null)
                InstallButtonClicked(this, EventArgs.Empty);
        }

        private void UninstallAction()
        {
            if (UninstallButtonClicked != null)
                UninstallButtonClicked(this, EventArgs.Empty);
        }

        private void ExitAction()
        {
            if (ExitButtonClicked != null)
                ExitButtonClicked(this, EventArgs.Empty);
        }
    }

    public class CommandHandler : ICommand
    {
        private readonly Action _action;
        private readonly bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
