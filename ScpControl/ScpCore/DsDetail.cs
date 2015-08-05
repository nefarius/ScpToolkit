using System;

namespace ScpControl.ScpCore
{
    public class DsDetail
    {
        private readonly byte[] m_Local = new byte[6];

        internal DsDetail()
        {
        }

        internal DsDetail(DsPadId PadId, DsState State, DsModel Model, byte[] Mac, DsConnection Mode, DsBattery Level)
        {
            Pad = PadId;
            this.State = State;
            this.Model = Model;
            this.Mode = Mode;
            Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);
        }

        public DsPadId Pad { get; private set; }
        public DsState State { get; private set; }
        public DsModel Model { get; private set; }

        public string Local
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2],
                    m_Local[3], m_Local[4], m_Local[5]);
            }
        }

        public DsConnection Mode { get; private set; }
        public DsBattery Charge { get; private set; }

        internal DsDetail Load(DsPadId PadId, DsState State, DsModel Model, byte[] Mac, DsConnection Mode,
            DsBattery Level)
        {
            Pad = PadId;
            this.State = State;
            this.Model = Model;
            this.Mode = Mode;
            Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);

            return this;
        }
    }
}
