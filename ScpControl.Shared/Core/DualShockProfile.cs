using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using WindowsInput;
using WindowsInput.Native;
using PropertyChanged;

namespace ScpControl.Shared.Core
{
    /// <summary>
    ///     Possible mapping target types (keystrokes, mouse movement etc.)
    /// </summary>
    public enum CommandType : byte
    {
        [Description("Keystrokes")] Keystrokes,
        [Description("Gamepad buttons")] GamepadButton,
        [Description("Mouse buttons")] MouseButtons,
        [Description("Mouse axis")] MouseAxis
    }

    /// <summary>
    ///     Describes a mapping target.
    /// </summary>
    [ImplementPropertyChanged]
    [DataContract]
    [KnownType(typeof (DsButtonMappingTarget))]
    public class DsButtonMappingTarget
    {
        #region Properties

        [DataMember]
        public CommandType CommandType { get; set; }

        [DataMember]
        public object CommandTarget { get; set; }

        #endregion
    }

    /// <summary>
    ///     Represents a DualShock button/axis mapping profile.
    /// </summary>
    [ImplementPropertyChanged]
    [DataContract]
    [KnownType(typeof (Ds3Button))]
    [KnownType(typeof (Ds4Button))]
    [KnownType(typeof (VirtualKeyCode))]
    [KnownType(typeof (MouseButton))]
    [DisplayName("DualShock Profile")]
    public class DualShockProfile
    {
        #region Ctor

        public DualShockProfile()
        {
            Id = Guid.NewGuid();
            Name = "New Profile";

            OnCreated();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            OnCreated();
        }

        /// <summary>
        ///     Initialize buttons/axes.
        /// </summary>
        private void OnCreated()
        {
            Ps = new DsButtonProfile(Ds3Button.Ps, Ds4Button.Ps);
            Circle = new DsButtonProfile(Ds3Button.Circle, Ds4Button.Circle);
            Cross = new DsButtonProfile(Ds3Button.Cross, Ds4Button.Cross);
            Square = new DsButtonProfile(Ds3Button.Square, Ds4Button.Square);
            Triangle = new DsButtonProfile(Ds3Button.Triangle, Ds4Button.Triangle);
            Select = new DsButtonProfile(Ds3Button.Select, Ds4Button.Share);
            Start = new DsButtonProfile(Ds3Button.Start, Ds4Button.Options);
            LeftShoulder = new DsButtonProfile(Ds3Button.L1, Ds4Button.L1);
            RightShoulder = new DsButtonProfile(Ds3Button.R1, Ds4Button.R1);
            LeftTrigger = new DsButtonProfile(Ds3Button.L2, Ds4Button.L2);
            RightTrigger = new DsButtonProfile(Ds3Button.R2, Ds4Button.R2);
            LeftThumb = new DsButtonProfile(Ds3Button.L3, Ds4Button.L3);
            RightThumb = new DsButtonProfile(Ds3Button.R3, Ds4Button.R3);

            // D-Pad
            Up = new DsButtonProfile(Ds3Button.Up, Ds4Button.Up);
            Right = new DsButtonProfile(Ds3Button.Right, Ds4Button.Right);
            Down = new DsButtonProfile(Ds3Button.Down, Ds4Button.Down);
            Left = new DsButtonProfile(Ds3Button.Left, Ds4Button.Left);
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Applies button re-mapping to the supplied report.
        /// </summary>
        /// <param name="report">The report to manipulate.</param>
        public void Remap(ScpHidReport report)
        {
            // determine if profile should be applied
            switch (Match)
            {
                case DsMatch.Global:
                    // always apply
                    break;
                case DsMatch.Mac:
                    // applies of MAC address matches
                    var reportMac = report.PadMacAddress.ToString();
                    if (string.CompareOrdinal(MacAddress.Replace(":", string.Empty), reportMac) != 0) return;
                    break;
                case DsMatch.None:
                    // never apply
                    return;
                case DsMatch.Pad:
                    // applies if pad IDs match
                    if (PadId != report.PadId) return;
                    break;
            }

            // walk through all buttons
            foreach (var buttonProfile in Buttons)
            {
                buttonProfile.Remap(report);
            }
        }

        public override bool Equals(object obj)
        {
            var profile = obj as DualShockProfile;

            return profile != null && profile.Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Properties

        [DataMember]
        [Category("Main")]
        [DisplayName("Profile is active")]
        [Description("If disabled, the entire profile will be ignored from processing.")]
        public bool IsActive { get; set; }

        [DataMember]
        [Category("Main")]
        [DisplayName("Profile name")]
        [Description("The friendly name of this profile.")]
        public string Name { get; set; }

        [DataMember]
        [ReadOnly(true)]
        [Description("The unique identifier of this profile.")]
        public Guid Id { get; private set; }

        [DataMember]
        [ReadOnly(true)]
        [DisplayName("Pad ID")]
        public DsPadId PadId { get; set; }

        [DataMember]
        [ReadOnly(true)]
        [DisplayName("MAC Address")]
        public string MacAddress { get; set; }

        [DataMember]
        [ReadOnly(true)]
        [DisplayName("Pad Model")]
        public DsModel Model { get; set; }

        [DataMember]
        [Category("Main")]
        [DisplayName("Match profile on")]
        public DsMatch Match { get; set; }

        [Browsable(false)]
        private IEnumerable<DsButtonProfile> Buttons
        {
            get
            {
                var props = GetType().GetProperties().Where(pi => pi.PropertyType == typeof (DsButtonProfile));

                return props.Select(b => b.GetValue(this)).Cast<DsButtonProfile>();
            }
        }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Ps { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Circle { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Cross { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Square { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Triangle { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Select { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Start { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile LeftShoulder { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile RightShoulder { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile LeftTrigger { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile RightTrigger { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile LeftThumb { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile RightThumb { get; set; }

        // D-Pad
        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Up { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Right { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Down { get; set; }

        [DataMember]
        [Browsable(false)]
        public DsButtonProfile Left { get; set; }

        #endregion
    }

    /// <summary>
    ///     Describes details about individual buttons.
    /// </summary>
    [ImplementPropertyChanged]
    [DataContract]
    public class DsButtonProfile
    {
        private static readonly InputSimulator VirtualInput = new InputSimulator();
        private const uint InputDelay = 100;

        #region Ctor

        public DsButtonProfile()
        {
            OnCreated();
        }

        /// <summary>
        ///     Creates a new button mapping profile.
        /// </summary>
        /// <param name="sources">A list of DualShock buttons which will be affected by this profile.</param>
        public DsButtonProfile(params IDsButton[] sources) : this()
        {
            SourceButtons = sources;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Applies button re-mapping to the supplied report.
        /// </summary>
        /// <param name="report">The report to manipulate.</param>
        public void Remap(ScpHidReport report)
        {
            // skip disabled mapping
            if (!IsEnabled) return;

            switch (MappingTarget.CommandType)
            {
                case CommandType.GamepadButton:
                    foreach (var button in SourceButtons)
                    {
                        // turbo is special, apply first
                        if (Turbo.IsEnabled)
                        {
                            Turbo.ApplyOn(report, button);
                        }

                        // get target button
                        IDsButton target = MappingTarget.CommandTarget as Ds3Button;
                        // if target is no valid button or none, skip setting it
                        if (target == null) continue;

                        // if it's a DS4, translate button
                        if (report.Model == DsModel.DS4)
                        {
                            target = Ds4Button.Buttons.First(b => b.Name.Equals(target.Name));
                        }

                        // if original isn't pressed we can ignore
                        if (!report[button].IsPressed) continue;

                        // unset original button
                        report.Unset(button);
                        // set new button
                        report.Set(target);
                    }
                    break;
                case CommandType.Keystrokes:
                    foreach (var button in SourceButtons)
                    {
                        var target = (VirtualKeyCode) Enum.ToObject(typeof(VirtualKeyCode), MappingTarget.CommandTarget);

                        if (report[button].IsPressed)
                        {
                            VirtualInput.Keyboard.KeyDown(target);
                        }
                        else
                        {
                            VirtualInput.Keyboard.KeyUp(target);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Properties

        [DataMember]
        private IEnumerable<IDsButton> SourceButtons { get; set; }

        [DataMember]
        public DsButtonMappingTarget MappingTarget { get; private set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public DsButtonProfileTurboSetting Turbo { get; set; }

        public byte CurrentValue { get; set; }

        #endregion

        #region Deserialization

        private void OnCreated()
        {
            MappingTarget = new DsButtonMappingTarget();
            Turbo = new DsButtonProfileTurboSetting();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            OnCreated();
        }

        #endregion
    }

    /// <summary>
    ///     Describes button turbo mode details.
    /// </summary>
    [ImplementPropertyChanged]
    [DataContract]
    public class DsButtonProfileTurboSetting
    {
        #region Ctor

        public DsButtonProfileTurboSetting()
        {
            Delay = 0;
            Interval = 50;
            Release = 100;
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Applies turbo algorithm for a specified <see cref="IDsButton" /> on a given <see cref="ScpHidReport" />.
        /// </summary>
        /// <param name="report">The HID report to manipulate.</param>
        /// <param name="button">The button to trigger turbo on.</param>
        public void ApplyOn(ScpHidReport report, IDsButton button)
        {
            // button type must match model, madness otherwise!
            if ((report.Model != DsModel.DS3 || !(button is Ds3Button)) &&
                (report.Model != DsModel.DS4 || !(button is Ds4Button))) return;

            // if button got released...
            if (_isActive && !report[button].IsPressed)
            {
                // ...disable, reset and return
                _isActive = false;
                _delayedFrame.Reset();
                _engagedFrame.Reset();
                _releasedFrame.Reset();
                return;
            }

            // if turbo is enabled and button is pressed...
            if (!_isActive && report[button].IsPressed)
            {
                // ...start calculating the activation delay...
                if (!_delayedFrame.IsRunning) _delayedFrame.Restart();

                // ...if we are still activating, don't do anything
                if (_delayedFrame.ElapsedMilliseconds < Delay) return;

                // time to activate!
                _isActive = true;
                _delayedFrame.Reset();
            }

            // if the button was released...
            if (!report[button].IsPressed)
            {
                // ...restore default states and skip processing
                _isActive = false;
                return;
            }

            // reset engaged ("keep pressed") time frame...
            if (!_engagedFrame.IsRunning) _engagedFrame.Restart();

            // ...do not change state while within frame and button is still pressed, then skip
            if (_engagedFrame.ElapsedMilliseconds < Interval && report[button].IsPressed) return;

            // reset released time frame ("forecefully release") for button
            if (!_releasedFrame.IsRunning) _releasedFrame.Restart();

            // while we're still within the released time frame...
            if (_releasedFrame.ElapsedMilliseconds < Release)
            {
                // ...re-set the button state to released
                report.Unset(button);
            }
            else
            {
                // all frames passed, reset and start over
                _isActive = false;

                _delayedFrame.Stop();
                _engagedFrame.Stop();
                _releasedFrame.Stop();
            }
        }

        #endregion

        #region Private fields

        private Stopwatch _delayedFrame = new Stopwatch();
        private Stopwatch _engagedFrame = new Stopwatch();
        private bool _isActive;
        private Stopwatch _releasedFrame = new Stopwatch();

        #endregion

        #region Properties

        /// <summary>
        ///     True if turbo mode is enabled for the current button, false otherwise.
        /// </summary>
        [DataMember]
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     The delay (in milliseconds) afther which the turbo mode shall engage (default is immediate).
        /// </summary>
        [DataMember]
        public int Delay { get; set; }

        /// <summary>
        ///     The timespan (in milliseconds) the button should be reported as remaining pressed to the output.
        /// </summary>
        [DataMember]
        public int Interval { get; set; }

        /// <summary>
        ///     The timespan (in milliseconds) the button state should be reported as released so the turbo event can repeat again.
        /// </summary>
        [DataMember]
        public int Release { get; set; }

        #endregion

        #region Deserialization

        private void OnCreated()
        {
            _delayedFrame = new Stopwatch();
            _engagedFrame = new Stopwatch();
            _releasedFrame = new Stopwatch();
            _isActive = false;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            OnCreated();
        }

        #endregion
    }
}