using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using PropertyChanged;

namespace ScpControl.Profiler
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
    public class DsButtonMappingTarget
    {
        public CommandType CommandType { get; set; }
        public object CommandTarget { get; set; }
    }

    /// <summary>
    ///     Represents a DualShock button/axis mapping profile.
    /// </summary>
    [ImplementPropertyChanged]
    [DataContract]
    public class DualShockProfile
    {
        #region Ctor

        public DualShockProfile()
        {
            Name = string.Empty;

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

            var props = this.GetType().GetProperties().Where(pi => pi.PropertyType == typeof (DsButtonProfile));

            Buttons = props.Select(b => b.GetValue(this)).Cast<DsButtonProfile>();
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Applies button re-mapping to the supplied report.
        /// </summary>
        /// <param name="report">The report to manipulate.</param>
        public void Remap(ref ScpHidReport report)
        {
            foreach (var buttonProfile in Buttons)
            {
                buttonProfile.Remap(ref report);
            }
        }

        /// <summary>
        ///     Deserializes an object from the specified XML file.
        /// </summary>
        /// <param name="file">The path of the XML file.</param>
        /// <returns>The retreived object.</returns>
        /// TODO: add error handling
        public static DualShockProfile Load(string file)
        {
            var knownTypes = new List<Type> {typeof (DsButtonProfile)};

            var serializer = new DataContractSerializer(typeof (DualShockProfile), knownTypes);

            using (var fs = File.OpenText(file))
            {
                using (var xml = XmlReader.Create(fs))
                {
                    return (DualShockProfile) serializer.ReadObject(xml);
                }
            }
        }

        /// <summary>
        ///     Serializes the current object to an XML file.
        /// </summary>
        /// <param name="file">The target XML file path.</param>
        /// TODO: add error handling
        public void Save(string file)
        {
            var serializer = new DataContractSerializer(typeof (DualShockProfile));

            using (var xml = XmlWriter.Create(file, new XmlWriterSettings {Indent = true}))
            {
                serializer.WriteObject(xml, this);
            }
        }

        public override bool Equals(object obj)
        {
            var profile = obj as DualShockProfile;

            return profile != null && profile.Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        private IEnumerable<DsButtonProfile> Buttons { get; set; }

        public DsButtonProfile Ps { get; set; }
        public DsButtonProfile Circle { get; set; }
        public DsButtonProfile Cross { get; set; }
        public DsButtonProfile Square { get; set; }
        public DsButtonProfile Triangle { get; set; }
        public DsButtonProfile Select { get; set; }
        public DsButtonProfile Start { get; set; }
        public DsButtonProfile LeftShoulder { get; set; }
        public DsButtonProfile RightShoulder { get; set; }
        public DsButtonProfile LeftTrigger { get; set; }
        public DsButtonProfile RightTrigger { get; set; }
        public DsButtonProfile LeftThumb { get; set; }
        public DsButtonProfile RightThumb { get; set; }

        #endregion
    }

    [ImplementPropertyChanged]
    [DataContract]
    public class DsButtonProfile
    {
        /// <summary>
        ///     Creates a new button mapping profile.
        /// </summary>
        /// <param name="sources">A list of DualShock buttons which will be affected by this profile.</param>
        public DsButtonProfile(params IDsButton[] sources)
        {
            SourceButtons = sources;
            MappingTarget = new DsButtonMappingTarget();
            Turbo = new DsButtonProfileTurboSetting();
        }

        private IEnumerable<IDsButton> SourceButtons { get; set; }
        public DsButtonMappingTarget MappingTarget { get; private set; }
        public bool IsEnabled { get; set; }
        public DsButtonProfileTurboSetting Turbo { get; set; }
        public byte CurrentValue { get; set; }

        /// <summary>
        ///     Applies button re-mapping to the supplied report.
        /// </summary>
        /// <param name="report">The report to manipulate.</param>
        public void Remap(ref ScpHidReport report)
        {
            if (!IsEnabled) return;

            switch (MappingTarget.CommandType)
            {
                case CommandType.GamepadButton:
                    foreach (var button in SourceButtons)
                    {
                        // if source button isn't pressed, skip it
                        if (!report[button].IsPressed) continue;

                        // unset original button
                        report.Unset(button);
                        // set target button
                        report.Set((IDsButton) MappingTarget.CommandTarget);
                    }
                    break;
            }
        }
    }

    /// <summary>
    ///     Describes button turbo mode details.
    /// </summary>
    [ImplementPropertyChanged]
    public class DsButtonProfileTurboSetting
    {
        public DsButtonProfileTurboSetting()
        {
            Delay = 0;
            Interval = 50;
            Release = 100;
        }

        /// <summary>
        ///     True if turbo mode is enabled for the current button, false otherwise.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     The delay (in milliseconds) afther which the turbo mode shall engage (default is immediate).
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        ///     The timespan (in milliseconds) the button should be reported as remaining pressed to the output.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        ///     The timespan (in milliseconds) the button state should be reported as released so the turbo event can repeat again.
        /// </summary>
        public int Release { get; set; }
    }
}
