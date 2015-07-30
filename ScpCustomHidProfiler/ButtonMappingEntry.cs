using System.ComponentModel;

namespace ScpCustomHidProfiler
{
    public enum CommandTypes : byte
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

    public class ButtonMappingEntry
    {
        public CommandTypes CommandType { get; set; }

        public object CommandTarget { get; set; }
    }
}
