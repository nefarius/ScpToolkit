using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class DsButtonMaps<T> : SortedDictionary<T, ButtonMappingTarget>
    {
    }

    public interface IDsButton
    {
        
    }

    public class DualShockProfile
    {
        public DsButtonMaps<IDsButton> ButtonMaps { get; set; }

        public DualShockProfile()
        {
            ButtonMaps = new DsButtonMaps<IDsButton>();


        }
    }
}
