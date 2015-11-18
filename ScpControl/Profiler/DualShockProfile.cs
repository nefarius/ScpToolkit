using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace ScpControl.Profiler
{
    public enum CommandType : byte
    {
        [Description("Keystrokes")]
        Keystrokes,
        [Description("Gamepad buttons")]
        GamepadButton,
        [Description("Mouse buttons")]
        MouseButtons,
        [Description("Mouse axis")]
        MouseAxis
    }

    public class DsButtonMappingTarget
    {
        public CommandType CommandType { get; set; }
        public object CommandTarget { get; set; }
    }

    public class DualShockProfile
    {
        #region Ctor

        public DualShockProfile()
        {
            Ps = new DsButtonProfile();
            Circle = new DsButtonProfile();
            Cross = new DsButtonProfile();
            Square = new DsButtonProfile();
            Triangle = new DsButtonProfile();
            Select = new DsButtonProfile();
            Start = new DsButtonProfile();
            LeftShoulder = new DsButtonProfile();
            RightShoulder = new DsButtonProfile();
            LeftTrigger = new DsButtonProfile();
            RightTrigger = new DsButtonProfile();
            LeftThumb = new DsButtonProfile();
            RightThumb = new DsButtonProfile();
        }

        #endregion

        #region Properties

        public IDsButtonProfile Ps { get; set; }
        public IDsButtonProfile Circle { get; set; }
        public IDsButtonProfile Cross { get; set; }
        public IDsButtonProfile Square { get; set; }
        public IDsButtonProfile Triangle { get; set; }
        public IDsButtonProfile Select { get; set; }
        public IDsButtonProfile Start { get; set; }
        public IDsButtonProfile LeftShoulder { get; set; }
        public IDsButtonProfile RightShoulder { get; set; }
        public IDsButtonProfile LeftTrigger { get; set; }
        public IDsButtonProfile RightTrigger { get; set; }
        public IDsButtonProfile LeftThumb { get; set; }
        public IDsButtonProfile RightThumb { get; set; }

        #endregion

        public static DualShockProfile Load(string file)
        {
            var serializer = new XmlSerializer(typeof (DualShockProfile));

            using (var fs = File.OpenText(file))
            {
                return (DualShockProfile) serializer.Deserialize(fs);
            }
        }

        public void Save(string file)
        {
            var serializer = new XmlSerializer(typeof(DualShockProfile));

            using (var fs = File.CreateText(file))
            {
                serializer.Serialize(fs, this);
            }
        }
    }

    public interface IDsButtonProfile
    {
        DsButtonMappingTarget MappingTarget { get; set; }
        bool IsEnabled { get; set; }
        DsButtonProfileTurboSetting Turbo { get; set; }
    }

    public class DsButtonProfile : IDsButtonProfile
    {
        public DsButtonMappingTarget MappingTarget { get; set; }
        public bool IsEnabled { get; set; }
        public DsButtonProfileTurboSetting Turbo { get; set; }

        public DsButtonProfile()
        {
            MappingTarget = new DsButtonMappingTarget();
            Turbo = new DsButtonProfileTurboSetting();
        }
    }

    public class DsButtonProfileTurboSetting
    {
        public bool IsEnabled { get; set; }
        public int Delay { get; set; }
        public int Interval { get; set; }
        public int Release { get; set; }

        public DsButtonProfileTurboSetting()
        {
            Delay = 0;
            Interval = 50;
            Release = 100;
        }
    }
}