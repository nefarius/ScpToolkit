using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using PropertyChanged;

namespace ScpControl.Profiler
{
    public enum CommandType : byte
    {
        [Description("Keystrokes")] Keystrokes,
        [Description("Gamepad buttons")] GamepadButton,
        [Description("Mouse buttons")] MouseButtons,
        [Description("Mouse axis")] MouseAxis
    }

    [ImplementPropertyChanged]
    public class DsButtonMappingTarget
    {
        public CommandType CommandType { get; set; }
        public object CommandTarget { get; set; }
    }

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

        public IDsButtonProfile Ps { get; set; }
        public IDsButtonProfile Circle { get; set; }
        public IDsButtonProfile Cross { get; set; }
        public IDsButtonProfile Square { get; set; }
        public IDsButtonProfile Triangle { get; set; }
        public IDsButtonProfile Select { get; set; }
        public IDsButtonProfile Start { get; set; }
        public IDsButtonProfile LeftShoulder { get; set; }
        public IDsButtonProfile RightShoulder { get; set; }
        public IDsButtonProfile LeftTrigger { get; set; }
        public IDsButtonProfile RightTrigger { get; set; }
        public IDsButtonProfile LeftThumb { get; set; }
        public IDsButtonProfile RightThumb { get; set; }

        #endregion
    }

    public interface IDsButtonProfile
    {
        DsButtonMappingTarget MappingTarget { get; set; }
        bool IsEnabled { get; set; }
        DsButtonProfileTurboSetting Turbo { get; set; }
        byte CurrentValue { get; set; }
    }

    [KnownType(typeof (DsButtonProfile))]
    [ImplementPropertyChanged]
    public class DsButtonProfile : IDsButtonProfile
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

    public class DsButtonProfileTurboSetting
    {
        public DsButtonProfileTurboSetting()
        {
            Delay = 0;
            Interval = 50;
            Release = 100;
        }

        public bool IsEnabled { get; set; }
        public int Delay { get; set; }
        public int Interval { get; set; }
        public int Release { get; set; }
    }
}