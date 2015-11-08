using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using ScpControl.ScpCore;

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

    public interface IDsButtonMaps
    {
        DsModel TargetModel { get; }
    }

    public class DsButtonMaps : SortedDictionary<IDsButton, ButtonMappingTarget>
    {
    }

    public class DualShockProfile
    {
        public DualShockProfile()
        {
            ButtonMaps = new DsButtonMaps();
        }

        public DsButtonMaps ButtonMaps { get; set; }
    }
}