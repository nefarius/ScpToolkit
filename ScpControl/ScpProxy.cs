using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml;
using log4net;

namespace ScpControl
{
    public sealed partial class ScpProxy : Component
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly char[] m_Delim = {'^'};
        private readonly IPEndPoint m_ClientEp = new IPEndPoint(IPAddress.Loopback, 26761);
        private readonly XmlMapper m_Mapper = new XmlMapper();
        private readonly UdpClient m_Server = new UdpClient();
        private readonly IPEndPoint m_ServerEp = new IPEndPoint(IPAddress.Loopback, 26760);
        private bool m_Active;
        private UdpClient m_Client = new UdpClient();
        private XmlDocument m_Map = new XmlDocument();

        public ScpProxy()
        {
            InitializeComponent();
        }

        public ScpProxy(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public XmlMapper Mapper
        {
            get { return m_Mapper; }
        }

        public string Active
        {
            get
            {
                var Active = string.Empty;

                try
                {
                    byte[] Send = {0, 6};

                    if (m_Server.Send(Send, Send.Length, m_ServerEp) == Send.Length)
                    {
                        var ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                        var Buffer = m_Server.Receive(ref ReferenceEp);

                        if (Buffer.Length > 0)
                        {
                            var Data = Encoding.Unicode.GetString(Buffer);
                            var Split = Data.Split(m_Delim, StringSplitOptions.RemoveEmptyEntries);

                            Active = Split[0];
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }

                return Active;
            }
        }

        public bool Enabled
        {
            get
            {
                var Native = false;

                try
                {
                    byte[] Send = {0, 3};

                    if (m_Server.Send(Send, Send.Length, m_ServerEp) == Send.Length)
                    {
                        var ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                        var Buffer = m_Server.Receive(ref ReferenceEp);

                        if (Buffer.Length > 0)
                        {
                            Native = Buffer[13] == 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error: {0}", ex);
                }

                return Native;
            }
        }

        public event EventHandler<DsPacket> Packet;

        public bool Start()
        {
            try
            {
                if (!m_Active)
                {
                    m_Client = new UdpClient(m_ClientEp);
                    m_Client.Client.ReceiveTimeout = 500;

                    NativeFeed_Worker.RunWorkerAsync();
                    m_Active = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return m_Active;
        }

        public bool Stop()
        {
            try
            {
                if (m_Active)
                {
                    NativeFeed_Worker.CancelAsync();
                    m_Active = false;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return !m_Active;
        }

        public bool Load()
        {
            var Loaded = false;

            try
            {
                byte[] Buffer = {0, 0x08};

                if (m_Server.Send(Buffer, Buffer.Length, m_ServerEp) == Buffer.Length)
                {
                    var ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                    Buffer = m_Server.Receive(ref ReferenceEp);

                    if (Buffer.Length > 0)
                    {
                        var Data = Encoding.UTF8.GetString(Buffer);

                        m_Map.LoadXml(Data);

                        m_Mapper.Initialize(m_Map);
                    }
                }

                Loaded = true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Loaded;
        }

        public bool Save()
        {
            var Saved = false;

            try
            {
                if (m_Active)
                {
                    if (m_Mapper.Construct(ref m_Map))
                    {
                        var Data = Encoding.UTF8.GetBytes(m_Map.InnerXml);
                        var Buffer = new byte[Data.Length + 2];

                        Buffer[1] = 0x09;
                        Array.Copy(Data, 0, Buffer, 2, Data.Length);

                        m_Client.Send(Buffer, Buffer.Length, m_ServerEp);
                        Saved = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Saved;
        }

        public bool Select(Profile Target)
        {
            var Selected = false;

            try
            {
                if (m_Active)
                {
                    var Data = Encoding.Unicode.GetBytes(Target.Name);
                    var Send = new byte[Data.Length + 2];

                    Send[1] = 0x07;
                    Array.Copy(Data, 0, Send, 2, Data.Length);

                    m_Server.Send(Send, Send.Length, m_ServerEp);

                    SetDefault(Target);
                    Selected = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Selected;
        }

        public DsDetail Detail(DsPadId Pad)
        {
            DsDetail Detail = null;

            try
            {
                byte[] Buffer = {(byte) Pad, 0x0A};

                if (m_Server.Send(Buffer, Buffer.Length, m_ServerEp) == Buffer.Length)
                {
                    var ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                    Buffer = m_Server.Receive(ref ReferenceEp);

                    if (Buffer.Length > 0)
                    {
                        var Local = new byte[6];
                        Array.Copy(Buffer, 5, Local, 0, Local.Length);

                        Detail = new DsDetail((DsPadId) Buffer[0], (DsState) Buffer[1], (DsModel) Buffer[2], Local,
                            (DsConnection) Buffer[3], (DsBattery) Buffer[4]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Detail;
        }

        public bool Rumble(DsPadId Pad, byte Large, byte Small)
        {
            var Rumbled = false;

            try
            {
                if (m_Active)
                {
                    byte[] Buffer = {(byte) Pad, 0x01, Large, Small};

                    m_Server.Send(Buffer, Buffer.Length, m_ServerEp);
                    Rumbled = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Rumbled;
        }

        public bool Remap(string Target, DsPacket Packet)
        {
            var Remapped = false;

            try
            {
                if (m_Active)
                {
                    var Output = new byte[Packet.Native.Length];

                    switch (Packet.Detail.Model)
                    {
                        case DsModel.DS3:
                            if (m_Mapper.RemapDs3(m_Mapper.Map[Target], Packet.Native, Output))
                            {
                                Array.Copy(Output, Packet.Native, Output.Length);
                                Packet.Remapped();
                            }
                            break;
                        case DsModel.DS4:
                            if (m_Mapper.RemapDs4(m_Mapper.Map[Target], Packet.Native, Output))
                            {
                                Array.Copy(Output, Packet.Native, Output.Length);
                                Packet.Remapped();
                            }
                            break;
                    }

                    Remapped = true;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Remapped;
        }

        public bool SetDefault(Profile Profile)
        {
            var Set = true;

            try
            {
                foreach (var Item in m_Mapper.Map.Values)
                {
                    Item.Default = false;
                }

                Profile.Default = true;
            }
            catch (Exception ex)
            {
                Set = false;
                Log.ErrorFormat("Unexpected error: {0}", ex);
            }

            return Set;
        }

        private void NativeFeed_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var Packet = new DsPacket();
            var Buffer = new byte[ReportEventArgs.Length];

            while (!NativeFeed_Worker.CancellationPending)
            {
                try
                {
                    m_Client.Client.Receive(Buffer);
                    LogPacket(Packet.Load(Buffer));
                }
                catch
                {
                }
            }

            m_Client.Close();
            e.Cancel = true;
        }

        private void LogPacket(DsPacket Data)
        {
            if (Packet != null)
            {
                Packet(this, Data);
            }
        }
    }

    public class DsPacket : EventArgs
    {
        private DsDetail m_Detail = new DsDetail();
        private Ds3Button m_Ds3Button = Ds3Button.None;
        private Ds4Button m_Ds4Button = Ds4Button.None;
        private byte[] m_Local = new byte[6];
        private byte[] m_Native = new byte[96];
        private int m_Packet;

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
            Array.Copy(Native, (int) DsOffset.Address, m_Local, 0, m_Local.Length);

            m_Detail.Load(
                (DsPadId) Native[(int) DsOffset.Pad],
                (DsState) Native[(int) DsOffset.State],
                (DsModel) Native[(int) DsOffset.Model],
                m_Local,
                (DsConnection) Native[(int) DsOffset.Connection],
                (DsBattery) Native[(int) DsOffset.Battery]
                );

            m_Packet = Native[4] << 0 | Native[5] << 8 | Native[6] << 16 | Native[7] << 24;
            Array.Copy(Native, m_Native, m_Native.Length);

            switch (m_Detail.Model)
            {
                case DsModel.DS3:
                    m_Ds3Button =
                        (Ds3Button) ((Native[10] << 0) | (Native[11] << 8) | (Native[12] << 16) | (Native[13] << 24));
                    break;
                case DsModel.DS4:
                    m_Ds4Button = (Ds4Button) ((Native[13] << 0) | (Native[14] << 8) | ((Native[15] & 0x03) << 16));
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
                        (Ds3Button) ((Native[10] << 0) | (Native[11] << 8) | (Native[12] << 16) | (Native[13] << 24));
                    break;
                case DsModel.DS4:
                    m_Ds4Button = (Ds4Button) ((Native[13] << 0) | (Native[14] << 8) | ((Native[15] & 0x03) << 16));
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

            return Native[(int) Offset];
        }

        public byte Axis(Ds4Axis Offset)
        {
            if (m_Detail.Model != DsModel.DS4) throw new InvalidEnumArgumentException();

            return Native[(int) Offset];
        }
    }

    public class DsDetail
    {
        private DsBattery m_Charge;
        private byte[] m_Local = new byte[6];
        private DsConnection m_Mode;
        private DsModel m_Model;
        private DsPadId m_Serial;
        private DsState m_State;

        internal DsDetail()
        {
        }

        internal DsDetail(DsPadId PadId, DsState State, DsModel Model, byte[] Mac, DsConnection Mode, DsBattery Level)
        {
            m_Serial = PadId;
            m_State = State;
            m_Model = Model;
            m_Mode = Mode;
            m_Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);
        }

        public DsPadId Pad
        {
            get { return m_Serial; }
        }

        public DsState State
        {
            get { return m_State; }
        }

        public DsModel Model
        {
            get { return m_Model; }
        }

        public string Local
        {
            get
            {
                return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2],
                    m_Local[3], m_Local[4], m_Local[5]);
            }
        }

        public DsConnection Mode
        {
            get { return m_Mode; }
        }

        public DsBattery Charge
        {
            get { return m_Charge; }
        }

        internal DsDetail Load(DsPadId PadId, DsState State, DsModel Model, byte[] Mac, DsConnection Mode,
            DsBattery Level)
        {
            m_Serial = PadId;
            m_State = State;
            m_Model = Model;
            m_Mode = Mode;
            m_Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);

            return this;
        }
    }
}