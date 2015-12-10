using System;
using ScpControl.Shared.Utilities;

namespace ScpControl.Shared.Core
{
    /// <summary>
    ///     Defines a DualShock axis state.
    /// </summary>
    public interface IDsAxisState
    {
        byte Value { get; set; }
        bool IsEngaged { get; set; }
        float Pressure { get; }
        float Axis { get; }
        bool Flip { get; set; }
        void ToX360Axis(ref byte xLow, ref byte xHigh, ref byte yLow, ref byte yHigh);
    }

    /// <summary>
    ///     Implements a DualShock axis state.
    /// </summary>
    public class DsAxisState : IDsAxisState
    {
        public DsAxisState()
        {
            Value = 0x80;
        }

        /// <summary>
        ///     The current value of the axis in question.
        /// </summary>
        public byte Value { get; set; }
        
        /// <summary>
        ///     True if the current value differs from the default value of the axis, false otherwise.
        /// </summary>
        public bool IsEngaged { get; set; }

        /// <summary>
        ///     Gets the pressure value of the current button compatible with PCSX2s XInput/LilyPad mod.
        /// </summary>
        public float Pressure
        {
            get
            {
                return (Value & 0xFF) / 255.0f;
            }
        }

        public float Axis { get { return DsMath.ToAxis(Value);} }

        public bool Flip { get; set; }

        public void ToX360Axis(ref byte xLow, ref byte xHigh, ref byte yLow, ref byte yHigh)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///     Defines a DualShock axis.
    /// </summary>
    public interface IDsAxis
    {
        byte DefaultValue { get; }
        uint Offset { get; }
        string Name { get; }
        string DisplayName { get; }
    }

    /// <summary>
    ///     Implementes a DualShock axis.
    /// </summary>
    public class DsAxis : IDsAxis
    {
        #region Ctors

        public DsAxis()
        {
        }

        public DsAxis(string name)
        {
            Name = name;
        }

        #endregion

        /// <summary>
        ///     The offset used to identify and access the appropriate byte in <see cref="ScpHidReport"/>.
        /// </summary>
        public uint Offset { get; protected set; }

        /// <summary>
        ///     The short name of the axis.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     The descriptive name of the axis.
        /// </summary>
        public string DisplayName { get; protected set; }

        /// <summary>
        ///     The default value of the axis reported at non-engaged state.
        /// </summary>
        public byte DefaultValue { get; protected set; }

        public override bool Equals(object obj)
        {
            var axis = obj as DsAxis;

            return (axis != null && axis.Name.Equals(this.Name));
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }

    /// <summary>
    ///     Definition of a DualShock 3 axis.
    /// </summary>
    public class Ds3Axis : DsAxis
    {
        #region Ctors

        public Ds3Axis(string name)
            : base(name)
        {
        }

        #endregion

        #region Axes

        private static readonly Lazy<IDsAxis> DsAxisNone = new Lazy<IDsAxis>(() => new Ds3Axis("None")
        {
            Offset = 0,
            DisplayName = "None",
            DefaultValue = 0x00
        });

        public static IDsAxis None
        {
            get { return DsAxisNone.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisLx = new Lazy<IDsAxis>(() => new Ds3Axis("Lx")
        {
            Offset = 14,
            DisplayName = "Lx",
            DefaultValue = 0x80
        });

        public static IDsAxis Lx
        {
            get { return DsAxisLx.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisLy = new Lazy<IDsAxis>(() => new Ds3Axis("Ly")
        {
            Offset = 15,
            DisplayName = "Ly",
            DefaultValue = 0x80
        });

        public static IDsAxis Ly
        {
            get { return DsAxisLy.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisRx = new Lazy<IDsAxis>(() => new Ds3Axis("Rx")
        {
            Offset = 16,
            DisplayName = "Rx",
            DefaultValue = 0x80
        });

        public static IDsAxis Rx
        {
            get { return DsAxisRx.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisRy = new Lazy<IDsAxis>(() => new Ds3Axis("Ry")
        {
            Offset = 17,
            DisplayName = "Ry",
            DefaultValue = 0x80
        });

        public static IDsAxis Ry
        {
            get { return DsAxisRy.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisUp = new Lazy<IDsAxis>(() => new Ds3Axis("Up")
        {
            Offset = 22,
            DisplayName = "D-Pad up",
            DefaultValue = 0x00
        });

        public static IDsAxis Up
        {
            get { return DsAxisUp.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisRight = new Lazy<IDsAxis>(() => new Ds3Axis("Right")
        {
            Offset = 23,
            DisplayName = "D-Pad right",
            DefaultValue = 0x00
        });

        public static IDsAxis Right
        {
            get { return DsAxisRight.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisDown = new Lazy<IDsAxis>(() => new Ds3Axis("Down")
        {
            Offset = 24,
            DisplayName = "D-Pad down",
            DefaultValue = 0x00
        });

        public static IDsAxis Down
        {
            get { return DsAxisDown.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisLeft = new Lazy<IDsAxis>(() => new Ds3Axis("Left")
        {
            Offset = 25,
            DisplayName = "D-Pad left",
            DefaultValue = 0x00
        });

        public static IDsAxis Left
        {
            get { return DsAxisLeft.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisL2 = new Lazy<IDsAxis>(() => new Ds3Axis("L2")
        {
            Offset = 26,
            DisplayName = "L2",
            DefaultValue = 0x00
        });

        public static IDsAxis L2
        {
            get { return DsAxisL2.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisR2 = new Lazy<IDsAxis>(() => new Ds3Axis("R2")
        {
            Offset = 27,
            DisplayName = "R2",
            DefaultValue = 0x00
        });

        public static IDsAxis R2
        {
            get { return DsAxisR2.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisL1 = new Lazy<IDsAxis>(() => new Ds3Axis("L1")
        {
            Offset = 28,
            DisplayName = "L1",
            DefaultValue = 0x00
        });

        public static IDsAxis L1
        {
            get { return DsAxisL1.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisR1 = new Lazy<IDsAxis>(() => new Ds3Axis("R1")
        {
            Offset = 29,
            DisplayName = "R1",
            DefaultValue = 0x00
        });

        public static IDsAxis R1
        {
            get { return DsAxisR1.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisTriangle = new Lazy<IDsAxis>(() => new Ds3Axis("Triangle")
        {
            Offset = 30,
            DisplayName = "Triangle",
            DefaultValue = 0x00
        });

        public static IDsAxis Triangle
        {
            get { return DsAxisTriangle.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisCircle = new Lazy<IDsAxis>(() => new Ds3Axis("Circle")
        {
            Offset = 31,
            DisplayName = "Circle",
            DefaultValue = 0x00
        });

        public static IDsAxis Circle
        {
            get { return DsAxisCircle.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisCross = new Lazy<IDsAxis>(() => new Ds3Axis("Cross")
        {
            Offset = 32,
            DisplayName = "Cross",
            DefaultValue = 0x00
        });

        public static IDsAxis Cross
        {
            get { return DsAxisCross.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisSquare = new Lazy<IDsAxis>(() => new Ds3Axis("Square")
        {
            Offset = 33,
            DisplayName = "Square",
            DefaultValue = 0x00
        });

        public static IDsAxis Square
        {
            get { return DsAxisSquare.Value; }
        }

        #endregion
    }

    /// <summary>
    ///     Definition of a DualShock 4 axis.
    /// </summary>
    public class Ds4Axis : DsAxis
    {
        #region Ctors

        public Ds4Axis(string name)
            : base(name)
        {
        }

        #endregion

        #region Axes

        private static readonly Lazy<IDsAxis> DsAxisNone = new Lazy<IDsAxis>(() => new Ds4Axis("None")
        {
            Offset = 0,
            DisplayName = "None",
            DefaultValue = 0x00
        });

        public static IDsAxis None
        {
            get { return DsAxisNone.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisLx = new Lazy<IDsAxis>(() => new Ds4Axis("Lx")
        {
            Offset = 9,
            DisplayName = "Lx",
            DefaultValue = 0x80
        });

        public static IDsAxis Lx
        {
            get { return DsAxisLx.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisLy = new Lazy<IDsAxis>(() => new Ds4Axis("Ly")
        {
            Offset = 10,
            DisplayName = "Ly",
            DefaultValue = 0x80
        });

        public static IDsAxis Ly
        {
            get { return DsAxisLy.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisRx = new Lazy<IDsAxis>(() => new Ds4Axis("Rx")
        {
            Offset = 11,
            DisplayName = "Rx",
            DefaultValue = 0x80
        });

        public static IDsAxis Rx
        {
            get { return DsAxisRx.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisRy = new Lazy<IDsAxis>(() => new Ds4Axis("Ry")
        {
            Offset = 12,
            DisplayName = "Ry",
            DefaultValue = 0x80
        });

        public static IDsAxis Ry
        {
            get { return DsAxisRy.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisL2 = new Lazy<IDsAxis>(() => new Ds4Axis("L2")
        {
            Offset = 16,
            DisplayName = "L2",
            DefaultValue = 0x00
        });

        public static IDsAxis L2
        {
            get { return DsAxisL2.Value; }
        }

        private static readonly Lazy<IDsAxis> DsAxisR2 = new Lazy<IDsAxis>(() => new Ds4Axis("R2")
        {
            Offset = 17,
            DisplayName = "R2",
            DefaultValue = 0x00
        });

        public static IDsAxis R2
        {
            get { return DsAxisR2.Value; }
        }

        #endregion
    }
}
