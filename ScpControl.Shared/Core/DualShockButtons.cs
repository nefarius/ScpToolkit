using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ScpControl.Shared.Core
{
    #region Interfaces

    /// <summary>
    ///     Describes the possible states for a DualShock button.
    /// </summary>
    public interface IDsButtonState
    {
        bool IsPressed { get; set; }
        float Pressure { get; }
        byte Value { get; }
        X360Button Xbox360Button { get; set; }
    }

    /// <summary>
    ///     Describes a DualShock button.
    /// </summary>
    public interface IDsButton
    {
        string Name { get; }
        string DisplayName { get; }
        X360Button Xbox360Button { get; }
    }

    #endregion

    /// <summary>
    ///     Implements a DualShock button.
    /// </summary>
    [DataContract]
    public class DsButton : IDsButton
    {
        #region Ctors

        public DsButton()
        {
        }

        public DsButton(string name)
            : this()
        {
            Name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The short name identifying the button.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        ///     A short descriptive name of the button.
        /// </summary>
        [DataMember]
        public string DisplayName { get; protected set; }

        /// <summary>
        ///     The equivalent button on an Xbox 360 controller.
        /// </summary>
        [DataMember]
        public X360Button Xbox360Button { get; protected set; }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            var button = obj as DsButton;

            return button != null && button.Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return DisplayName;
        }

        #endregion
    }

    /// <summary>
    ///     Definition of a DualShock 3 button.
    /// </summary>
    public class Ds3Button : DsButton
    {
        #region Properties

        private static readonly Lazy<IEnumerable<Ds3Button>> Ds3Buttons =
            new Lazy<IEnumerable<Ds3Button>>(() => typeof (Ds3Button).GetProperties(
                BindingFlags.Public | BindingFlags.Static)
                .Select(b => b.GetValue(null, null))
                .Where(o => o.GetType() == typeof (Ds3Button)).Cast<Ds3Button>());

        public static IEnumerable<Ds3Button> Buttons
        {
            get { return Ds3Buttons.Value; }
        }

        #endregion

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

        private static readonly Lazy<IDsButton> DsBtnNone = new Lazy<IDsButton>(() => new Ds3Button("None")
        {
            DisplayName = "None"
        });

        public static IDsButton None
        {
            get { return DsBtnNone.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnSelect = new Lazy<IDsButton>(() => new Ds3Button("Select")
        {
            DisplayName = "Select",
            Xbox360Button = X360Button.Back
        });

        public static IDsButton Select
        {
            get { return DsBtnSelect.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnL3 = new Lazy<IDsButton>(() => new Ds3Button("L3")
        {
            DisplayName = "Left thumb",
            Xbox360Button = X360Button.LS
        });

        public static IDsButton L3
        {
            get { return DsBtnL3.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnR3 = new Lazy<IDsButton>(() => new Ds3Button("R3")
        {
            DisplayName = "Right thumb",
            Xbox360Button = X360Button.RS
        });

        public static IDsButton R3
        {
            get { return DsBtnR3.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnStart = new Lazy<IDsButton>(() => new Ds3Button("Start")
        {
            DisplayName = "Start",
            Xbox360Button = X360Button.Start
        });

        public static IDsButton Start
        {
            get { return DsBtnStart.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnUp = new Lazy<IDsButton>(() => new Ds3Button("Up")
        {
            DisplayName = "D-Pad up",
            Xbox360Button = X360Button.Up
        });

        public static IDsButton Up
        {
            get { return DsBtnUp.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnRight = new Lazy<IDsButton>(() => new Ds3Button("Right")
        {
            DisplayName = "D-Pad right",
            Xbox360Button = X360Button.Right
        });

        public static IDsButton Right
        {
            get { return DsBtnRight.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnDown = new Lazy<IDsButton>(() => new Ds3Button("Down")
        {
            DisplayName = "D-Pad down",
            Xbox360Button = X360Button.Down
        });

        public static IDsButton Down
        {
            get { return DsBtnDown.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnLeft = new Lazy<IDsButton>(() => new Ds3Button("Left")
        {
            Xbox360Button = X360Button.Left
        });

        public static IDsButton Left
        {
            get { return DsBtnLeft.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnL2 = new Lazy<IDsButton>(() => new Ds3Button("L2")
        {
            DisplayName = "Left trigger",
        });

        public static IDsButton L2
        {
            get { return DsBtnL2.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnR2 = new Lazy<IDsButton>(() => new Ds3Button("R2")
        {
            DisplayName = "Right trigger",
        });

        public static IDsButton R2
        {
            get { return DsBtnR2.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnL1 = new Lazy<IDsButton>(() => new Ds3Button("L1")
        {
            DisplayName = "Left shoulder",
            Xbox360Button = X360Button.LB
        });

        public static IDsButton L1
        {
            get { return DsBtnL1.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnR1 = new Lazy<IDsButton>(() => new Ds3Button("R1")
        {
            DisplayName = "Right shoulder",
            Xbox360Button = X360Button.RB
        });

        public static IDsButton R1
        {
            get { return DsBtnR1.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnTriangle = new Lazy<IDsButton>(() => new Ds3Button("Triangle")
        {
            DisplayName = "Triangle",
            Xbox360Button = X360Button.Y
        });

        public static IDsButton Triangle
        {
            get { return DsBtnTriangle.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnCircle = new Lazy<IDsButton>(() => new Ds3Button("Circle")
        {
            DisplayName = "Circle",
            Xbox360Button = X360Button.B
        });

        public static IDsButton Circle
        {
            get { return DsBtnCircle.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnCross = new Lazy<IDsButton>(() => new Ds3Button("Cross")
        {
            DisplayName = "Cross",
            Xbox360Button = X360Button.A
        });

        public static IDsButton Cross
        {
            get { return DsBtnCross.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnSquare = new Lazy<IDsButton>(() => new Ds3Button("Square")
        {
            DisplayName = "Square",
            Xbox360Button = X360Button.X
        });

        public static IDsButton Square
        {
            get { return DsBtnSquare.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnPs = new Lazy<IDsButton>(() => new Ds3Button("PS")
        {
            DisplayName = "PS",
            Xbox360Button = X360Button.Guide
        });

        public static IDsButton Ps
        {
            get { return DsBtnPs.Value; }
        }

        #endregion
    }

    /// <summary>
    ///     Definition of a DualShock 4 button.
    /// </summary>
    public class Ds4Button : DsButton
    {
        #region Properties

        private static readonly Lazy<IEnumerable<Ds4Button>> Ds4Buttons =
            new Lazy<IEnumerable<Ds4Button>>(() => typeof (Ds4Button).GetProperties(
                BindingFlags.Public | BindingFlags.Static)
                .Select(b => b.GetValue(null, null))
                .Where(o => o.GetType() == typeof (Ds4Button)).Cast<Ds4Button>());

        public static IEnumerable<Ds4Button> Buttons
        {
            get { return Ds4Buttons.Value; }
        }

        #endregion

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

        private static readonly Lazy<IDsButton> DsBtnNone = new Lazy<IDsButton>(() => new Ds4Button("None")
        {
            DisplayName = "None"
        });

        public static IDsButton None
        {
            get { return DsBtnNone.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnUp = new Lazy<IDsButton>(() => new Ds4Button("Up")
        {
            DisplayName = "D-Pad up",
            Xbox360Button = X360Button.Up
        });

        public static IDsButton Up
        {
            get { return DsBtnUp.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnRight = new Lazy<IDsButton>(() => new Ds4Button("Right")
        {
            DisplayName = "D-Pad right",
            Xbox360Button = X360Button.Right
        });

        public static IDsButton Right
        {
            get { return DsBtnRight.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnDown = new Lazy<IDsButton>(() => new Ds4Button("Down")
        {
            DisplayName = "D-Pad down",
            Xbox360Button = X360Button.Down
        });

        public static IDsButton Down
        {
            get { return DsBtnDown.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnLeft = new Lazy<IDsButton>(() => new Ds4Button("Left")
        {
            DisplayName = "D-Pad left",
            Xbox360Button = X360Button.Left
        });

        public static IDsButton Left
        {
            get { return DsBtnLeft.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnSquare = new Lazy<IDsButton>(() => new Ds4Button("Square")
        {
            DisplayName = "Square",
            Xbox360Button = X360Button.X
        });

        public static IDsButton Square
        {
            get { return DsBtnSquare.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnCross = new Lazy<IDsButton>(() => new Ds4Button("Cross")
        {
            DisplayName = "Cross",
            Xbox360Button = X360Button.A
        });

        public static IDsButton Cross
        {
            get { return DsBtnCross.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnCircle = new Lazy<IDsButton>(() => new Ds4Button("Circle")
        {
            DisplayName = "Circle",
            Xbox360Button = X360Button.B
        });

        public static IDsButton Circle
        {
            get { return DsBtnCircle.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnTriangle = new Lazy<IDsButton>(() => new Ds4Button("Triangle")
        {
            DisplayName = "Triangle",
            Xbox360Button = X360Button.Y
        });

        public static IDsButton Triangle
        {
            get { return DsBtnTriangle.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnL1 = new Lazy<IDsButton>(() => new Ds4Button("L1")
        {
            DisplayName = "Left shoulder",
            Xbox360Button = X360Button.LB
        });

        public static IDsButton L1
        {
            get { return DsBtnL1.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnR1 = new Lazy<IDsButton>(() => new Ds4Button("R1")
        {
            DisplayName = "Right shoulder",
            Xbox360Button = X360Button.RB
        });

        public static IDsButton R1
        {
            get { return DsBtnR1.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnL2 = new Lazy<IDsButton>(() => new Ds4Button("L2")
        {
            DisplayName = "Left trigger",
        });

        public static IDsButton L2
        {
            get { return DsBtnL2.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnR2 = new Lazy<IDsButton>(() => new Ds4Button("R2")
        {
            DisplayName = "Right trigger",
        });

        public static IDsButton R2
        {
            get { return DsBtnR2.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnShare = new Lazy<IDsButton>(() => new Ds4Button("Select")
        {
            DisplayName = "Share",
            Xbox360Button = X360Button.Back
        });

        public static IDsButton Share
        {
            get { return DsBtnShare.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnOptions = new Lazy<IDsButton>(() => new Ds4Button("Start")
        {
            DisplayName = "Options",
            Xbox360Button = X360Button.Start
        });

        public static IDsButton Options
        {
            get { return DsBtnOptions.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnL3 = new Lazy<IDsButton>(() => new Ds4Button("L3")
        {
            DisplayName = "Left thumb",
            Xbox360Button = X360Button.LS
        });

        public static IDsButton L3
        {
            get { return DsBtnL3.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnR3 = new Lazy<IDsButton>(() => new Ds4Button("R3")
        {
            DisplayName = "Right thumb",
            Xbox360Button = X360Button.RS
        });

        public static IDsButton R3
        {
            get { return DsBtnR3.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnPs = new Lazy<IDsButton>(() => new Ds4Button("PS")
        {
            DisplayName = "PS",
            Xbox360Button = X360Button.Guide
        });

        public static IDsButton Ps
        {
            get { return DsBtnPs.Value; }
        }

        private static readonly Lazy<IDsButton> DsBtnTouchPad = new Lazy<IDsButton>(() => new Ds4Button("TouchPad")
        {
            DisplayName = "Touchpad",
        });

        public static IDsButton TouchPad
        {
            get { return DsBtnTouchPad.Value; }
        }

        #endregion
    }
}