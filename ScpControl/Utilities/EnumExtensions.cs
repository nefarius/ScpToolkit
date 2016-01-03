using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ScpControl.Utilities
{
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum en) //ext method
        {
            var type = en.GetType();

            var memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(
                    typeof (DescriptionAttribute),
                    false);

                if (attrs != null && attrs.Length > 0)

                    return ((DescriptionAttribute) attrs[0]).Description;
            }

            return en.ToString();
        }

        // Might want to return a named type, this is a lazy example (which does work though)
        public static IList<EnumMetaData> GetValuesAndDescriptions(Type enumType)
        {
            var values = Enum.GetValues(enumType).Cast<object>();
            var valuesAndDescriptions = from value in values
                                        select new EnumMetaData()
                                        {
                                            Value = value,
                                            Description = value.GetType()
                                                .GetMember(value.ToString())[0]
                                                .GetCustomAttributes(true)
                                                .OfType<DescriptionAttribute>()
                                                .First()
                                                .Description
                                        };
            return valuesAndDescriptions.ToList();
        }
    }

    public class EnumMetaData
    {
        public string Description { get; set; }
        public object Value { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (EnumMetaData)) return false;
            return Equals((EnumMetaData) obj);
        }

        protected bool Equals(EnumMetaData other)
        {
            return string.Equals(Description, other.Description);
        }

        public override int GetHashCode()
        {
            return (Description != null ? Description.GetHashCode() : 0);
        }

        public static bool operator ==(EnumMetaData left, EnumMetaData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EnumMetaData left, EnumMetaData right)
        {
            return !Equals(left, right);
        }
    }
}