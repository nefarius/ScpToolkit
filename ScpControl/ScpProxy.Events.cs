using System;
using ScpControl.Rx;

namespace ScpControl
{
    public partial class ScpProxy
    {
        #region Public events

        public event EventHandler<DsPacket> NativeFeedReceived;

        public event EventHandler<ScpCommandPacket> StatusDataReceived;

        public event EventHandler<ScpCommandPacket> XmlReceived;

        public event EventHandler<ScpCommandPacket> ConfigReceived;

        public event EventHandler<EventArgs> RootHubDisconnected;

        #endregion

        #region Event methods

        private void OnFeedPacketReceived(DsPacket data)
        {
            if (NativeFeedReceived != null)
            {
                NativeFeedReceived(this, data);
            }
        }

        private void OnStatusData(ScpCommandPacket packet)
        {
            if (StatusDataReceived != null)
            {
                StatusDataReceived(this, packet);
            }
        }

        private void OnXmlReceived(ScpCommandPacket packet)
        {
            if (XmlReceived != null)
            {
                XmlReceived(this, packet);
            }
        }

        private void OnConfigReceived(ScpCommandPacket packet)
        {
            if (ConfigReceived != null)
            {
                ConfigReceived(this, packet);
            }
        }

        private void OnRootHubDisconnected(EventArgs args)
        {
            if (RootHubDisconnected != null)
            {
                RootHubDisconnected(this, args);
            }
        }

        #endregion
    }
}
