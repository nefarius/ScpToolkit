using System;
using System.IO;
using System.Linq;
using PropertyChanged;
using ScpControl.Bluetooth;
using ScpControl.Properties;

namespace ScpControl.ScpCore
{
    [ImplementPropertyChanged]
    public class GlobalConfiguration : ICloneable
    {
        private static readonly string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

        private static readonly Lazy<GlobalConfiguration> LazyInstance =
            new Lazy<GlobalConfiguration>(() => new GlobalConfiguration());

        private static readonly byte[] MBdLink =
        {
            0x56, 0xE8, 0x81, 0x38, 0x08, 0x06, 0x51, 0x41, 0xC0, 0x7F, 0x12, 0xAA,
            0xD9, 0x66, 0x3C, 0xCE
        };

        private GlobalConfiguration()
        {
        }

        public static GlobalConfiguration Instance
        {
            get { return LazyInstance.Value; }
        }

        public static int IdleTimeoutMultiplier
        {
            get { return 60000; }
        }

        public static int LatencyMultiplier
        {
            get { return 16; }
        }

        public object Clone()
        {
            return (GlobalConfiguration)MemberwiseClone();
        }

        public static void Load()
        {
            Settings.Default.Reload();
        }

        public static void Save()
        {
            Settings.Default.Save();
        }

        public static GlobalConfiguration Request()
        {
            return (GlobalConfiguration)Instance.Clone();
        }

        public static void Submit(GlobalConfiguration configuration)
        {
            foreach (
                var propertyInfo in
                    typeof(GlobalConfiguration).GetProperties().Where(propertyInfo => propertyInfo.CanWrite))
            {
                propertyInfo.SetValue(Instance, propertyInfo.GetValue(configuration));
            }
        }

        #region Public properties

        public bool FlipLX
        {
            get { return Settings.Default.FlipAxisLx; }
            set { Settings.Default.FlipAxisLx = value; }
        }

        public bool FlipLY
        {
            get { return Settings.Default.FlipAxisLy; }
            set { Settings.Default.FlipAxisLy = value; }
        }

        public bool FlipRX
        {
            get { return Settings.Default.FlipAxisRx; }
            set { Settings.Default.FlipAxisRx = value; }
        }

        public bool FlipRY
        {
            get { return Settings.Default.FlipAxisRy; }
            set { Settings.Default.FlipAxisRy = value; }
        }

        public bool DisableRumble
        {
            get { return Settings.Default.DisableRumble; }
            set { Settings.Default.DisableRumble = value; }
        }

        public bool SwapTriggers
        {
            get { return Settings.Default.SwapTriggers; }
            set { Settings.Default.SwapTriggers = value; }
        }

        public bool IsLightBarDisabled
        {
            get { return Settings.Default.Ds4LightBarBrightness == 0; }
        }

        public bool IdleDisconnect
        {
            get { return Settings.Default.IdleTimout != 0; }
        }

        public int IdleTimeout
        {
            get { return Settings.Default.IdleTimout; }
            set { Settings.Default.IdleTimout = value; }
        }

        public int Latency
        {
            get { return Settings.Default.Ds3RumbleLatency; }
            set { Settings.Default.Ds3RumbleLatency = value; }
        }

        public byte DeadZoneL
        {
            get { return Settings.Default.DeadZoneL; }
            set { Settings.Default.DeadZoneL = value; }
        }

        public byte DeadZoneR
        {
            get { return Settings.Default.DeadZoneR; }
            set { Settings.Default.DeadZoneR = value; }
        }

        public bool DisableNative
        {
            get { return Settings.Default.DisableNativeFeed; }
            set { Settings.Default.DisableNativeFeed = value; }
        }

        public bool DisableSSP
        {
            get { return Settings.Default.DisableSecureSimplePairing; }
            set { Settings.Default.DisableSecureSimplePairing = value; }
        }

        public byte Brightness
        {
            get { return Settings.Default.Ds4LightBarBrightness; }
            set { Settings.Default.Ds4LightBarBrightness = value; }
        }

        public int Bus
        {
            get { return Settings.Default.BusId; }
            set { Settings.Default.BusId = value; }
        }

        public bool Repair
        {
            get { return Settings.Default.Ds4Repair; }
            set { Settings.Default.Ds4Repair = value; }
        }

        public byte[] BdLink
        {
            get { return MBdLink; }
        }

        public Ds4UpdateRate Ds4InputUpdateDelay
        {
            get { return Settings.Default.Ds4InputUpdateDelay; }
            set
            {
                if (Enum.IsDefined(typeof(Ds4UpdateRate), value))
                    Settings.Default.Ds4InputUpdateDelay = value;
            }
        }

        public bool ForceBluetoothDriverReinstallation
        {
            get { return Settings.Default.ForceBluetoothDriverReinstallation; }
            set { Settings.Default.ForceBluetoothDriverReinstallation = value; }
        }

        public bool ForceDs3DriverReinstallation
        {
            get { return Settings.Default.ForceDs3DriverReinstallation; }
            set { Settings.Default.ForceDs3DriverReinstallation = value; }
        }

        public bool ForceDs4DriverReinstallation
        {
            get { return Settings.Default.ForceDs4DriverReinstallation; }
            set { Settings.Default.ForceDs4DriverReinstallation = value; }
        }

        public bool SoundsEnabled
        {
            get { return Settings.Default.SoundsEnabled; }
            set { Settings.Default.SoundsEnabled = value; }
        }

        public string StartupSoundFile
        {
            get
            {
                return Path.IsPathRooted(Settings.Default.StartupSoundFile)
                    ? Settings.Default.StartupSoundFile
                    : Path.Combine(WorkingDirectory, Settings.Default.StartupSoundFile);
            }
            set { Settings.Default.StartupSoundFile = value; }
        }

        public string UsbConnectSoundFile
        {
            get
            {
                return Path.IsPathRooted(Settings.Default.UsbConnectSoundFile)
                    ? Settings.Default.UsbConnectSoundFile
                    : Path.Combine(WorkingDirectory, Settings.Default.UsbConnectSoundFile);
            }
            set { Settings.Default.UsbConnectSoundFile = value; }
        }

        public string UsbDisconnectSoundFile
        {
            get
            {
                return Path.IsPathRooted(Settings.Default.UsbDisconnectSoundFile)
                    ? Settings.Default.UsbDisconnectSoundFile
                    : Path.Combine(WorkingDirectory, Settings.Default.UsbDisconnectSoundFile);
            }
            set { Settings.Default.UsbDisconnectSoundFile = value; }
        }

        public string BluetoothConnectSoundFile
        {
            get
            {
                return Path.IsPathRooted(Settings.Default.BluetoothConnectSoundFile)
                    ? Settings.Default.BluetoothConnectSoundFile
                    : Path.Combine(WorkingDirectory, Settings.Default.BluetoothConnectSoundFile);
            }
            set { Settings.Default.BluetoothConnectSoundFile = value; }
        }

        public string BluetoothDisconnectSoundFile
        {
            get
            {
                return Path.IsPathRooted(Settings.Default.BluetoothDisconnectSoundFile)
                    ? Settings.Default.BluetoothDisconnectSoundFile
                    : Path.Combine(WorkingDirectory, Settings.Default.BluetoothDisconnectSoundFile);
            }
            set { Settings.Default.BluetoothDisconnectSoundFile = value; }
        }

        public int Ds3LEDsPeriod
        {
            get { return Settings.Default.Ds3LEDsFlashingPeriod; }
            set { Settings.Default.Ds3LEDsFlashingPeriod = value; }
        }

        public int Ds3LEDsFunc
        {
            get { return Settings.Default.Ds3LEDsFunction; }
            set { Settings.Default.Ds3LEDsFunction = value; }
        }

        public bool Ds3PadIDLEDsFlashCharging
        {
            get { return Settings.Default.Ds3PadIDLEDsFlashCharging; }
            set { Settings.Default.Ds3PadIDLEDsFlashCharging = value; }
        }

        public bool Ds3LEDsCustom1
        {
            get { return Settings.Default.Ds3LEDsCustom1; }
            set { Settings.Default.Ds3LEDsCustom1 = value; }
        }

        public bool Ds3LEDsCustom2
        {
            get { return Settings.Default.Ds3LEDsCustom2; }
            set { Settings.Default.Ds3LEDsCustom2 = value; }
        }

        public bool Ds3LEDsCustom3
        {
            get { return Settings.Default.Ds3LEDsCustom3; }
            set { Settings.Default.Ds3LEDsCustom3 = value; }
        }

        public bool Ds3LEDsCustom4
        {
            get { return Settings.Default.Ds3LEDsCustom4; }
            set { Settings.Default.Ds3LEDsCustom4 = value; }
        }

        public bool UseDs3CounterfeitWorkarounds
        {
            get { return Settings.Default.UseDs3CounterfeitWorkarounds; }
            set { Settings.Default.UseDs3CounterfeitWorkarounds = value; }
        }

        #endregion
    }
}