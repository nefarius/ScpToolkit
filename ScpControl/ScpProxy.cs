using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Reflection;
using log4net;

namespace ScpControl
{
    public partial class ScpProxy : Component
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected static Char[] m_Delim = new Char[] { '^' };

        protected IPEndPoint m_ServerEp = new IPEndPoint(IPAddress.Loopback, 26760);
        protected UdpClient m_Server = new UdpClient();

        protected IPEndPoint m_ClientEp = new IPEndPoint(IPAddress.Loopback, 26761);
        protected UdpClient m_Client = new UdpClient();

        protected XmlDocument m_Map = new XmlDocument();
        protected XmlMapper m_Mapper = new XmlMapper();
        protected Boolean m_Active = false;

        public event EventHandler<DsPacket> Packet = null;


        public virtual XmlMapper Mapper
        {
            get { return m_Mapper; }
        }

        public virtual String Active
        {
            get
            {
                String Active = String.Empty;

                try
                {
                    Byte[] Send = { 0, 6 };

                    if (m_Server.Send(Send, Send.Length, m_ServerEp) == Send.Length)
                    {
                        IPEndPoint ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                        Byte[] Buffer = m_Server.Receive(ref ReferenceEp);

                        if (Buffer.Length > 0)
                        {
                            String Data = Encoding.Unicode.GetString(Buffer);
                            String[] Split = Data.Split(m_Delim, StringSplitOptions.RemoveEmptyEntries);

                            Active = Split[0];
                        }
                    }
                }
                catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

                return Active;
            }
        }

        public virtual Boolean Enabled
        {
            get
            {
                Boolean Native = false;

                try
                {
                    Byte[] Send = { 0, 3 };

                    if (m_Server.Send(Send, Send.Length, m_ServerEp) == Send.Length)
                    {
                        IPEndPoint ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                        Byte[] Buffer = m_Server.Receive(ref ReferenceEp);

                        if (Buffer.Length > 0)
                        {
                            Native = Buffer[13] == 0;
                        }
                    }
                }
                catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

                return Native;
            }
        }


        public ScpProxy()
        {
            InitializeComponent();
        }

        public ScpProxy(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public virtual Boolean Start()
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
            catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

            return m_Active;
        }

        public virtual Boolean Stop()
        {
            try
            {
                if (m_Active)
                {
                    NativeFeed_Worker.CancelAsync();
                    m_Active = false;
                }
            }
            catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

            return !m_Active;
        }


        public virtual Boolean Load()
        {
            Boolean Loaded = false;

            try
            {
                Byte[] Buffer = { 0, 0x08 };

                if (m_Server.Send(Buffer, Buffer.Length, m_ServerEp) == Buffer.Length)
                {
                    IPEndPoint ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                    Buffer = m_Server.Receive(ref ReferenceEp);

                    if (Buffer.Length > 0)
                    {
                        String Data = Encoding.UTF8.GetString(Buffer);

                        m_Map.LoadXml(Data);

                        m_Mapper.Initialize(m_Map);
                    }
                }

                Loaded = true;
            }
            catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

            return Loaded;
        }

        public virtual Boolean Save()
        {
            Boolean Saved = false;

            try
            {
                if (m_Active)
                {
                    if (m_Mapper.Construct(ref m_Map))
                    {
                        Byte[] Data = Encoding.UTF8.GetBytes(m_Map.InnerXml);
                        Byte[] Buffer = new Byte[Data.Length + 2];

                        Buffer[1] = 0x09;
                        Array.Copy(Data, 0, Buffer, 2, Data.Length);

                        m_Client.Send(Buffer, Buffer.Length, m_ServerEp);
                        Saved = true;
                    }
                }
            }
            catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

            return Saved;
        }


        public virtual Boolean Select(Profile Target)
        {
            Boolean Selected = false;

            try
            {
                if (m_Active)
                {
                    Byte[] Data = Encoding.Unicode.GetBytes(Target.Name);
                    Byte[] Send = new Byte[Data.Length + 2];

                    Send[1] = 0x07;
                    Array.Copy(Data, 0, Send, 2, Data.Length);

                    m_Server.Send(Send, Send.Length, m_ServerEp);

                    SetDefault(Target);
                    Selected = true;
                }
            }
            catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

            return Selected;
        }

        public virtual DsDetail Detail(DsPadId Pad)
        {
            DsDetail Detail = null;

            try
            {
                Byte[] Buffer = { (Byte)Pad, 0x0A };

                if (m_Server.Send(Buffer, Buffer.Length, m_ServerEp) == Buffer.Length)
                {
                    IPEndPoint ReferenceEp = new IPEndPoint(IPAddress.Loopback, 0);

                    Buffer = m_Server.Receive(ref ReferenceEp);

                    if (Buffer.Length > 0)
                    {
                        Byte[] Local = new Byte[6]; Array.Copy(Buffer, 5, Local, 0, Local.Length);

                        Detail = new DsDetail((DsPadId)Buffer[0], (DsState)Buffer[1], (DsModel)Buffer[2], Local, (DsConnection)Buffer[3], (DsBattery)Buffer[4]);
                    }
                }
            }
            catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

            return Detail;
        }


        public virtual Boolean Rumble(DsPadId Pad, Byte Large, Byte Small)
        {
            Boolean Rumbled = false;

            try
            {
                if (m_Active)
                {
                    Byte[] Buffer = { (Byte)Pad, 0x01, Large, Small };

                    m_Server.Send(Buffer, Buffer.Length, m_ServerEp);
                    Rumbled = true;
                }
            }
            catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

            return Rumbled;
        }

        public virtual Boolean Remap(String Target, DsPacket Packet)
        {
            Boolean Remapped = false;

            try
            {
                if (m_Active)
                {
                    Byte[] Output = new Byte[Packet.Native.Length];

                    switch (Packet.Detail.Model)
                    {
                        case DsModel.DS3: if (m_Mapper.RemapDs3(m_Mapper.Map[Target], Packet.Native, Output)) { Array.Copy(Output, Packet.Native, Output.Length); Packet.Remapped(); } break;
                        case DsModel.DS4: if (m_Mapper.RemapDs4(m_Mapper.Map[Target], Packet.Native, Output)) { Array.Copy(Output, Packet.Native, Output.Length); Packet.Remapped(); } break;
                    }

                    Remapped = true;
                }
            }
            catch (Exception ex) { Log.ErrorFormat("Unexpected error: {0}", ex); }

            return Remapped;
        }


        public virtual Boolean SetDefault(Profile Profile)
        {
            Boolean Set = true;

            try
            {
                foreach (Profile Item in m_Mapper.Map.Values)
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


        protected virtual void NativeFeed_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            DsPacket Packet = new DsPacket();
            Byte[] Buffer = new Byte[ReportEventArgs.Length];

            while (!NativeFeed_Worker.CancellationPending)
            {
                try
                {
                    m_Client.Client.Receive(Buffer);
                    LogPacket(Packet.Load(Buffer));
                }
                catch { }
            }

            m_Client.Close();
            e.Cancel = true;
        }

        protected virtual void LogPacket(DsPacket Data)
        {
            if (Packet != null)
            {
                Packet(this, Data);
            }
        }
    }

    public class DsPacket : EventArgs
    {
        protected Int32 m_Packet;
        protected DsDetail m_Detail = new DsDetail();
        protected Byte[] m_Native = new Byte[96];
        protected Byte[] m_Local = new Byte[6];
        protected Ds3Button m_Ds3Button = Ds3Button.None;
        protected Ds4Button m_Ds4Button = Ds4Button.None;

        internal DsPacket() { }

        internal DsPacket Load(Byte[] Native)
        {
            Array.Copy(Native, (Int32)DsOffset.Address, m_Local, 0, m_Local.Length);

            m_Detail.Load(
                    (DsPadId)Native[(Int32)DsOffset.Pad],
                    (DsState)Native[(Int32)DsOffset.State],
                    (DsModel)Native[(Int32)DsOffset.Model],
                    m_Local,
                    (DsConnection)Native[(Int32)DsOffset.Connection],
                    (DsBattery)Native[(Int32)DsOffset.Battery]
                    );

            m_Packet = (Int32)(Native[4] << 0 | Native[5] << 8 | Native[6] << 16 | Native[7] << 24);
            Array.Copy(Native, m_Native, m_Native.Length);

            switch (m_Detail.Model)
            {
                case DsModel.DS3: m_Ds3Button = (Ds3Button)((Native[10] << 0) | (Native[11] << 8) | (Native[12] << 16) | (Native[13] << 24)); break;
                case DsModel.DS4: m_Ds4Button = (Ds4Button)((Native[13] << 0) | (Native[14] << 8) | ((Native[15] & 0x03) << 16)); break;
            }

            return this;
        }


        internal Byte[] Native
        {
            get { return m_Native; }
        }

        internal void Remapped()
        {
            switch (m_Detail.Model)
            {
                case DsModel.DS3: m_Ds3Button = (Ds3Button)((Native[10] << 0) | (Native[11] << 8) | (Native[12] << 16) | (Native[13] << 24)); break;
                case DsModel.DS4: m_Ds4Button = (Ds4Button)((Native[13] << 0) | (Native[14] << 8) | ((Native[15] & 0x03) << 16)); break;
            }
        }


        public DsDetail Detail
        {
            get { return m_Detail; }
        }


        public Boolean Button(Ds3Button Flag)
        {
            if (m_Detail.Model != DsModel.DS3) throw new InvalidEnumArgumentException();

            return m_Ds3Button.HasFlag(Flag);
        }

        public Boolean Button(Ds4Button Flag)
        {
            if (m_Detail.Model != DsModel.DS4) throw new InvalidEnumArgumentException();

            return m_Ds4Button.HasFlag(Flag);
        }


        public Byte Axis(Ds3Axis Offset)
        {
            if (m_Detail.Model != DsModel.DS3) throw new InvalidEnumArgumentException();

            return Native[(Int32)Offset];
        }

        public Byte Axis(Ds4Axis Offset)
        {
            if (m_Detail.Model != DsModel.DS4) throw new InvalidEnumArgumentException();

            return Native[(Int32)Offset];
        }
    }

    public class DsDetail
    {
        protected DsPadId m_Serial;
        protected DsModel m_Model;
        protected Byte[] m_Local = new Byte[6];
        protected DsConnection m_Mode;
        protected DsBattery m_Charge;
        protected DsState m_State;

        internal DsDetail() { }

        internal DsDetail(DsPadId PadId, DsState State, DsModel Model, Byte[] Mac, DsConnection Mode, DsBattery Level)
        {
            m_Serial = PadId;
            m_State = State;
            m_Model = Model;
            m_Mode = Mode;
            m_Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);
        }

        internal DsDetail Load(DsPadId PadId, DsState State, DsModel Model, Byte[] Mac, DsConnection Mode, DsBattery Level)
        {
            m_Serial = PadId;
            m_State = State;
            m_Model = Model;
            m_Mode = Mode;
            m_Charge = Level;

            Array.Copy(Mac, m_Local, m_Local.Length);

            return this;
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

        public String Local
        {
            get { return String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", m_Local[0], m_Local[1], m_Local[2], m_Local[3], m_Local[4], m_Local[5]); }
        }

        public DsConnection Mode
        {
            get { return m_Mode; }
        }

        public DsBattery Charge
        {
            get { return m_Charge; }
        }
    }
}
