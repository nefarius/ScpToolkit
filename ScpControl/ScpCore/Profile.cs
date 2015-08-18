using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ScpControl.ScpCore
{
    public class ProfileCollection : SortedDictionary<string, Profile>
    {
    }

    [DataContract]
    public class Profile
    {
        private readonly Ds3AxisMap _ds3AxisMap = new Ds3AxisMap();
        private readonly Ds3ButtonMap _ds3ButtonMap = new Ds3ButtonMap();
        private readonly Ds4AxisMap _ds4AxisMap = new Ds4AxisMap();
        private readonly Ds4ButtonMap _ds4ButtonMap = new Ds4ButtonMap();
        private readonly DsMatch _match = DsMatch.Global;
        private readonly string _pad = string.Empty;
        private readonly string _mac = string.Empty;

        public Profile(string name)
        {
            Name = name;
        }

        public Profile(bool setDefault, string name, string type, string qualifier)
            : this(name)
        {
            Type = type;

            IsDefault = setDefault;
            _match = (DsMatch)Enum.Parse(typeof(DsMatch), type, true);

            switch (_match)
            {
                case DsMatch.Pad:
                    _pad = qualifier;
                    break;
                case DsMatch.Mac:
                    _mac = qualifier;
                    break;
            }
        }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public DsMatch Match
        {
            get { return _match; }
        }

        public string Qualifier
        {
            get
            {
                var qualifier = string.Empty;

                switch (_match)
                {
                    case DsMatch.Pad:
                        qualifier = _pad;
                        break;
                    case DsMatch.Mac:
                        qualifier = _mac;
                        break;
                }

                return qualifier;
            }
        }

        public bool IsDefault { get; set; }

        public Ds3ButtonMap Ds3Button
        {
            get { return _ds3ButtonMap; }
        }

        public Ds3AxisMap Ds3Axis
        {
            get { return _ds3AxisMap; }
        }

        public Ds4ButtonMap Ds4Button
        {
            get { return _ds4ButtonMap; }
        }

        public Ds4AxisMap Ds4Axis
        {
            get { return _ds4AxisMap; }
        }

        public DsMatch Usage(string Pad, string Mac)
        {
            var matched = DsMatch.None;

            switch (_match)
            {
                case DsMatch.Mac:
                    if (Mac == _mac) matched = DsMatch.Mac;
                    break;
                case DsMatch.Pad:
                    if (Pad == _pad) matched = DsMatch.Pad;
                    break;
                case DsMatch.Global:
                    if (IsDefault) matched = DsMatch.Global;
                    break;
            }

            return matched;
        }
    }
}
