using System;

namespace ScpControl.Profiler
{
    public interface IDsButtonState
    {
        bool IsPressed { get; set; }
    }

    public class DsButtonState : IDsButtonState
    {
        public bool IsPressed { get; set; }
    }

    public interface IDsButton
    {
        uint Offset { get; }
        string Name { get; }
        string DisplayName { get; }
        int MaskOffset { get; }
        int ArrayIndex { get; }
        void ToggleBit(ref byte source, bool value);
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

        public uint Offset { get; protected set; }
        public string Name { get; private set; }
        public string DisplayName { get; protected set; }

        public override bool Equals(object obj)
        {
            var button = obj as DsButton;

            return (button != null && button.Name.Equals(this.Name));
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public void ToggleBit(ref byte source, bool value)
        {
            if (value)
            {
                source |= (byte) (1 << MaskOffset);
            }
            else
            {
                source &= (byte) ~(1 << MaskOffset);
            }
        }

        public int MaskOffset { get; protected set; }
        
        public int ArrayIndex { get; protected set; }
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
                    Offset = 0,
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
                    Offset = 1 << 0,
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
                    Offset = 1 << 1,
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
                    Offset = 1 << 2,
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
                    Offset = 1 << 3,
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
                    Offset = 1 << 4,
                    DisplayName = "D-Pad up",
                    ArrayIndex = 10,
                    MaskOffset = 0x10
                };
            }
        }

        public static IDsButton Right
        {
            get
            {
                return new Ds3Button("Right")
                {
                    Offset = 1 << 5,
                    DisplayName = "D-Pad right",
                    ArrayIndex = 10,
                    MaskOffset = 0x20
                };
            }
        }

        public static IDsButton Down
        {
            get
            {
                return new Ds3Button("Down")
                {
                    Offset = 1 << 6,
                    DisplayName = "D-Pad down",
                    ArrayIndex = 10,
                    MaskOffset = 0x40
                };
            }
        }

        public static IDsButton Left
        {
            get
            {
                return new Ds3Button("Left")
                {
                    Offset = 1 << 7,
                    DisplayName = "D-Pad left",
                    ArrayIndex = 10,
                    MaskOffset = 0x80
                };
            }
        }

        public static IDsButton L2
        {
            get
            {
                return new Ds3Button("L2")
                {
                    Offset = 1 << 8,
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
                    Offset = 1 << 9,
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
                    Offset = 1 << 10,
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
                    Offset = 1 << 11,
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
                    Offset = 1 << 12,
                    DisplayName = "Triangle",
                    ArrayIndex = 11,
                    MaskOffset = 4
                };
            }
        }

        public static IDsButton Circle
        {
            get
            {
                return new Ds3Button("Circle")
                {
                    Offset = 1 << 13,
                    DisplayName = "Circle",
                    ArrayIndex = 11,
                    MaskOffset = 5
                };
            }
        }

        public static IDsButton Cross
        {
            get
            {
                return new Ds3Button("Cross")
                {
                    Offset = 1 << 14,
                    DisplayName = "Cross",
                    ArrayIndex = 11,
                    MaskOffset = 6
                };
            }
        }

        public static IDsButton Square
        {
            get
            {
                return new Ds3Button("Square")
                {
                    Offset = 1 << 15,
                    DisplayName = "Square",
                    ArrayIndex = 11,
                    MaskOffset = 7
                };
            }
        }

        public static IDsButton Ps
        {
            get
            {
                return new Ds3Button("PS")
                {
                    Offset = 1 << 16,
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
                    Offset = 0,
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
                    Offset = 1 << 0,
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
                    Offset = 1 << 1,
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
                    Offset = 1 << 2,
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
                    Offset = 1 << 3,
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
                    Offset = 1 << 4,
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
                    Offset = 1 << 5,
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
                    Offset = 1 << 6,
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
                    Offset = 1 << 7,
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
                    Offset = 1 << 8,
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
                    Offset = 1 << 9,
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
                    Offset = 1 << 10,
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
                    Offset = 1 << 11,
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
                    Offset = 1 << 12,
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
                    Offset = 1 << 13,
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
                    Offset = 1 << 14,
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
                    Offset = 1 << 15,
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
                    Offset = 1 << 16,
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
                    Offset = 1 << 17,
                    DisplayName = "Touchpad"
                };
            }
        }

        #endregion
    }
}
