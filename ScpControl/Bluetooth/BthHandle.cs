using System;

namespace ScpControl.Bluetooth
{
    public sealed class BthHandle : IEquatable<BthHandle>, IComparable<BthHandle>
    {
        private readonly byte[] _mHandle = new byte[2] { 0x00, 0x00 };
        private readonly ushort _mValue;

        public BthHandle(byte lsb, byte msb)
        {
            _mHandle[0] = lsb;
            _mHandle[1] = msb;

            _mValue = (ushort)(_mHandle[0] | (ushort)(_mHandle[1] << 8));
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
            get { return _mHandle; }
        }

        public ushort Short
        {
            get { return _mValue; }
        }

        #region IComparable<BthHandle> Members

        public int CompareTo(BthHandle other)
        {
            return _mValue.CompareTo(other._mValue);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0:X4}", _mValue);
        }

        #region IEquatable<BthHandle> Members

        public bool Equals(BthHandle other)
        {
            return _mValue == other._mValue;
        }

        public bool Equals(byte lsb, byte msb)
        {
            return _mHandle[0] == lsb && _mHandle[1] == msb;
        }

        public bool Equals(byte[] other)
        {
            return Equals(other[0], other[1]);
        }

        #endregion
    }

}
