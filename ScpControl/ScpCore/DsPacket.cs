using System;
using System.ComponentModel;

namespace ScpControl.ScpCore
{
    public class DsPacket : EventArgs
    {
        private Ds3Button m_Ds3Button = Ds3Button.None;
        private Ds4Button m_Ds4Button = Ds4Button.None;
        private int m_Packet;
        private readonly DsDetail m_Detail = new DsDetail();
        private readonly byte[] m_Local = new byte[6];
        private readonly byte[] m_Native = new byte[96];

        internal DsPacket()
        {
        }

        internal byte[] Native
        {
            get { return m_Native; }
        }

        public DsDetail Detail
        {
            get { return m_Detail; }
        }

        internal DsPacket Load(byte[] Native)
        {
            Buffer.BlockCopy(Native, (int)DsOffset.Address, m_Local, 0, m_Local.Length);

            m_Detail.Load(
                (DsPadId)Native[(int)DsOffset.Pad],
                (DsState)Native[(int)DsOffset.State],
                (DsModel)Native[(int)DsOffset.Model],
                m_Local,
                (DsConnection)Native[(int)DsOffset.Connection],
                (DsBattery)Native[(int)DsOffset.Battery]
                );

            m_Packet = Native[4] << 0 | Native[5] << 8 | Native[6] << 16 | Native[7] << 24;
            Array.Copy(Native, m_Native, m_Native.Length);

            switch (m_Detail.Model)
            {
                case DsModel.DS3:
                    m_Ds3Button =
                        (Ds3Button)((Native[10] << 0) | (Native[11] << 8) | (Native[12] << 16) | (Native[13] << 24));
                    break;
                case DsModel.DS4:
                    m_Ds4Button = (Ds4Button)((Native[13] << 0) | (Native[14] << 8) | ((Native[15] & 0x03) << 16));
                    break;
            }

            return this;
        }

        internal void Remapped()
        {
            switch (m_Detail.Model)
            {
                case DsModel.DS3:
                    m_Ds3Button =
                        (Ds3Button)((Native[10] << 0) | (Native[11] << 8) | (Native[12] << 16) | (Native[13] << 24));
                    break;
                case DsModel.DS4:
                    m_Ds4Button = (Ds4Button)((Native[13] << 0) | (Native[14] << 8) | ((Native[15] & 0x03) << 16));
                    break;
            }
        }

        public bool Button(Ds3Button Flag)
        {
            if (m_Detail.Model != DsModel.DS3) throw new InvalidEnumArgumentException();

            return m_Ds3Button.HasFlag(Flag);
        }

        public bool Button(Ds4Button Flag)
        {
            if (m_Detail.Model != DsModel.DS4) throw new InvalidEnumArgumentException();

            return m_Ds4Button.HasFlag(Flag);
        }

        public byte Axis(Ds3Axis Offset)
        {
            if (m_Detail.Model != DsModel.DS3) throw new InvalidEnumArgumentException();

            return Native[(int)Offset];
        }

        public byte Axis(Ds4Axis Offset)
        {
            if (m_Detail.Model != DsModel.DS4) throw new InvalidEnumArgumentException();

            return Native[(int)Offset];
        }
    }
}
