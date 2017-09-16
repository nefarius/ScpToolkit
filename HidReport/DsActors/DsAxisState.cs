using System;
using HidReport.Contract.DsActors;

namespace HidReport.DsActors
{
    /// <summary>
    ///     Implements a DualShock axis state.
    /// </summary>
    [Serializable]
    public class DsAxisState : IDsAxisStateImmutable
    {
        
        public DsAxisState(byte defaultValue)
        {
            Value = 0x80;
            DefaultValue = defaultValue;
        }

        public byte DefaultValue { get; }

        public byte Value { get; set; }

        public bool IsEngaged
        {
            get
            {
                return DefaultValue == 0x00
                    ? DefaultValue != Value
                    // match a range for jitter compensation
                    : (DefaultValue - 10 > Value)
                      || (DefaultValue + 10 < Value);
            }
        }

        private static float ClampAxis(float value) { if (value > 1.0f) return 1.0f; else if (value < -1.0f) return -1.0f; else return value; }

        public static float ToAxis(byte value) { return ClampAxis((((value & 0xFF) - 0x7F) * 2) / 254.0f); }
        public float Axis => ToAxis(Value);
    }
}