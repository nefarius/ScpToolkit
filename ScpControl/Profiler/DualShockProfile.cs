using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    public class DualShockProfile
    {
        #region Ctor

        public DualShockProfile()
        {
            Name = string.Empty;

            Ps = new DsButtonProfile();
            Circle = new DsButtonProfile();
            Cross = new DsButtonProfile();
            Square = new DsButtonProfile();
            Triangle = new DsButtonProfile();
            Select = new DsButtonProfile();
            Start = new DsButtonProfile();
            LeftShoulder = new DsButtonProfile();
            RightShoulder = new DsButtonProfile();
            LeftTrigger = new DsButtonProfile();
            RightTrigger = new DsButtonProfile();
            LeftThumb = new DsButtonProfile();
            RightThumb = new DsButtonProfile();
        }

        #endregion

        #region Public methods

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
    public class DsButtonProfile
    {
        public DsButtonProfile()
        {
            MappingTarget = new DsButtonMappingTarget();
            Turbo = new DsButtonProfileTurboSetting();
        }

        public DsButtonMappingTarget MappingTarget { get; set; }
        public bool IsEnabled { get; set; }
        public DsButtonProfileTurboSetting Turbo { get; set; }
        public byte CurrentValue { get; set; }
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
