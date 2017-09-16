using ScpControl.Utilities;

namespace ScpDriverInstaller.View_Models
{
    public class InstallationOptionsViewModel
    {
        public bool IsXbox360DriverNeeded
        {
            get { return !(OsInfoHelper.OsParse(OsInfoHelper.OsInfo) >= OsType.Win8); }
        }

        public bool InstallDs3ButtonEnabled { get; set; }

        public bool InstallBthButtonEnabled { get; set; }

        public InstallationOptionsViewModel()
        {
            InstallDs3ButtonEnabled = false;
            InstallBthButtonEnabled = false;
        }
    }
}
