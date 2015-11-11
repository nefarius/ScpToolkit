namespace ScpControl.Profiler
{
    /// <summary>
    ///     Defines a DualShock axis state.
    /// </summary>
    public interface IDsAxisState
    {
        byte Value { get; set; }
        bool IsEngaged { get; set; }
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

        public byte Value { get; set; }
        
        public bool IsEngaged { get; set; }
    }

    public interface IDsAxis
    {
        byte DefaultValue { get; }
        uint Offset { get; }
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

        public uint Offset { get; protected set; }
        public string Name { get; private set; }
        public string DisplayName { get; protected set; }
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

        public static IDsAxis None
        {
            get
            {
                return new Ds3Axis("None")
                {
                    Offset = 0,
                    DisplayName = "None",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Lx
        {
            get
            {
                return new Ds3Axis("Lx")
                {
                    Offset = 14,
                    DisplayName = "Lx",
                    DefaultValue = 0x80
                };
            }
        }

        public static IDsAxis Ly
        {
            get
            {
                return new Ds3Axis("Ly")
                {
                    Offset = 15,
                    DisplayName = "Ly",
                    DefaultValue = 0x80
                };
            }
        }

        public static IDsAxis Rx
        {
            get
            {
                return new Ds3Axis("Rx")
                {
                    Offset = 16,
                    DisplayName = "Rx",
                    DefaultValue = 0x80
                };
            }
        }

        public static IDsAxis Ry
        {
            get
            {
                return new Ds3Axis("Ry")
                {
                    Offset = 17,
                    DisplayName = "Ry",
                    DefaultValue = 0x80
                };
            }
        }

        public static IDsAxis Up
        {
            get
            {
                return new Ds3Axis("Up")
                {
                    Offset = 22,
                    DisplayName = "D-Pad up",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Right
        {
            get
            {
                return new Ds3Axis("Right")
                {
                    Offset = 23,
                    DisplayName = "D-Pad right",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Down
        {
            get
            {
                return new Ds3Axis("Down")
                {
                    Offset = 24,
                    DisplayName = "D-Pad down",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Left
        {
            get
            {
                return new Ds3Axis("Left")
                {
                    Offset = 25,
                    DisplayName = "D-Pad left",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis L2
        {
            get
            {
                return new Ds3Axis("L2")
                {
                    Offset = 26,
                    DisplayName = "L2",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis R2
        {
            get
            {
                return new Ds3Axis("R2")
                {
                    Offset = 27,
                    DisplayName = "R2",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis L1
        {
            get
            {
                return new Ds3Axis("L1")
                {
                    Offset = 28,
                    DisplayName = "L1",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis R1
        {
            get
            {
                return new Ds3Axis("R1")
                {
                    Offset = 29,
                    DisplayName = "R1",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Triangle
        {
            get
            {
                return new Ds3Axis("Triangle")
                {
                    Offset = 30,
                    DisplayName = "Triangle",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Circle
        {
            get
            {
                return new Ds3Axis("Circle")
                {
                    Offset = 31,
                    DisplayName = "Circle",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Cross
        {
            get
            {
                return new Ds3Axis("Cross")
                {
                    Offset = 32,
                    DisplayName = "Cross",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Square
        {
            get
            {
                return new Ds3Axis("Square")
                {
                    Offset = 33,
                    DisplayName = "Square",
                    DefaultValue = 0x00
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

        public Ds4Axis(string name)
            : base(name)
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
                    Offset = 0,
                    DisplayName = "None",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis Lx
        {
            get
            {
                return new Ds4Axis("Lx")
                {
                    Offset = 9,
                    DisplayName = "Lx",
                    DefaultValue = 0x80
                };
            }
        }

        public static IDsAxis Ly
        {
            get
            {
                return new Ds4Axis("Ly")
                {
                    Offset = 10,
                    DisplayName = "Ly",
                    DefaultValue = 0x80
                };
            }
        }

        public static IDsAxis Rx
        {
            get
            {
                return new Ds4Axis("Rx")
                {
                    Offset = 11,
                    DisplayName = "Rx",
                    DefaultValue = 0x80
                };
            }
        }

        public static IDsAxis Ry
        {
            get
            {
                return new Ds4Axis("Ry")
                {
                    Offset = 12,
                    DisplayName = "Ry",
                    DefaultValue = 0x80
                };
            }
        }

        public static IDsAxis L2
        {
            get
            {
                return new Ds4Axis("L2")
                {
                    Offset = 16,
                    DisplayName = "L2",
                    DefaultValue = 0x00
                };
            }
        }

        public static IDsAxis R2
        {
            get
            {
                return new Ds4Axis("R2")
                {
                    Offset = 17,
                    DisplayName = "R2",
                    DefaultValue = 0x00
                };
            }
        }

        #endregion
    }
}
