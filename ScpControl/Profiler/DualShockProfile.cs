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

    public class DsButtonMaps<T> : SortedDictionary<T, ButtonMappingTarget>
    {
    }

    public interface IDsButton
    {
        uint RawValue { get; }
        string Name { get; }
        string DisplayName { get; }
    }

    public class DsButton : IDsButton
    {
        public DsButton()
        {
        }

        public DsButton(string name)
            : this()
        {
            Name = name;
        }

        public uint RawValue { get; protected set; }
        public string Name { get; private set; }
        public string DisplayName { get; protected set; }
    }

    /// <summary>
    ///     Definition of a DualShock 3 button.
    /// </summary>
    public class Ds3Button : DsButton
    {
        #region Ctors

        public Ds3Button()
        {
        }

        public Ds3Button(string name)
            : base(name)
        {
        }

        #endregion

        #region Buttons

        public static IDsButton None
        {
            get
            {
                return new Ds3Button("None")
                {
                    RawValue = 0,
                    DisplayName = "None"
                };
            }
        }

        public static IDsButton Select
        {
            get
            {
                return new Ds3Button("Select")
                {
                    RawValue = 1 << 0,
                    DisplayName = "Select"
                };
            }
        }

        public static IDsButton L3
        {
            get
            {
                return new Ds3Button("L3")
                {
                    RawValue = 1 << 1,
                    DisplayName = "Left thumb"
                };
            }
        }

        public static IDsButton R3
        {
            get
            {
                return new Ds3Button("R3")
                {
                    RawValue = 1 << 2,
                    DisplayName = "Right thumb"
                };
            }
        }

        public static IDsButton Start
        {
            get
            {
                return new Ds3Button("Start")
                {
                    RawValue = 1 << 3,
                    DisplayName = "Start"
                };
            }
        }

        public static IDsButton Up
        {
            get
            {
                return new Ds3Button("Up")
                {
                    RawValue = 1 << 4,
                    DisplayName = "D-Pad up"
                };
            }
        }

        public static IDsButton Right
        {
            get
            {
                return new Ds3Button("Right")
                {
                    RawValue = 1 << 5,
                    DisplayName = "D-Pad right"
                };
            }
        }

        public static IDsButton Down
        {
            get
            {
                return new Ds3Button("Down")
                {
                    RawValue = 1 << 6,
                    DisplayName = "D-Pad down"
                };
            }
        }

        public static IDsButton Left
        {
            get
            {
                return new Ds3Button("Left")
                {
                    RawValue = 1 << 7,
                    DisplayName = "D-Pad left"
                };
            }
        }

        public static IDsButton L2
        {
            get
            {
                return new Ds3Button("L2")
                {
                    RawValue = 1 << 8,
                    DisplayName = "Left trigger"
                };
            }
        }

        public static IDsButton R2
        {
            get
            {
                return new Ds3Button("R2")
                {
                    RawValue = 1 << 9,
                    DisplayName = "Right trigger"
                };
            }
        }

        public static IDsButton L1
        {
            get
            {
                return new Ds3Button("L1")
                {
                    RawValue = 1 << 10,
                    DisplayName = "Left shoulder"
                };
            }
        }

        public static IDsButton R1
        {
            get
            {
                return new Ds3Button("R1")
                {
                    RawValue = 1 << 11,
                    DisplayName = "Right shoulder"
                };
            }
        }

        public static IDsButton Triangle
        {
            get
            {
                return new Ds3Button("Triangle")
                {
                    RawValue = 1 << 12,
                    DisplayName = "Triangle"
                };
            }
        }

        public static IDsButton Circle
        {
            get
            {
                return new Ds3Button("Circle")
                {
                    RawValue = 1 << 13,
                    DisplayName = "Circle"
                };
            }
        }

        public static IDsButton Cross
        {
            get
            {
                return new Ds3Button("Cross")
                {
                    RawValue = 1 << 14,
                    DisplayName = "Cross"
                };
            }
        }

        public static IDsButton Square
        {
            get
            {
                return new Ds3Button("Square")
                {
                    RawValue = 1 << 15,
                    DisplayName = "Square"
                };
            }
        }

        public static IDsButton Ps
        {
            get
            {
                return new Ds3Button("PS")
                {
                    RawValue = 1 << 16,
                    DisplayName = "PS"
                };
            }
        }

        #endregion
    }

    /// <summary>
    ///     Definition of a DualShock 4 button.
    /// </summary>
    public class Ds4Button : DsButton
    {
        #region Ctors

        public Ds4Button()
        {
        }

        public Ds4Button(string name)
            : base(name)
        {
        }

        #endregion

        #region Buttons

        public static IDsButton None
        {
            get
            {
                return new Ds4Button("None")
                {
                    RawValue = 0,
                    DisplayName = "None"
                };
            }
        }

        public static IDsButton Up
        {
            get
            {
                return new Ds4Button("Up")
                {
                    RawValue = 1 << 0,
                    DisplayName = "D-Pad up"
                };
            }
        }

        public static IDsButton Right
        {
            get
            {
                return new Ds4Button("Right")
                {
                    RawValue = 1 << 1,
                    DisplayName = "D-Pad right"
                };
            }
        }

        public static IDsButton Down
        {
            get
            {
                return new Ds4Button("Down")
                {
                    RawValue = 1 << 2,
                    DisplayName = "D-Pad down"
                };
            }
        }

        public static IDsButton Left
        {
            get
            {
                return new Ds4Button("Left")
                {
                    RawValue = 1 << 3,
                    DisplayName = "D-Pad left"
                };
            }
        }

        public static IDsButton Square
        {
            get
            {
                return new Ds4Button("Square")
                {
                    RawValue = 1 << 4,
                    DisplayName = "Square"
                };
            }
        }

        public static IDsButton Cross
        {
            get
            {
                return new Ds4Button("Cross")
                {
                    RawValue = 1 << 5,
                    DisplayName = "Cross"
                };
            }
        }

        public static IDsButton Circle
        {
            get
            {
                return new Ds4Button("Circle")
                {
                    RawValue = 1 << 6,
                    DisplayName = "Circle"
                };
            }
        }

        public static IDsButton Triangle
        {
            get
            {
                return new Ds4Button("Triangle")
                {
                    RawValue = 1 << 7,
                    DisplayName = "Triangle"
                };
            }
        }

        public static IDsButton L1
        {
            get
            {
                return new Ds4Button("L1")
                {
                    RawValue = 1 << 8,
                    DisplayName = "Left shoulder"
                };
            }
        }

        public static IDsButton R1
        {
            get
            {
                return new Ds4Button("R1")
                {
                    RawValue = 1 << 9,
                    DisplayName = "Right shoulder"
                };
            }
        }

        public static IDsButton L2
        {
            get
            {
                return new Ds4Button("L2")
                {
                    RawValue = 1 << 10,
                    DisplayName = "Left trigger"
                };
            }
        }

        public static IDsButton R2
        {
            get
            {
                return new Ds4Button("R2")
                {
                    RawValue = 1 << 11,
                    DisplayName = "Right trigger"
                };
            }
        }

        public static IDsButton Share
        {
            get
            {
                return new Ds4Button("Share")
                {
                    RawValue = 1 << 12,
                    DisplayName = "Share"
                };
            }
        }

        public static IDsButton Options
        {
            get
            {
                return new Ds4Button("Options")
                {
                    RawValue = 1 << 13,
                    DisplayName = "Options"
                };
            }
        }

        public static IDsButton L3
        {
            get
            {
                return new Ds4Button("L3")
                {
                    RawValue = 1 << 14,
                    DisplayName = "Left thumb"
                };
            }
        }

        public static IDsButton R3
        {
            get
            {
                return new Ds4Button("R3")
                {
                    RawValue = 1 << 15,
                    DisplayName = "Right thumb"
                };
            }
        }

        public static IDsButton Ps
        {
            get
            {
                return new Ds4Button("PS")
                {
                    RawValue = 1 << 16,
                    DisplayName = "PS"
                };
            }
        }

        public static IDsButton TouchPad
        {
            get
            {
                return new Ds4Button("TouchPad")
                {
                    RawValue = 1 << 17,
                    DisplayName = "Touchpad"
                };
            }
        }

        #endregion
    }

    public class DualShockProfile
    {
        public DualShockProfile()
        {
            ButtonMaps = new DsButtonMaps<IDsButton>();
        }

        public DsButtonMaps<IDsButton> ButtonMaps { get; set; }
    }
}