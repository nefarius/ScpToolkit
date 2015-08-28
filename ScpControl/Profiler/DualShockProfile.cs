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

    public class DsButton : IDsButton
    {
        public uint RawValue { get; protected set; }
        public string Name { get; protected set; }
        public string DisplayName { get; protected set; }

        public DsButton()
        { }

        public DsButton(string name)
            : this()
        {
            Name = name;
        }
    }

    public class Ds3Button : DsButton
    {
        public Ds3Button() { }

        public Ds3Button(string name)
            : base(name)
        { }

        public static IList<IDsButton> Buttons
        {
            get
            {
                return new List<IDsButton>
                {
                    new Ds3Button("None")
                    {
                        RawValue = 0,
                        DisplayName = "None"
                    },
                    new Ds3Button("Select")
                    {
                        RawValue = 1 << 0,
                        DisplayName = "Select"
                    },
                    new Ds3Button("L3")
                    {
                        RawValue = 1 << 1,
                        DisplayName = "Left thumb"
                    },
                    new Ds3Button("R3")
                    {
                        RawValue = 1 << 2,
                        DisplayName = "Right thumb"
                    },
                    new Ds3Button("Start")
                    {
                        RawValue = 1 << 3,
                        DisplayName = "Start"
                    },
                    new Ds3Button("Up")
                    {
                        RawValue = 1 << 4,
                        DisplayName = "D-Pad up"
                    },
                    new Ds3Button("Right")
                    {
                        RawValue = 1 << 5,
                        DisplayName = "D-Pad right"
                    },
                    new Ds3Button("Down")
                    {
                        RawValue = 1 << 6,
                        DisplayName = "D-Pad down"
                    },
                    new Ds3Button("Left")
                    {
                        RawValue = 1 << 7,
                        DisplayName = "D-Pad left"
                    },
                    new Ds3Button("L2")
                    {
                        RawValue = 1 << 8,
                        DisplayName = "Left trigger"
                    },
                    new Ds3Button("R2")
                    {
                        RawValue = 1 << 9,
                        DisplayName = "Right trigger"
                    },
                    new Ds3Button("L1")
                    {
                        RawValue = 1 << 10,
                        DisplayName = "Left shoulder"
                    },
                    new Ds3Button("R1")
                    {
                        RawValue = 1 << 11,
                        DisplayName = "Right shoulder"
                    },
                    new Ds3Button("Triangle")
                    {
                        RawValue = 1 << 12,
                        DisplayName = "Triangle"
                    },
                    new Ds3Button("Circle")
                    {
                        RawValue = 1 << 13,
                        DisplayName = "Circle"
                    },
                    new Ds3Button("Cross")
                    {
                        RawValue = 1 << 14,
                        DisplayName = "Cross"
                    },
                    new Ds3Button("Square")
                    {
                        RawValue = 1 << 15,
                        DisplayName = "Square"
                    },
                    new Ds3Button("PS")
                    {
                        RawValue = 1 << 16,
                        DisplayName = "PS"
                    }
                };
            }
        }
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
