using System.ComponentModel;

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

    public class ButtonMappingTarget
    {
        public CommandType CommandType { get; set; }
        public object CommandTarget { get; set; }
    }

    public class DualShockProfile
    {
        public DualShockProfile()
        {
            Ps = new DualShockButtonProfile();
            Circle = new DualShockButtonProfile();
            Cross = new DualShockButtonProfile();
            Square = new DualShockButtonProfile();
            Triangle = new DualShockButtonProfile();
            Select = new DualShockButtonProfile();
            Start = new DualShockButtonProfile();
            LeftShoulder = new DualShockButtonProfile();
            RightShoulder = new DualShockButtonProfile();
            LeftTrigger = new DualShockButtonProfile();
            RightTrigger = new DualShockButtonProfile();
            LeftThumb = new DualShockButtonProfile();
            RightThumb = new DualShockButtonProfile();
        }

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
    }

    public interface IDsButtonProfile
    {
        ButtonMappingTarget MappingTarget { get; set; }
        bool IsEnabled { get; set; }
        DsButtonProfileTurboSetting Turbo { get; set; }
    }

    public class DualShockButtonProfile : IDsButtonProfile
    {
        public ButtonMappingTarget MappingTarget { get; set; }
        public bool IsEnabled { get; set; }
        public DsButtonProfileTurboSetting Turbo { get; set; }

        public DualShockButtonProfile()
        {
            MappingTarget = new ButtonMappingTarget();
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