using System;

namespace ScpControl.Bluetooth
{
    public sealed class BthHandle : IEquatable<BthHandle>, IComparable<BthHandle>
    {
        private readonly byte[] _handle = new byte[2] { 0x00, 0x00 };
        private readonly ushort _value;

        public BthHandle(byte lsb, byte msb)
        {
            _handle[0] = lsb;
            _handle[1] = msb;

            _value = (ushort)(_handle[0] | (ushort)(_handle[1] << 8));
        }

        public BthHandle(byte[] handle)
            : this(handle[0], handle[1])
        {
        }

        public BthHandle(ushort Short)
            : this((byte)((Short >> 0) & 0xFF), (byte)((Short >> 8) & 0xFF))
        {
        }

        public byte[] Bytes
        {
            get { return _handle; }
        }

        public ushort Short
        {
            get { return _value; }
        }

        #region IComparable<BthHandle> Members

        public int CompareTo(BthHandle other)
        {
            return _value.CompareTo(other._value);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0:X4}", _value);
        }

        #region IEquatable<BthHandle> Members

        public bool Equals(BthHandle other)
        {
            return _value == other._value;
        }

        public bool Equals(byte lsb, byte msb)
        {
            return _handle[0] == lsb && _handle[1] == msb;
        }

        public bool Equals(byte[] other)
        {
            return Equals(other[0], other[1]);
        }

        #endregion
    }

}
