using System;
using ScpControl.Profiler;

namespace ScpControl.ScpCore
{
    [Obsolete]
    public class DsDetail
    {
        private readonly byte[] _localMac = new byte[6];

        internal DsDetail()
        {
        }

        internal DsDetail(DsPadId padId, DsState state, DsModel model, byte[] mac, DsConnection mode, DsBattery level)
        {
            Pad = padId;
            State = state;
            Model = model;
            Mode = mode;
            Charge = level;

            Buffer.BlockCopy(mac, 0, _localMac, 0, _localMac.Length);
        }

        public DsPadId Pad { get; private set; }
        public DsState State { get; private set; }
        public DsModel Model { get; private set; }

        public string Local
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", _localMac[0], _localMac[1],
                    _localMac[2],
                    _localMac[3], _localMac[4], _localMac[5]);
            }
        }

        public DsConnection Mode { get; private set; }
        public DsBattery Charge { get; private set; }

        internal DsDetail Load(DsPadId padId, DsState state, DsModel model, byte[] mac, DsConnection mode,
            DsBattery level)
        {
            Pad = padId;
            State = state;
            Model = model;
            Mode = mode;
            Charge = level;

            Buffer.BlockCopy(mac, 0, _localMac, 0, _localMac.Length);

            return this;
        }
    }
}