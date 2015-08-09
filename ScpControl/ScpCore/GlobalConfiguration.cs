using System;
using System.Linq;
using System.Windows.Forms;
using ScpControl.Properties;

namespace ScpControl.ScpCore
{
    public class GlobalConfiguration : ICloneable
    {
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

        public bool DisableLED
        {
            get { return Settings.Default.DisableLed; }
            set { Settings.Default.DisableLed = value; }
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

        public bool DisableLightBar
        {
            get { return Settings.Default.Ds4LightBarBrightness == 0; }
            set { Settings.Default.Ds4LightBarBrightness = (byte)(value ? 0 : 128); }
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
    }
}