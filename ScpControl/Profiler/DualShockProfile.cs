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

    public interface IDsButtonState
    {
        bool IsPressed { get; set; }
        byte Value { get; set; }
    }

    public class DsButtonState : IDsButtonState
    {

        public bool IsPressed
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public byte Value
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public interface IDsAxis
    {
        uint RawValue { get; }
        string Name { get; }
        string DisplayName { get; }
    }

    public class DsAxis : IDsAxis
    {
        public DsAxis()
        {
        }

        public DsAxis(string name)
        {
            Name = name;
        }

        public uint RawValue { get; protected set; }
        public string Name { get; private set; }
        public string DisplayName { get; protected set; }
    }

    /// <summary>
    ///     Definition of a DualShock 3 axis.
    /// </summary>
    public class Ds3Axis : DsAxis
    {
        #region Ctors

        public Ds3Axis(string name) : base(name)
        {
        }

        #endregion

        #region Axes

        public static IDsAxis None
        {
            get
            {
                return new Ds3Axis("None")
                {
                    RawValue = 0,
                    DisplayName = "None"
                };
            }
        }

        public static IDsAxis Lx
        {
            get
            {
                return new Ds3Axis("Lx")
                {
                    RawValue = 14,
                    DisplayName = "Lx"
                };
            }
        }

        public static IDsAxis Ly
        {
            get
            {
                return new Ds3Axis("Ly")
                {
                    RawValue = 15,
                    DisplayName = "Ly"
                };
            }
        }

        public static IDsAxis Rx
        {
            get
            {
                return new Ds3Axis("Rx")
                {
                    RawValue = 16,
                    DisplayName = "Rx"
                };
            }
        }

        public static IDsAxis Ry
        {
            get
            {
                return new Ds3Axis("Ry")
                {
                    RawValue = 17,
                    DisplayName = "Ry"
                };
            }
        }

        public static IDsAxis Up
        {
            get
            {
                return new Ds3Axis("Up")
                {
                    RawValue = 22,
                    DisplayName = "D-Pad up"
                };
            }
        }

        public static IDsAxis Right
        {
            get
            {
                return new Ds3Axis("Right")
                {
                    RawValue = 23,
                    DisplayName = "D-Pad right"
                };
            }
        }

        public static IDsAxis Down
        {
            get
            {
                return new Ds3Axis("Down")
                {
                    RawValue = 24,
                    DisplayName = "D-Pad down"
                };
            }
        }

        public static IDsAxis Left
        {
            get
            {
                return new Ds3Axis("Left")
                {
                    RawValue = 25,
                    DisplayName = "D-Pad left"
                };
            }
        }

        public static IDsAxis L2
        {
            get
            {
                return new Ds3Axis("L2")
                {
                    RawValue = 26,
                    DisplayName = "L2"
                };
            }
        }

        public static IDsAxis R2
        {
            get
            {
                return new Ds3Axis("R2")
                {
                    RawValue = 27,
                    DisplayName = "R2"
                };
            }
        }

        public static IDsAxis L1
        {
            get
            {
                return new Ds3Axis("L1")
                {
                    RawValue = 28,
                    DisplayName = "L1"
                };
            }
        }

        public static IDsAxis R1
        {
            get
            {
                return new Ds3Axis("R1")
                {
                    RawValue = 29,
                    DisplayName = "R1"
                };
            }
        }

        public static IDsAxis Triangle
        {
            get
            {
                return new Ds3Axis("Triangle")
                {
                    RawValue = 30,
                    DisplayName = "Triangle"
                };
            }
        }

        public static IDsAxis Circle
        {
            get
            {
                return new Ds3Axis("Circle")
                {
                    RawValue = 31,
                    DisplayName = "Circle"
                };
            }
        }

        public static IDsAxis Cross
        {
            get
            {
                return new Ds3Axis("Cross")
                {
                    RawValue = 32,
                    DisplayName = "Cross"
                };
            }
        }

        public static IDsAxis Square
        {
            get
            {
                return new Ds3Axis("Square")
                {
                    RawValue = 33,
                    DisplayName = "Square"
                };
            }
        }

        #endregion
    }

    /// <summary>
    ///     Definition of a DualShock 4 axis.
    /// </summary>
    public class Ds4Axis : DsAxis
    {
        #region Ctors

        public Ds4Axis(string name) : base(name)
        {
        }

        #endregion

        #region Axes

        public static IDsAxis None
        {
            get
            {
                return new Ds4Axis("None")
                {
                    RawValue = 0,
                    DisplayName = "None"
                };
            }
        }

        public static IDsAxis Lx
        {
            get
            {
                return new Ds4Axis("Lx")
                {
                    RawValue = 9,
                    DisplayName = "Lx"
                };
            }
        }

        public static IDsAxis Ly
        {
            get
            {
                return new Ds4Axis("Ly")
                {
                    RawValue = 10,
                    DisplayName = "Ly"
                };
            }
        }

        public static IDsAxis Rx
        {
            get
            {
                return new Ds4Axis("Rx")
                {
                    RawValue = 11,
                    DisplayName = "Rx"
                };
            }
        }

        public static IDsAxis Ry
        {
            get
            {
                return new Ds4Axis("Ry")
                {
                    RawValue = 12,
                    DisplayName = "Ry"
                };
            }
        }

        public static IDsAxis L2
        {
            get
            {
                return new Ds4Axis("L2")
                {
                    RawValue = 16,
                    DisplayName = "L2"
                };
            }
        }

        public static IDsAxis R2
        {
            get
            {
                return new Ds4Axis("R2")
                {
                    RawValue = 17,
                    DisplayName = "R2"
                };
            }
        }

        #endregion
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